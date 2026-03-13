using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Style;
using Monod.AssetsModule;
using Monod.Graphics;
using Monod.Graphics.Components;
using Monod.Graphics.Fonts;
using Monod.InputModule;
using Monod.Localization;
using Monod.Modding.ModdingOld;
using Monod.Shared;
using Monod.TimeModule;
using System.Globalization;

namespace Monod;

/// <summary>
/// Class containing similar code for all apps made with MonodEngine, to reduce repetitive code. Every app must have a class inheriting from <see cref="MonodGame"/>.
/// </summary>
public abstract class MonodGame : Game
{
    /// <summary>
    /// Main <see cref="AssetManager"/> for vanilla game.
    /// </summary>
    public static AssetManager MainAssetManager = null!;

    public UiSystem MainUiSystem;

    /// <summary>
    /// Creates a new <see cref="MonodGame"/>.
    /// </summary>
    protected MonodGame()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        Renderer.OnGameCreated(this);
        IsFixedTimeStep = false;
        Window.AllowUserResizing = true;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        Renderer.Initialize(this);
        Input.Initialize(this);
        Assets.Initialize();
        Locale.Initialize();

        Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);

        string contentPath = $"{AppContext.BaseDirectory}Content";
        MainAssetManager = new AssetManager(new AssetLoader((contentPath)));
        Assets.RegisterAssetManager(MainAssetManager, "");

        base.Initialize();
    }

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        MainAssetManager.LoadAssets();
        var style = new UntexturedStyle(Renderer.spriteBatch);
        style.Font = GlobalFonts.MenuFont;
        MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
        MainUiSystem = new UiSystem(this, style);
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        Time.Update(gameTime, IsActive);
        if (GraphicsSettings.FocusLossBehaviour > GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        base.Update(gameTime);
        Input.Update();
        MainThread.Update();

        ModManager.Update();
        Assets.Update();

        if (Assets.IsLoading) //Don't care about thread safety, readonly with no side effects other than one frame delay
        {
            return;
        }

        if (ModManager.InProgress) return;
        //DevConsole.Update(); TODO dev console in-game w/ Console class support like in DD.

        UpdateM();
        MainUiSystem.Update(gameTime);

        Input.PostUpdate();
    }

    /// <inheritdoc/> 
    protected override void Draw(GameTime gameTime)
    {
        if (GraphicsSettings.FocusLossBehaviour > GraphicsSettings.OnFocusLossBehaviour.Eco && !IsActive) return;
        GraphicsDevice.Clear(Color.Black);
        if (ModManager.InProgress)
        {
            //font.DrawText("Loading mods.", Renderer.spriteBatch, Vector2.Zero); TODO have some sort of default font or something. Requires MainAssetManager.
            return;
        }

        if (Assets.IsLoading)
        {
            GenericFont? font = GlobalFonts.MenuFont;
            float width = Window.ClientBounds.Width;
            float height = Window.ClientBounds.Height;

            Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);

            if (Assets.ActiveCommand is null) return;

            if (font is not null)
            {
                font.DrawString(Renderer.spriteBatch, "Loading assets:", new(width * 0.1f, height * 0.1f), Color.White);
                font.DrawString(Renderer.spriteBatch, $"{Assets.CommandsFinished}/{Assets.CommandsTotal}", new(width * 0.1f, height * 0.2f), Color.White);
                font.DrawString(Renderer.spriteBatch, $"{Assets.ActiveCommand.CurrentProgress}/{Assets.ActiveCommand.TotalProgress}", new(width * 0.1f, height * 0.6f), Color.White);
                font.DrawString(Renderer.spriteBatch, $"{Assets.ActiveCommand.GetText()}", new(width * 0.1f, height * 0.7f), Color.White);
            }
            ProgressBar.Draw((float)Assets.CommandsFinished / Assets.CommandsTotal, new(width * 0.1f, height * 0.3f), new(width * 0.8f, height * 0.1f));
            ProgressBar.Draw((float)Assets.ActiveCommand.CurrentProgress / Assets.ActiveCommand.TotalProgress, new(width * 0.1f, height * 0.8f), new(width * 0.8f, height * 0.1f));
            Renderer.End();
            return;
        }

        DrawM();
        MainUiSystem.Draw(gameTime, Renderer.spriteBatch);
        base.Draw(gameTime);
    }

    /// <summary>
    /// Called when the game should update. Override this to update your game.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Time"/> to access delta time.
    /// </remarks>
    protected abstract void UpdateM();

    /// <summary>
    /// Called when the game should draw a frame. Override this to render your game.
    /// </summary>
    protected abstract void DrawM();
}