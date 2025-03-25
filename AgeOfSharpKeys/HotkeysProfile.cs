using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static aoe2.hotkeys.PathPart;

namespace aoe2.hotkeys;
/// <summary>Represents a single hotkeys profile, equivalent to the ones you save/load in game as a single configuration.
/// <br/>The profile consists of two, <see cref="FileFormat.HKI"/> and <see cref="FileFormat.HKP"/> files.
/// <br/>The newest standard for a profile in game's directory:
/// <br/>{profileName}.hkp - the older HKI format
/// <br/>{profileName}\base.hkp or {profileName}\pompeii.hkp - the newer HKP file format.
/// <br/>
/// <br/>That been said, this class supports also both files "{profileName}.hkp" and "base.hkp" residing in the same folder,
/// <br/>or "{profileName}.hkp" and "{profileName}.khi" as well.
/// <br/>
/// <br/>There may be some other quirks for older game versions but I'm not really interested to implement support for the whole legacy.
/// </summary>
public class HotkeysProfile : IReadOnlyList<Hotkey> {
	/// <summary>Name of the profile (taken from the <see cref="hki"/> file).
	/// This should be the same name you see in game.</summary>
	public string name { get; }
	/// <summary>Direct parent folder for the profile path.
	/// In practice this would most likely be "%userprofile%\Games\Age of Empires 2 DE\{steamID}\profile\".</summary>
	public string folder { get; }
	/// <summary>Profile file contents</summary>
	public HotkeysFile hki { get; }
	/// <summary>Base file contents.</summary>
	public HotkeysFile hkp { get; }

	/// <summary>Returns hotkey at given index from all loaded hotkeys.</summary>
	public Hotkey this[int index] => all[index];
	private List<Hotkey> all = [];
	public int Count => all.Count;

	/// <summary>Loads profile files.</summary>
	/// <param name="profileFile">Path to {profileName}.hkp file (<see cref="FileFormat.HKI"/>).</param>
	/// <param name="baseFile">Path to <see cref="FileFormat.HKP"/> file.
	/// This object will try to determine path to this file based on <paramref name="profileFile"/> path (the files may be placed directly in the same folder for sharing purposes).
	/// You would only need to specify this parameter if you want to load the file form path unrelated to <paramref name="profileFile"/>.
	/// Note however the file must be present. If the is no base file, the load will fail and exception will be thrown. If you want to load only a single file, use <see cref="HotkeysFile"/> class instead.</param>
	public HotkeysProfile(string profileFile, string? baseFile = null) {
		name = Path.GetFileNameWithoutExtension(profileFile);
		folder = Path.GetDirectoryName(profileFile);
		hki = new HotkeysFile(profileFile);
		hkp = new HotkeysFile(getBasePath(baseFile));
		combineFiles();
	}

	/// <summary>Loads a single profile with given name, from user's profiles directory.</summary>
	public static HotkeysProfile load(string name) {
		var f = $@"{Hotkeys.userProfilesFolder}\{name}.hkp";
		return new HotkeysProfile(f);
	}

	/// <summary>Creates copy of given profile in the same <see cref="folder"/>.
	/// Note: New profile will have recent files structure (name.hkp and name/base.hkp), even if original files from the <paramref name="source"/> profile have different relative paths.
	/// Note2: The file are not written to file system until you call <see cref="save(bool)"/>.</summary>
	/// <param name="source">Profile to be copied.</param>
	/// <param name="name">Name for new profile.</param>
	/// <param name="over">Allows override existing profile files.</param>
	/// <param name="folder">Changes the <see cref="folder"/> where the profile is installed.</param>
	public HotkeysProfile(HotkeysProfile source, string name, bool over = false, string? folder = null) {
		this.name = name;
		this.folder = folder ?? source.folder;
		var lfn = $@"{this.folder}\{name}"; //local folder name
		Directory.CreateDirectory(lfn);
		hki = new HotkeysFile(source.hki, $@"{lfn}.hkp", over);
		hkp = new HotkeysFile(source.hkp, $@"{lfn}\Base.hkp", over);
		combineFiles();
	}

	#region Saving
	/// <summary>Saves all current changes made to this profile.
	/// Returns exceptions from <see cref="HotkeysFile.save(bool)"/> methods.</summary>
	/// <param name="force">Allows write the files even if they were edited by external applications.</param>
	public Exception? save(bool force = false) {
		var e = hki.save(force);
		if (e != null) return e;
		return hkp.save();
	}

	/// <summary>Saves copy of this profile with different name under the same profiles <see cref="folder"/>.</summary>
	/// <param name="name">The name should be unique, or existing profile files will be overridden.</param>
	/// <param name="over">Allows to override other existing profile, otherwise exceptions will be thrown (no returned).</param>
	public Exception? save(string name, bool over = false) {
		var cpy = new HotkeysProfile(this, name, over);
		return cpy.save();
	}

	/// <summary>Saves copy of this profile with different name under given folder <see cref="folder"/>.</summary>
	/// <param name="name">The name should be unique to <see cref="folder"/>, or existing profile files will be overridden.</param>
	/// <param name="over">Allows to override other existing profile, otherwise exceptions will be thrown (no returned).</param>
	public Exception? save(string name, string folder, bool over = false) {
		if (folder == "") folder = Environment.CurrentDirectory;
		var cpy = new HotkeysProfile(this, name, over, folder);
		return cpy.save();
	}
	#endregion

	private void combineFiles() {
		foreach (var h in hkp.hotkeys) all.Add(h);
		foreach (var h in hki.hotkeys) {
			//if (all.Any(o => o.nameID == h.nameI
			//	Debugger.Break();
			all.Add(h);
		}
	}

	private string? getBasePath(string? baseFile)
		=> FileSystem.findFile( baseFile,
					OR, folder, name, "base.hkp", //"official" scheme
					OR, folder, name, "pompeii.hkp",
					OR, folder, "base.hkp", //check if placed directly in the same folder
					OR, folder, "pompeii.hkp",
					OR, folder, name + ".hki");//Old scheme? Should this swap paths?

	public IEnumerator<Hotkey> GetEnumerator() => all.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => all.GetEnumerator();
	public override string ToString() {
		return $@"""{name}"" profile.";
	}
}



