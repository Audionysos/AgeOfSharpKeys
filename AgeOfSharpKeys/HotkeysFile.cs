using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace aoe2.hotkeys;

/// <summary>Reads hotkeys from a single hotkey file, either <see cref="FileFormat.HKI"/> or <see cref="FileFormat.HKP"/>.
/// Note that single hotkeys setup you use in game does not consist of only a single file - see <see cref="HotkeysProfile"/>.</summary>
public class HotkeysFile : IDisposable {
	/// <summary>Full memory stream decompressed from the <see cref="file"/>.</summary>
	private Stream stream;
	private BinaryReader sr;
	/// <summary>Allows simple read/write of hotkey struct in a stream.</summary>
	private StructToStream<HotkeyData> str;

	/// <summary>Path to file that was read.</summary>
	public string file { get; }
	/// <summary>Older/newer version of file. The only difference in data structure is that the never formant adds 3 additional hotkeys groups in the middle of the header.
	/// If anyone knows why they did this instead of increasing the count and adding the groups at the end of the file, please let me know.</summary>
	public FileFormat format { get; private set; }
	/// <summary>Describes specific version of the file, or I guess in more proper words version of the game.
	/// This doesn't affect file format in anyway but it's only used to validate it it's actually a hotkey file.
	/// The <see cref="HotkeysFile"/> doesn't check data length.</summary>
	public HotkeysFileVersion version { get; private set; }
	/// <summary>List of all loaded hotkeys.</summary>
	public List<Hotkey> hotkeys { get; } = [];
	public DateTime lastWrite { get; private set; }

	/// <summary>Reads entire file into <see cref="hotkeys"/> list.</summary>
	/// <param name="file">Path to a .hkp/.hki file. The <see cref="FileFormat"/> is determined based on file name.</param>
	public HotkeysFile(string? file) {
		format = getFileFormat(file ?? throw new ArgumentNullException());
		this.file = file;

		using var fs = File.Open(file, FileMode.Open);
		lastWrite = File.GetLastWriteTimeUtc(file);
		using var df = new DeflateStream(fs, CompressionMode.Decompress);
		stream = new MemoryStream();
		df.CopyTo(stream); stream.Position = 0;
		str = new StructToStream<HotkeyData>(stream);
		sr = new BinaryReader(stream);

		version = Versions.get[sr.ReadUInt32()];
		checkFileSize();
		if (format == FileFormat.HKP)
			readMenu(3);
		var menusCount = sr.ReadUInt32();
		readMenu(menusCount);
	}

	/// <summary>Create in-memory copy of given hotkeys file that can be later saved to a given file.</summary>
	public HotkeysFile(HotkeysFile source, string file) {
		this.file = file;
		format = source.format;
		version = source.version;
		stream = new MemoryStream();
		source.stream.Position = 0;
		source.stream.CopyTo(stream);
		foreach (var h in source.hotkeys) {
			hotkeys.Add(new Hotkey(this, h.data, h.position));
		}
		str = new StructToStream<HotkeyData>(stream);
		sr = new BinaryReader(stream);
		lastWrite = File.GetLastWriteTimeUtc(file);
	}

	public void Dispose() {
		str.Dispose();
		stream.Dispose();
	}

	private FileFormat getFileFormat(string file) {
		var c = StringComparison.OrdinalIgnoreCase;
		if (file.EndsWith("base.hkp", c) || file.EndsWith("pompeii.hkp", c))
			return FileFormat.HKP;
		else return FileFormat.HKI;
	}

	private void readMenu(uint count) {
		for (int i = 0; i < count; i++) {
			var hc = sr.ReadUInt32(); //hotkeys count
			for (int j = 0; j < hc; j++) {
				var p = stream.Position;
				var hd = str.read();
				var h = new Hotkey(this, hd, p);
				hotkeys.Add(h);
			}
		}
	}

	public void write(Hotkey h) {
		str.write(h.data, h.position);
	}

