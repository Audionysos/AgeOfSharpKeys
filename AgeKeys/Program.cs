// See https://aka.ms/new-console-template for more information
using aoe2.hotkeys;
using static System.Console;

WriteLine("Hello, World!");
//AOE2Paths.steamDir = @"O:\rozrywka\Gry\Steam\";
//new ReadingExample();
//EditingExamples.cloneProfile();
//EditingExamples.editingProfile();

var p = HotkeysProfile.load("edited");
//var p = new HotkeysFile(AOE2Paths.remoteDir + @"\edited.hkp");
var sel = p//.hotkeys
	//.Where(h => h.name.StartsWith("Create Group #") && !h.alt);
	.Where(h => h.name.StartsWith("Create Group #6"));
	//.Where(h => h.ctrl && h.key == ConsoleKey.D3);
print(sel);
var h = sel.First();
h.alt = true;
h.ctrl = false;

p.save();



void print<T>(IEnumerable<T> en) {
	foreach (var o in en)
		WriteLine(o);
}