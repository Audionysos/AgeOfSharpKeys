// See https://aka.ms/new-console-template for more information
using aoe2.hotkeys;
using System.Diagnostics;
using static System.Console;

WriteLine("Hello, World!");
var pd = Hotkeys.userProfilesFolder	
	?? throw new Exception(Hotkeys.userProfilesIssue?.ToString());
var hotkeys = new Hotkeys(); //AoE installation needed, otherwise `new Hotkeys(customFolder)`

WriteLine($@"Loaded {hotkeys.profiles.Count} profiles.");
foreach (var profile in hotkeys)
	WriteLine(profile);

WriteLine($@"Encountered {hotkeys.issues.Count / 2} issues."); //currently we only have summary + exception object hence the /2
foreach (var i in hotkeys.issues)
	WriteLine(i);

Debugger.Break(); //Before you continue, you need to change string literals to one of valid profile names you have installed
//this is the name of the profile as you see in game (on my PC :).
WriteLine(@"Printing all hotkeys of ""lapek"" profile:");
foreach (var hotkey in hotkeys["lapek"]) 
	WriteLine(hotkey); //For example: "Go to Stable: Alt + E"

//loads both files as a single profile, printing version from the other one.
var singleProfile = new HotkeysProfile(Path.Combine(pd, "HOTKEYS.hkp"));
WriteLine($@"HOTKEYS.hkp version: {singleProfile.hki.version}"); 
WriteLine($@"HOTKEYS\Base.hkp version: {singleProfile.hkp.version}");

var singleFile = new HKP(Path.Combine(pd, "HOTKEYS.hkp"));
WriteLine($"HOTKEYS.hkp format: {singleFile.format}");

ReadLine();