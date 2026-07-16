using System;
using System.Numerics;
using Hexa.NET.ImGui;

namespace Friflo.EcGui;

internal static class GlobalColors
{
	internal static Vector4 tagBg;

	internal static Vector4 componentBg;

	internal static Vector4 componentAddBg;

	internal static Vector4 componentActiveBg;

	internal static Vector4 fieldActiveBg;

	internal static Vector4 windowBg;

	internal static Vector4 explorerModeBg;

	internal static Vector4 errorText;

	internal static Vector4 activeFilterBg;

	internal static Vector4 filterBg;

	internal static Vector4 popupBg;

	internal static Vector4 addBg;

	internal static Vector4 removeBg;

	internal static Vector4 frameBg;

	internal static Vector4 frameBgOpaque;

	internal static Vector4 checkMark;

	internal static Vector4 flagHovered;

	internal static Vector4 flagHoveredSet;

	internal static Vector4 flagText;

	internal static Vector4 flagTextSet;

	internal static Vector4 lightFrameBg;

	internal static Vector4 text;

	internal static Vector4 textLight;

	internal static Vector4 queryText;

	internal static Vector4 textDisabled;

	internal static bool stylesUpdated;

	internal static void UpdateStyles(bool forceUpdate)
	{
		if (!stylesUpdated || forceUpdate)
		{
			stylesUpdated = true;
			UI.scale = ImGui.GetFontSize() / 40f;
			Span<Vector4> colors = ImGui.GetStyle().Colors;
			frameBg = colors[7];
			float w = frameBg.W;
			windowBg = colors[2];
			windowBg.W = 1f;
			frameBgOpaque = windowBg * (1f - w) + frameBg * w;
			frameBgOpaque.W = 1f;
			bool num = IsDark(windowBg);
			lightFrameBg = 0.5f * windowBg + 0.5f * frameBg;
			lightFrameBg.W = 0.7f;
			float num2 = (num ? 0.2f : (-0.1f));
			tagBg = new Vector4(windowBg.X + num2, windowBg.Y + num2, windowBg.Z + num2, 1f);
			num2 = (num ? 0.15f : (-0.1f));
			componentBg = new Vector4(windowBg.X + num2, windowBg.Y + num2, windowBg.Z + num2, 1f);
			componentActiveBg = new Vector4(componentBg.X + num2, componentBg.Y + num2, componentBg.Z + num2, 1f);
			componentAddBg = (num ? new Vector4(0.1f, 0.4f, 0.1f, 1f) : new Vector4(0.6f, 0.85f, 0.6f, 1f));
			num2 = (num ? 0.2f : (-0.07f));
			fieldActiveBg = new Vector4(windowBg.X + num2, windowBg.Y + num2, windowBg.Z + num2, 1f);
			errorText = (num ? new Vector4(1f, 0.58f, 0.58f, 1f) : new Vector4(1f, 0f, 0f, 1f));
			filterBg = (num ? new Vector4(0f, 0.2f, 0f, 1f) : new Vector4(0.95f, 1f, 0.95f, 1f));
			activeFilterBg = (num ? new Vector4(0f, 0.4f, 0f, 1f) : new Vector4(0.7f, 1f, 0.7f, 1f));
			popupBg = colors[2];
			popupBg.W = 1f;
			addBg = new Vector4(0f, 0.5f, 0f, 1f);
			removeBg = new Vector4(1f, 0f, 0f, 1f);
			explorerModeBg = new Vector4(0.6f, 0.6f, 0.6f, 1f);
			text = colors[0];
			textLight = new Vector4(0.5f, 0.5f, 0.5f, 1f);
			queryText = 0.3f * text + 0.7f * textLight;
			textDisabled = colors[1];
			checkMark = colors[18];
			flagHovered = componentBg;
			flagHoveredSet = 0.6f * checkMark + 0.4f * frameBg;
			flagText = 0.6f * colors[1] + 0.4f * text;
			flagTextSet = (num ? new Vector4(0f, 0f, 0f, 1f) : new Vector4(1f, 1f, 1f, 1f));
		}
	}

	private static bool IsDark(Vector4 color)
	{
		return 0.299 * (double)color.X + 0.587 * (double)color.Y + 0.114 * (double)color.Z < 0.5;
	}
}
