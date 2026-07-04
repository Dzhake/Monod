using System;
using System.Text;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class DateTimeDrawer : TypeDrawer
{
	private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFZ";

	internal const string DateTimeFormatCsv = "yyyy-MM-dd HH:mm:ss.FFFFFF";

	private static readonly byte[] Bytes = new byte[32];

	public override int DefaultWidth => 400;

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<DateTime>(out DateTime value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		value = value.ToUniversalTime();
		Span<char> span = stackalloc char[32];
		value.TryFormat(span, out var charsWritten, "yyyy-MM-ddTHH:mm:ss.FFFFFFZ");
		span[charsWritten] = '\0';
		int bytes = Encoding.UTF8.GetBytes(span.Slice(0, charsWritten), Bytes);
		Bytes[bytes] = 0;
		if (ImGui.InputText("##field", Bytes, 32u, ImGuiInputTextFlags.EnterReturnsTrue))
		{
			bytes = new ReadOnlySpan<byte>(Bytes).IndexOf<byte>(0);
			if (DateTime.TryParse(span[..Encoding.UTF8.GetChars(new ReadOnlySpan<byte>(Bytes, 0, bytes), span)], out value))
			{
				value = value.ToUniversalTime();
				drawValue.SetValue(value);
			}
		}
		return TypeDrawer.Flags();
	}
}
