using Friflo.Engine.ECS;
using Hexa.NET.ImGui;
using System;
using System.Numerics;
using System.Text;

namespace Friflo.EcGui;

public readonly struct DrawValue
{
    internal readonly DrawContext context;

    internal readonly MemberDrawer memberDrawer;

    internal readonly DrawValueFlags flags;

    internal readonly IContainer? container;

    private readonly ItemMember? itemMember;

    public Entity Entity => context.entity;

    public ref Vector2 Size => ref context.drawRect.size;

    public bool MultiLine => context.multiLine;

    internal DrawValue(DrawContext context, in MemberDrawer memberDrawer, DrawValueFlags flags)
    {
        this.context = context;
        context.drawRect = context.rect;
        this.memberDrawer = memberDrawer;
        this.flags = flags;
        container = null;
        itemMember = null;
    }

    internal DrawValue(DrawContext context, IContainer? container, ItemMember itemMember)
    {
        this.context = context;
        context.drawRect = context.rect;
        memberDrawer = itemMember.drawer;
        flags = DrawValueFlags.Value;
        this.container = container;
        this.itemMember = itemMember;
    }

    public override string ToString()
    {
        return $"path: {memberDrawer.member}  entity: {Entity.Id}";
    }

    public bool GetValue<TMember>(out TMember value, out Exception exception)
    {
        if (itemMember == null)
        {
            if (memberDrawer.typeDrawer is RelationsDrawer relationsDrawer)
            {
                exception = null;
                return relationsDrawer.GetEntityRelations<TMember>(context.entity, out value);
            }
            return EntityUtils.GetEntityComponentMember<TMember>(context.entity, memberDrawer.member, out value, out exception);
        }
        return ((GetItemMember<TMember>)itemMember.getter)(this, out value, out exception);
    }

    public bool SetValue<TMember>(TMember value)
    {
        if (memberDrawer.member.setter is null)
        {
            return false;
        }
        context.Edit();
        Exception exception;
        if (itemMember == null)
        {
            return EntityUtils.SetEntityComponentMember(context.entity, memberDrawer.member, value, MemberChangedHandlers.Get(in memberDrawer), out exception);
        }
        return ((SetItemMember<TMember>)itemMember.setter)(this, in value, out exception);
    }

    public ItemFlags DrawException(Exception exception)
    {
        string exceptionCode = ExceptionUtils.GetExceptionCode(exception);
        ImGui.PushStyleColor(ImGuiCol.Text, GlobalColors.errorText);
        ImGui.PushStyleColor(ImGuiCol.Button, GlobalColors.frameBg);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, GlobalColors.frameBg);
        if (ImGui.Button(exceptionCode, Size))
        {
            ImGui.SetClipboardText(TextUtils.AsBytes(ExceptionUtils.GetExceptionDetails(exception, memberDrawer.member, Entity)));
        }
        ItemFlags result = TypeDrawer.Flags();
        ImGui.PopStyleColor(3);
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.ForTooltip))
        {
            StringBuilder exceptionDetails = ExceptionUtils.GetExceptionDetails(exception, memberDrawer.member, Entity);
            exceptionDetails.Append("\n\n(Click to copy error to clipboard)");
            ImGui.SetItemTooltip(TextUtils.AsBytes(exceptionDetails));
        }
        return result;
    }
}
