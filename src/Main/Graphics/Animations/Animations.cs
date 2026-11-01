namespace Monod.Graphics.Animations;

public static class Animations
{
    public static Dictionary<string, Animation> Dict = new();
    public static ReaderWriterLockSlim DictLock = new();
}
