namespace Monod.InputModule;

/// <summary>
/// Enum of modifiers that can applied to <see cref="Keybind"/>.
/// </summary>
[Flags]
public enum KeyModifiers
{
    /// <summary>
    /// Any key modifiers, including <see cref="None"/>.
    /// </summary>
    Any = -1,

    /// <summary>
    /// No key modifiers.
    /// </summary>
    None = 0,

    /// <summary>
    /// <see cref="Key.LeftControl"/> or <see cref="Key.RightControl"/>.
    /// </summary>
    Ctrl = 1,

    /// <summary>
    /// <see cref="Key.LeftShift"/> or <see cref="Key.RightShift"/>.
    /// </summary>
    Shift = 1 << 1,

    /// <summary>
    /// <see cref="Key.LeftAlt"/> or <see cref="Key.RightAlt"/>.
    /// </summary>
    Alt = 1 << 2,
}