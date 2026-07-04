using System;
using System.Text;

namespace Friflo.EcGui;

public static class TextUtils
{
	private static readonly StringBuilder Sb = new StringBuilder();

	private static char[] _chars = Array.Empty<char>();

	private static byte[] _bytes = Array.Empty<byte>();

	public static Span<char> AsSpan()
	{
		return AsSpan(Sb);
	}

	public static Span<char> AsSpan(StringBuilder sb)
	{
		if (_chars.Length < sb.Length)
		{
			_chars = new char[sb.Length];
		}
		sb.CopyTo(0, _chars, 0, sb.Length);
		return new Span<char>(_chars, 0, sb.Length);
	}

	public static byte[] AsBytes(StringBuilder sb)
	{
		Span<char> span = AsSpan(sb);
		int byteCount = Encoding.UTF8.GetByteCount(span);
		if (_bytes.Length < byteCount + 1)
		{
			_bytes = new byte[byteCount + 1];
		}
		Span<byte> bytes = new Span<byte>(_bytes, 0, byteCount);
		Encoding.UTF8.GetBytes(span, bytes);
		_bytes[byteCount] = 0;
		return _bytes;
	}

	public static ReadOnlySpan<char> IntAsSpan(int value)
	{
		Sb.Clear();
		Sb.Append(value);
		return StringBufferAsSpan();
	}

	public static ReadOnlySpan<char> LongAsSpan(long value)
	{
		Sb.Clear();
		Sb.Append(value);
		return StringBufferAsSpan();
	}

	public static StringBuilder Clear()
	{
		Sb.Clear();
		return Sb;
	}

	internal static ReadOnlySpan<char> StringBufferAsSpan()
	{
		if (_chars.Length < Sb.Length)
		{
			_chars = new char[Sb.Length];
		}
		Sb.CopyTo(0, _chars, 0, Sb.Length);
		return new ReadOnlySpan<char>(_chars, 0, Sb.Length);
	}
}
