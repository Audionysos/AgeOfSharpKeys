This is .NET Standard 2.1 library for reading **Age of Empires 2: Definitive Edition** hotkeys profiles. 

This is mostly an outline so things like writing back to original format is not there at the moment, but it should be easy to add that if you want. API is relatively well documented.

Solution contains following projects:

- **AgeOfSharpKeys** - main library.

- **AgeKeys** - a sandbox console application.

Here are few main classes you would want to use, listed hierarchically:

- **Hotkeys** - manages multiple hotkeys profiles either from your OS user profile files or custom directories. 

- **HotkeysProfile** - represent a named profile that combines hotkey files which together form a single hotkeys configuration - this is the thing you save/load through the game GUI.

- **HKP** -  handles reading of a single hotkey file in known formats (.hkp or .hki)

- **HotkeyData** - a struct representing binding for a single action/command. Maintains the native memory layout.
  
  

##### Basic examples:

```csharp
using aoe2.hotkeys;
using static System.Console;

WriteLine("Hello, World!");
var pd = Hotkeys.userProfilesFolder	
	?? throw new Exception(Hotkeys.userProfilesIssue?.ToString());
var hotkeys = new Hotkeys(); //AoE installation needed, otherwise `new Hotkeys(customFolder)`

WriteLine($@"Loaded {hotkeys.profiles.Count} profiles.");
foreach (var profile in hotkeys)
	WriteLine(profile); //pritns profile names

WriteLine($@"Encountered {hotkeys.issues.Count / 2} issues."); //currently we only have summary + exception object hence the /2
foreach (var i in hotkeys.issues)
	WriteLine(i);  //in case the files were corrupted or something

//this is the name of the profile as you see in game (on my PC :).
WriteLine(@"Printing all hotkeys of ""lapek"" profile:");
foreach (var hotkey in hotkeys["lapek"]) 
	WriteLine(hotkey); //For example: "Go to Stable: Alt + E"

//loads both files as a single profile, printing version from the other one.
var singleProfile = new HotkeysProfile(Path.Combine(pd, "HOTKEYS.hkp"));
WriteLine($@"HOTKEYS.hkp version: {singleProfile.hki.version}"); 
WriteLine($@"HOTKEYS\Base.hkp version: {singleProfile.hkp.version}");

var singleFile = new HKP(Path.Combine(pd, "HOTKEYS.hkp"));
//This will (correctly) print "HKI" - I'm stating to suspect they did this mess on purpose for some reason :P
WriteLine($"HOTKEYS.hkp format: {singleFile.format}");

ReadLine();
```

#### Known Issues

Note I didn't tested this on any other PC or hotkeys but I hope it works fine.

- One thing which bug me currently the most is that I have two file in the profile folder called "myKeys.hki" and "myKeys.hkp", both read as HKI and have exactly the same content. I'm not sure how to understand that.

- I didn't bother to parse the keys so I used build-in `ConsoleKey` enum for `HotkeyData.key`. While  most keys are covered, some are not printed nicely for example "Oem3", "D1" instead of just "1" and muse buttons have just plain numbers.

- There are some hotkeys with missing names, though they also don't have any bindings  and I took the names from [this file](https://github.com/Patchnote-v2/hotkeyeditor.com/blob/main/src/hotkeys/hkp/strings.py) so I'm not sure what those hotkeys are.

### Credits

This project was started thanks to [The new HKP hotkey file format explained · GitHub](https://gist.github.com/KSneijders/9231eeec1a66b314c3402729f0c455fa#struct-hkimenu)

Also thanks to [HotkeyEditor.com](https://github.com/Patchnote-v2/hotkeyeditor.com) repo from where I insolently taken some data and analysed the format.


