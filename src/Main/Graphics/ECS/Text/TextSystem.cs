using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Microsoft.Xna.Framework;
using MLEM.Formatting;
using Monod.ECS.DefaultComponents;
using Monod.Graphics.Fonts;
using Monod.TimeModule;

namespace Monod.Graphics.ECS.Text;

public class TextSystem : QuerySystem<TextComponent>
{
    public TextFormatter Formatter = new();

    protected override void OnUpdate()
    {
        Query.ForEachEntity(Update);
    }

    private void Update(ref TextComponent text, Entity entity)
    {
        if (text.tokenized is null) CreateTokenizedString(text);
        var data = entity.Data;

        Vector2 position;
        if (data.TryGet<Transform2D>(out var position2D))
            position = position2D.Position;
        else
            position = Vector2.Zero;

        float rotation;
        if (data.TryGet<Rotation2D>(out var rotation2D))
            rotation = rotation2D.Angle;
        else
            rotation = 0;

        Vector2 scale;
        if (data.TryGet<Scale2D>(out var scale2D))
            scale = scale2D.Value;
        else
            scale = Vector2.One;

        float depth;
        if (data.TryGet<RenderDepth>(out var renderDepth))
            depth = renderDepth.Depth;
        else
            depth = 0;


        text.tokenized?.Draw(Time.gameTime, Renderer.spriteBatch, position, GlobalFonts.MenuFont, text.color, scale, depth, rotation, new(0, 0));
    }

    private void CreateTokenizedString(TextComponent text)
    {
        text.tokenized = Formatter.Tokenize(GlobalFonts.MenuFont, text.Text, text.Alignment);
    }
}
