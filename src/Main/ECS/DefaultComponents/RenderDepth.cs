using Friflo.Engine.ECS;

namespace Monod.ECS.DefaultComponents;

[ComponentKey("RenderDepth")]
[ComponentSymbol("D")]
public struct RenderDepth : IComponent
{
    public float Depth;

    public RenderDepth(float depth)
    {
        Depth = depth;
    }
}
