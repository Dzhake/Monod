using Microsoft.Xna.Framework.Input;

namespace Monod.InputModule;

/// <summary>
/// Class extensions used by input module.
/// </summary>
public static class InputExtensions
{
    /// <summary>
    /// Convert the <paramref name="value"/> to a "input value" - float representation of the <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Bool value to convert.</param>
    /// <returns>Converted "input value".</returns>
    public static float ToInputValue(this bool value) => value ? 1f : 0f;

    /// <summary>
    /// Convert the <paramref name="value"/> to a "input value" - float representation of the <paramref name="value"/>.
    /// </summary>
    /// <param name="value"><see cref="ButtonState"/> value to convert.</param>
    /// <returns>Converted "input value".</returns>
    public static float ToInputValue(this ButtonState value) => value == ButtonState.Pressed ? 1f : 0f;
}
