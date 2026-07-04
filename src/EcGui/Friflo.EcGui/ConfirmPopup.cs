using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal struct ConfirmPopup
{
	internal readonly string title;

	private bool setKeyboardFocus;

	public ConfirmPopup(string title, string button)
	{
		_003Cbutton_003EP = button;
		setKeyboardFocus = false;
		this.title = title;
	}

	internal void OpenPopup()
	{
		setKeyboardFocus = true;
		ImGui.OpenPopup(title);
	}

	internal bool Draw(ref string value)
	{
		ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize(title).X) / 2f);
		ImGui.Text(title);
		ImGui.PushStyleColor(ImGuiCol.FrameBg, GlobalColors.frameBg);
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + UI.Scl(20f));
		ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - UI.Scl(60f));
		if (setKeyboardFocus)
		{
			ImGui.SetKeyboardFocusHere();
		}
		setKeyboardFocus = false;
		bool num = ImGui.InputText("##confirmValue", ref value, 50u, ImGuiInputTextFlags.EnterReturnsTrue);
		ImGui.PopStyleColor();
		ImGui.SetCursorPosX((ImGui.GetWindowWidth() - ImGui.CalcTextSize("Save").X) / 2f);
		return num | ImGui.Button(_003Cbutton_003EP);
	}
}
