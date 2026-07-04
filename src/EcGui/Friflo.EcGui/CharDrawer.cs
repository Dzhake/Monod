using System;
using System.Text;
using ImGuiNET;

namespace Friflo.EcGui;

internal sealed class CharDrawer : TypeDrawer
{
	private static readonly byte[] ByteBuffer = new byte[5];

	private static readonly char[] CharBuffer = new char[2];

	public override ItemFlags DrawValue(in DrawValue drawValue)
	{
		if (!drawValue.GetValue<char>(out char value, out Exception exception))
		{
			return drawValue.DrawException(exception);
		}
		CharBuffer[0] = value;
		CharBuffer[1] = '\0';
		Encoding.UTF8.GetBytes(CharBuffer, ByteBuffer);
		if (ImGui.InputText("##field", ByteBuffer, 4u))
		{
			int length = new ReadOnlySpan<byte>(ByteBuffer).IndexOf<byte>(0);
			ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(ByteBuffer, 0, length);
			int charCount = Encoding.UTF8.GetCharCount(bytes);
			if (charCount == 0)
			{
				value = '\0';
				ActiveItem<char>.SetValue(in drawValue, value);
			}
			if (charCount == 1)
			{
				Encoding.UTF8.GetChars(bytes, CharBuffer);
				value = CharBuffer[0];
				ActiveItem<char>.SetValue(in drawValue, value);
			}
		}
		ActiveItem<char>.SetActiveState(in drawValue, value);
		return TypeDrawer.Flags();
	}
}
