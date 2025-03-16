using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace aoe2.hotkeys;

/// <summary>Reads hotkeys from a single hotkey file, either <see cref="FileFormat.HKI"/> or <see cref="FileFormat.HKP"/>.
/// Note that single hotkeys setup you use in game does not consist of only a single file - see <see cref="HotkeysProfile"/>.</summary>
public class HKP {
	private Stream stream;
	private BinaryReader sr;

	/// <summary>Path to file that was read.</summary>
	public string file { get; }
	/// <summary>Older/newer version of file. The only difference in data structure is that the never formant adds 3 additional hotkeys groups in the middle of the header.
	/// If anyone knows why they did this instead of increasing the count and adding the groups at the end of the file, please let me know.</summary>
	public FileFormat format { get; private set; }
	/// <summary>Describes specific version of the file, or I guess in more proper words version of the game.
	/// This doesn't affect file format in anyway but it's only used to validate it it's actually a hotkey file.
	/// The <see cref="HKP"/> doesn't check data length as decompressed stream is read on the fly.</summary>
	public HotkeysFileVersion version { get; private set; }
	/// <summary>List of all loaded hotkeys.</summary>
	public List<HotkeyData> hotkeys { get; } = [];

	/// <summary>Reads entire file into <see cref="hotkeys"/> list.</summary>
	/// <param name="file">Path to a .hkp/.hki file. The <see cref="FileFormat"/> is determined based on file name.</param>
	public HKP(string? file) {
		format = getFileFormat(file ?? throw new ArgumentNullException());
		using var fs = File.Open(file, FileMode.Open);
		stream = new DeflateStream(fs, CompressionMode.Decompress);
		//stream = new MemoryStream().CopyTo()
		sr = new BinaryReader(stream);
		version = Versions.get[sr.ReadUInt32()];
		if (format == FileFormat.HKP)
			readMenu(3);
		var menusCount = sr.ReadUInt32();
		readMenu(menusCount);
		sr.Close();
		this.file = file;
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
				var h = readStruct<HotkeyData>();
				hotkeys.Add(h);
			}
		}
	}

	private byte[] buf = new byte[16];
	private T readStruct<T>() {
		var s = Marshal.SizeOf<T>();
		stream.Read(buf, 0, s);
		var p = Marshal.AllocHGlobal(s);
		Marshal.Copy(buf, 0, p, s);
		var r = Marshal.PtrToStructure<T>(p);
		Marshal.FreeHGlobal(p);
		return r;
	}

	public override string ToString()
		=> $"{Path.GetFileNameWithoutExtension(file)} [{format}]";

}

/// <summary>Format of hotkeys file.
/// Note that .hki and .hkp file extensions does not necessarily correspond to actual format.
/// .hkp file may be actually be written in a <see cref="HKI"/> format.
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
	public override string ToString() => (val>0)+"";
}

