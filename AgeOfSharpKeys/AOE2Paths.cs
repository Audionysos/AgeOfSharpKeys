using System.IO;
using System;
using static System.Environment;

namespace aoe2.hotkeys;

/// <summary>Provides common paths to files/folders used by Age of Empires 2: DE.</summary>
public static class AOE2Paths {

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

	#region Steam directory
	/// <summary>A potential issue which could occur when trying to determine <see cref="steamDir"/>.</summary>
	public static object? steamIssue { get; set; }
	//O:\rozrywka\Gry\Steam\userdata\127472334\813780\remote
	public static string? _steamDir = @"O:\rozrywka\Gry\Steam\";
	/// <summary>Directory where Steam is installed. This defaults to "C:\Program Files (x86)\Steam".
	/// If the folder is not found, the property will be null and you can find <see cref="steamIssue"/>.
	/// You can set this path manually if you have a custom Steam installation path.</summary>
	public static string? steamDir {
		get {
			if (_steamDir != null) return _steamDir;
			findSteamDirectory(); return _steamDir;
		}
		set {
			if (value == null) { _steamDir = null; return; }
			if (!Directory.Exists(value)) throw new ArgumentException($@"Given directory does not exist ""{value}""");
			if (!Directory.Exists(Path.Combine(value, "userData"))) throw new ArgumentException($@"""userdata"" not found in ""{value}""");
			_steamDir = value;
		}
	}
	private static void findSteamDirectory() {
		if (_steamDir != null) return;
		_steamDir = @"C:\Program Files (x86)\Steam";
		if (Directory.Exists(_steamDir)
			&& Directory.Exists(Path.Combine("userdata"))) return; //assuming correct path
		steamIssue = $@"Couldn't find steam directory. If you have not standard path, set '{nameof(steamDir)}' manually.";
		_steamDir = null;
	}
	#endregion

	#region AoE remote directory
	/// <summary>A potential issue which could occur when trying to determine <see cref="remoteDir"/>.</summary>
	public static object? remoteIssue { get; set; }
	private static string? _remote;
	/// <summary>AoE directory in Steam where copies of hotkey files are stored.
	/// Edited hotkeys files need to be copied there to prevent game from overriding changes to previous version.
	/// Usually this will be @"C:\Program Files (x86)\Steam\userdata\{someNumber}\813780\remote".
	/// If you have Steam installed in non-default directory, you need to manually set <see cref="steamDir"/> before this path could be acquired.
	/// If directory is not found this property returns null.
	/// </summary>
	public static string? remoteDir {
		get {
			if (_remote != null) return _remote;
			remoteIssue = findRemote();
			return _remote;
		}
	}

	private static object? findRemote() {
		if (steamDir == null) return null;
		var ud = Path.Combine(_steamDir, "userData");
		var ds = Directory.GetDirectories(ud);
		if (ds.Length > 1) return @"Found multiple directories in ""Steam\userdata\"" - don't know how to handle that.";
		var ar = Path.Combine(ds[0], "813780", "remote"); //AoE2 dir
		if (!Directory.Exists(ar)) return $@"Couldn't fine remote AoE2 folder ""{ar}""";
		_remote = ar;
		return null;
	}
	#endregion

	/// <summary>Tells if given file resides in directory used by the game (<see cref="userProfilesFolder"/>).</summary>
	/// <param name="file"></param>
	/// <returns></returns>
	public static bool isGameFile(string file) {
		var fp = Path.GetFullPath(file);
		var bp = Path.GetFullPath(userProfilesFolder);
		return fp.StartsWith(bp);

	} 
}
