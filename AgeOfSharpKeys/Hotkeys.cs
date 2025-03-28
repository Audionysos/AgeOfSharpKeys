using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static System.Environment;

namespace aoe2.hotkeys;

/// <summary>Represents a collections of profiles in a profiles directory.</summary>
public class Hotkeys : IReadOnlyList<HotkeysProfile> {

	/// <summary>Main directory that can contains many <see cref="HotkeysProfile"/>.
	/// Currently standard game directory is "%userprofile%\Games\Age of Empires 2 DE\{steamID}\profile\" but this property holds paths to any directory given to constructor.</summary>
	public string? folder { get; private set; }
	public List<HotkeysProfile> profiles { get; } = [];
	public int Count => profiles.Count;
	public HotkeysProfile this[int index] => profiles[index];
	/// <summary>List of any issues that were encountered during scanning or profiles loading.</summary>
	public List<object> issues { get; } = [];


	/// <summary>Searches for profile with given name and throws if not found.</summary>
	public HotkeysProfile this[string name]
		=> profiles.Find(p => p.name == name);

	/// <summary>Scans given directory for any profiles.</summary>
	public Hotkeys(string folder) {
		this.folder = folder;
		scanFolder();
	}

	/// <summary>Scans for profiles of current OS user.</summary>
	public Hotkeys() {
		folder = AOE2Paths.userProfilesFolder;
		if (AOE2Paths.userProfilesIssue != null)
			addIssue(AOE2Paths.userProfilesIssue);
		else scanFolder();
	}


	private void scanFolder() {
		foreach (var f in Directory.EnumerateFiles(folder, "*.hkp")) {
			loadProfile(f);
		}
	}

	private void loadProfile(string profileFile) {
		try {
			var p = new HotkeysProfile(profileFile);
			profiles.Add(p);
		}catch (Exception e) {
			issues.Add($@"Failed to load ""{profileFile}""");
			issues.Add(e);
		}
	}

	private object addIssue(object v) {
		issues.Add(v); return v;
	}

	public IEnumerator<HotkeysProfile> GetEnumerator() => profiles.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => profiles.GetEnumerator();
}
