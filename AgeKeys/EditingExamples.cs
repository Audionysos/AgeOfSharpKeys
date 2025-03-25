using aoe2.hotkeys;

namespace AgeKeys;

class EditingExamples {
	public static void cloneProfile() {
		var p = HotkeysProfile.load("myProfile");
		p.save("myClone");
	}

	public static void editingProfile() {
		var p = HotkeysProfile.load("myClone");
		var sel = p.Where(h => h.name.StartsWith("Create Group #") && !h.alt);
		foreach (var h in sel) {
			h.ctrl = false;
			h.alt = true;
		}
		p.save();
	}

}
