using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monod.AssetsSystem;
using Monod.AssetsSystem.AssetLoaders;
using Monod.GraphicsSystem;
using Monod.GraphicsSystem.Components;
using Monod.GraphicsSystem.Fonts;
using Monod.InputSystem;
using Monod.ModSystem;
using Monod.TimeSystem;
using Monod.Utils.General;

namespace Monod;

/// <summary>
/// Class containing similar code for all apps made with MonodEngine, to reduce repetitive code. Every app must have a class inheriting from <see cref="MonodGame"/>.
/// </summary>
public abstract class MonodGame : Game
{
    /// <summary>
    /// Main <see cref="AssetManager"/> for vanilla game.
    /// </summary>
    public static AssetManager? MainAssetManager;

    /// <summary>
    /// Creates a new <see cref="MonodGame"/>.
    /// </summary>
    protected MonodGame()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        MonodMain.OnGameCreated(this);
        IsFixedTimeStep = false;
        Window.AllowUserResizing = true;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        MonodMain.OnGameInitialize(this);
        Assets.Initialize();
        base.Initialize();
    }

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);

        string contentPath = $"{AppContext.BaseDirectory}Content";
        MainAssetManager = new AssetManager(new FileAssetLoader((contentPath)));
        Assets.RegisterAssetManager(MainAssetManager, "");
        MainAssetManager.LoadAssets();
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

        if (Assets.LoadingAssetLoaders.Count != 0) //Don't care about thread safety, readonly with no side effects other than one frame delay
        {
            return;
        }

        if (ModManager.InProgress || Assets.LoadingAssetLoaders.Count != 0) return;
        //DevConsole.Update(); TODO dev console in-game w/ Console class support like in DD.

        UpdateM();

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

        //if (true)
        if (Assets.LoadingAssetLoaders.Count != 0)
        {
            IFont? font = GlobalFonts.MenuFont;
            float width = Window.ClientBounds.Width;
            float height = Window.ClientBounds.Height;

            Renderer.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp);

            if (font is not null)
            {
                font.DrawText("Loading assets:", new(width * 0.1f, height * 0.6f), scale: new(3));
                font.DrawText($"{Assets.LoadedAssets}/{Assets.TotalAssets}", new(width * 0.1f, height * 0.7f), scale: new(3));
            }
            ProgressBar.Draw((float)Assets.LoadedAssets / Assets.TotalAssets, new(width * 0.1f, height * 0.8f), new(width * 0.8f, height * 0.1f));
            Renderer.End();
            return;
        }
        
        DrawM();
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