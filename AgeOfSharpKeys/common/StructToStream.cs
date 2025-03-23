using System;
using System.IO;
using System.Runtime.InteropServices;

namespace aoe2.hotkeys;

/// <summary>Simplifies stream read/write of specific, fixed structure type.</summary>
/// <typeparam name="T">Type of structure to read/write.</typeparam>
public sealed class StructToStream<T> : IDisposable {
	/// <summary>Parent stream to write or read.</summary>
	public Stream stream { get; }
	/// <summary>Structure size in bytes</summary>
	public int size { get; }
	private byte[] buf;
	IntPtr pt;

	/// <summary></summary>
	/// <param name="s"></param>
	public StructToStream(Stream s) {
		stream = s;
		size = Marshal.SizeOf<T>();
		buf = new byte[size];
		pt = Marshal.AllocHGlobal(size);
	}

	~StructToStream() {
		Dispose();
	}

	public void Dispose() {
		if (pt == IntPtr.Zero) return;
		Marshal.FreeHGlobal(pt);
		pt = IntPtr.Zero;
	}

	private void throwIfDisposed() {
		if (pt == IntPtr.Zero) throw new InvalidOperationException("The object was disposed.");
	}

	/// <summary>Reads the structure from current <see cref="Stream.Position"/>.</summary>
	public T read() {
		throwIfDisposed();
		stream.Read(buf, 0, size);
		Marshal.Copy(buf, 0, pt, size);
		var r = Marshal.PtrToStructure<T>(pt);
		return r;
	}

	/// <summary>Writes given structure at given position in the stream.</summary>
	public void write(T d, long pos) {
		throwIfDisposed();
		Marshal.StructureToPtr(d, pt, true);
		Marshal.Copy(pt, buf, 0, size);
		stream.Position = pos;
		stream.Write(buf, 0, size);
	}
	
}
