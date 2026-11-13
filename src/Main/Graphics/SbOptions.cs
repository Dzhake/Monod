using Microsoft.Xna.Framework.Graphics;

namespace Monod.Graphics;

/// <summary>
/// Holds options for the <see cref="SpriteBatch.Begin"/>, for passing them around easily.
/// </summary>
public struct SbOptions
{
    /// <summary>The drawing order for sprites and text. The effective value is determined by the first non-<see langword="null"/> value in the following order: <see cref="SortMode"/>, <see cref="Renderer.DefaultSpriteSortMode"/>, <see cref="SpriteSortMode.Deferred"/>.</summary>
    public SpriteSortMode SortMode = SpriteSortMode.Deferred;

    /// <summary>State of the blending. The effective value is determined by the first non-<see langword="null"/> value in the following order: <see cref="BlendState"/>, <see cref="Renderer.DefaultBlendState"/>, <see cref="BlendState.AlphaBlend"/>.</summary>
    public BlendState? blendState;

    /// <summary>State of the sampler. The effective value is determined by the first non-<see langword="null"/> value in the following order: <see cref="SamplerState"/>, <see cref="Renderer.DefaultSamplerState"/>, <see cref="SamplerState.LinearClamp"/>.</summary>
    public SamplerState? samplerState;

    /// <summary>State of the depth-stencil buffer. The effective value is determined by the first non-<see langword="null"/> value in the following order: <see cref="DepthStencilState"/>, <see cref="Renderer.DefaultDepthStencilState"/>, <see cref="DepthStencilState.None"/>.</summary>
    public DepthStencilState? depthStencilState;

    /// <summary>State of the rasterization. The effective value is determined by the first non-<see langword="null"/> value in the following order: <see cref="RasterizerState"/>, <see cref="Renderer.DefaultRasterizerState"/>, <see cref="RasterizerState.CullCounterClockwise"/>.</summary>
    public RasterizerState? rasterizerState;

    /// <summary>A custom <see cref="Effect"/> to override the default sprite effect. The effective value is determined by the first non-<see langword="null"/> value in the following order: <see cref="effect"/>, <see cref="Renderer.DefaultEffect"/>.</summary>
    public Effect effect;

    /// <summary>An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.</summary>
    public Matrix? TransformMatrix;

    public SbOptions() { }
}
