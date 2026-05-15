using Friflo.Engine.ECS;
using Friflo.Json.Fliox;
using Microsoft.Xna.Framework;
using MLEM.Formatting;

namespace Monod.Graphics.ECS.Text;

public struct TextComponent : IComponent
{
    public string Text;
    public Color color;
    public TextAlignment Alignment;

    [Ignore]
    public TokenizedString tokenized;

    public TextComponent(string text, Color? color = null, TextAlignment alignment = TextAlignment.Center) : this()
    {
        Text = text;
        this.color = color ?? Color.White;
        Alignment = alignment;
    }
}
