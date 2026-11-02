using Monod.AssetsModule;

namespace Monod.Graphics.Animations;

/// <summary>
/// Object, that is stored as an asset when the animations are loaded. Actual animations are automatically loaded to the <see cref="Animations.Dict"/> when <see cref="AnimationAsset"/> is created.
/// </summary>
public class AnimationAsset : IAsset
{
    /// <summary>
    /// Animations that were read from the asset.
    /// </summary>
    public Animation[] Anims;

    public AnimationAsset(Animation[] anims)
    {
        Anims = anims;
    }

    ///<inheritdoc/>
    public void OnAddedToCache(AssetCacheContext ctx)
    {
        try
        {
            Animations.DictLock.EnterWriteLock();
            for (int i = 0; i < Anims.Length; i++)
            {
                var anim = Anims[i];
                if (Animations.Dict.ContainsKey(anim.Id))
                {
                    Assets.Logger.Error("Tried to add two animations with the same id '{Id}' to the Animations dictonary! When an animation is unloaded it removes itself from the dictionary, so if one of those anims is unloaded, the other becomes invalid too!", anim.Id);
                    continue;
                }
                Animations.Dict.Add(anim.Id, anim);
            }
        }
        finally
        {
            Animations.DictLock.ExitWriteLock();
        }
    }

    ///<inheritdoc/>
    public void OnRemovedFromCache(AssetCacheContext ctx)
    {
        try
        {
            Animations.DictLock.EnterWriteLock();
            foreach (var anim in Anims)
                Animations.Dict.Remove(anim.Id);
        }
        finally
        {
            Animations.DictLock.ExitWriteLock();
        }
    }
}
