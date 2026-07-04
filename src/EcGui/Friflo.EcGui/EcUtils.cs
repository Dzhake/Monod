using System.Globalization;
using System.Numerics;
using ImGuiNET;

namespace Friflo.EcGui;

public static class EcUtils
{
	public static class ID
	{
		private static int level;

		private static readonly int[] stack = new int[64];

		public static void PushID(int id)
		{
			stack[level++] = id;
			ImGui.PushID(id);
		}

		public static void PopID()
		{
			ImGui.PopID();
			level--;
		}

		internal static void PopAll()
		{
			for (int i = 0; i < level; i++)
			{
				ImGui.PopID();
			}
		}

		internal static void PushRestore()
		{
			for (int i = 0; i < level; i++)
			{
				ImGui.PushID(stack[i]);
			}
		}
	}

	private const float FltSpeed = 0.001f;

	private const float FltMin = 0f;

	private const float FltMax = 1f;

	private const ImGuiColorEditFlags FltFlags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Float;

	private const int ByteSpeed = 1;

	private const int ByteMin = 0;

	private const int ByteMax = 255;

	private const ImGuiColorEditFlags ByteFlags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel;

	public static bool ColorEdit3(ref float x, ref float y, ref float z, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float frameHeight = ImGui.GetFrameHeight();
		float nextItemWidth = (drawValue.Size.X - 3f * ImGui.GetStyle().ItemSpacing.X - frameHeight) / 3f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##field", ref x, 0.001f, 0f, 1f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##y", ref y, 0.001f, 0f, 1f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##z", ref z, 0.001f, 0f, 1f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(frameHeight);
		Vector3 col = new Vector3(x, y, z);
		if (ImGui.ColorEdit3("##col", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Float))
		{
			x = col.X;
			y = col.Y;
			z = col.Z;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool ColorEdit4(ref float x, ref float y, ref float z, ref float w, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float frameHeight = ImGui.GetFrameHeight();
		float nextItemWidth = (drawValue.Size.X - 4f * ImGui.GetStyle().ItemSpacing.X - frameHeight) / 4f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##field", ref x, 0.001f, 0f, 1f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##y", ref y, 0.001f, 0f, 1f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##z", ref z, 0.001f, 0f, 1f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##w", ref w, 0.001f, 0f, 1f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(frameHeight);
		Vector4 col = new Vector4(x, y, z, w);
		if (ImGui.ColorEdit4("##col", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.Float))
		{
			x = col.X;
			y = col.Y;
			z = col.Z;
			w = col.W;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool ColorEditRGB(ref byte R, ref byte G, ref byte B, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float frameHeight = ImGui.GetFrameHeight();
		float nextItemWidth = (drawValue.Size.X - 3f * ImGui.GetStyle().ItemSpacing.X - frameHeight) / 3f;
		int v = R;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##field", ref v, 1f, 0, 255))
		{
			R = (byte)v;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		int v2 = G;
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##g", ref v2, 1f, 0, 255))
		{
			G = (byte)v2;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		int v3 = B;
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##b", ref v3, 1f, 0, 255))
		{
			B = (byte)v3;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(frameHeight);
		Vector3 col = new Vector3((float)(int)R / 255f, (float)(int)G / 255f, (float)(int)B / 255f);
		if (ImGui.ColorEdit3("##col", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
		{
			R = (byte)(col.X * 255f);
			G = (byte)(col.Y * 255f);
			B = (byte)(col.Z * 255f);
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool ColorEditRGBA(ref byte R, ref byte G, ref byte B, ref byte A, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float frameHeight = ImGui.GetFrameHeight();
		float nextItemWidth = (drawValue.Size.X - 4f * ImGui.GetStyle().ItemSpacing.X - frameHeight) / 4f;
		int v = R;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##field", ref v, 1f, 0, 255))
		{
			R = (byte)v;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		int v2 = G;
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##g", ref v2, 1f, 0, 255))
		{
			G = (byte)v2;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		int v3 = B;
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##b", ref v3, 1f, 0, 255))
		{
			B = (byte)v3;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		int v4 = A;
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##a", ref v4, 1f, 0, 255))
		{
			A = (byte)v4;
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(frameHeight);
		Vector4 col = new Vector4((float)(int)R / 255f, (float)(int)G / 255f, (float)(int)B / 255f, (float)(int)A / 255f);
		if (ImGui.ColorEdit4("##col", ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel))
		{
			R = (byte)(col.X * 255f);
			G = (byte)(col.Y * 255f);
			B = (byte)(col.Z * 255f);
			A = (byte)(col.W * 255f);
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool InputFloat(ref float value, in DrawValue drawValue, out ItemFlags flags)
	{
		if (drawValue.memberDrawer.widget is DragWidget dragWidget)
		{
			bool result = ImGui.DragFloat("##field", ref value, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format);
			flags = TypeDrawer.Flags();
			return result;
		}
		bool result2 = ImGui.InputFloat("##field", ref value);
		flags = TypeDrawer.Flags();
		return result2;
	}

	public static bool InputDouble(ref double value, in DrawValue _, out ItemFlags flags)
	{
		bool result = ImGui.InputDouble("##field", ref value);
		flags = TypeDrawer.Flags();
		return result;
	}

	public static bool InputDecimal(ref decimal value, in DrawValue _, out ItemFlags flags)
	{
		string input = value.ToString(CultureInfo.InvariantCulture);
		bool result = false;
		if (ImGui.InputText("##field", ref input, 32u))
		{
			input = input.Replace(',', '.');
			if (decimal.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
			{
				result = true;
			}
		}
		flags = TypeDrawer.Flags();
		return result;
	}

	public static bool InputInt32(ref int value, in DrawValue drawValue, out ItemFlags flags)
	{
		if (drawValue.memberDrawer.widget is DragWidget dragWidget)
		{
			bool result = ImGui.DragInt("##field", ref value, (int)dragWidget.speed, (int)dragWidget.min, (int)dragWidget.max, dragWidget.format);
			flags = TypeDrawer.Flags();
			return result;
		}
		bool result2 = ImGui.InputInt("##field", ref value, 0, 0);
		flags = TypeDrawer.Flags();
		return result2;
	}

	public static bool InputFloat2(ref float x, ref float y, in DrawValue drawValue, out ItemFlags flags)
	{
		if (drawValue.memberDrawer.widget is DragWidget)
		{
			return DragFloat2(ref x, ref y, in drawValue, out flags);
		}
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 1f * ImGui.GetStyle().ItemSpacing.X) / 2f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##field", ref x, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##y", ref y, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool DragFloat2(ref float x, ref float y, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		DragWidget dragWidget = (DragWidget)drawValue.memberDrawer.widget;
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 1f * ImGui.GetStyle().ItemSpacing.X) / 2f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##field", ref x, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##y", ref y, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool InputFloat3(ref float x, ref float y, ref float z, in DrawValue drawValue, out ItemFlags flags)
	{
		if (drawValue.memberDrawer.widget is DragWidget)
		{
			return DragFloat3(ref x, ref y, ref z, in drawValue, out flags);
		}
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 2f * ImGui.GetStyle().ItemSpacing.X) / 3f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##field", ref x, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##y", ref y, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##z", ref z, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool DragFloat3(ref float x, ref float y, ref float z, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		DragWidget dragWidget = (DragWidget)drawValue.memberDrawer.widget;
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 2f * ImGui.GetStyle().ItemSpacing.X) / 3f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##field", ref x, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##y", ref y, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##z", ref z, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool InputFloat4(ref float x, ref float y, ref float z, ref float w, in DrawValue drawValue, out ItemFlags flags)
	{
		if (drawValue.memberDrawer.widget is DragWidget)
		{
			return DragFloat4(ref x, ref y, ref z, ref w, in drawValue, out flags);
		}
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 3f * ImGui.GetStyle().ItemSpacing.X) / 4f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##field", ref x, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##y", ref y, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##z", ref z, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputFloat("##w", ref w, 0f))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool DragFloat4(ref float x, ref float y, ref float z, ref float w, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		DragWidget dragWidget = (DragWidget)drawValue.memberDrawer.widget;
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 3f * ImGui.GetStyle().ItemSpacing.X) / 4f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##field", ref x, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##y", ref y, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##z", ref z, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragFloat("##w", ref w, dragWidget.speed, dragWidget.min, dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool InputInt2(ref int x, ref int y, in DrawValue drawValue, out ItemFlags flags)
	{
		if (drawValue.memberDrawer.widget is DragWidget)
		{
			return DragInt2(ref x, ref y, in drawValue, out flags);
		}
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 1f * ImGui.GetStyle().ItemSpacing.X) / 2f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputInt("##field", ref x, 0))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputInt("##y", ref y, 0))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool DragInt2(ref int x, ref int y, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		DragWidget dragWidget = (DragWidget)drawValue.memberDrawer.widget;
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 1f * ImGui.GetStyle().ItemSpacing.X) / 2f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##field", ref x, (int)dragWidget.speed, (int)dragWidget.min, (int)dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##y", ref y, (int)dragWidget.speed, (int)dragWidget.min, (int)dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool InputInt3(ref int x, ref int y, ref int z, in DrawValue drawValue, out ItemFlags flags)
	{
		if (drawValue.memberDrawer.widget is DragWidget)
		{
			return DragInt3(ref x, ref y, ref z, in drawValue, out flags);
		}
		TypeDrawer.PushItemStyle();
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 2f * ImGui.GetStyle().ItemSpacing.X) / 3f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputInt("##field", ref x, 0))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputInt("##y", ref y, 0))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.InputInt("##z", ref z, 0))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}

	public static bool DragInt3(ref int x, ref int y, ref int z, in DrawValue drawValue, out ItemFlags flags)
	{
		TypeDrawer.PushItemStyle();
		DragWidget dragWidget = (DragWidget)drawValue.memberDrawer.widget;
		flags = ItemFlags.None;
		bool result = false;
		float nextItemWidth = (drawValue.Size.X - 2f * ImGui.GetStyle().ItemSpacing.X) / 3f;
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##field", ref x, (int)dragWidget.speed, (int)dragWidget.min, (int)dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##y", ref y, (int)dragWidget.speed, (int)dragWidget.min, (int)dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		ImGui.SameLine();
		ImGui.SetNextItemWidth(nextItemWidth);
		if (ImGui.DragInt("##z", ref z, (int)dragWidget.speed, (int)dragWidget.min, (int)dragWidget.max, dragWidget.format))
		{
			result = true;
		}
		flags |= TypeDrawer.Flags();
		TypeDrawer.PopItemStyle();
		return result;
	}
}
