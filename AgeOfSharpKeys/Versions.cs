using System;
using System.Collections;
using System.Collections.Generic;

namespace aoe2.hotkeys;

/// <summary>See <see cref="Versions.Add(string, uint, List{int}, string)"/> method.</summary>
public record HotkeysFileVersion(string id, uint code, List<int> sizes, string description);
/// <summary>The version are not really needed for the parsing. It's only used to validate file.</summary>
public class Versions : IEnumerable {
	private Versions() { }
	public static Versions get => vs;
	private static Versions vs = new () {
		{"aok", 0x3f800000, [2080, 2192], "Vanilla AoK, AoC/FE, WololoKingdoms"},
		{"22", 0x40000000, [2432], "HD2.2-3"},  // different header, gotta keep this one
		{"50", 0x40400000, [2192, 2204, 2252, 2264], "HD5.0+"},
		{"de", 0x40866666, [4632, 4644, 4664, 4676, 4712, 4724,
							4748, 4820, 2672, 2324, 2796, 2336, 4996], "Definitive Edition"},
		//{"24", 0x40400000, [], "HD2.4-8"},  // don't ever pick this version
		//{"30", 0x40400000, [], "HD3.0-4.3"},  // don't ever pick this version
		//{"44", 0x40400000, [], "HD4.4-4.9"},  // don't ever pick this version
		//{"wk", 0x3f800000, [2240], "WololoKingdoms"},
		//{"deo", 0x40400000, [], "DE (old)"},  // don't ever pick this version
	};
	private Dictionary<uint, HotkeysFileVersion> map = [];
	/// <summary>Returns version info or throws verbose exception.</summary>
	public HotkeysFileVersion this[uint version] {
		get {
			map.TryGetValue(version, out var v);
			return v ?? throw new Exception($"Unrecognized file version code - 0x{version:X8}.");
		}
	}

	/// <summary></summary>
	/// <param name="k">Key for dictionary (in the python script I've copied the list from). I don't us it.</param>
	/// <param name="c">Version Code stored in file.</param>
	/// <param name="s">List of correct file sizes after decompression - they use it to validate file.</param>
	/// <param name="d">Description for the version.</param>
	/// <returns></returns>
	public HotkeysFileVersion Add(string k, uint c, List<int> s, string d) {
		var v = new HotkeysFileVersion(k, c, s, d);
		this.map.Add(c, v); return v;
	}
	public IEnumerator GetEnumerator() => map.GetEnumerator();
}

