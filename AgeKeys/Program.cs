// See https://aka.ms/new-console-template for more information
using aoe2.hotkeys;
using static System.Console;

WriteLine("Hello, World!");
//new ReadingExample();
//EditingExamples.cloneProfile();
//EditingExamples.editingProfile();

var p = HotkeysProfile.load("lapek");
var sel = p
	//.Where(h => h.name.StartsWith("Create Group #") && !h.alt);
	.Where(h => h.name.StartsWith("Create Group #3"));
	//.Where(h => h.ctrl && h.key == ConsoleKey.D3);
print(sel);



void print<T>(IEnumerable<T> en) {
	foreach (var o in en)
		WriteLine(o);
}