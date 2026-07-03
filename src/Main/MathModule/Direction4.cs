namespace Monod.MathModule;

/// <summary>
/// Enum that represents 4 possible directions as flags.
/// </summary>
[Flags]
public enum Direction4
{
    /// <summary>
    /// (0, 1)
    /// </summary>
    Down = 1,

    /// <summary>
    /// (0, -1)
    /// </summary>
    Up = 1 << 1,

    /// <summary>
    /// (-1, 0)
    /// </summary>
    Left = 1 << 2,

    /// <summary>
    /// (1, 0)
    /// </summary>
    Right = 1 << 3,


    /// <summary>
    /// All directions.
    /// </summary>
    All = Down | Up | Left | Right
}

public static class Direction4Extensions
{
    /// <summary>
    /// Check whether <paramref name="vector"/> matches any of the directions in the <paramref name="directions"/>. I.e., if directions contains left and the vector has negative X and zero Y, then the method will return <see langword="true"/>.
    /// </summary>
    /// <param name="directions">Directions to check.</param>
    /// <param name="vector">Vector to check, doesn't need to be normalized.</param>
    /// <returns>Whether <paramref name="vector"/> matches any of the directions in the <paramref name="directions"/>. I.e., if directions contains left and the vector has negative X and zero Y, then the method will return <see langword="true"/>.</returns>
    public static bool Matches(this Direction4 directions, Vector2 vector)
    {
        return (directions.HasFlag(Direction4.Down) && vector.X == 0 && vector.Y > 0) || (directions.HasFlag(Direction4.Up) && vector.X == 0 && vector.Y < 0) || (directions.HasFlag(Direction4.Left) && vector.X < 0 && vector.Y == 0) || (directions.HasFlag(Direction4.Right) && vector.X > 0 && vector.Y == 0);
    }
}