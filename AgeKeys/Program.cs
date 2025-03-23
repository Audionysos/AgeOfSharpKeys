// See https://aka.ms/new-console-template for more information
using AgeKeys;
using aoe2.hotkeys;
using System.Diagnostics;
using static System.Console;

WriteLine("Hello, World!");
//new ReadingExample();

var p = HotkeysProfile.load("lapek");
var sel = p
	.Where(h => 
		h.ctrl && !h.alt
		&& h.key > ConsoleKey.D0
		&& h.key < ConsoleKey.D9);
print(sel);
foreach (var h in sel) {
	h.ctrl = false; h.alt = true;
}
p.hki.save("edited.hkp", out HotkeysFile newFile, over:true);
var loaded = new HotkeysFile("edited.hkp");
WriteLine("------------------EDITED--------------------");
print(loaded.hotkeys.Where(h => h.key > ConsoleKey.D0 && h.key < ConsoleKey.D9));

ReadLine();

void print<T>(IEnumerable<T> en) {
	foreach (var o in en)
		WriteLine(o);
}