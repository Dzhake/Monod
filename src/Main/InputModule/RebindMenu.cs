using MLEM.Ui.Elements;
using Monod.Graphics;
using Monod.InputModule.InputActions;
using Monod.TimeModule;

namespace Monod.InputModule;

public class RebindMenu
{
    public Group Root;

    public float timeout;
    public int playerIndex;

    public bool bindingActive;
    public int actionToBind;

    public void RebuildUI()
    {
        Root = new Group(MLEM.Ui.Anchor.AutoInline, Renderer.Window.ClientBounds.Size.ToVector2());
        for (int actionIndex = 0; actionIndex < Input.ActionNames.MaxValue; actionIndex++)
        {
            string actionName = Input.ActionNames.GetName(actionIndex);
            InputAction action = Input.Players[playerIndex].Map.Actions[actionIndex];
            if (action is KeyBasedAction)
            {

            }
        }
    }

    public void Update()
    {
        UpdateBinding();
    }

    private void UpdateBinding()
    {
        if (bindingActive)
        {
            if (timeout > 0)
            {
                timeout -= Time.RawDeltaTime;
                return;
            }

            Key anyKey = Input.FirstKeyDown(playerIndex);
            if (anyKey == Key.None) return;
            bool flowControl = BindKey(anyKey);
            if (!flowControl)
                return;
        }
    }

    private bool BindKey(Key keyToBind)
    {
        InputAction newAction = new DownAction(keyToBind);
        var actions = Input.Players[playerIndex].Map.Actions;
        var prevAction = actions[actionToBind];

        if (prevAction is null)
        {
            actions[actionToBind] = newAction;
            bindingActive = false;
            return false;
        }

        if (prevAction is OrAction orAction)
        {
            var orActionsList = orAction.Actions.ToList();
            orActionsList.Add(newAction);
            orAction.Actions = orActionsList.ToArray();
            bindingActive = false;
            return false;
        }

        actions[actionToBind] = new OrAction([actions[actionToBind], newAction]);
        return true;
    }
}
