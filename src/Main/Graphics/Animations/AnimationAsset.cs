using Monod.AssetsModule;

namespace Monod.Graphics.Animations;

/// <summary>
/// Object, that is stored as an asset when the animations are loaded. Actual animations are automatically loaded to the <see cref="Animations.Dict"/> when <see cref="AnimationAsset"/> is created.
/// </summary>
public class AnimationAsset : IDisposable
{
    /// <summary>
    /// Ids of the animations that were in the asset.
    /// </summary>
    public string[] Ids;

    public AnimationAsset(Animation[] anims)
    {
        Ids = new string[anims.Length];
        try
        {
            Animations.DictLock.EnterWriteLock();
            for (int i = 0; i < anims.Length; i++)
            {
                var anim = anims[i];
                if (Animations.Dict.ContainsKey(anim.Id))
                {
                    Assets.Logger.Warning("Tried to add two animations with the same id '{Id}' to the anims dictonary! When an animation is unloaded it removes itself from the dictionary, so if one of those anims is unloaded, the other becomes invalid too!", anim.Id);
                }
                Animations.Dict.Add(anim.Id, anim);
                Ids[i] = anim.Id;
            }
        }
        finally
        {
            Animations.DictLock.ExitWriteLock();
        }
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        try
        {
            Animations.DictLock.EnterWriteLock();
            foreach (string id in Ids)
                Animations.Dict.Remove(id);
        }
        finally
        {
            Animations.DictLock.ExitWriteLock();
        }
    }
}
