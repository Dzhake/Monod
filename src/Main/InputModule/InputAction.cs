namespace Monod.InputModule;

public class InputAction(List<Keybind> keybinds)
{
    public List<Keybind> Keybinds = keybinds;

    [NonSerialized]
    public bool WasActive;

    [NonSerialized]
    public bool IsActive;

    public void NextFrame()
    {
        WasActive = IsActive;
        IsActive = false;
    }

    public void UpdateIsActive(InputState state, int playerIndex)
    {
        foreach (Keybind keybind in Keybinds)
        {
            if (keybind.IsActive(state, playerIndex))
            {
                IsActive = true;
                break;
            }
        }
    }

    public bool Down() => IsActive;
    public bool Up() => !IsActive;
    public bool Pressed() => IsActive && !WasActive;
    public bool Released() => !IsActive && WasActive;
    public bool Held() => IsActive && WasActive;

    public InputAction Clone()
    {
        List<Keybind> newKeybinds = new();
        foreach (Keybind keybind in Keybinds) newKeybinds.Add(keybind);
        return new InputAction(newKeybinds);
    }
}