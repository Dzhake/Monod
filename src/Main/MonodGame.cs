using Friflo.Engine.ECS.Serialize;
using Friflo.Engine.ECS.Systems;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Ui.Style;
using Monod.AssetsModule;
using Monod.Graphics;
using Monod.Graphics.Components;
using Monod.Graphics.Fonts;
using Monod.Graphics.Settings;
using Monod.ImGuiModule;
using Monod.InputModule;
using Monod.Localization;
using Monod.ModsModule;
using Monod.SaveModule;
using Monod.TimeModule;
using Monod.Utils.General;
using SDL3;
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

    /// <summary>
    /// Main <see cref="EntityStore"/> of the game.
    /// </summary>
    public static EntityStore Store;

    public static EntityStore PrefabsStore;

    public static EntitySerializer entitySerializer = new();
    public static EntityConverter entityConverter = new();

    /// <summary>
    /// Root of the systems that should be called in <see cref="Update"/> (more specifically, <see cref="UpdateLogicSystems"/>).
    /// </summary>
    public required SystemRoot LogicSystemRoot;


    /// <summary>
    /// Root of the systems that should be called in <see cref="Draw"/> (more specifically, <see cref="UpdateDrawSystems"/>).
    /// </summary>
    public required SystemRoot DrawSystemRoot;

    /// <summary>
    /// Object that handles rendering of <see cref="ImGui"/> calls.
    /// </summary>
    public ImGuiRenderer imGuiRenderer;

    /// <summary>
    /// Aspect ratio of the game (X divided by Y).
    /// </summary>
    public static float AspectRatio => 16f / 9f;


    /// <summary>
    /// Creates a new <see cref="MonodGame"/>.
    /// </summary>
    protected MonodGame()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        SDL.SDL_Init(SDL.SDL_InitFlags.SDL_INIT_VIDEO);

        Renderer.OnGameCreated(this);
        IsFixedTimeStep = false;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowSizeChanged;

        CreateStore();
        LogicSystemRoot = new();
        DrawSystemRoot = new();
        LogicSystemRoot.AddStore(Store);
        DrawSystemRoot.AddStore(Store);

        Exiting += OnExit;
    }

    public virtual void CreateStore()
    {
        Store = new();
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        if (!GraphicsSettings.ListenToEvents) return;
        //window was just maximized
        bool windowMaximized = GraphicsSettings.IsWindowMaximized(Renderer.Window.Handle);
        bool windowSetToMaximized = GraphicsSettings.windowMode == WindowMode.Maximized;
        if (windowMaximized != windowSetToMaximized)
        {
            GraphicsSettings.windowMode = windowMaximized ? WindowMode.Maximized : WindowMode.Windowed;
            GraphicsSettings.ApplyWindowMode();
        }
        else
        {
            GraphicsSettings.WindowSize = Window.ClientBounds.Size;
            GraphicsSettings.ApplyWindowSize();
        }
    }

    private void OnExit(object? sender, ExitingEventArgs e)
    {
        SaveManager.Save(SaveType.Settings, SaveManager.SavesLocation);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        SaveManager.Load(SaveType.Settings, SaveManager.SavesLocation);

        Renderer.Initialize(this);
        Input.Initialize(this);
        Assets.Initialize();
        Locale.Initialize();

        Renderer.spriteBatch = new SpriteBatch(GraphicsDevice);

        string contentPath = $"{AppContext.BaseDirectory}Content";
        MainAssetManager = new AssetManager(new AssetLoader(contentPath));
        Assets.RegisterAssetManager(MainAssetManager, "");

        imGuiRenderer = new ImGuiRenderer(this);

        base.Initialize();
        ModManager.EnqueueLoadEnabledMods();
    }

    /// <inheritdoc/>
    protected override void LoadContent()
    {
        MainAssetManager.LoadAssets();
        var style = new UntexturedStyle(Renderer.spriteBatch);
        style.Font = GlobalFonts.MenuFont;
        MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        // Meta-modules update (responsible to tracking time and pausing)
        Time.Update(gameTime, IsActive);
        GraphicsSettings.WindowPosition = Window.Position;
        if (GraphicsSettings.FocusLossBehaviour > OnFocusLossBehaviour.Eco && !IsActive) return;

        // Pre-update
        base.Update(gameTime);
        Input.Update();
        MainThread.Update();

        // Early update for modules that may block the normal update
        Assets.Update();

        if (Assets.IsLoading) return;
        if (ModManager.InProgress) return;
        //DevConsole.Update(); TODO (maybe a bit later - medium-low priority) dev console in-game w/ Console class support like in DD.

        // Normal update
        UpdateM();

        // Post update
        Input.PostUpdate();
    }

    public void UpdateLogicSystems()
    {
        LogicSystemRoot.Update(GetUpdateTick());
    }

    public void UpdateDrawSystems()
    {
        DrawSystemRoot.Update(GetUpdateTick());
    }

    public static UpdateTick GetUpdateTick()
    {
        return new UpdateTick(Time.RawDeltaTime, Time.RawTotalTime);
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        if (GraphicsSettings.FocusLossBehaviour > OnFocusLossBehaviour.Eco && !IsActive) return;
        GraphicsDevice.Clear(Color.Black);
        if (ModManager.InProgress)
        {
            GenericFont? font = GlobalFonts.MenuFont;
            float width = Window.ClientBounds.Width;
            float height = Window.ClientBounds.Height;
            int finished = ModManager.FinishedTasksThisCommand;
            int total = ModManager.TotalTasksThisCommand;

            Renderer.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
            font.DrawString(Renderer.spriteBatch, $"Loading mods: {finished}/{total}", Vector2.Zero, Color.White);
            ProgressBar.Draw((float)finished / total, new(width * 0.1f, height * 0.8f), new(width * 0.8f, height * 0.1f));
            Renderer.End();
            return;
        }

        if (Assets.IsLoading)
        {
            GenericFont? font = GlobalFonts.MenuFont;
            float width = Window.ClientBounds.Width;
            float height = Window.ClientBounds.Height;

            Renderer.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);

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
        base.Draw(gameTime);
        imGuiRenderer.BeforeLayout(Time.DeltaTime);
        DrawImGui();
        imGuiRenderer.AfterLayout();
    }

    public virtual void DrawImGui() { }

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