using System;
using System.Numerics;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal sealed class ErrorPopup
{
	private bool errorNew;

	private string? errorMessage;

	private string? errorException;

	internal void OnError(string message, Exception? exception)
	{
		errorNew = true;
		if (exception != null)
		{
			errorMessage = message + "\n" + exception.Message;
			errorException = exception.ToString();
		}
		else
		{
			errorMessage = message;
			errorException = null;
		}
	}

	internal void DrawError()
	{
		if (errorNew)
		{
			errorNew = false;
			ImGui.OpenPopup("error-popup");
		}
		if (errorMessage == null)
		{
			return;
		}
		float x = UI.Scl(1400f);
		float frameHeight = ImGui.GetFrameHeight();
		ImGui.SetNextWindowSize(new Vector2(x, 5f * frameHeight) + 2f * ImGui.GetStyle().WindowPadding + 2f * ImGui.GetStyle().FramePadding, ImGuiCond.Always);
		if (ImGui.BeginPopup("error-popup", ImGuiWindowFlags.NoDocking))
		{
			ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.errorText);
			ImGui.InputTextMultiline("##error-message", ref errorMessage, (uint)errorMessage.Length, new Vector2(x, 2f * frameHeight));
			ImGui.PopStyleColor();
			if (errorException != null)
			{
				ImGui.PushStyleColor(ImGuiCol.FrameBg, GlobalColors.windowBg);
				ImGui.InputTextMultiline("##error-exception", ref errorException, (uint)errorException.Length, new Vector2(x, 3f * frameHeight));
				ImGui.PopStyleColor();
			}
			ImGui.EndPopup();
		}
		else
		{
			errorMessage = null;
			errorException = null;
		}
	}
}
