using MLEM.Maths;
using MLEM.Ui;
using MLEM.Ui.Elements;
using Monod.Graphics;
using Monod.Localization;
using Monod.TimeModule;

namespace Monod.InputModule;

public class RebindMenu
{
    public Panel Root;
    public int playerIndex;

    /// <summary>
    /// Timeout after the player pressed "bind" button, to prevent the key that was used to press that button register as the new binding.
    /// </summary>
    public float timeout;
    public bool bindingActive;
    public int actionToBind;

    public RebindMenu(UiSystem system)
    {
        Root = new Panel(Anchor.AutoInline, new(Renderer.Window.ClientBounds.Width, Renderer.Window.ClientBounds.Height));
        Root.Padding = new Padding(0, 2);
        system.Add("RebindMenu", Root);
        RebuildUi();
    }

    public void RebuildUi()
    {
        Root.RemoveChildren();
        float windowWidth = Renderer.Window.ClientBounds.Width;
        float windowHeight = Renderer.Window.ClientBounds.Height;

        foreach ((int actionIndex, InputAction action) in Input.Players[playerIndex].Map)
        {
            string actionName = Input.ActionNames.GetName(actionIndex);
            Paragraph label = new(Anchor.AutoLeft, 1, actionName, true);
            Root.AddChild(label);
            Group actionsGroup = new(Anchor.AutoCenter, new(1, 1));
            actionsGroup.ChildPadding = new Padding(0, 1);
            Root.AddChild(actionsGroup);
            for (int i = 0; i < action.Keybinds.Count; i++)
            {
                Button keybindButton = BasicButton();
                int keybindIndex = i; //dereference
                keybindButton.OnPressed += _ =>
                {
                    Input.GetPlayer(playerIndex).Map[actionIndex].Keybinds.RemoveAt(keybindIndex);
                    RebuildUi();
                };
                keybindButton.OnSecondaryPressed += _ =>
                {
                    var keybinds = Input.GetPlayer(playerIndex).Map[actionIndex].Keybinds;
                    var keybind = keybinds[keybindIndex];
                    if (keybind.modifiers != KeyModifiers.None)
                        keybinds[keybindIndex] = new(keybind.key, KeyModifiers.None);
                    else
                        keybinds[keybindIndex] = new(keybind.key, KeyModifiers.Any);

                    RemoveDuplicates(actionIndex);
                    RebuildUi();
                };
                actionsGroup.AddChild(keybindButton);

                Keybind keybind = action.Keybinds[i];
                string keybindText = "";
                if (keybind.modifiers != KeyModifiers.Any)
                {
                    if (keybind.modifiers == KeyModifiers.None) keybindText += "(None) ";
                    if (keybind.modifiers.HasFlag(KeyModifiers.Ctrl)) keybindText += "Ctrl+";
                    if (keybind.modifiers.HasFlag(KeyModifiers.Shift)) keybindText += "Shift+";
                    if (keybind.modifiers.HasFlag(KeyModifiers.Alt)) keybindText += "Alt+";
                }
                keybindText += keybind.key.ToString();
                keybindButton.AddChild(new Paragraph(Anchor.Center, 1, keybindText, true));
            }
            Button startRebindingButton = AddBindButton(actionIndex);
            actionsGroup.AddChild(startRebindingButton);
        }
    }

    private Button AddBindButton(int actionIndex)
    {
        Button button = BasicButton();
        button.AddChild(new Paragraph(Anchor.Center, 1, element => GetAddBindButtonText(actionIndex), true));
        button.OnPressed += element => StartBinding(actionIndex);

        return button;
    }

    private string GetAddBindButtonText(int actionIndex)
    {
        if (actionIndex == actionToBind && bindingActive) return Locale.Get("Waiting for input");
        return "+";
    }

    private static Button BasicButton()
    {
        Button basicButton = new(Anchor.AutoInline, new(1, 1));
        basicButton.SetHeightBasedOnChildren = true;
        basicButton.SetWidthBasedOnChildren = true;
        basicButton.ChildPadding = new Padding(6, 1);
        basicButton.Padding = new Padding(2, 0);
        return basicButton;
    }

    public void Update()
    {
        if (Root is null) RebuildUi();
        UpdateBinding();
    }

    private void UpdateBinding()
    {
        if (bindingActive)
        {
            timeout -= Time.DeltaTime;
            if (timeout > 0) return;

            Key anyKey = Input.FirstKeyReleased(playerIndex);
            if (anyKey == Key.None) return;
            BindKey(anyKey);
            RebuildUi();
            StopBinding();
        }
    }

    private void StartBinding(int actionIndex)
    {
        actionToBind = actionIndex;
        timeout = 0.2f;
        bindingActive = true;
    }

    private void StopBinding()
    {
        bindingActive = false;
        timeout = 0;
        actionToBind = -1;
    }

    private void BindKey(Key keyToBind)
    {
        KeyModifiers modifiers = KeyModifiers.Any;
        KeyModifiers currentModifiers = Input.CurState.GetActiveModifiers();
        if (Input.GetPlayer(playerIndex).UsesKeyboard && currentModifiers != KeyModifiers.None)
            modifiers = currentModifiers;

        Keybind keybind = new(keyToBind, modifiers);

        var keybinds = Input.Players[playerIndex].Map[actionToBind].Keybinds;

        keybinds.Add(keybind);
        RemoveDuplicates(actionToBind);
    }

    private void RemoveDuplicates(int actionIndex)
    {
        InputAction action = Input.Players[playerIndex].Map[actionIndex];
        action.Keybinds = action.Keybinds.ToHashSet().ToList();
    }
}
