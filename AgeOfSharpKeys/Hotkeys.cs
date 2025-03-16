using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static System.Environment;

namespace aoe2.hotkeys;

/// <summary>Represents a collections of profiles in a profiles directory.</summary>
public class Hotkeys : IReadOnlyList<HotkeysProfile> {

	#region User's profiles directory 
	/// <summary>Describes an issue encountered when determining user's profiles directory.</summary>
	public static object? userProfilesIssue { get; set; }
	private static string? _userProfilesFolder;
	/// <summary>Specifies path to current OS user profiles directory  i.e. "%userprofile%\Games\Age of Empires 2 DE\{steamID}\profile\".
	/// This may be null if for example the game is not installed. If the folder is null, you should find cause under <see cref="userProfilesIssue"/> property.</summary>
	public static string? userProfilesFolder {
		get {
			if (userProfilesIssue != null) return null;
			if (_userProfilesFolder != null) return _userProfilesFolder;
			userProfilesIssue = findProfilesDirectory(out _userProfilesFolder);
			return _userProfilesFolder;
		}
	}

	private static object? findProfilesDirectory(out string? folder) {
		folder = null;
		var uf = GetFolderPath(SpecialFolder.UserProfile);
		var aoe = Path.Combine(uf, @"Games\Age of Empires 2 DE\");
		if (!Directory.Exists(aoe)) return "Couldn't find AOE2DE directory of current user";
		string? steam = null;
		foreach (var d in Directory.EnumerateDirectories(aoe)) {
			var p = Path.GetFileName(d);
			if (p.Length != 17) continue;
			if (!ulong.TryParse(p, out _)) continue;
			steam = d; break;
		}
		if (steam == null) return $@"Couldn't find steam folder under ""{aoe}"""; // addIssue();
		folder = Path.Combine(steam, "profile");
		return null;
	}
	#endregion

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
		folder = userProfilesFolder;
		if (userProfilesIssue != null) addIssue(userProfilesIssue);
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
