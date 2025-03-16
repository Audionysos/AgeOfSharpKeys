using System.Collections.Generic;
using System.IO;
using static aoe2.hotkeys.PathPart;

namespace aoe2.hotkeys;

/// <summary>Helper methods for navigating in files systems.</summary>
public static class FileSystem {
	/// <summary>Combines given strings into paths and returns first path at which a file exists.
	/// Returns null if no file was found.
	/// Strings are combined using <see cref="Path.Combine(string[])"/>.
	/// Separate multiple paths with <see cref="OR"/> value.</summary>
	/// <param name="parts">Can be `null`, string or `<see cref="OR"/>`.</param>
	public static string? findFile(params PathPart[] parts) {
		var cps = new List<string>();
		for (int i = 0; i < parts.Length; i++) {
			var p = parts[i];
			if (p != OR) cps.Add(p.ToString());
			if (p == OR || i == parts.Length - 1) {
				var tp = Path.Combine([.. cps]);
				if (File.Exists(tp)) return tp;
				cps.Clear();
			}
		}
		return null;
	}
}


/// <summary>Represents part of a url path that could be applicable for <see cref="Path.Combine(string[])"/> method.
/// Defines special <see cref="OR"/> member that could be used to separate alternative paths for <see cref="FileSystem.findFile(PathPart[])"/>.
/// Otherwise a part can be implicitly converted from string to represent folder, file, or complex path.</summary>
/// <param name="value">Null is stored in the property but it should be treated as empty string.
/// Calling <see cref="ToString"/> for part with `null` <see cref="value"/> returns empty string.</param>
public record PathPart(string? value) {
	/// <summary>Represents a separator for multiple paths provided to methods such as <see cref="FileSystem.findFile(PathPart[])"/>.</summary>
	public static readonly PathPart OR = new("<or>");
	public static implicit operator PathPart(string? s) => new (s);
	public override string ToString() => value ?? "";
}