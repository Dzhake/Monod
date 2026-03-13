using MLEM.Maths;
using MLEM.Ui;
using MLEM.Ui.Elements;
using Monod.Graphics;
using Monod.InputModule.InputActions;
using Monod.InputModule.Parsing;
using Monod.Localization;
using Monod.TimeModule;

namespace Monod.InputModule;

public class RebindMenu
{
    public Panel Root;
    public int playerIndex;

    public bool AdvancedMode;

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
    }

    public void RebuildUi()
    {
        Root.RemoveChildren();
        float windowWidth = Renderer.Window.ClientBounds.Width;
        float windowHeight = Renderer.Window.ClientBounds.Height;

        Checkbox advancedModeCheckbox = new Checkbox(Anchor.AutoLeft, new(1, 18), "Advanced mode", AdvancedMode);
        advancedModeCheckbox.SetWidthBasedOnChildren = true;
        advancedModeCheckbox.OnCheckStateChange += (_, isChecked) =>
        {
            AdvancedMode = isChecked;
            RebuildUi();
        };

        Root.AddChild(advancedModeCheckbox);

        for (int actionIndex = 0; actionIndex < Input.ActionNames.MaxValue; actionIndex++)
        {
            string actionName = Input.ActionNames.GetName(actionIndex);
            Paragraph label = new(Anchor.AutoLeft, 1, actionName, true);
            Root.AddChild(label);
            Group actionsGroup = new(Anchor.AutoCenter, new(1, 1));
            actionsGroup.ChildPadding = new Padding(0, 1);
            Root.AddChild(actionsGroup);
            if (AdvancedMode)
            {
                string text = "";
                if (Input.Players[playerIndex].Map.Actions.TryGetValue(actionIndex, out var action)) text = action.ToString() ?? "";
                TextField textField = new(Anchor.AutoInlineCenter, new(1, 30), text: text);
                int actionIndexCopy = actionIndex; //dereference, to avoid issues with lambda in 'for' loop.
                textField.OnEnterPressed += element =>
                {
                    if (element is not TextField textField) return;
                    ParseAction(actionIndexCopy, textField.Text);
                };
                actionsGroup.AddChild(textField);
            }
            else
            {
                if (Input.Players[playerIndex].Map.Actions.TryGetValue(actionIndex, out var action))
                    AddActionButtons(actionIndex, actionsGroup, action, null);
                Button startRebindingButton = AddBindButton(actionIndex);
                actionsGroup.AddChild(startRebindingButton);
            }
        }
    }

    private void ParseAction(int actionIndex, string text)
    {
        InputAction action = InputActionParser.Parse(text);
        if (action is InvalidInputAction)
        {

        }
        else
        {
            Input.Players[playerIndex].Map.Actions[actionIndex] = action;
        }
        RebuildUi();
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

    private void AddActionButtons(int actionIndex, Group actionsGroup, InputAction action, InputAction? parentAction)
    {
        if (action is KeyBasedAction keyBasedAction)
        {
            Button actionButton = ActionRemovalButton(actionIndex, keyBasedAction);
            actionsGroup.AddChild(actionButton);
            if (parentAction is ArrayBasedAction arrayBasedAction)
                actionButton.OnPressed = element => RemoveInnerAction(arrayBasedAction.Actions.IndexOf(action), arrayBasedAction);
            else
                actionButton.OnPressed = element => RemoveTopLevelAction(actionIndex);

        }
        else if (action is OrAction orAction)
        {
            foreach (var innerAction in orAction.Actions)
            {
                AddActionButtons(actionIndex, actionsGroup, innerAction, orAction);
            }
        }
        else if (action is AndAction andAction)
        {
            foreach (var innerAction in andAction.Actions)
            {
                AddActionButtons(actionIndex, actionsGroup, innerAction, andAction);
            }
        }
    }


    private static Button ActionRemovalButton(int actionIndex, KeyBasedAction keyBasedAction)
    {
        Button actionButton = BasicButton();
        actionButton.AddChild(new Paragraph(Anchor.Center, 1, keyBasedAction.Keybind.ToString(), true));

        return actionButton;
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

    private void RemoveInnerAction(int indexInArray, ArrayBasedAction parentAction)
    {
        var actionsList = parentAction.Actions.ToList();
        actionsList.RemoveAt(indexInArray);
        parentAction.Actions = actionsList.ToArray();
        RebuildUi();
    }

    private void RemoveTopLevelAction(int actionIndex)
    {
        Input.Players[playerIndex].Map.Actions.Remove(actionIndex);
        RebuildUi();
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

            Key anyKey = Input.FirstKeyDown(playerIndex);
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
        InputAction newAction = new DownAction(keyToBind);
        var actions = Input.Players[playerIndex].Map.Actions;
        if (!actions.TryGetValue(actionToBind, out var prevAction) || prevAction is null)
        {
            actions[actionToBind] = newAction;
            return;
        }

        if (SameAsNewBindAction(prevAction, keyToBind))
        {
            actions.Remove(actionToBind);
            return;
        }

        if (prevAction is OrAction orAction)
        {
            var orActionsList = orAction.Actions.ToList();
            bool foundSameAction = false;

            for (int i = 0; i < orActionsList.Count; i++)
            {
                //Exactly same as the action we wanted to add - remove it instead.
                if (SameAsNewBindAction(orActionsList[i], keyToBind))
                {
                    orActionsList.RemoveAt(i);
                    foundSameAction = true;
                    break;
                }
            }

            if (!foundSameAction)
                orActionsList.Add(newAction);

            orAction.Actions = orActionsList.ToArray();
            return;
        }

        actions[actionToBind] = new OrAction([actions[actionToBind], newAction]);
    }

    private static bool SameAsNewBindAction(InputAction action, Key keyToBind)
    {
        return action is DownAction downAction && downAction.Keybind == keyToBind;
    }

    public void Draw()
    {
        Root?.Draw(Time.gameTime, Renderer.spriteBatch, 1, new());
    }
}