	/// <summary>Save any changes made tho the <see cref="hotkeys"/> in the same <see cref="file"/> it they were loaded from.
	/// Returns an issue if the load was aborted for some reason, particularly when the file was modified externally from a different application.
	/// While such situation don't throw but only returns an exception, this method does not use any try catch to capture other, typical exceptions which may occur when working with a file.</summary>
	/// <param name="force">Tells is external changes should be discarded and the file should be written anyway.</param>
	public Exception? save(bool force = false) {
		var lw = File.GetLastWriteTimeUtc(file);
		if (!force && lastWrite < lw) return new IOException($@"The file was changed externally since it was loaded ""{file}"" . Set force flag to override anyway.");
		var fs = File.Open(file, FileMode.Create);
		using var df = new DeflateStream(fs, CompressionMode.Compress);
		stream.Position = 0;
		stream.CopyTo(df);
		lastWrite = File.GetLastWriteTimeUtc(file);
		return null;
	}

	/// <summary>Calls <see cref="save(bool)"/> on a copy of this file, which points to a different file you specified.</summary>
	/// <param name="file">Path to different file.</param>
	/// <param name="copy">New object pointing to given file.</param>
	/// <param name="over">Tells if file should be overridden. Otherwise if a file already exist on a given path, the method returns an exception and the <see cref="save(bool)"/> is not called.</param>
	public Exception? save(string file, out HotkeysFile copy, bool over = false) {
		copy = new HotkeysFile(this, file);
		if (File.Exists(file) && !over) return new IOException($@"A file already exist at ""{file}"".");
		return copy.save();
	}

	private void checkFileSize() { }

	public override string ToString()
		=> $"{Path.GetFileNameWithoutExtension(file)} [{format}]";

}

/// <summary>Encapsulates native data for a single hotkey binding along with additional information that is not save in the file and used only for management purposes.
/// Any changes made to a <see cref="Hotkey"/> object will stored in a hotkey file after you call <see cref="HotkeysFile.save(bool)"/> method on it's parent <see cref="file"/>.</summary>
public class Hotkey {
	private HotkeyData _data;
	/// <summary>AoE native binary data format.</summary>
	public HotkeyData data => _data;
	/// <summary>File form where the hotkey was loaded.</summary>
	public HotkeysFile file { get; }
	/// <summary>Position in decompressed stream from where the structure was read.</summary>
	public long position { get; }

	public ConsoleKey key { get => _data.key; set { _data.key = value; write(); } }
	public BOOL ctrl { get => _data.ctrl; set { _data.ctrl = value; write(); }	}
	public BOOL alt { get => _data.alt; set { _data.alt = value; write();}}
	public BOOL shift { get => _data.shift; set { _data.shift = value; write();}}

	public Hotkey(HotkeysFile file, HotkeyData data, long position) {
		this.file = file;
		this._data = data;
		this.position = position;
	}

	private void write() {
		if (file == null) return;
		file.write(this);
	}

	public override string ToString() => _data +"";

}

/// <summary>Format of hotkeys file.
/// Note that .hki and .hkp file extensions does not necessarily correspond to actual format.
/// .hkp file may be actually be  written in a <see cref="HKI"/> format.
/// This is determined by actual file name - "base.hkp" or "pompeii.hkp".</summary>
public enum FileFormat {
	/// <summary>The older format.</summary>
	HKI,
	/// <summary>The newer format with the bizarre data insert in the header.</summary>
	HKP,
}

/// <summary>Data layout as stored in .hkp file, representing binding for single command/action.</summary>
public struct HotkeyData {
	public ConsoleKey key; //uint32
	/// <summary>Game string id that can be passed to <see cref="Names"/> to get string name.
	/// Use <see cref="name"/> property get the string.</summary>
	public uint nameID;
	public BOOL ctrl;
	public BOOL alt;
	public BOOL shift;
	public byte alignmentMaybe;
	public string? name => Names.get(nameID);

	public override string ToString() {
		return $@"{name}: {printMod()}{key}";
	}

	private string printMod() {
		var s = "";
		if (ctrl) s += "Ctrl + ";
		if (alt) s += "Alt + ";
		if (shift) s += "Shift + ";
		//if (mouse) s += $"Mouse {mouse.val}";
		return s;
	}
}

/// <summary>One byte boolean.</summary>
public struct BOOL {
	public byte val;
	public static implicit operator bool(BOOL s)
		=> s.val > 0;
	public static implicit operator BOOL(bool b)
		=> new() { val = (byte)(b ? 1 : 0) };
	public override string ToString() => (val>0)+"";
}


