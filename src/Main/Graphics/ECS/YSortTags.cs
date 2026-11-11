using Monod.ECS.DefaultComponents;

namespace Monod.Graphics.ECS;

/// <summary>
/// Indicates, that the entity's <see cref="RenderDepth.YSortOffset"/> should be changed based on the entity's own <see cref="Transform2D"/>.
/// </summary>
public struct YSortSelfTag : ITag;

/// <summary>
/// Indicates, that the entity's <see cref="RenderDepth.YSortOffset"/> should be copied from the entity's parent.
/// </summary>
public struct YSortParentTag : ITag;