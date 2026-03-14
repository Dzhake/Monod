using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

namespace Monod.InputModule;

/// <summary>
/// A single binding: key and modifiers, attached to some action.
/// </summary>
public readonly record struct Keybind(Key key, KeyModifiers modifiers)
{
    /// <summary>
    /// Key used by this Keybinds.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public readonly Key key = key;

    /// <summary>
    /// Modifiers that need to be down for this Keybinds to trigger.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public readonly KeyModifiers modifiers = modifiers;

    /// <summary>
    /// Whether this Keybinds is considered active in the <paramref name="state"/> for <paramref name="playerIndex"/>.
    /// </summary>
    /// <param name="state">State in which to check.</param>
    /// <param name="playerIndex">Index of the player in <see cref="Input.Players"/>.</param>
    /// <returns>Whether this Keybinds is considered active in the <paramref name="state"/> for <paramref name="playerIndex"/>.</returns>
    [Pure]
    public bool IsActive(InputState state, int playerIndex)
    {
        return modifiers == KeyModifiers.Any || modifiers == state.GetActiveModifiers()
            ? Input.GetValue(state, key, playerIndex) != 0
            : false;
    }
}
