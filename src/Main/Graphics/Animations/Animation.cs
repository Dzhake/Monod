namespace Monod.Graphics.Animations;

/// <summary>
/// Animation asse
/// </summary>
public class Animation
{
    /// <summary>
    /// Global id of the animation, used for serialization.
    /// </summary>
    public string Id;

    /// <summary>
    /// An array of frames in order, for example [1,2,2,3,2].
    /// </summary>
    public int[] Frames;

    /// <summary>
    /// Time (in seconds) between frames.
    /// </summary>
    public float TimeBetweenFrames = 1f;

    /// <summary>
    /// Suffix added after the base path, and before the frame number. For example 'idle'.
    /// </summary>
    public string PathSuffix = "";

    public AnimBehaviour OnEnd = AnimBehaviour.Loop;
}
