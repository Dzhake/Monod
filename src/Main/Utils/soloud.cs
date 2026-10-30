
// SoLoud wrapper for C# (cs)
// Copied from https://github.com/dungeonneko/Sharploud (MIT license), and improved (comments from the official docs, enums instead of just ints, LibraryImport, etc.)

using System.Runtime.InteropServices;

namespace SoLoud;

public class SoloudObject
{
    public IntPtr objhandle;
}

file class SoloudDllName
{
    public const string DllName = "soloud.dll";
}


public enum SoloudInitFlags : uint
{
    /// <summary>
    /// Use roundoff clipper. Without this flag the clipper defaults to "hard" clipping to -1/+1
    /// </summary>
    CLIP_ROUNDOFF = 1,
    /// <summary>
    /// Enable gathering of visualization data. Can be changed at runtime with setVisualizationEnable()
    /// </summary>
    ENABLE_VISUALIZATION = 2,
    /// <summary>
    /// Use left-handed (Direct3D) 3d coordinates. Default is right-handed (OpenGL) coordinates.
    /// </summary>
    LEFT_HANDED_3D = 4,
    /// <summary>
    /// Do not alter the FPU state in audio threads. By default, SoLoud uses "fast" fpu options.
    /// </summary>
    NO_FPU_REGISTER_CHANGE = 8,
}

public enum SoloudBackend
{
    /// <summary>
    /// Select backend automatically
    /// </summary>
    AUTO = 0,
    /// <summary>
    /// SDL1 dynamic or static linking
    /// </summary>
    SDL1 = 1,
    /// <summary>
    /// SDL2 dynamic or static linking
    /// </summary>
    SDL2 = 2,
    /// <summary>
    /// PortAudio
    /// </summary>
    PORTAUDIO = 3,
    /// <summary>
    /// Windows MultiMedia
    /// </summary>
    WINMM = 4,
    /// <summary>
    /// XAudio2
    /// </summary>
    XAUDIO2 = 5,
    /// <summary>
    /// Windows Audio Session API
    /// </summary>
    WASAPI = 6,
    /// <summary>
    /// Advanced Linux Sound Architecture
    /// </summary>
    ALSA = 7,
    /// <summary>
    /// JACK Audio Connection Kit
    /// </summary>
    JACK = 8,
    /// <summary>
    /// Open Sound System
    /// </summary>
    OSS = 9,
    /// <summary>
    /// OpenAL (high latency)
    /// </summary>
    OPENAL = 10,
    /// <summary>
    /// OSX CoreAudio
    /// </summary>
    COREAUDIO = 11,
    /// <summary>
    /// OpenSL ES
    /// </summary>
    OPENSLES = 12,
    /// <summary>
    /// Vita console homebrew SDK
    /// </summary>
    VITA_HOMEBREW = 13,
    /// <summary>
    /// MiniAudio
    /// </summary>
    MINIAUDIO = 14,
    /// <summary>
    /// No-sound driver
    /// </summary>
    NOSOUND = 15,
    /// <summary>
    /// Null driver
    /// </summary>
    NULLDRIVER = 16,
    /// <summary>
    /// Last enum value, represents the total amount of backends.
    /// </summary>
    BACKEND_MAX = 17,
}

/// <summary>
/// <para>In order to use SoLoud, you have to create a SoLoud::Soloud object. The object must be cleaned up or destroyed before your back-end is shut down; the safest way to do this is to call soloud.deinit() manually before terminating.</para>
/// <para>The object may be global, member variable, or even a local variable, it can be allocated from the heap or the stack, as long as the above demand is met. If the back-end gets destroyed before the back-end clean-up call is made, the result is undefined. As in, bad. Most likely, a crash. Blue screens in Windows are not out of the question.</para>
///
/// <code>
/// SoLoud::Soloud *soloud = new SoLoud::Soloud; // object created
/// soloud-&gt;init();             // back-end initialization
/// ...
/// soloud-&gt;deinit();           // clean-up
/// delete soloud;              // this cleans up too
/// </code>
///
/// <para>Seriously: remember to call the cleanup function. The SoLoud object destructor also calls the cleanup function, but if you perform your application's tear-down in an unpredictable order (such as having the SoLoud object be a global variable), the back-end may end up trying to use resources that are no longer available. So, it's best to call the cleanup function manually.</para>
/// </summary>
public partial class Soloud : SoloudObject
{
    /// <summary>
    /// Represents the 'default' value for the argument. Seriously, couldn't you just use default values?
    /// </summary>
    public const uint AUTO = 0;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Soloud_create();
    public Soloud()
    {
        objhandle = Soloud_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Soloud_destroy(IntPtr aObjHandle);
    ~Soloud()
    {
        Soloud_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_initEx(IntPtr aObjHandle, SoloudInitFlags aFlags, SoloudBackend aBackend, uint aSamplerate, uint aBufferSize, uint aChannels);
    /// <summary>
    /// <para>Initializes the SoLoud object. The function has several optional parameters you can use to adjust SoLoud's behavior. The default values should work for most cases.</para>
    ///
    /// <code>
    /// result init(unsigned int aFlags = Soloud::CLIP_ROUNDOFF,
    ///             unsigned int aBackend = Soloud::AUTO,
    ///             unsigned int aSamplerate = Soloud::AUTO,
    ///             unsigned int aBufferSize = Soloud::AUTO);
    /// </code>
    ///
    /// <para>By default SoLoud is initializes with the roundoff clipping enabled, and the rest of the parameters set to auto. SoLoud will then pick the backend it prefers, and its default parameters.</para>
    /// </summary>
    public int init(SoloudInitFlags aFlags = SoloudInitFlags.CLIP_ROUNDOFF, SoloudBackend aBackend = SoloudBackend.AUTO, uint aSamplerate = AUTO, uint aBufferSize = AUTO, uint aChannels = 2)
    {
        return Soloud_initEx(objhandle, aFlags, aBackend, aSamplerate, aBufferSize, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_deinit(IntPtr aObjHandle);
    /// <summary>
    /// <para>Shut down the SoLoud object.</para>
    ///
    /// <code>
    /// void deinit();
    /// </code>
    ///
    /// <para>Should be called before shutting down. While the shutdown probably will work out fine without calling this function, it is recommended that applications call the deinit function explicitly.</para>
    /// <para>If the cleanup is left for the executable teardown, it is possible that the backend gets shut down first, and then SoLoud's audio thread may try to call it afterwards, leading to undefined behavior.</para>
    /// </summary>
    public void deinit()
    {
        Soloud_deinit(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getVersion(IntPtr aObjHandle);
    /// <summary>
    /// <para>Returns the version of the SoLoud library. Same as SOLOUD_VERSION macro. Mostly useful when using the DLL, to check the DLL's library version.</para>
    ///
    /// <code>
    /// if (soloud.getVersion() != SOLOUD_VERSION)
    ///     panic();
    /// </code>
    /// </summary>
    public uint getVersion()
    {
        return Soloud_getVersion(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Soloud_getErrorString(IntPtr aObjHandle, int aErrorCode);
    /// <summary>
    /// <para>Converts SoLoud's error values to printable zero-terminated ascii strings.</para>
    ///
    /// <code>
    /// int err = mod.load("foo.mod")
    /// printf("Mod load:%s", soloud.getErrorString(err));
    /// </code>
    /// </summary>
    public string getErrorString(int aErrorCode)
    {
        IntPtr p = Soloud_getErrorString(objhandle, aErrorCode);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getBackendId(IntPtr aObjHandle);
    /// <summary>
    /// <para>Get the current backend ID. This is typically the same as the Soloud::BACKENDS enumeration, but for AUTO it will be set to whatever the auto-selected backend was, or in case of SDL2 dynamic linking, it will return SDL, as those two share the same backend.</para>
    ///
    /// <code>
    /// printf("Current backend id: %d", gSoloud.getBackendId());
    /// </code>
    /// </summary>
    public uint getBackendId()
    {
        return Soloud_getBackendId(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Soloud_getBackendString(IntPtr aObjHandle);
    /// <summary>
    /// <para>Get a printable asciiz string naming the current back-end. May return NULL.</para>
    ///
    /// <code>
    /// if (gSoloud.getBackendString())
    ///   printf("Current backend: %s", gSoloud.getBackendString());
    /// </code>
    /// </summary>
    public string getBackendString()
    {
        IntPtr p = Soloud_getBackendString(objhandle);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getBackendChannels(IntPtr aObjHandle);
    /// <summary>
    /// <para>Get the number of channels the currently active backend plays.</para>
    ///
    /// <code>
    /// printf("Current backend channels: %d", gSoloud.getBackendChannels());
    /// </code>
    ///
    /// <para>1 for mono, 2 for stereo, various others for surround speaker setups.</para>
    /// </summary>
    public uint getBackendChannels()
    {
        return Soloud_getBackendChannels(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getBackendSamplerate(IntPtr aObjHandle);
    /// <summary>
    /// <para>Get the samplerate, in Hz, of the currently active backend. This may differ from the one requested.</para>
    ///
    /// <code>
    /// printf("Current backend rate: %d", gSoloud.getBackendSamplerate());
    /// </code>
    /// </summary>
    public uint getBackendSamplerate()
    {
        return Soloud_getBackendSamplerate(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getBackendBufferSize(IntPtr aObjHandle);
    /// <summary>
    /// <para>Get the current backend's buffer size. May differ from the one requested. Some backends (such as PortAudio) may lie about this and use smaller buffer in reality.</para>
    ///
    /// <code>
    /// printf("Current backend buffer: %d", gSoloud.getBackendBufferSize());
    /// </code>
    /// </summary>
    public uint getBackendBufferSize()
    {
        return Soloud_getBackendBufferSize(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_setSpeakerPosition(IntPtr aObjHandle, uint aChannel, float aX, float aY, float aZ);
    /// <summary>
    /// <para>Get or set a speaker position in 3d space. Used to configure spakers in multi-speaker systems.</para>
    ///
    /// <code>
    /// float x,y,z;
    /// gSoloud.getSpeakerPosition(0, x, y, z); // get channel 0 speaker coordinates
    /// gSoloud.setSpeakerPosition(0, 1.0f, 2.0f, 3.0f); // set channel 0 to play from (1,2,3)
    /// </code>
    ///
    /// <para>For typical use these functions do not need to be called.</para>
    /// </summary>
    public int setSpeakerPosition(uint aChannel, float aX, float aY, float aZ)
    {
        return Soloud_setSpeakerPosition(objhandle, aChannel, aX, aY, aZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_getSpeakerPosition(IntPtr aObjHandle, uint aChannel, float[] aX, float[] aY, float[] aZ);
    /// <summary>
    /// <para>Get or set a speaker position in 3d space. Used to configure spakers in multi-speaker systems.</para>
    ///
    /// <code>
    /// float x,y,z;
    /// gSoloud.getSpeakerPosition(0, x, y, z); // get channel 0 speaker coordinates
    /// gSoloud.setSpeakerPosition(0, 1.0f, 2.0f, 3.0f); // set channel 0 to play from (1,2,3)
    /// </code>
    ///
    /// <para>For typical use these functions do not need to be called.</para>
    /// </summary>
    public int getSpeakerPosition(uint aChannel, float[] aX, float[] aY, float[] aZ)
    {
        return Soloud_getSpeakerPosition(objhandle, aChannel, aX, aY, aZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_playEx(IntPtr aObjHandle, IntPtr aSound, float aVolume, float aPan, int aPaused, uint aBus);
    /// <summary>
    /// <para>The play function can be used to start playing a sound source. The function has more than one parameter, with typical default values set to most of them.</para>
    ///
    /// <code>
    /// int play(AudioSource &amp;aSound,
    ///          float aVolume = 1.0f, // Full volume
    ///          float aPan = 0.0f,    // Centered
    ///          int aPaused = 0,      // Not paused
    ///          int aBus = 0);        // Primary bus
    /// </code>
    ///
    /// <para>Unless you know what you're doing, leave the aBus parameter to zero.</para>
    /// <para>The play function returns a channel handle which can be used to adjust the parameters of the sound while it's playing. The most common parameters can be set with the play function parameters, but for more complex processing you may want to start the sound paused, adjust the parameters, and then un-pause it.</para>
    ///
    /// <code>
    /// int h = soloud.play(sound, 1, 0, 1);  // start paused
    /// soloud.setRelativePlaySpeed(h, 0.8f); // change a parameter
    /// soloud.setPause(h, 0);                // unpause
    /// </code>
    /// </summary>
    public uint play(SoloudObject aSound, float aVolume = -1.0f, float aPan = 0.0f, int aPaused = 0, uint aBus = 0)
    {
        return Soloud_playEx(objhandle, aSound.objhandle, aVolume, aPan, aPaused, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_playClockedEx(IntPtr aObjHandle, double aSoundTime, IntPtr aSound, float aVolume, float aPan, uint aBus);
    /// <summary>
    /// <para>This is a variant of the play function that takes additional parameter, the time offset for the sound. While the vanilla play() tries to play sounds as soon as possible, the playClocked will delay the start of sounds so that rapidly launched sounds don't all get clumped to the start of the next outgoing sound buffer.</para>
    /// <para>Let's say we have a short sample and we want to play it repeatedly.</para>
    ///
    /// <para>If our play calls are rapid enough, several calls will hit the same audio buffer, and the effect simply amplifies the sound:</para>
    ///
    /// <para>If we use playClocked instead, SoLoud will notice that several calls are being made within one audio buffer, and will delay the later ones based on the time given as a parameter, resulting in what we wanted in the first place:</para>
    ///
    /// <para>The "pew pew" example gives an interactive and intuitive way of understanding how this function is used, and what problem it solves.</para>
    ///
    /// <code>
    /// t = time_from_game_engine();        // Game physics time
    /// int h = soloud.playClocked(t, pew); // Shoot!
    /// </code>
    ///
    /// <para>Apart from the delayed start, the playClocked() works exactly like the play() function, except that there's no way to start them in a paused state.</para>
    /// <para>The time parameter should be your game's "physics time", in seconds. SoLoud will then use that time (as well as the time you previously used) to calculate the delay between two sound effects. If a output sound buffer threshold is passed between the two sounds, SoLoud will adjust the delay accordingly.</para>
    /// <para>Note that if your "physics time" granularity is low, playClocked is not really useful. Audio buffers tend to be 40ms long at most.</para>
    /// </summary>
    public uint playClocked(double aSoundTime, SoloudObject aSound, float aVolume = -1.0f, float aPan = 0.0f, uint aBus = 0)
    {
        return Soloud_playClockedEx(objhandle, aSoundTime, aSound.objhandle, aVolume, aPan, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_play3dEx(IntPtr aObjHandle, IntPtr aSound, float aPosX, float aPosY, float aPosZ, float aVelX, float aVelY, float aVelZ, float aVolume, int aPaused, uint aBus);
    /// <summary>
    /// <para>play3d() is the 3d version of the play() call.</para>
    ///
    /// <code>
    /// handle play3d(AudioSource &amp;aSound,
    ///               float aPosX,
    ///               float aPosY,
    ///               float aPosZ,
    ///               float aVelX = 0.0f,
    ///               float aVelY = 0.0f,
    ///               float aVelZ = 0.0f,
    ///               float aVolume = 1.0f,
    ///               bool aPaused = 0,
    ///               unsigned int aBus = 0);
    ///
    /// </code>
    ///
    /// <para>Instead of panning like with the "2d" version of the call, the 3d version requires 3d position and optionally velocity vector. Like its 2d version, this one tries to launch the sound as soon as possible, which means the start of the next audio buffer.</para>
    /// <para>The play3d can also add the "distance delay" to the sound, if enabled.</para>
    ///
    /// <code>
    /// int h = gSoloud.play3d(west, -10, 0, 0);
    /// gSoloud.set3dSourceVelocity(h, -1, 0, 0); // go west
    /// </code>
    /// </summary>
    public uint play3d(SoloudObject aSound, float aPosX, float aPosY, float aPosZ, float aVelX = 0.0f, float aVelY = 0.0f, float aVelZ = 0.0f, float aVolume = 1.0f, int aPaused = 0, uint aBus = 0)
    {
        return Soloud_play3dEx(objhandle, aSound.objhandle, aPosX, aPosY, aPosZ, aVelX, aVelY, aVelZ, aVolume, aPaused, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_play3dClockedEx(IntPtr aObjHandle, double aSoundTime, IntPtr aSound, float aPosX, float aPosY, float aPosZ, float aVelX, float aVelY, float aVelZ, float aVolume, uint aBus);
    /// <summary>
    /// <para>play3dClocked() is the 3d version of the playClocked() call.</para>
    ///
    /// <code>
    /// handle play3d(time aSoundTime,
    ///               AudioSource &amp;aSound,
    ///               float aPosX,
    ///               float aPosY,
    ///               float aPosZ,
    ///               float aVelX = 0.0f,
    ///               float aVelY = 0.0f,
    ///               float aVelZ = 0.0f,
    ///               float aVolume = 1.0f,
    ///               unsigned int aBus = 0);
    ///
    /// </code>
    ///
    /// <para>Instead of panning like with the "2d" version of the call, the 3d version requires 3d position and optionally velocity vector. Like its 2d version, this one delays the start of the sound based on the aSoundTime parameter, so that firing off sounds rapidly won't cause the sounds to "clump" together at the start of the next sound buffer.</para>
    /// <para>The play3dClocked can also add the "distance delay" to the sound, if enabled.</para>
    ///
    /// <code>
    /// // trigger boom at specific coords
    /// gSoloud.play3dClocked(physicstime, boom, bx, by, bz);
    /// </code>
    /// </summary>
    public uint play3dClocked(double aSoundTime, SoloudObject aSound, float aPosX, float aPosY, float aPosZ, float aVelX = 0.0f, float aVelY = 0.0f, float aVelZ = 0.0f, float aVolume = 1.0f, uint aBus = 0)
    {
        return Soloud_play3dClockedEx(objhandle, aSoundTime, aSound.objhandle, aPosX, aPosY, aPosZ, aVelX, aVelY, aVelZ, aVolume, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_playBackgroundEx(IntPtr aObjHandle, IntPtr aSound, float aVolume, int aPaused, uint aBus);
    /// <summary>
    /// <para>The playBackground() function can be used to play sounds without panning, primarily meant for background music.</para>
    ///
    /// <code>
    ///     handle playBackground(AudioSource &amp;aSound,
    ///                           float aVolume = -1.0f,
    ///                           bool aPaused = 0,
    ///                           unsigned int aBus = 0);
    /// </code>
    ///
    /// <para>It's a convenience function, and is equivalent to play() followed by setPanAbsolute() to set left and right channels to full volme.</para>
    ///
    /// <code>
    /// gSoloud.playBackground(gDramaticScore);
    /// </code>
    /// </summary>
    public uint playBackground(SoloudObject aSound, float aVolume = -1.0f, int aPaused = 0, uint aBus = 0)
    {
        return Soloud_playBackgroundEx(objhandle, aSound.objhandle, aVolume, aPaused, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_seek(IntPtr aObjHandle, uint aVoiceHandle, double aSeconds);
    /// <summary>
    /// <para>You can seek to a specific time in the sound with the seek function. Note that the seek operation may be rather heavy, and some audio sources will not support seeking backwards at all.</para>
    ///
    /// <code>
    /// int h = soloud.play(sound, 1, 0, 1); // start paused
    /// soloud.seek(h, 3.8f);                // seek to 3.8 seconds
    /// soloud.setPause(h, 0);               // unpause
    /// </code>
    /// </summary>
    public int seek(uint aVoiceHandle, double aSeconds)
    {
        return Soloud_seek(objhandle, aVoiceHandle, aSeconds);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_stop(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>The stop function can be used to stop a sound.</para>
    ///
    /// <code>
    /// soloud.stop(h); // Silence!
    /// </code>
    /// </summary>
    public void stop(uint aVoiceHandle)
    {
        Soloud_stop(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_stopAll(IntPtr aObjHandle);
    /// <summary>
    /// <para>The stop function can be used to stop all sounds. Note that this will also stop the protected sounds.</para>
    ///
    /// <code>
    /// soloud.stopAll(); // Total silence!
    /// </code>
    /// </summary>
    public void stopAll()
    {
        Soloud_stopAll(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_stopAudioSource(IntPtr aObjHandle, IntPtr aSound);
    /// <summary>
    /// <para>The stop function can be used to stop all sounds that were started through a certain sound source. Will also stop protected sounds.</para>
    ///
    /// <code>
    /// soloud.stopAudioSource(duck); // silence all the ducks
    /// </code>
    /// </summary>
    public void stopAudioSource(SoloudObject aSound)
    {
        Soloud_stopAudioSource(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_countAudioSource(IntPtr aObjHandle, IntPtr aSound);
    /// <summary>
    /// <para>Returns the number of concurrent soudns that are playing a specific audio source.</para>
    ///
    /// <code>
    /// if (soloud.countAudioSource(cheer) == 3) three_cheers();
    /// </code>
    /// </summary>
    public int countAudioSource(SoloudObject aSound)
    {
        return Soloud_countAudioSource(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setFilterParameter(IntPtr aObjHandle, uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aValue);
    /// <summary>
    /// <para>Sets a parameter for a live instance of a filter. The filter must support changing of live parameters; otherwise this call does nothing.</para>
    ///
    /// <code>
    /// soloud.setFilterParameter(h, 3, FILTER::CUTOFF, 1000);
    /// // set h's 3rd filter's "cutoff" value to 1000
    /// </code>
    /// </summary>
    public void setFilterParameter(uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aValue)
    {
        Soloud_setFilterParameter(objhandle, aVoiceHandle, aFilterId, aAttributeId, aValue);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getFilterParameter(IntPtr aObjHandle, uint aVoiceHandle, uint aFilterId, uint aAttributeId);
    /// <summary>
    /// <para>Gets a parameter from a live instance of a filter. The filter must support changing of live parameters; otherwise this call returns zero.</para>
    ///
    /// <code>
    /// float v = soloud.getFilterParameter(h,3,FILTER::CUTOFF);
    /// // get h's 3rd filter's "cutoff" value
    /// </code>
    /// </summary>
    public float getFilterParameter(uint aVoiceHandle, uint aFilterId, uint aAttributeId)
    {
        return Soloud_getFilterParameter(objhandle, aVoiceHandle, aFilterId, aAttributeId);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_fadeFilterParameter(IntPtr aObjHandle, uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aTo, double aTime);
    /// <summary>
    /// <para>Fades a parameter on a live instance of a filter. The filter must support changing of live parameters; otherwise this call does nothing.</para>
    ///
    /// <code>
    /// soloud.fadeFilterParameter(h,3,FILTER::CUTOFF,1000,1);
    /// // Fades h's 3rd filter CUTOFF to 1000 in 1 second
    /// </code>
    /// </summary>
    public void fadeFilterParameter(uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aTo, double aTime)
    {
        Soloud_fadeFilterParameter(objhandle, aVoiceHandle, aFilterId, aAttributeId, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_oscillateFilterParameter(IntPtr aObjHandle, uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aFrom, float aTo, double aTime);
    /// <summary>
    /// <para>Oscillates a parameter on a live instance of a filter. The filter must support changing of live parameters; otherwise this call does nothing.</para>
    ///
    /// <code>
    /// soloud.setFilterParameter(h,3,FILTER::CUTOFF,500,1000,2);
    /// // Oscillates the h's 3rd filter's CUTOFF between 500 and 1000
    /// </code>
    /// </summary>
    public void oscillateFilterParameter(uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aFrom, float aTo, double aTime)
    {
        Soloud_oscillateFilterParameter(objhandle, aVoiceHandle, aFilterId, aAttributeId, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Soloud_getStreamTime(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>The getStreamTime function can be used to get the current play position, in seconds.</para>
    ///
    /// <code>
    /// double t = soloud.getStreamTime(h); // get time
    /// if (t == hammertime) hammer();
    /// </code>
    ///
    /// <para>Note that time is a double instead of float because float will cause precision problems within 24 hours of playing, and eventually, in about 6 days, will cause the "time" to stop.</para>
    /// <para>Also note that the granularity is likely to be rather high (possibly around 45ms), so using this as the sole clock source for animation will lead to rather low framerate (possibly around 20Hz). To fix this, either use some other clock source and only sync with the stream time occasionally, or use some kind of low-pass filter, such as..</para>
    ///
    /// <code>
    /// mytime = (mytime * 9 + soloud.getStreamTime(h)) / 10;
    /// </code>
    ///
    /// <para>While not perfect, that's way better than using the stream time directly.</para>
    /// </summary>
    public double getStreamTime(uint aVoiceHandle)
    {
        return Soloud_getStreamTime(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Soloud_getStreamPosition(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>Gets the stream position of a specific voice handle.</para>
    ///
    /// <code>
    /// if (soloud.getStreamPosition(music) &gt; 30) past_the_good_bit();
    /// </code>
    /// </summary>
    public double getStreamPosition(uint aVoiceHandle)
    {
        return Soloud_getStreamPosition(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_getPause(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>The setPause function can be used to pause, or unpause, a sound.</para>
    ///
    /// <code>
    /// if (soloud.getPause(h)) hum_silently();
    /// soloud.setPause(h, 0); // resumes playback
    /// </code>
    ///
    /// <para>Note that even if a sound is paused, its channel may be taken over. Trying to resume a sound that's no longer in a channel doesn't do anything.</para>
    /// <para>If the handle is invalid, the getPause will return 0.</para>
    /// </summary>
    public int getPause(uint aVoiceHandle)
    {
        return Soloud_getPause(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getVolume(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's current volume setting.</para>
    ///
    /// <code>
    /// float v = soloud.getVolume(h); // Get current volume
    /// soloud.setVolume(h, v * 2);    // Double it
    /// </code>
    ///
    /// <para>Note that the volume is the "volume setting", and the actual volume will depend on the sound source. Namely, a whisper will most likely be more quiet than a scream, even if both are played at the same volume setting.</para>
    /// <para>If an invalid handle is given to getVolume, it will return 0.</para>
    /// </summary>
    public float getVolume(uint aVoiceHandle)
    {
        return Soloud_getVolume(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getOverallVolume(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>For 3d sounds, returns the current volume based on the audio sources' set volume and the calculated 3d volume.</para>
    ///
    /// <code>
    /// float v = soloud.getOverallVolume(foghorn);
    /// if (v &lt; 0.1) foghornTooFarToHear();
    /// </code>
    /// </summary>
    public float getOverallVolume(uint aVoiceHandle)
    {
        return Soloud_getOverallVolume(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getPan(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's current pan setting.</para>
    ///
    /// <code>
    /// float v = soloud.getPan(h); // Get current pan
    /// soloud.setPan(h, v - 0.1);  // Little bit to the left
    /// </code>
    ///
    /// <para>The range of the pan values is -1 to 1, where -1 is left, 0 is middle and and 1 is right. Setting value outside this range may cause undefined behavior.</para>
    /// <para>SoLoud calculates the left/right volumes from the pan to keep a constant volume; to set the volumes directly, use setPanAbsolute.</para>
    /// <para>If an invalid handle is given to getPan, it will return 0.</para>
    /// </summary>
    public float getPan(uint aVoiceHandle)
    {
        return Soloud_getPan(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getSamplerate(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's base sample rate.</para>
    ///
    /// <code>
    /// float v = soloud.getSamplerate(h); // Get the base sample rate
    /// soloud.setSamplerate(h, v * 2);    // Double it
    /// </code>
    ///
    /// <para>Setting the value to 0 will cause undefined behavior, likely a crash.</para>
    /// <para>To adjust the play speed, while leaving the base sample rate alone, use setRelativePlaySpeed instead.</para>
    /// <para>If an invalid handle is given to getSamplerate, it will return 0.</para>
    /// </summary>
    public float getSamplerate(uint aVoiceHandle)
    {
        return Soloud_getSamplerate(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_getProtectVoice(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's protection state.</para>
    ///
    /// <code>
    /// int v = soloud.getProtectVoice(h);   // Get the protection state
    /// if (v) soloud.setProtectVoice(h, 0); // Disable if protected
    /// </code>
    ///
    /// <para>Normally, if you try to play more sounds than there are voices, SoLoud will kill off the oldest playing sound to make room. This will most likely be your background music. This can be worked around by protecting the sound.</para>
    /// <para>If all voices are protected, the result will be undefined.</para>
    /// <para>If an invalid handle is given to getProtectChannel, it will return 0.</para>
    /// </summary>
    public int getProtectVoice(uint aVoiceHandle)
    {
        return Soloud_getProtectVoice(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getActiveVoiceCount(IntPtr aObjHandle);
    /// <summary>
    /// <para>Returns the number of concurrent sounds that are playing at the moment.</para>
    ///
    /// <code>
    /// if (soloud.getActiveVoiceCount() == 0) enjoy_the_silence();
    /// </code>
    /// </summary>
    public uint getActiveVoiceCount()
    {
        return Soloud_getActiveVoiceCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getVoiceCount(IntPtr aObjHandle);
    /// <summary>
    /// <para>Returns the number of voices the application has told SoLoud to play.</para>
    ///
    /// <code>
    /// if (soloud.getVoiceCount() &gt; 1000) lots_of_voices();
    /// </code>
    /// </summary>
    public uint getVoiceCount()
    {
        return Soloud_getVoiceCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_isValidVoiceHandle(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>The isValidVoiceHandle function can be used to check if a handle is still valid.</para>
    ///
    /// <code>
    /// if (!soloud.isValidVoiceHandle(h)) delete foobar;
    /// </code>
    ///
    /// <para>If the handle is invalid, the isValidVoiceHandle will return 0.</para>
    /// </summary>
    public int isValidVoiceHandle(uint aVoiceHandle)
    {
        return Soloud_isValidVoiceHandle(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getRelativePlaySpeed(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's relative play speed.</para>
    ///
    /// <code>
    /// float v = soloud.getRelativePlaySpeed(h); // Get relative play speed
    /// soloud.setRelativePlaySpeed(h, v * 0.5f); // Halve it
    /// </code>
    ///
    /// <para>Setting the value to 0 will cause undefined behavior, likely a crash.</para>
    /// <para>Change the relative play speed of a sample. This changes the effective sample rate while leaving the base sample rate alone.</para>
    /// <para>Note that playing a sound at a higher sample rate will require SoLoud to request more samples from the sound source, which will require more memory and more processing power. Playing at a slower sample rate is cheaper.</para>
    /// <para>If an invalid handle is given to getRelativePlaySpeed, it will return 1.</para>
    /// </summary>
    public float getRelativePlaySpeed(uint aVoiceHandle)
    {
        return Soloud_getRelativePlaySpeed(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getPostClipScaler(IntPtr aObjHandle);
    /// <summary>
    /// <para>These functions can be used to get and set the post-clip scaler. The scaler is applied after clipping. Sometimes lowering the post-clip result sound volume may be beneficial. For instance, recording video with some video capture software results in distorted sound if the volume is too high.</para>
    ///
    /// <code>
    /// float v = soloud.getPostClipScaler(); // get the current post-clip scaler
    /// soloud.setPostClipScaler(v * 0.5f);   // halve it
    /// </code>
    ///
    /// <para>Note that the scale is not limited to 0..1 range. Negative values may result in strange behavior, while huge values will likely cause distortion.</para>
    /// </summary>
    public float getPostClipScaler()
    {
        return Soloud_getPostClipScaler(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getGlobalVolume(IntPtr aObjHandle);
    /// <summary>
    /// <para>These functions can be used to get and set the global volume. The volume is applied before clipping. Lowering the global volume is one way to combat clipping artifacts.</para>
    ///
    /// <code>
    /// float v = soloud.getGlobalVolume(); // get the current global volume
    /// soloud.setGlobalVolume(v * 0.5f);   // halve it
    /// </code>
    ///
    /// <para>Note that the volume is not limited to 0..1 range. Negative values may result in strange behavior, while huge values will likely cause distortion.</para>
    /// </summary>
    public float getGlobalVolume()
    {
        return Soloud_getGlobalVolume(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getMaxActiveVoiceCount(IntPtr aObjHandle);
    /// <summary>
    /// <para>Get or set the current maximum active voice count. If voice count is higher than the maximum active voice count, SoLoud will pick the ones with the highest volume to actually play.</para>
    ///
    /// <code>
    /// int voices = gSoloud.getMaxActiveVoiceCount();
    /// if (fps &lt; 60 &amp;&amp; voices &gt; 16)
    ///     gSoloud.setMaxActiveVoiceCount(voices / 2);
    /// </code>
    /// </summary>
    public uint getMaxActiveVoiceCount()
    {
        return Soloud_getMaxActiveVoiceCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_getLooping(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>Get or set the looping behavior of a live sound.</para>
    ///
    /// <code>
    /// bool l = soloud.getLooping(h);
    /// soloud.setLooping(h, !l); // anti-loop
    /// </code>
    /// </summary>
    public int getLooping(uint aVoiceHandle)
    {
        return Soloud_getLooping(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Soloud_getLoopPoint(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = soloud.getLoopPoint(h);
    /// snd.setLoopPoint(h, t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(h, 0); // loop from start
    /// </code>
    /// </summary>
    public double getLoopPoint(uint aVoiceHandle)
    {
        return Soloud_getLoopPoint(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setLoopPoint(IntPtr aObjHandle, uint aVoiceHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = soloud.getLoopPoint(h);
    /// snd.setLoopPoint(h, t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(h, 0); // loop from start
    /// </code>
    /// </summary>
    public void setLoopPoint(uint aVoiceHandle, double aLoopPoint)
    {
        Soloud_setLoopPoint(objhandle, aVoiceHandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setLooping(IntPtr aObjHandle, uint aVoiceHandle, int aLooping);
    /// <summary>
    /// <para>Get or set the looping behavior of a live sound.</para>
    ///
    /// <code>
    /// bool l = soloud.getLooping(h);
    /// soloud.setLooping(h, !l); // anti-loop
    /// </code>
    /// </summary>
    public void setLooping(uint aVoiceHandle, int aLooping)
    {
        Soloud_setLooping(objhandle, aVoiceHandle, aLooping);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_setMaxActiveVoiceCount(IntPtr aObjHandle, uint aVoiceCount);
    /// <summary>
    /// <para>Get or set the current maximum active voice count. If voice count is higher than the maximum active voice count, SoLoud will pick the ones with the highest volume to actually play.</para>
    ///
    /// <code>
    /// int voices = gSoloud.getMaxActiveVoiceCount();
    /// if (fps &lt; 60 &amp;&amp; voices &gt; 16)
    ///     gSoloud.setMaxActiveVoiceCount(voices / 2);
    /// </code>
    /// </summary>
    public int setMaxActiveVoiceCount(uint aVoiceCount)
    {
        return Soloud_setMaxActiveVoiceCount(objhandle, aVoiceCount);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setInaudibleBehavior(IntPtr aObjHandle, uint aVoiceHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of a live sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // The dictator's speech must go on even if not heard
    /// soloud.setInaudibleBehavior(h, true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(uint aVoiceHandle, int aMustTick, int aKill)
    {
        Soloud_setInaudibleBehavior(objhandle, aVoiceHandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setGlobalVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>These functions can be used to get and set the global volume. The volume is applied before clipping. Lowering the global volume is one way to combat clipping artifacts.</para>
    ///
    /// <code>
    /// float v = soloud.getGlobalVolume(); // get the current global volume
    /// soloud.setGlobalVolume(v * 0.5f);   // halve it
    /// </code>
    ///
    /// <para>Note that the volume is not limited to 0..1 range. Negative values may result in strange behavior, while huge values will likely cause distortion.</para>
    /// </summary>
    public void setGlobalVolume(float aVolume)
    {
        Soloud_setGlobalVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setPostClipScaler(IntPtr aObjHandle, float aScaler);
    /// <summary>
    /// <para>These functions can be used to get and set the post-clip scaler. The scaler is applied after clipping. Sometimes lowering the post-clip result sound volume may be beneficial. For instance, recording video with some video capture software results in distorted sound if the volume is too high.</para>
    ///
    /// <code>
    /// float v = soloud.getPostClipScaler(); // get the current post-clip scaler
    /// soloud.setPostClipScaler(v * 0.5f);   // halve it
    /// </code>
    ///
    /// <para>Note that the scale is not limited to 0..1 range. Negative values may result in strange behavior, while huge values will likely cause distortion.</para>
    /// </summary>
    public void setPostClipScaler(float aScaler)
    {
        Soloud_setPostClipScaler(objhandle, aScaler);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setPause(IntPtr aObjHandle, uint aVoiceHandle, int aPause);
    /// <summary>
    /// <para>The setPause function can be used to pause, or unpause, a sound.</para>
    ///
    /// <code>
    /// if (soloud.getPause(h)) hum_silently();
    /// soloud.setPause(h, 0); // resumes playback
    /// </code>
    ///
    /// <para>Note that even if a sound is paused, its channel may be taken over. Trying to resume a sound that's no longer in a channel doesn't do anything.</para>
    /// <para>If the handle is invalid, the getPause will return 0.</para>
    /// </summary>
    public void setPause(uint aVoiceHandle, int aPause)
    {
        Soloud_setPause(objhandle, aVoiceHandle, aPause);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setPauseAll(IntPtr aObjHandle, int aPause);
    /// <summary>
    /// <para>The setPauseAll function can be used to pause, or unpause, all sounds.</para>
    ///
    /// <code>
    /// soloud.setPauseAll(0); // resumes playback of all channels
    /// </code>
    ///
    /// <para>Note that this function will overwrite the pause state of all channels at once. If your game uses this to pause/unpause the sound while the game is paused, do note that it will also pause/unpause any sounds that you may have paused/unpaused separately.</para>
    /// </summary>
    public void setPauseAll(int aPause)
    {
        Soloud_setPauseAll(objhandle, aPause);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_setRelativePlaySpeed(IntPtr aObjHandle, uint aVoiceHandle, float aSpeed);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's relative play speed.</para>
    ///
    /// <code>
    /// float v = soloud.getRelativePlaySpeed(h); // Get relative play speed
    /// soloud.setRelativePlaySpeed(h, v * 0.5f); // Halve it
    /// </code>
    ///
    /// <para>Setting the value to 0 will cause undefined behavior, likely a crash.</para>
    /// <para>Change the relative play speed of a sample. This changes the effective sample rate while leaving the base sample rate alone.</para>
    /// <para>Note that playing a sound at a higher sample rate will require SoLoud to request more samples from the sound source, which will require more memory and more processing power. Playing at a slower sample rate is cheaper.</para>
    /// <para>If an invalid handle is given to getRelativePlaySpeed, it will return 1.</para>
    /// </summary>
    public int setRelativePlaySpeed(uint aVoiceHandle, float aSpeed)
    {
        return Soloud_setRelativePlaySpeed(objhandle, aVoiceHandle, aSpeed);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setProtectVoice(IntPtr aObjHandle, uint aVoiceHandle, int aProtect);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's protection state.</para>
    ///
    /// <code>
    /// int v = soloud.getProtectVoice(h);   // Get the protection state
    /// if (v) soloud.setProtectVoice(h, 0); // Disable if protected
    /// </code>
    ///
    /// <para>Normally, if you try to play more sounds than there are voices, SoLoud will kill off the oldest playing sound to make room. This will most likely be your background music. This can be worked around by protecting the sound.</para>
    /// <para>If all voices are protected, the result will be undefined.</para>
    /// <para>If an invalid handle is given to getProtectChannel, it will return 0.</para>
    /// </summary>
    public void setProtectVoice(uint aVoiceHandle, int aProtect)
    {
        Soloud_setProtectVoice(objhandle, aVoiceHandle, aProtect);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setSamplerate(IntPtr aObjHandle, uint aVoiceHandle, float aSamplerate);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's base sample rate.</para>
    ///
    /// <code>
    /// float v = soloud.getSamplerate(h); // Get the base sample rate
    /// soloud.setSamplerate(h, v * 2);    // Double it
    /// </code>
    ///
    /// <para>Setting the value to 0 will cause undefined behavior, likely a crash.</para>
    /// <para>To adjust the play speed, while leaving the base sample rate alone, use setRelativePlaySpeed instead.</para>
    /// <para>If an invalid handle is given to getSamplerate, it will return 0.</para>
    /// </summary>
    public void setSamplerate(uint aVoiceHandle, float aSamplerate)
    {
        Soloud_setSamplerate(objhandle, aVoiceHandle, aSamplerate);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setPan(IntPtr aObjHandle, uint aVoiceHandle, float aPan);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's current pan setting.</para>
    ///
    /// <code>
    /// float v = soloud.getPan(h); // Get current pan
    /// soloud.setPan(h, v - 0.1);  // Little bit to the left
    /// </code>
    ///
    /// <para>The range of the pan values is -1 to 1, where -1 is left, 0 is middle and and 1 is right. Setting value outside this range may cause undefined behavior.</para>
    /// <para>SoLoud calculates the left/right volumes from the pan to keep a constant volume; to set the volumes directly, use setPanAbsolute.</para>
    /// <para>If an invalid handle is given to getPan, it will return 0.</para>
    /// </summary>
    public void setPan(uint aVoiceHandle, float aPan)
    {
        Soloud_setPan(objhandle, aVoiceHandle, aPan);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setPanAbsoluteEx(IntPtr aObjHandle, uint aVoiceHandle, float aLVolume, float aRVolume, float aLBVolume, float aRBVolume, float aCVolume, float aSVolume);
    /// <summary>
    /// <para>These function can be used to set the left/right volumes directly.</para>
    ///
    /// <code>
    /// soloud.setPanAbsolute(h, 1, 1); // full blast
    /// </code>
    ///
    /// <para>Note that this does not affect the value returned by getPan.</para>
    /// <para>If an invalid handle is given to getPan, it will return 0.</para>
    /// </summary>
    public void setPanAbsolute(uint aVoiceHandle, float aLVolume, float aRVolume, float aLBVolume = 0, float aRBVolume = 0, float aCVolume = 0, float aSVolume = 0)
    {
        Soloud_setPanAbsoluteEx(objhandle, aVoiceHandle, aLVolume, aRVolume, aLBVolume, aRBVolume, aCVolume, aSVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setVolume(IntPtr aObjHandle, uint aVoiceHandle, float aVolume);
    /// <summary>
    /// <para>These functions can be used to get and set a sound's current volume setting.</para>
    ///
    /// <code>
    /// float v = soloud.getVolume(h); // Get current volume
    /// soloud.setVolume(h, v * 2);    // Double it
    /// </code>
    ///
    /// <para>Note that the volume is the "volume setting", and the actual volume will depend on the sound source. Namely, a whisper will most likely be more quiet than a scream, even if both are played at the same volume setting.</para>
    /// <para>If an invalid handle is given to getVolume, it will return 0.</para>
    /// </summary>
    public void setVolume(uint aVoiceHandle, float aVolume)
    {
        Soloud_setVolume(objhandle, aVoiceHandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setDelaySamples(IntPtr aObjHandle, uint aVoiceHandle, uint aSamples);
    /// <summary>
    /// <para>Sets number of samples to delay before starting to play a sound. This is used privately by the playClocked() function. In the unlikely event that you may want to use it manually, it's available in the public API.</para>
    ///
    /// <code>
    /// h = soloud.play(snd, 1, 0, 1); // start paused
    /// soloud.setDelaySamples(h, 44100); // delay for a second
    /// soloud.setPause(h, 0); // go
    /// </code>
    ///
    /// <para>Calling this on a "live" voice will cause silence to be inserted at the start of the next audio buffer. Since this is rather unpredictable (as audio buffer sizes may vary), it's not recommended, even if it may be a rather funky effect..</para>
    /// </summary>
    public void setDelaySamples(uint aVoiceHandle, uint aSamples)
    {
        Soloud_setDelaySamples(objhandle, aVoiceHandle, aSamples);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_fadeVolume(IntPtr aObjHandle, uint aVoiceHandle, float aTo, double aTime);
    /// <summary>
    /// <para>Smoothly change a channel's volume over specified time.</para>
    ///
    /// <code>
    /// soloud.fadeVolume(orchestra, 1, 60); // The orchestra creeps in for a minute
    /// </code>
    ///
    /// <para>The fader is disabled if you change the channel's volume with setVolume()</para>
    /// </summary>
    public void fadeVolume(uint aVoiceHandle, float aTo, double aTime)
    {
        Soloud_fadeVolume(objhandle, aVoiceHandle, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_fadePan(IntPtr aObjHandle, uint aVoiceHandle, float aTo, double aTime);
    /// <summary>
    /// <para>Smoothly change a channel's pan setting over specified time.</para>
    ///
    /// <code>
    /// soloud.setPan(racecar, -1); // set start value
    /// soloud.fadePan(racecar, 1, 0.5); // Swoosh!
    /// </code>
    ///
    /// <para>The fader is disabled if you change the channel's panning with setPan() or setPanAbsolute()</para>
    /// </summary>
    public void fadePan(uint aVoiceHandle, float aTo, double aTime)
    {
        Soloud_fadePan(objhandle, aVoiceHandle, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_fadeRelativePlaySpeed(IntPtr aObjHandle, uint aVoiceHandle, float aTo, double aTime);
    /// <summary>
    /// <para>Smoothly change a channel's relative play speed over specified time.</para>
    ///
    /// <code>
    /// soloud.fadeRelativePlaySpeed(hal, 0.1, 6); // Hal's message slows down
    /// </code>
    ///
    /// <para>The fader is disabled if you change the channel's play speed with setRelativePlaySpeed()</para>
    /// </summary>
    public void fadeRelativePlaySpeed(uint aVoiceHandle, float aTo, double aTime)
    {
        Soloud_fadeRelativePlaySpeed(objhandle, aVoiceHandle, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_fadeGlobalVolume(IntPtr aObjHandle, float aTo, double aTime);
    /// <summary>
    /// <para>Smoothly change the global volume over specified time.</para>
    ///
    /// <code>
    /// soloud.fadeGlobalVolume(0, 2); // Fade everything out in 2 seconds
    /// </code>
    ///
    /// <para>The fader is disabled if you change the global volume with setGlobalVolume()</para>
    /// </summary>
    public void fadeGlobalVolume(float aTo, double aTime)
    {
        Soloud_fadeGlobalVolume(objhandle, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_schedulePause(IntPtr aObjHandle, uint aVoiceHandle, double aTime);
    /// <summary>
    /// <para>After specified time, pause the channel</para>
    ///
    /// <code>
    /// soloud.fadeVolume(jukebox, 0, 2); // Fade out the music in 2 seconds
    /// soloud.schedulePause(jukebox, 2);    // Pause the music after 2 seconds
    /// </code>
    ///
    /// <para>The scheduler is disabled if you set the pause state with setPause() or setPauseAll().</para>
    /// </summary>
    public void schedulePause(uint aVoiceHandle, double aTime)
    {
        Soloud_schedulePause(objhandle, aVoiceHandle, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_scheduleStop(IntPtr aObjHandle, uint aVoiceHandle, double aTime);
    /// <summary>
    /// <para>After specified time, stop the channel</para>
    ///
    /// <code>
    /// soloud.fadeVolume(applause, 0, 10); // Fade out the cheers for 10 seconds
    /// soloud.scheduleStop(applause, 10);     // Stop the sound after 10 seconds
    /// </code>
    ///
    /// <para>There's no way (currently) to disable this scheduler.</para>
    /// </summary>
    public void scheduleStop(uint aVoiceHandle, double aTime)
    {
        Soloud_scheduleStop(objhandle, aVoiceHandle, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_oscillateVolume(IntPtr aObjHandle, uint aVoiceHandle, float aFrom, float aTo, double aTime);
    /// <summary>
    /// <para>Set fader to oscillate the volume at specified frequency.</para>
    ///
    /// <code>
    /// soloud.oscillateVolume(murmur, 0, 0.2, 5); // murmur comes and goes
    /// </code>
    ///
    /// <para>The fader is disabled if you change the channel's volume with setVolume()</para>
    /// </summary>
    public void oscillateVolume(uint aVoiceHandle, float aFrom, float aTo, double aTime)
    {
        Soloud_oscillateVolume(objhandle, aVoiceHandle, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_oscillatePan(IntPtr aObjHandle, uint aVoiceHandle, float aFrom, float aTo, double aTime);
    /// <summary>
    /// <para>Set fader to oscillate the panning at specified frequency.</para>
    ///
    /// <code>
    /// soloud.oscillatePan(ambulance, -1, 1, 10); // Round and round it goes
    /// </code>
    ///
    /// <para>The fader is disabled if you change the channel's panning with setPan() or setPanAbsolute()</para>
    /// </summary>
    public void oscillatePan(uint aVoiceHandle, float aFrom, float aTo, double aTime)
    {
        Soloud_oscillatePan(objhandle, aVoiceHandle, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_oscillateRelativePlaySpeed(IntPtr aObjHandle, uint aVoiceHandle, float aFrom, float aTo, double aTime);
    /// <summary>
    /// <para>Set fader to oscillate the relative play speed at specified frequency.</para>
    ///
    /// <code>
    /// soloud.oscillateRelativePlaySpeed(vinyl, 0.9, 1.1, 3); // Wobbly record
    /// </code>
    ///
    /// <para>The fader is disabled if you change the channel's play speed with setRelativePlaySpeed()</para>
    /// </summary>
    public void oscillateRelativePlaySpeed(uint aVoiceHandle, float aFrom, float aTo, double aTime)
    {
        Soloud_oscillateRelativePlaySpeed(objhandle, aVoiceHandle, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_oscillateGlobalVolume(IntPtr aObjHandle, float aFrom, float aTo, double aTime);
    /// <summary>
    /// <para>Set fader to oscillate the global volume at specified frequency.</para>
    ///
    /// <code>
    /// soloud.oscillateGlobalVolume(0.5, 1.0, 0.2); // Go crazy
    /// </code>
    ///
    /// <para>The fader is disabled if you change the global volume with setGlobalVolume()</para>
    /// </summary>
    public void oscillateGlobalVolume(float aFrom, float aTo, double aTime)
    {
        Soloud_oscillateGlobalVolume(objhandle, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setGlobalFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>Sets, or clears, the global filter.</para>
    ///
    /// <code>
    /// soloud.setGlobalFilter(0, &amp;echochamber); // set first filter
    /// </code>
    ///
    /// <para>Setting the global filter to NULL will clear the global filter. The default maximum number of global filters active is 4, but this can be changed in a global constant in soloud.h (and rebuilding SoLoud).</para>
    /// </summary>
    public void setGlobalFilter(uint aFilterId, SoloudObject aFilter)
    {
        Soloud_setGlobalFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_setVisualizationEnable(IntPtr aObjHandle, int aEnable);
    public void setVisualizationEnable(int aEnable)
    {
        Soloud_setVisualizationEnable(objhandle, aEnable);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Soloud_calcFFT(IntPtr aObjHandle);
    /// <summary>
    /// <para>Calculates FFT of the currently playing sound (post-clipping) and returns a pointer to the result.</para>
    ///
    /// <code>
    /// float * fft = soloud.calcFFT();
    /// int i;
    /// for (i = 0; i &lt; 256; i++)
    ///   drawline(0, i, fft[i] * 32, i);
    /// </code>
    ///
    /// <para>The FFT data has 256 floats, from low to high frequencies.</para>
    /// <para>SoLoud performs a mono mix of the audio, passes it to FFT, and then calculates the magnitude of the complex numbers for application to use. For more advanced FFT use, SoLoud code changes are needed.</para>
    /// <para>The returned pointer points at a buffer that's always around, but the data is only updated when calcFFT() is called.</para>
    /// <para>For the FFT to work, you also need to initialize SoLoud with the Soloud::ENABLE_VISUALIZATION flag, or by enabling visualization with the Soloud.setVisualizationEnable() call. Otherwise the source data for the FFT calculation will not be gathered.</para>
    /// </summary>
    public float[] calcFFT()
    {
        float[] ret = new float[256];
        IntPtr p = Soloud_calcFFT(objhandle);

        byte[] buffer = new byte[4];
        for (int i = 0; i < ret.Length; ++i)
        {
            int f_bits = Marshal.ReadInt32(p, i * 4);
            buffer[0] = (byte)((f_bits >> 0) & 0xff);
            buffer[1] = (byte)((f_bits >> 8) & 0xff);
            buffer[2] = (byte)((f_bits >> 16) & 0xff);
            buffer[3] = (byte)((f_bits >> 24) & 0xff);
            ret[i] = BitConverter.ToSingle(buffer, 0);
        }
        return ret;
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Soloud_getWave(IntPtr aObjHandle);
    /// <summary>
    /// <para>Gets 256 samples of the currently playing sound (post-clipping) and returns a pointer to the result.</para>
    ///
    /// <code>
    /// float * wav = soloud.getWave();
    /// int i;
    /// for (i = 0; i &lt; 256; i++)
    ///   drawline(0, i, wav[i] * 32, i);
    /// </code>
    ///
    /// <para>The returned pointer points at a buffer that's always around, but the data is only updated when getWave() is called. The data is the same that is used to generate visualization FFT data.</para>
    /// <para>For this function to work properly, you also need to initialize SoLoud with the Soloud::ENABLE_VISUALIZATION flag or by enabling visualization with the Soloud.setVisualizationEnable() call. Otherwise the source data will not be gathered, and the result is undefined (probably zero).</para>
    /// </summary>
    public float[] getWave()
    {
        float[] ret = new float[256];
        IntPtr p = Soloud_getWave(objhandle);

        byte[] buffer = new byte[4];
        for (int i = 0; i < ret.Length; ++i)
        {
            int f_bits = Marshal.ReadInt32(p, i * 4);
            buffer[0] = (byte)((f_bits >> 0) & 0xff);
            buffer[1] = (byte)((f_bits >> 8) & 0xff);
            buffer[2] = (byte)((f_bits >> 16) & 0xff);
            buffer[3] = (byte)((f_bits >> 24) & 0xff);
            ret[i] = BitConverter.ToSingle(buffer, 0);
        }
        return ret;
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getApproximateVolume(IntPtr aObjHandle, uint aChannel);
    /// <summary>
    /// <para>Gets the approximate volume for output per output channel (i.e, per speaker). Returns zero for invalid parameters.</para>
    ///
    /// <code>
    /// float ch1 = gSoloud.getApproximateVolume(0);
    /// float ch2 = gSoloud.getApproximateVolume(1);
    /// printf("Volume is %3.3f %3.3f", ch1, ch2);
    /// </code>
    ///
    /// <para>Visualization needs to be enabled for this function to work.</para>
    /// </summary>
    public float getApproximateVolume(uint aChannel)
    {
        return Soloud_getApproximateVolume(objhandle, aChannel);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_getLoopCount(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>Some sound sources that support looping also keep count of the loop count. This can be useful at least to detect when some sound loops.</para>
    ///
    /// <code>
    /// int c = soloud.getLoopCount(h);
    /// if (c != old_c)
    ///     printf("Looped!);
    /// old_c = c;
    /// </code>
    ///
    /// <para>Invalid handles and unsupported sound sources will cause the function to return 0.</para>
    /// </summary>
    public uint getLoopCount(uint aVoiceHandle)
    {
        return Soloud_getLoopCount(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_getInfo(IntPtr aObjHandle, uint aVoiceHandle, uint aInfoKey);
    /// <summary>
    /// <para>Some sound sources let you get real-time information from the active voice.</para>
    ///
    /// <code>
    /// float reg10 = soloud.getInfo(c64song, 10);
    /// </code>
    ///
    /// <para>If the call is not supported, or the handle is invalid, the function returns 0.</para>
    /// </summary>
    public float getInfo(uint aVoiceHandle, uint aInfoKey)
    {
        return Soloud_getInfo(objhandle, aVoiceHandle, aInfoKey);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Soloud_createVoiceGroup(IntPtr aObjHandle);
    /// <summary>
    /// <para>Used to create a new voice group. Returns 0 if not successful (either out of memory or out of voice groups).</para>
    ///
    /// <code>
    /// grouphandle = soloud.createVoiceGroup();
    /// if (grouphandle == 0) panic();
    /// </code>
    /// </summary>
    public uint createVoiceGroup()
    {
        return Soloud_createVoiceGroup(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_destroyVoiceGroup(IntPtr aObjHandle, uint aVoiceGroupHandle);
    /// <summary>
    /// <para>Deallocates the voice group. Does not stop the voices attached to the voice group.</para>
    ///
    /// <code>
    /// soloud.destroyVoiceGroup(grouphandle);
    /// </code>
    /// </summary>
    public int destroyVoiceGroup(uint aVoiceGroupHandle)
    {
        return Soloud_destroyVoiceGroup(objhandle, aVoiceGroupHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_addVoiceToGroup(IntPtr aObjHandle, uint aVoiceGroupHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>Adds voice handle to the voice group. The voice handles can still be used separate from the group.</para>
    ///
    /// <code>
    /// soloud.addVoiceToGroup(grouphandle, music1);
    /// soloud.addVoiceToGroup(grouphandle, music2);
    /// soloud.setPause(grouphandle, 0); // play both musics
    /// soloud.fadeVolume(music1, 1, 5); // fade music 1 up
    /// soloud.fadeVolume(music2, 0, 5); // fade music 2 down
    /// </code>
    /// </summary>
    public int addVoiceToGroup(uint aVoiceGroupHandle, uint aVoiceHandle)
    {
        return Soloud_addVoiceToGroup(objhandle, aVoiceGroupHandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_isVoiceGroup(IntPtr aObjHandle, uint aVoiceGroupHandle);
    /// <summary>
    /// <para>Checks if the handle is a valid voice group. Does not care if the voice group is empty.</para>
    ///
    /// <code>
    /// if (soloud.isVoiceGroup(grouphandle))
    ///     probably_some_debug_thing();
    ///
    /// </code>
    /// </summary>
    public int isVoiceGroup(uint aVoiceGroupHandle)
    {
        return Soloud_isVoiceGroup(objhandle, aVoiceGroupHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_isVoiceGroupEmpty(IntPtr aObjHandle, uint aVoiceGroupHandle);
    /// <summary>
    /// <para>Checks whether a voice group is empty. SoLoud automatically trims the voice groups of voices that have ended, so the group may be empty even though you've added valid voice handles to it.</para>
    ///
    /// <code>
    /// while (!soloud.isVoiceGroupEmpty(voicegroup))
    /// {
    ///     party_on();
    /// }
    /// </code>
    /// </summary>
    public int isVoiceGroupEmpty(uint aVoiceGroupHandle)
    {
        return Soloud_isVoiceGroupEmpty(objhandle, aVoiceGroupHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_update3dAudio(IntPtr aObjHandle);
    /// <summary>
    /// <para>Update the 3d parameters for the 3d voices. No positional effect occurs unless this function is called periodically.</para>
    ///
    /// <code>
    /// gSoloud.set3dSourceVelocity(h, -1, 0, 0); // go west
    /// gSoloud.update3dAudio(); // apply change to voices
    /// </code>
    /// </summary>
    public void update3dAudio()
    {
        Soloud_update3dAudio(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Soloud_set3dSoundSpeed(IntPtr aObjHandle, float aSpeed);
    /// <summary>
    /// <para>You can set and get the current value of the speed of sound width the get3dSoundSpeed() and set3dSoundSpeed() functions. The speed of sound is used to calculate doppler effects in addition to the distance delay.</para>
    /// <para>Since SoLoud has no knowledge of the scale of your coordinates, you may need to adjust the speed of sound for these effects to work correctly. The default value is 343, which assumes that your world coordinates are in meters (where 1 unit is 1 meter), and that the environment is dry air at around 20 degrees Celsius.</para>
    ///
    /// <code>
    /// int speed = gSoloud.get3dSoundSpeed(); // Get the current speed of sound
    /// gSoloud.set3dSoundSpeed(speed / 2); // Halve it
    /// </code>
    /// </summary>
    public int set3dSoundSpeed(float aSpeed)
    {
        return Soloud_set3dSoundSpeed(objhandle, aSpeed);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Soloud_get3dSoundSpeed(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can set and get the current value of the speed of sound width the get3dSoundSpeed() and set3dSoundSpeed() functions. The speed of sound is used to calculate doppler effects in addition to the distance delay.</para>
    /// <para>Since SoLoud has no knowledge of the scale of your coordinates, you may need to adjust the speed of sound for these effects to work correctly. The default value is 343, which assumes that your world coordinates are in meters (where 1 unit is 1 meter), and that the environment is dry air at around 20 degrees Celsius.</para>
    ///
    /// <code>
    /// int speed = gSoloud.get3dSoundSpeed(); // Get the current speed of sound
    /// gSoloud.set3dSoundSpeed(speed / 2); // Halve it
    /// </code>
    /// </summary>
    public float get3dSoundSpeed()
    {
        return Soloud_get3dSoundSpeed(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dListenerParametersEx(IntPtr aObjHandle, float aPosX, float aPosY, float aPosZ, float aAtX, float aAtY, float aAtZ, float aUpX, float aUpY, float aUpZ, float aVelocityX, float aVelocityY, float aVelocityZ);
    /// <summary>
    /// <para>You can set the position, at-vector, up-vector and velocity parameters of the 3d audio listener with one call using the set3dListenerParameters.</para>
    ///
    /// <code>
    /// void set3dListenerParameters(float aPosX,
    ///                              float aPosY,
    ///                              float aPosZ,
    ///                              float aAtX,
    ///                              float aAtY,
    ///                              float aAtZ,
    ///                              float aUpX,
    ///                              float aUpY,
    ///                              float aUpZ,
    ///                              float aVelocityX,
    ///                              float aVelocityY,
    ///                              float aVelocityZ)
    /// </code>
    ///
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dListenerParameters(float aPosX, float aPosY, float aPosZ, float aAtX, float aAtY, float aAtZ, float aUpX, float aUpY, float aUpZ, float aVelocityX = 0.0f, float aVelocityY = 0.0f, float aVelocityZ = 0.0f)
    {
        Soloud_set3dListenerParametersEx(objhandle, aPosX, aPosY, aPosZ, aAtX, aAtY, aAtZ, aUpX, aUpY, aUpZ, aVelocityX, aVelocityY, aVelocityZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dListenerPosition(IntPtr aObjHandle, float aPosX, float aPosY, float aPosZ);
    /// <summary>
    /// <para>You can set the position parameter of the 3d audio listener via set3dListenerPosition()</para>
    ///
    /// <code>
    /// void set3dListenerPosition(float aPosX,
    ///                            float aPosY,
    ///                            float aPosZ)
    /// </code>
    ///
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dListenerPosition(float aPosX, float aPosY, float aPosZ)
    {
        Soloud_set3dListenerPosition(objhandle, aPosX, aPosY, aPosZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dListenerAt(IntPtr aObjHandle, float aAtX, float aAtY, float aAtZ);
    /// <summary>
    /// <para>You can set the "at" vector parameter of the 3d audio listener via set3dListenerAt()</para>
    ///
    /// <code>
    /// void set3dListenerAt(float aAtX,
    ///                      float aAtY,
    ///                      float aAtZ)
    /// </code>
    ///
    /// <para>The "at" vector means the direction the listener is facing. The vector does not need to be normalized.</para>
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dListenerAt(float aAtX, float aAtY, float aAtZ)
    {
        Soloud_set3dListenerAt(objhandle, aAtX, aAtY, aAtZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dListenerUp(IntPtr aObjHandle, float aUpX, float aUpY, float aUpZ);
    /// <summary>
    /// <para>You can set the "up" vector parameter of the 3d audio listener via set3dListenerUp()</para>
    ///
    /// <code>
    /// void set3dListenerUp(float aUpX,
    ///                      float aUpY,
    ///                      float aUpZ)
    /// </code>
    ///
    /// <para>The "up" vector means the direction upwards from the listener. The vector does not need to be normalized. Typically this is always set to 0,1,0.</para>
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dListenerUp(float aUpX, float aUpY, float aUpZ)
    {
        Soloud_set3dListenerUp(objhandle, aUpX, aUpY, aUpZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dListenerVelocity(IntPtr aObjHandle, float aVelocityX, float aVelocityY, float aVelocityZ);
    /// <summary>
    /// <para>You can set the listener's velocity vector parameter via set3dListenerVelocity()</para>
    ///
    /// <code>
    /// void set3dListenerVelocity(float aVelocityX,
    ///                            float aVelocityY,
    ///                            float aVelocityZ)
    /// </code>
    ///
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dListenerVelocity(float aVelocityX, float aVelocityY, float aVelocityZ)
    {
        Soloud_set3dListenerVelocity(objhandle, aVelocityX, aVelocityY, aVelocityZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dSourceParametersEx(IntPtr aObjHandle, uint aVoiceHandle, float aPosX, float aPosY, float aPosZ, float aVelocityX, float aVelocityY, float aVelocityZ);
    /// <summary>
    /// <para>You can set the position and velocity parameters of a live 3d audio source with one call using the set3dSourceParameters().</para>
    ///
    /// <code>
    /// void set3dSourceParameters(handle aVoiceHandle,
    ///                            float aPosX,
    ///                            float aPosY,
    ///                            float aPosZ,
    ///                            float aVelocityX,
    ///                            float aVelocityY,
    ///                            float aVelocityZ)
    ///
    /// </code>
    ///
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dSourceParameters(uint aVoiceHandle, float aPosX, float aPosY, float aPosZ, float aVelocityX = 0.0f, float aVelocityY = 0.0f, float aVelocityZ = 0.0f)
    {
        Soloud_set3dSourceParametersEx(objhandle, aVoiceHandle, aPosX, aPosY, aPosZ, aVelocityX, aVelocityY, aVelocityZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dSourcePosition(IntPtr aObjHandle, uint aVoiceHandle, float aPosX, float aPosY, float aPosZ);
    /// <summary>
    /// <para>You can set the position parameters of a live 3d audio source with the set3dSourcePosition() function.</para>
    ///
    /// <code>
    /// void set3dSourcePosition(handle aVoiceHandle,
    ///                          float aPosX,
    ///                          float aPosY,
    ///                          float aPosZ)
    /// </code>
    ///
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dSourcePosition(uint aVoiceHandle, float aPosX, float aPosY, float aPosZ)
    {
        Soloud_set3dSourcePosition(objhandle, aVoiceHandle, aPosX, aPosY, aPosZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dSourceVelocity(IntPtr aObjHandle, uint aVoiceHandle, float aVelocityX, float aVelocityY, float aVelocityZ);
    /// <summary>
    /// <para>You can set the velocity parameters of a live 3d audio source with the set3dSourceVelocity() function.</para>
    ///
    /// <code>
    /// void set3dSourceVelocity(handle aVoiceHandle,
    ///                          float aVelocityX,
    ///                          float aVelocityY,
    ///                          float aVelocityZ)
    /// </code>
    ///
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dSourceVelocity(uint aVoiceHandle, float aVelocityX, float aVelocityY, float aVelocityZ)
    {
        Soloud_set3dSourceVelocity(objhandle, aVoiceHandle, aVelocityX, aVelocityY, aVelocityZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dSourceMinMaxDistance(IntPtr aObjHandle, uint aVoiceHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>You can set the minimum and maximum distance parameters of a live 3d audio source with set3dSourceMinMaxDistance().</para>
    ///
    /// <code>
    /// void set3dSourceMinMaxDistance(handle aVoiceHandle,
    ///                                float aMinDistance,
    ///                                float aMaxDistance)
    /// </code>
    ///
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dSourceMinMaxDistance(uint aVoiceHandle, float aMinDistance, float aMaxDistance)
    {
        Soloud_set3dSourceMinMaxDistance(objhandle, aVoiceHandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dSourceAttenuation(IntPtr aObjHandle, uint aVoiceHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>You can change the attenuation model and rolloff factor parameters of a live 3d audio source with set3dSourceAttenuation().</para>
    ///
    /// <code>
    /// void set3dSourceAttenuation(handle aVoiceHandle,
    ///                             unsigned int aAttenuationModel,
    ///                             float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>See AudioSource.set3dAttenuation() below for a list of attenuation models.</para>
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dSourceAttenuation(uint aVoiceHandle, uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Soloud_set3dSourceAttenuation(objhandle, aVoiceHandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_set3dSourceDopplerFactor(IntPtr aObjHandle, uint aVoiceHandle, float aDopplerFactor);
    /// <summary>
    /// <para>You can change the doppler factor of a live 3d audio source with set3dSourceDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dSourceDopplerFactor(handle aVoiceHandle,
    ///                               float aDopplerFactor)
    /// </code>
    ///
    /// <para>The changes to these parameters are only evaluated when the update3dAudio() function is called.</para>
    /// </summary>
    public void set3dSourceDopplerFactor(uint aVoiceHandle, float aDopplerFactor)
    {
        Soloud_set3dSourceDopplerFactor(objhandle, aVoiceHandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_mix(IntPtr aObjHandle, float[] aBuffer, uint aSamples);
    /// <summary>
    /// <para>The back-end can call the mix function to request a number of stereo samples from SoLoud. The samples will be in float format, and the back-end is responsible for converting them to the desired output format.</para>
    ///
    /// <code>
    /// void mix(float *aBuffer, // Destination buffer
    ///          int aSamples);  // Number of requested stereo samples
    /// </code>
    ///
    /// <para>If the number of samples exceeds the buffer size set at init, the result is undefined (most likely a crash).</para>
    /// </summary>
    public void mix(float[] aBuffer, uint aSamples)
    {
        Soloud_mix(objhandle, aBuffer, aSamples);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Soloud_mixSigned16(IntPtr aObjHandle, IntPtr aBuffer, uint aSamples);
    /// <summary>
    /// <para>Since so many back-ends prefer 16 bit signed data instead of float data, SoLoud also provides a mix call that outputs signed 16 bit data.</para>
    ///
    /// <code>
    /// void mixSigned16(short *aBuffer, // Destination buffer
    ///                  int aSamples);  // Number of requested stereo samples
    /// </code>
    /// </summary>
    public void mixSigned16(IntPtr aBuffer, uint aSamples)
    {
        Soloud_mixSigned16(objhandle, aBuffer, aSamples);
    }
}

/// <summary>
/// <para>The bassboost filter is a proof of concept FFT filter. It simply multiplies the first few bands of the FFT by the boost value.</para>
/// <para>There is one parameter - how strong the boost effect is.</para>
///
/// <code>
/// // Set up bass boost filter
/// gBassboost.setParams(0.1f);
/// // Set the filter as the first filter of the bus
/// gBus.setFilter(0, &amp;gBassboost);
/// </code>
/// </summary>
public partial class BassboostFilter : SoloudObject
{
    public const int WET = 0;
    public const int BOOST = 1;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr BassboostFilter_create();
    public BassboostFilter()
    {
        objhandle = BassboostFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr BassboostFilter_destroy(IntPtr aObjHandle);
    ~BassboostFilter()
    {
        BassboostFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int BassboostFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return BassboostFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr BassboostFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = BassboostFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint BassboostFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return BassboostFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float BassboostFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return BassboostFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float BassboostFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return BassboostFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int BassboostFilter_setParams(IntPtr aObjHandle, float aBoost);
    /// <summary>
    /// <para>Set the parameters of the filter.</para>
    ///
    /// <code>
    /// gBassboost.setParams(11); // ours goes to 11
    /// </code>
    ///
    /// <para>Changing the parameters via setParams() does not affect "live" sounds. Live parameters can be set, faded or oscillated:</para>
    ///
    /// <code>
    /// gSoloud.fadeFilterParameter(
    ///   gMusicHandle, // Sound handle
    ///   0,            // First filter
    ///   SoLoud::BassboostFilter::BOOST, // What to adjust
    ///   0,            // Target value
    ///   3);           // Time in seconds
    /// </code>
    /// </summary>
    public int setParams(float aBoost)
    {
        return BassboostFilter_setParams(objhandle, aBoost);
    }
}

/// <summary>
/// <para>The biquad resonant filter is a surprisingly cheap way to implement low and high pass filters, as well as some kind of band bass filter.</para>
/// <para>The implementation in SoLoud is based on "Using the Biquad Resonant Filter", Phil Burk, Game Programming Gems 3, p. 606.</para>
/// <para>The filter has three parameters - sample rate, cutoff frequency and resonance. These can also be adjusted on live streams, for instance to fade the low pass filter cutoff frequency for a outdoors/indoors transition effect.</para>
/// <para>The resonance parameter adjusts the sharpness (or bandwidth) of the cutoff.</para>
///
/// <code>
/// // Set up low-pass filter
/// gBQRFilter.setParams(SoLoud::BiquadResonantFilter::LOWPASS, 500, 2);
/// // Set the filter as the second filter of the bus
/// gBus.setFilter(1, &amp;gBQRFilter);
/// </code>
///
/// <para>It's also possible to set, fade or oscillate the parameters of a "live" filter</para>
///
/// <code>
/// gSoloud.fadeFilterParameter(
///   gMusicHandle, // Sound handle
///   0,            // First filter
///   SoLoud::BiquadResonantFilter::FREQUENCY, // What to adjust
///   2000,         // Target value
///   3);           // Time in seconds
///
/// </code>
///
/// <para>Currently, four parameters can be adjusted:</para>
///
/// Parameter
/// Description
///
/// WET
/// Filter's wet signal; 1.0f for fully filtered, 0.0f for original, 0.5f for half and half.
///
/// FREQUENCY
/// Filter's cutoff frequency
///
/// RESONANCE
/// Filter's resonance - higher means sharper cutoff
/// </summary>
public partial class BiquadResonantFilter : SoloudObject
{
    public const int LOWPASS = 0;
    public const int HIGHPASS = 1;
    public const int BANDPASS = 2;
    public const int WET = 0;
    public const int TYPE = 1;
    public const int FREQUENCY = 2;
    public const int RESONANCE = 3;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr BiquadResonantFilter_create();
    public BiquadResonantFilter()
    {
        objhandle = BiquadResonantFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr BiquadResonantFilter_destroy(IntPtr aObjHandle);
    ~BiquadResonantFilter()
    {
        BiquadResonantFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int BiquadResonantFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return BiquadResonantFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr BiquadResonantFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = BiquadResonantFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint BiquadResonantFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return BiquadResonantFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float BiquadResonantFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return BiquadResonantFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float BiquadResonantFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return BiquadResonantFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int BiquadResonantFilter_setParams(IntPtr aObjHandle, int aType, float aFrequency, float aResonance);
    /// <summary>
    /// <para>Set the parameters of the filter.</para>
    ///
    /// <code>
    /// gBQRFilter.setParams(SoLoud::BiquadResonantFilter::LOWPASS, 44100, 500, 2);
    /// </code>
    ///
    /// <para>Changing the parameters does not affect "live" sounds. If invalid parameters are given, the function will return error.</para>
    /// </summary>
    public int setParams(int aType, float aFrequency, float aResonance)
    {
        return BiquadResonantFilter_setParams(objhandle, aType, aFrequency, aResonance);
    }
}

/// <summary>
/// <para>The mixing busses are a special case of an audio stream. They are a kind of audio stream that plays other audio streams. Mixing bus can also play other mixing busses. Like any other audio stream, mixing bus has volume, panning and filters.</para>
/// <para>Only one instance of a mixing bus can play at the same time, however; trying to play the same bus several times stops the earlier instance.</para>
///
/// <para>While a mixing bus doesn't generate audio by itself, playing it counts against the maximum number of concurrent streams.</para>
/// <para>Mixing busses are protected by default (i.e, won't stop playing if maximum number of concurrent streams is reached), and also marked as "must tick" (i.e, will always be present in the active voices list).</para>
/// <para>To play a stream through the mixing bus, use the bus play() command.</para>
///
/// <code>
/// int bushandle = gSoloud.play(gBus); // Play the bus
/// gSoloud.setVolume(bushandle, 0.5f); // Set bus volume
///
/// int fxhandle = gBus.play(gSoundEffect); // Play sound effect through bus
/// gSoloud.setVolume(fxhandle, 0.5f); // set sound effect volume
/// </code>
///
/// <para>When chaining busses care should be taken, as resampling will offset the output by a number of samples. Point sampling by 0, linear by 1, and higher resamplers with higher number of samples.</para>
/// </summary>
public partial class Bus : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Bus_create();
    public Bus()
    {
        objhandle = Bus_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Bus_destroy(IntPtr aObjHandle);
    ~Bus()
    {
        Bus_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to busses.</para>
    ///
    /// <code>
    /// gSfxBus.setFilter(0, &amp;gEnvironment);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Bus_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Bus_playEx(IntPtr aObjHandle, IntPtr aSound, float aVolume, float aPan, int aPaused);
    /// <summary>
    /// <para>Equivalent of soloud.play(), but plays the sound source through the bus instead of at "global" scope.</para>
    /// </summary>
    public uint play(SoloudObject aSound, float aVolume = 1.0f, float aPan = 0.0f, int aPaused = 0)
    {
        return Bus_playEx(objhandle, aSound.objhandle, aVolume, aPan, aPaused);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Bus_playClockedEx(IntPtr aObjHandle, double aSoundTime, IntPtr aSound, float aVolume, float aPan);
    /// <summary>
    /// <para>Equivalent of soloud.playClocked(), but plays the sound source through the bus instead of at "global" scope.</para>
    /// </summary>
    public uint playClocked(double aSoundTime, SoloudObject aSound, float aVolume = 1.0f, float aPan = 0.0f)
    {
        return Bus_playClockedEx(objhandle, aSoundTime, aSound.objhandle, aVolume, aPan);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Bus_play3dEx(IntPtr aObjHandle, IntPtr aSound, float aPosX, float aPosY, float aPosZ, float aVelX, float aVelY, float aVelZ, float aVolume, int aPaused);
    /// <summary>
    /// <para>Equivalent of soloud.play3d(), but plays the sound source through the bus instead of at "global" scope.</para>
    /// </summary>
    public uint play3d(SoloudObject aSound, float aPosX, float aPosY, float aPosZ, float aVelX = 0.0f, float aVelY = 0.0f, float aVelZ = 0.0f, float aVolume = 1.0f, int aPaused = 0)
    {
        return Bus_play3dEx(objhandle, aSound.objhandle, aPosX, aPosY, aPosZ, aVelX, aVelY, aVelZ, aVolume, aPaused);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Bus_play3dClockedEx(IntPtr aObjHandle, double aSoundTime, IntPtr aSound, float aPosX, float aPosY, float aPosZ, float aVelX, float aVelY, float aVelZ, float aVolume);
    /// <summary>
    /// <para>Equivalent of soloud.play3dClocked(), but plays the sound sounce through the bus instead of at "global" scope.</para>
    /// </summary>
    public uint play3dClocked(double aSoundTime, SoloudObject aSound, float aPosX, float aPosY, float aPosZ, float aVelX = 0.0f, float aVelY = 0.0f, float aVelZ = 0.0f, float aVolume = 1.0f)
    {
        return Bus_play3dClockedEx(objhandle, aSoundTime, aSound.objhandle, aPosX, aPosY, aPosZ, aVelX, aVelY, aVelZ, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Bus_setChannels(IntPtr aObjHandle, uint aChannels);
    /// <summary>
    /// <para>Set the number of channels this bus should handle. Defaults to 2 (stereo).</para>
    ///
    /// <code>
    /// gBus.setChannels(1); // set to mono
    /// </code>
    /// </summary>
    public int setChannels(uint aChannels)
    {
        return Bus_setChannels(objhandle, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_setVisualizationEnable(IntPtr aObjHandle, int aEnable);
    /// <summary>
    /// <para>Enable (or disable) gathering of visualization wave data from this bus.</para>
    /// </summary>
    public void setVisualizationEnable(int aEnable)
    {
        Bus_setVisualizationEnable(objhandle, aEnable);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_annexSound(IntPtr aObjHandle, uint aVoiceHandle);
    /// <summary>
    /// <para>Move live audio stream under this bus. This can be useful if you have some heavy filtering under one bus and want to move playing sounds in and out of the bus.</para>
    ///
    /// <code>
    /// gTunnelBus.annexSound(sfxBus);
    /// </code>
    /// </summary>
    public void annexSound(uint aVoiceHandle)
    {
        Bus_annexSound(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Bus_calcFFT(IntPtr aObjHandle);
    /// <summary>
    /// <para>Calculates FFT of the sound currently playing through this bus, and returns a pointer to the result.</para>
    ///
    /// <code>
    /// float * fft = fxbus.calcFFT();
    /// int i;
    /// for (i = 0; i &lt; 256; i++)
    ///   drawline(0, i, fft[i] * 32, i);
    /// </code>
    ///
    /// <para>The FFT data has 256 floats, from low to high frequencies.</para>
    /// <para>SoLoud performs a mono mix of the audio, passes it to FFT, and then calculates the magnitude of the complex numbers for application to use. For more advanced FFT use, SoLoud code changes are needed.</para>
    /// <para>The returned pointer points at a buffer that's around as long as the bus object exists, but the data is only updated when calcFFT() is called.</para>
    /// <para>For the FFT to work, you also need to enable visualization with the Bus.setVisualizationEnable() call. Otherwise the source data for the FFT calculation will not be gathered.</para>
    /// </summary>
    public float[] calcFFT()
    {
        float[] ret = new float[256];
        IntPtr p = Bus_calcFFT(objhandle);

        byte[] buffer = new byte[4];
        for (int i = 0; i < ret.Length; ++i)
        {
            int f_bits = Marshal.ReadInt32(p, i * 4);
            buffer[0] = (byte)((f_bits >> 0) & 0xff);
            buffer[1] = (byte)((f_bits >> 8) & 0xff);
            buffer[2] = (byte)((f_bits >> 16) & 0xff);
            buffer[3] = (byte)((f_bits >> 24) & 0xff);
            ret[i] = BitConverter.ToSingle(buffer, 0);
        }
        return ret;
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Bus_getWave(IntPtr aObjHandle);
    /// <summary>
    /// <para>Gets 256 samples of the sound currently playing through this bus, and returns a pointer to the result.</para>
    ///
    /// <code>
    /// float * wav = speechbus.getWave();
    /// int i;
    /// for (i = 0; i &lt; 256; i++)
    ///   drawline(0, i, wav[i] * 32, i);
    /// </code>
    ///
    /// <para>The returned pointer points at a buffer that's around as long as the bus object exists, but the data is only updated when getWave() is called. The data is the same that is used to generate visualization FFT data.</para>
    /// <para>For this function to work properly, you also need enable visualization with the Bus.setVisualizationEnable() call. Otherwise the source data will not be gathered, and the result is undefined (probably zero).</para>
    /// </summary>
    public float[] getWave()
    {
        float[] ret = new float[256];
        IntPtr p = Bus_getWave(objhandle);

        byte[] buffer = new byte[4];
        for (int i = 0; i < ret.Length; ++i)
        {
            int f_bits = Marshal.ReadInt32(p, i * 4);
            buffer[0] = (byte)((f_bits >> 0) & 0xff);
            buffer[1] = (byte)((f_bits >> 8) & 0xff);
            buffer[2] = (byte)((f_bits >> 16) & 0xff);
            buffer[3] = (byte)((f_bits >> 24) & 0xff);
            ret[i] = BitConverter.ToSingle(buffer, 0);
        }
        return ret;
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float Bus_getApproximateVolume(IntPtr aObjHandle, uint aChannel);
    /// <summary>
    /// <para>Gets the approximate volume for output per output channel (i.e, per speaker). Returns zero for invalid parameters.</para>
    ///
    /// <code>
    /// float ch1 = gBus.getApproximateVolume(0);
    /// float ch2 = gBus.getApproximateVolume(1);
    /// printf("Volume is %3.3f %3.3f", ch1, ch2);
    /// </code>
    ///
    /// <para>Visualization needs to be enabled for this function to work.</para>
    /// </summary>
    public float getApproximateVolume(uint aChannel)
    {
        return Bus_getApproximateVolume(objhandle, aChannel);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Bus_getActiveVoiceCount(IntPtr aObjHandle);
    /// <summary>
    /// <para>Returns the number of concurrent sounds that are playing through this bus at the moment.</para>
    ///
    /// <code>
    /// if (sfxbus.getActiveVoiceCount() == 0) enjoy_the_silence();
    /// </code>
    /// </summary>
    public uint getActiveVoiceCount()
    {
        return Bus_getActiveVoiceCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the bus.</para>
    ///
    /// <code>
    /// gMusicBus.setVolume(11);
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Bus_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Trying to change the looping state of a bus has no effect.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Bus_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Bus_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Bus_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Bus_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Bus_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Bus_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Bus_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Bus_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Kill off the sfx if they're not heard.
    /// gSfxBus.setInaudibleBehavior(false, true);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Bus_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>Trying to change the looping state of a bus has no effect.</para>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Bus_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Bus_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>Trying to change the looping state of a bus has no effect.</para>
    /// </summary>
    public double getLoopPoint()
    {
        return Bus_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Bus_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    /// </summary>
    public void stop()
    {
        Bus_stop(objhandle);
    }
}

/// <summary>
/// <para>This filter tries to remove DC signal from the audio. In other words, it tries to center the waveform around 0. This can be useful if some of the input waveforms gets stuck on non-zero values for a long time.</para>
/// <para>The filter does not support changing of parameters on the fly, nor does it take changing of relative play speed into account.</para>
/// <para>The DC removal is performed by calculating the average sample value over a relatively long period of time, and subtracting this from the output.</para>
/// <para>There is one parameter - how long the averaging buffer should be. The time is in seconds.</para>
///
/// <code>
/// // Set up DC removal filter
/// gDCRemovalFilter.setParams(0.1f);
/// // Set the filter as the first filter of the bus
/// gBus.setFilter(0, &amp;gDCRemovalFilter);
/// </code>
/// </summary>
public partial class DCRemovalFilter : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr DCRemovalFilter_create();
    public DCRemovalFilter()
    {
        objhandle = DCRemovalFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr DCRemovalFilter_destroy(IntPtr aObjHandle);
    ~DCRemovalFilter()
    {
        DCRemovalFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int DCRemovalFilter_setParamsEx(IntPtr aObjHandle, float aLength);
    /// <summary>
    /// <para>Set the parameters of the filter.</para>
    ///
    /// <code>
    /// gDCRemovalFilter.setParams(0.1f);
    /// </code>
    ///
    /// <para>Changing the parameters does not affect "live" sounds. If invalid parameters are given, the function will return error.</para>
    /// </summary>
    public int setParams(float aLength = 0.1f)
    {
        return DCRemovalFilter_setParamsEx(objhandle, aLength);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int DCRemovalFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return DCRemovalFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr DCRemovalFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = DCRemovalFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint DCRemovalFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return DCRemovalFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float DCRemovalFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return DCRemovalFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float DCRemovalFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return DCRemovalFilter_getParamMin(objhandle, aParamIndex);
    }
}

/// <summary>
/// <para>The echo filter in SoLoud is a very simple one. When the sound starts to play, the echo filter allocates a buffer to contain the echo samples, and loops through this until the sound ends.</para>
/// <para>The filter does not support changing of parameters on the fly, nor does it take changing of relative play speed into account.</para>
/// <para>There are two parameters - delay and decay. Delay is the time in seconds until the echo, and decay is multiplier for the echo. If the multiplier is outside the [0..1[ range, the results are unpredictable.</para>
///
/// <code>
/// // Set up echo filter
/// gEchoFilter.setParams(0.5f, 0.5f);
/// // Set the filter as the first filter of the bus
/// gBus.setFilter(0, &amp;gEchoFilter);
/// </code>
/// </summary>
public partial class EchoFilter : SoloudObject
{
    public const int WET = 0;
    public const int DELAY = 1;
    public const int DECAY = 2;
    public const int FILTER = 3;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr EchoFilter_create();
    public EchoFilter()
    {
        objhandle = EchoFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr EchoFilter_destroy(IntPtr aObjHandle);
    ~EchoFilter()
    {
        EchoFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int EchoFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return EchoFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr EchoFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = EchoFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint EchoFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return EchoFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float EchoFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return EchoFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float EchoFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return EchoFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int EchoFilter_setParamsEx(IntPtr aObjHandle, float aDelay, float aDecay, float aFilter);
    /// <summary>
    /// <para>Set the parameters of the filter.</para>
    ///
    /// <code>
    /// gEchoFilter.setParams(0.5f, 0.5f);
    /// </code>
    ///
    /// <para>Changing the parameters does not affect "live" sounds. If invalid parameters are given, the function will return error.</para>
    /// </summary>
    public int setParams(float aDelay, float aDecay = 0.7f, float aFilter = 0.0f)
    {
        return EchoFilter_setParamsEx(objhandle, aDelay, aDecay, aFilter);
    }
}

/// <summary>
/// <para>The FFT filter is a short-time Fourier transform filter which can be used as basis for FFT-based effects. The base implementation does a simple tone downshifting.</para>
/// <para>The filter exists mainly to adjust the speech synthesizer's voice in strange ways. It can also be used as basis for other FFT-based filters.</para>
/// </summary>
public partial class FFTFilter : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FFTFilter_create();
    public FFTFilter()
    {
        objhandle = FFTFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FFTFilter_destroy(IntPtr aObjHandle);
    ~FFTFilter()
    {
        FFTFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int FFTFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return FFTFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FFTFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = FFTFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint FFTFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return FFTFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float FFTFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return FFTFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float FFTFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return FFTFilter_getParamMin(objhandle, aParamIndex);
    }
}

/// <summary>
/// <para>The flanger filter can be used to create a "flanger" effect on the signal. Applying this on a human voice may sound more "robotic", for instance.</para>
///
/// <code>
/// // Set up flanger filter
/// gFlangerFilter.setParams(0.005f, 10);
/// // Set the filter as the first filter of the bus
/// gBus.setFilter(0, &amp;gFlangerFilter);
/// </code>
///
/// <para>It's also possible to set, fade or oscillate the parameters of a "live" filter</para>
///
/// <code>
/// gSoloud.fadeFilterParameter(
///   gMusicHandle, // Sound handle
///   0,            // First filter
///   SoLoud::LofiFilter::WET, // What to adjust
///   0,            // Target value
///   3);           // Time in seconds
///
/// </code>
///
/// <para>Currently, four parameters can be adjusted:</para>
///
/// Parameter
/// Description
///
/// WET
/// Filter's wet signal; 1.0f for fully filtered, 0.0f for original, 0.5f for half and half.
///
/// FREQ
/// Filter's frequency
///
/// DELAY
/// Filter's delay
/// </summary>
public partial class FlangerFilter : SoloudObject
{
    public const int WET = 0;
    public const int DELAY = 1;
    public const int FREQ = 2;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FlangerFilter_create();
    public FlangerFilter()
    {
        objhandle = FlangerFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FlangerFilter_destroy(IntPtr aObjHandle);
    ~FlangerFilter()
    {
        FlangerFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int FlangerFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return FlangerFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FlangerFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = FlangerFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint FlangerFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return FlangerFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float FlangerFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return FlangerFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float FlangerFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return FlangerFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int FlangerFilter_setParams(IntPtr aObjHandle, float aDelay, float aFreq);
    /// <summary>
    /// <para>Set the parameters of the filter.</para>
    ///
    /// <code>
    /// gFlangerFilter.setParams(0.005f, 10);
    /// </code>
    ///
    /// <para>Changing the parameters does not affect "live" sounds. If invalid parameters are given, the function will return error.</para>
    /// </summary>
    public int setParams(float aDelay, float aFreq)
    {
        return FlangerFilter_setParams(objhandle, aDelay, aFreq);
    }
}

/// <summary>
/// <para>Freeverb is a reverb filter based on the code written by Jezar at Dreampoint, June 2000 http://www.dreampoint.co.uk, and placed under public domain. There are other versions of Freeverb out there with different licenses.</para>
/// <para>While it sounds great, the filter is relatively heavy, and should be used sparingly.</para>
/// </summary>
public partial class FreeverbFilter : SoloudObject
{
    public const int WET = 0;
    public const int FREEZE = 1;
    public const int ROOMSIZE = 2;
    public const int DAMP = 3;
    public const int WIDTH = 4;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FreeverbFilter_create();
    public FreeverbFilter()
    {
        objhandle = FreeverbFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FreeverbFilter_destroy(IntPtr aObjHandle);
    ~FreeverbFilter()
    {
        FreeverbFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int FreeverbFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return FreeverbFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr FreeverbFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = FreeverbFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint FreeverbFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return FreeverbFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float FreeverbFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return FreeverbFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float FreeverbFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return FreeverbFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int FreeverbFilter_setParams(IntPtr aObjHandle, float aMode, float aRoomSize, float aDamp, float aWidth);
    /// <summary>
    /// <para>Set parameters of the filter. Does not affect "live" filters.</para>
    ///
    /// <code>
    /// verb.setParams(0, 0.5f, 0.5f, 1);
    /// </code>
    ///
    /// <para>The mode parameter freezes the audio currently flowing through the filter; setting it to 1 before playing audio will cause the filter to just replicate silence.</para>
    /// </summary>
    public int setParams(float aMode, float aRoomSize, float aDamp, float aWidth)
    {
        return FreeverbFilter_setParams(objhandle, aMode, aRoomSize, aDamp, aWidth);
    }
}

/// <summary>
/// <para>The lofi filter is a signal degrading filter. You can adjust both the bit depth and the sample rate of the output, and these parameters can also be adjusted (and even faded) on the fly.</para>
///
/// <code>
/// // Set up low-pass filter
/// gLofiFilter.setParams(8000, 5);
/// // Set the filter as the first filter of the bus
/// gBus.setFilter(0, &amp;gLofiFilter);
/// </code>
///
/// <para>It's also possible to set, fade or oscillate the parameters of a "live" filter</para>
///
/// <code>
/// gSoloud.fadeFilterParameter(
///   gMusicHandle, // Sound handle
///   0,            // First filter
///   SoLoud::LofiFilter::BITDEPTH, // What to adjust
///   2,            // Target value
///   3);           // Time in seconds
///
/// </code>
///
/// <para>Currently, four parameters can be adjusted:</para>
///
/// Parameter
/// Description
///
/// WET
/// Filter's wet signal; 1.0f for fully filtered, 0.0f for original, 0.5f for half and half.
///
/// SAMPLERATE
/// Filter's samplerate parameter
///
/// BITDEPTH
/// Filter's bit-depth parameter
///
/// <para>Note that the bit depth does not need to be an integer value.</para>
/// </summary>
public partial class LofiFilter : SoloudObject
{
    public const int WET = 0;
    public const int SAMPLERATE = 1;
    public const int BITDEPTH = 2;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr LofiFilter_create();
    public LofiFilter()
    {
        objhandle = LofiFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr LofiFilter_destroy(IntPtr aObjHandle);
    ~LofiFilter()
    {
        LofiFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int LofiFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return LofiFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr LofiFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = LofiFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint LofiFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return LofiFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float LofiFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return LofiFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float LofiFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return LofiFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int LofiFilter_setParams(IntPtr aObjHandle, float aSampleRate, float aBitdepth);
    /// <summary>
    /// <para>Set the parameters of the filter.</para>
    ///
    /// <code>
    /// gLofiFilter.setParams(8000, 5);
    /// </code>
    ///
    /// <para>Changing the parameters does not affect "live" sounds. If invalid parameters are given, the function will return error.</para>
    /// </summary>
    public int setParams(float aSampleRate, float aBitdepth)
    {
        return LofiFilter_setParams(objhandle, aSampleRate, aBitdepth);
    }
}

/// <summary>
/// <para>The SoLoud::Monotone is a replayer for MONOTONE tracker songs. MONOTONE is a pc-speaker tracker, available on GitHub at</para>
/// <para>https://github.com/MobyGamer/MONOTONE/</para>
/// <para>The SoLoud MONOTONE replayer can play MONOTONE v1 songs (only format available at the time of this writing). You can pick the number of "hardware" voices used - typically the songs are composed for a single voice (PC beeper).</para>
/// <para>The default waveform used is square wave, but other waveforms are also supported.</para>
/// </summary>
public partial class Monotone : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Monotone_create();
    public Monotone()
    {
        objhandle = Monotone_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Monotone_destroy(IntPtr aObjHandle);
    ~Monotone()
    {
        Monotone_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Monotone_setParamsEx(IntPtr aObjHandle, int aHardwareChannels, int aWaveform);
    /// <summary>
    /// <para>The setParams() function can be used to adjust the way SoLoud plays the MONOTONE file.</para>
    ///
    /// <code>
    /// result setParams(int aHardwareChannels, int aWaveform);
    /// </code>
    ///
    /// <para>Most songs are composed for a single hardware channel. SoLoud supports up to 12 hardware channels (more can be easily added by editing soloud_monotone.cpp)</para>
    /// <para>The supported waveforms include:</para>
    ///
    /// Waveform
    /// Description
    ///
    /// WAVE_SQUARE
    /// Raw, harsh square wave
    ///
    /// WAVE_SAW
    /// Raw, harsh saw wave
    ///
    /// WAVE_SIN
    /// Sine wave
    ///
    /// WAVE_TRIANGLE
    /// Triangle wave
    ///
    /// WAVE_BOUNCE
    /// Bounce, i.e, abs(sin())
    ///
    /// WAVE_JAWS
    /// Quater sine wave, rest of period quiet
    ///
    /// WAVE_HUMPS
    /// Half sine wave, rest of period quiet
    ///
    /// WAVE_FSQUARE
    /// "Fourier" square wave; less noisy
    ///
    /// WAVE_FSAW
    /// "Fourier" saw wave; less noisy
    /// </summary>
    public int setParams(int aHardwareChannels, int aWaveform = 0/*SoLoud:Misc::WAVE_SQUARE*/)
    {
        return Monotone_setParamsEx(objhandle, aHardwareChannels, aWaveform);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Monotone_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    /// <summary>
    /// <para>You tell monotone to load a file with the load function:</para>
    ///
    /// <code>
    /// result load(const char *aFilename);
    /// </code>
    ///
    /// <para>If loading fails, the function returns an error code.</para>
    ///
    /// <code>
    /// SoLoud::Monotone swingin1;
    /// swingin1.load("swingin1.mon");
    /// </code>
    /// </summary>
    public int load(string aFilename)
    {
        return Monotone_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Monotone_loadMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    /// <summary>
    /// <para>Alternate way of loading the file is to read from a memory buffer.</para>
    ///
    /// <code>
    /// result loadMem(unsigned char *aMem, int aLength,
    ///                bool aCopy, bool aTakeOwnership);
    /// </code>
    ///
    /// <para>The aTakeOwnership parameter can be used to tell SoLoud to free the pointer once it's done with it. The aCopy parameter is here for compatibility with the other loadMem functions.</para>
    /// </summary>
    public int loadMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Monotone_loadMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Monotone_loadFile(IntPtr aObjHandle, IntPtr aFile);
    /// <summary>
    /// <para>The loadFile() can be used to load audio from a SoLoud::File object. This is useful for integrating with virtual filesystems / packfiles, such as PhysFS.</para>
    /// </summary>
    public int loadFile(SoloudObject aFile)
    {
        return Monotone_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    ///
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Monotone_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Adjusting the looping of a monotone sound does not currently have any effect. All music is set to loop by default.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Monotone_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Monotone_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Monotone_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Monotone_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Monotone_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Monotone_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Monotone_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Monotone_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Monotone_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Monotone_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Monotone_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public double getLoopPoint()
    {
        return Monotone_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to monotone audio sources.</para>
    ///
    /// <code>
    /// gMusic.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Monotone_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Monotone_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a monotone sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gMusic.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        Monotone_stop(objhandle);
    }
}

/// <summary>
/// <para>The SoLoud::Noise audio source generates... noise. This can be useful in some situations, such as when you want to drown out everything into white noise, or when you want to have some other kind of noise in the background. Noise can also be filtered to produce different kinds of sounds, such as wind hissing or water pouring.</para>
/// </summary>
public partial class Noise : SoloudObject
{
    public const int WHITE = 0;
    public const int PINK = 1;
    public const int BROWNISH = 2;
    public const int BLUEISH = 3;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Noise_create();
    public Noise()
    {
        objhandle = Noise_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Noise_destroy(IntPtr aObjHandle);
    ~Noise()
    {
        Noise_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_setOctaveScale(IntPtr aObjHandle, float aOct0, float aOct1, float aOct2, float aOct3, float aOct4, float aOct5, float aOct6, float aOct7, float aOct8, float aOct9);
    /// <summary>
    /// <para>Instead of selecting the noise type, you can also define the octave weights manually. The noise genrator has 10 octaves; the first one generates new value for every sample, the second for every second sample, the third for every 4 samples, 8, 16, 32, and so on.</para>
    ///
    /// <code>
    /// gNoise.setOctaveScale(0,0,0,0,0,1,0,0,0,0); // Very particular noise
    /// </code>
    ///
    /// <para>The weight values do not need to sum up to 1; the total will be used as a divisor. As such, the total must not be zero.</para>
    /// </summary>
    public void setOctaveScale(float aOct0, float aOct1, float aOct2, float aOct3, float aOct4, float aOct5, float aOct6, float aOct7, float aOct8, float aOct9)
    {
        Noise_setOctaveScale(objhandle, aOct0, aOct1, aOct2, aOct3, aOct4, aOct5, aOct6, aOct7, aOct8, aOct9);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_setType(IntPtr aObjHandle, int aType);
    /// <summary>
    /// <para>The type of the desired noise can be set before playing.</para>
    ///
    /// Parameter
    /// Effect
    ///
    /// WHITE
    /// White noise. 100% weight on the first octave.
    ///
    /// PINK
    /// Pink noise. Equal weight on all octaves.
    ///
    /// BROWNISH
    /// Brown-ish noise, increasing weight on deeper octaves.
    ///
    /// BLUEISH
    /// Blue-ish noise, decreasing weight on deeper octaves.
    ///
    /// <para>White noise is harsh, new random value on every sample kind of noise. This is the most useful one as base for filtering. The other kinds of noises may be useful as they are for background noise at low volume.</para>
    ///
    /// <code>
    /// gNoise.setType(SoLoud::Noise::PINK); // Some pink noise, please
    /// </code>
    /// </summary>
    public void setType(int aType)
    {
        Noise_setType(objhandle, aType);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    ///
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Noise_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Adjusting the looping of a noise sound does not have any effect.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Noise_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Noise_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Noise_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Noise_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Noise_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Noise_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Noise_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Noise_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Noise_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>Adjusting the looping of a noise sound does not have any effect.</para>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Noise_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Noise_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>Adjusting the looping of a noise sound does not have any effect.</para>
    /// </summary>
    public double getLoopPoint()
    {
        return Noise_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to monotone audio sources.</para>
    ///
    /// <code>
    /// gMusic.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Noise_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Noise_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a noise sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gMusic.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        Noise_stop(objhandle);
    }
}

/// <summary>
/// <para>Openmpt is a module-playing engine, capable of replaying wide variety of multichannel music (669, amf, ams, dbm, digi, dmf, dsm, far, gdm, ice, imf, it, itp, j2b, m15, mdl, med, mid, mo3, mod, mptm, mt2, mtm, okt, plm, psm, ptm, s3m, stm, ult, umx, wow, xm). It also loads wav files, and may support wider support for wav files than the stand-alone wav audio source.</para>
/// <para>SoLoud uses Openmpt through DLL, available from https://lib.openmpt.org/. Note that Openmpt has a more restrictive license than SoLoud, so be sure to check the license.</para>
/// <para>The midi formats (.mid and .abc) require a library of instruments (patches) to be available. One free set can be downloaded from the SoLoud downloads page.</para>
/// <para>If the DLL is not available, the SoLoud::Openmpt interfaces will silently fail.</para>
/// </summary>
public partial class Openmpt : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Openmpt_create();
    public Openmpt()
    {
        objhandle = Openmpt_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Openmpt_destroy(IntPtr aObjHandle);
    ~Openmpt()
    {
        Openmpt_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Openmpt_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    /// <summary>
    /// <para>You tell Openmpt to load a file with the load function:</para>
    ///
    /// <code>
    /// result load(const char *aFilename); // File to load
    /// </code>
    ///
    /// <para>If loading fails, the function returns an error code.</para>
    ///
    /// <code>
    /// SoLoud::Openmpt spacedeb;
    /// spacedeb.load("spacedeb.mod");
    /// </code>
    /// </summary>
    public int load(string aFilename)
    {
        return Openmpt_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Openmpt_loadMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    /// <summary>
    /// <para>Alternate way of loading the file is to read from a memory buffer.</para>
    ///
    /// <code>
    /// result loadMem(unsigned char *aMem, int aLength,
    ///                bool aCopy, bool aTakeOwnership);
    /// </code>
    ///
    /// <para>The aTakeOwnership parameter can be used to tell SoLoud to free the pointer once it's done with it. The aCopy parameter is here for compatibility with the other loadMem functions.</para>
    /// </summary>
    public int loadMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Openmpt_loadMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Openmpt_loadFile(IntPtr aObjHandle, IntPtr aFile);
    /// <summary>
    /// <para>The loadFile() can be used to load audio from a SoLoud::File object. This is useful for integrating with virtual filesystems / packfiles, such as PhysFS.</para>
    /// </summary>
    public int loadFile(SoloudObject aFile)
    {
        return Openmpt_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Openmpt_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Adjusting the looping of a Openmpt sound does not currently have any effect. All music is set to loop by default.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Openmpt_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Openmpt_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Openmpt_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Openmpt_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Openmpt_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Openmpt_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Openmpt_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Openmpt_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Openmpt_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    ///
    /// <para>Note that since looping occurs on openmpt's side, this setting currently has no effect.</para>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Openmpt_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Openmpt_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    ///
    /// <para>Note that since looping occurs on openmpt's side, this setting currently has no effect.</para>
    /// </summary>
    public double getLoopPoint()
    {
        return Openmpt_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to Openmpt audio sources.</para>
    ///
    /// <code>
    /// gMusic.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Openmpt_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Openmpt_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a Openmpt sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gMusic.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        Openmpt_stop(objhandle);
    }
}

/// <summary>
/// <para>Queue is a special audio source which can be used to queue other audio sources. Possible uses include making endless mix of music by queueing random music patterns, streaming generated audio by cycling a few Wav objects you rewrite with loadRaw(), concatenative speech synthesis, or just simply chaining a couple sounds for some reason.</para>
/// <para>The parameters of the played audio sources are ignored. This means that all of the audio sources should have the same parameters: sample rate, channels - and that the queue must be set up to those parameters before playing.</para>
/// <para>Only one instance of a queue can play at a time.</para>
/// </summary>
public partial class Queue : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Queue_create();
    public Queue()
    {
        objhandle = Queue_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Queue_destroy(IntPtr aObjHandle);
    ~Queue()
    {
        Queue_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Queue_play(IntPtr aObjHandle, IntPtr aSound);
    /// <summary>
    /// <para>Queues an audio source for playback.</para>
    ///
    /// <code>
    /// gQueue.play(gAsTimeGoesBy); // Play it, Sam
    /// </code>
    /// </summary>
    public int play(SoloudObject aSound)
    {
        return Queue_play(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint Queue_getQueueCount(IntPtr aObjHandle);
    /// <summary>
    /// <para>Returns the number of audio sources remaining in queue.</para>
    ///
    /// <code>
    /// // If queue is getting short, queue another pattern.
    /// if (gQueue.getQueueCount() &lt; 3)
    ///   gQueue.play(gPattern[rand()%8]);
    /// </code>
    /// </summary>
    public uint getQueueCount()
    {
        return Queue_getQueueCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Queue_isCurrentlyPlaying(IntPtr aObjHandle, IntPtr aSound);
    /// <summary>
    /// <para>Check if the audio source is currently playing in queue.</para>
    ///
    /// <code>
    /// if (gQueue.isCurrentlyPlaying(gTheGoodBit))
    ///     print("Yay, the good bit");
    /// </code>
    /// </summary>
    public int isCurrentlyPlaying(SoloudObject aSound)
    {
        return Queue_isCurrentlyPlaying(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Queue_setParamsFromAudioSource(IntPtr aObjHandle, IntPtr aSound);
    /// <summary>
    /// <para>Get audio playback parameters from an audio source, instead of specifying them explicitly.</para>
    ///
    /// <code>
    /// gQueue.setParamsFromAudioSource(gMusic);
    /// </code>
    /// </summary>
    public int setParamsFromAudioSource(SoloudObject aSound)
    {
        return Queue_setParamsFromAudioSource(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Queue_setParamsEx(IntPtr aObjHandle, float aSamplerate, uint aChannels);
    /// <summary>
    /// <para>Set audio parameters. Use this or the setParamsFromAudioSource() before using the queue.</para>
    ///
    /// <code>
    /// // Stereo 44.1kHz, just like the redbook ordered.
    /// gQueue.setParams(44100, 2);
    /// </code>
    /// </summary>
    public int setParams(float aSamplerate, uint aChannels = 2)
    {
        return Queue_setParamsEx(objhandle, aSamplerate, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the queue.</para>
    ///
    /// <code>
    /// gMusicBus.setVolume(11);
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Queue_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Trying to change the looping state of a queue has no effect.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Queue_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Queue_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Queue_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Queue_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Queue_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Queue_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Queue_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Queue_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Kill off the sfx if they're not heard.
    /// gFireworkQueue.setInaudibleBehavior(false, true);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Queue_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>Trying to change the looping state of a queue has no effect.</para>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Queue_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Queue_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>Trying to change the looping state of a queue has no effect.</para>
    /// </summary>
    public double getLoopPoint()
    {
        return Queue_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to queues.</para>
    ///
    /// <code>
    /// gMusicQueue.setFilter(0, &amp;gConcertHall);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Queue_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Queue_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    /// </summary>
    public void stop()
    {
        Queue_stop(objhandle);
    }
}

/// <summary>
/// <para>The robotize filter modulates the signal with a waveform. By default this is a square wave adjusted to have half of its period at zero, resulting at a choppy sound. Various waveforms can be selected for different effects.</para>
/// </summary>
public partial class RobotizeFilter : SoloudObject
{
    public const int WET = 0;
    public const int FREQ = 1;
    public const int WAVE = 2;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr RobotizeFilter_create();
    public RobotizeFilter()
    {
        objhandle = RobotizeFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr RobotizeFilter_destroy(IntPtr aObjHandle);
    ~RobotizeFilter()
    {
        RobotizeFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int RobotizeFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return RobotizeFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr RobotizeFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = RobotizeFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint RobotizeFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return RobotizeFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float RobotizeFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return RobotizeFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float RobotizeFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return RobotizeFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void RobotizeFilter_setParams(IntPtr aObjHandle, float aFreq, int aWaveform);
    /// <summary>
    /// <para>Set the parameters for the filter. Does not affect "live" sounds.</para>
    ///
    /// <code>
    /// robo.setParams(30, 0);
    /// </code>
    ///
    /// <para>Supported waveforms include:</para>
    ///
    /// Waveform
    /// Description
    ///
    /// WAVE_SQUARE
    /// Raw, harsh square wave
    ///
    /// WAVE_SAW
    /// Raw, harsh saw wave
    ///
    /// WAVE_SIN
    /// Sine wave
    ///
    /// WAVE_TRIANGLE
    /// Triangle wave
    ///
    /// WAVE_BOUNCE
    /// Bounce, i.e, abs(sin())
    ///
    /// WAVE_JAWS
    /// Quater sine wave, rest of period quiet
    ///
    /// WAVE_HUMPS
    /// Half sine wave, rest of period quiet
    ///
    /// WAVE_FSQUARE
    /// "Fourier" square wave; less noisy
    ///
    /// WAVE_FSAW
    /// "Fourier" saw wave; less noisy
    /// </summary>
    public void setParams(float aFreq, int aWaveform)
    {
        RobotizeFilter_setParams(objhandle, aFreq, aWaveform);
    }
}

/// <summary>
/// <para>The SoLoud::Sfxr is a "retro" sound effect synthesizer based on the original Sfxr by Tomas Pettersson.</para>
///
/// <para>The original sfxr tool was designed to easily generate sound effects for Ludum Dare 48h games. SoLoud includes the same engine built in, so you can (should you wish) make every coin, explosion etc. sound different.</para>
/// <para>The Sfxr sound sources also include a pseudo-random number generator which should probably be moved to more general use at some point.</para>
/// </summary>
public partial class Sfxr : SoloudObject
{
    public const int COIN = 0;
    public const int LASER = 1;
    public const int EXPLOSION = 2;
    public const int POWERUP = 3;
    public const int HURT = 4;
    public const int JUMP = 5;
    public const int BLIP = 6;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Sfxr_create();
    public Sfxr()
    {
        objhandle = Sfxr_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Sfxr_destroy(IntPtr aObjHandle);
    ~Sfxr()
    {
        Sfxr_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_resetParams(IntPtr aObjHandle);
    /// <summary>
    /// <para>This function resets all of the Sfxr parameters to a "sensible" default. Used by loadPreset(), which then only adjusts a few parameters over the defaults.</para>
    /// </summary>
    public void resetParams()
    {
        Sfxr_resetParams(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Sfxr_loadParams(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    /// <summary>
    /// <para>Effect parameters can also be loaded from a configuration file saved from the sfxr tool.</para>
    ///
    /// <code>
    /// result loadParams(const char *aFilename); // File to load
    /// </code>
    ///
    /// <para>If loading fails, the function returns an error code.</para>
    ///
    /// <code>
    /// SoLoud::Sfxr boom;
    /// boom.loadParams("boom.sfx");
    /// </code>
    /// </summary>
    public int loadParams(string aFilename)
    {
        return Sfxr_loadParams(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Sfxr_loadParamsMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    /// <summary>
    /// <para>Alternate way of loading the parameters is to read from a memory buffer.</para>
    ///
    /// <code>
    /// result loadParamsMem(unsigned char *aMem, int aLength,
    ///                      bool aCopy, bool aTakeOwnership);
    /// </code>
    ///
    /// <para>The aTakeOwnership parameter can be used to tell SoLoud to free the pointer once it's done with it. The aCopy parameter is here for compatibility with the other loadMem functions.</para>
    /// </summary>
    public int loadParamsMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Sfxr_loadParamsMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Sfxr_loadParamsFile(IntPtr aObjHandle, IntPtr aFile);
    /// <summary>
    /// <para>The loadFile() can be used to load parameters from a SoLoud::File object. This is useful for integrating with virtual filesystems / packfiles, such as PhysFS.</para>
    /// </summary>
    public int loadParamsFile(SoloudObject aFile)
    {
        return Sfxr_loadParamsFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Sfxr_loadPreset(IntPtr aObjHandle, int aPresetNo, int aRandSeed);
    /// <summary>
    /// <para>You can simply tell Sfxr to use one of the presets (COIN, LASER, EXPLOSION, POWERUP, HURT, JUMP, BLIP). Each of the presets has several random components, so you can get virtually unlimited variations of each. (Not all variants sound good, though).</para>
    ///
    /// <code>
    /// void loadPreset(int aPresetNo, int aRandSeed); // Preset to load
    /// </code>
    ///
    /// <para>If loading fails, the function returns an error code.</para>
    ///
    /// <code>
    /// SoLoud::Sfxr coin;
    /// coin.loadPreset(Sfxr::COIN, 3247);
    /// </code>
    /// </summary>
    public int loadPreset(int aPresetNo, int aRandSeed)
    {
        return Sfxr_loadPreset(objhandle, aPresetNo, aRandSeed);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Sfxr_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Adjusting the looping of a sfxr sound does not currently have any effect. The sounds do not loop.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Sfxr_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Sfxr_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Sfxr_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Sfxr_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Sfxr_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Sfxr_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Sfxr_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Sfxr_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Sfxr_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Sfxr_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Sfxr_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public double getLoopPoint()
    {
        return Sfxr_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to Sfxr audio sources.</para>
    ///
    /// <code>
    /// gMusic.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Sfxr_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Sfxr_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a Sfxr sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gBoom.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        Sfxr_stop(objhandle);
    }
}

/// <summary>
/// <para>The SoLoud::Speech class implements a simple Klatt-style formant speech synthesizer. It's barely legible, not really human-like, but it's free, and it's here.</para>
/// <para>Adjusting the speech synthesizer's output with audio filters should allow for various voices, which, along with subtitles, will let you add voice to your games cheaply.</para>
/// <para>For more serious use, feel free to study the source code and play with the various private parameters, as well as apply various filters to the sound.</para>
/// <para>For legal notes, please see the license page.</para>
/// </summary>
public partial class Speech : SoloudObject
{
    public const int KW_SAW = 0;
    public const int KW_TRIANGLE = 1;
    public const int KW_SIN = 2;
    public const int KW_SQUARE = 3;
    public const int KW_PULSE = 4;
    public const int KW_NOISE = 5;
    public const int KW_WARBLE = 6;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Speech_create();
    public Speech()
    {
        objhandle = Speech_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Speech_destroy(IntPtr aObjHandle);
    ~Speech()
    {
        Speech_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Speech_setText(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aText);
    /// <summary>
    /// <para>The setText function can be used to set the text to be spoken.</para>
    ///
    /// <code>
    /// SoLoud::Speech sp;
    /// sp.setText("Hello world.  You will be assimilated.");
    /// </code>
    ///
    /// <para>If the setText function is called while speech is playing, SoLoud stops any playing instances to avoid crashing.</para>
    /// <para>Trying to set the text to NULL will return an error code.</para>
    /// </summary>
    public int setText(string aText)
    {
        return Speech_setText(objhandle, aText);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Speech_setParamsEx(IntPtr aObjHandle, uint aBaseFrequency, float aBaseSpeed, float aBaseDeclination, int aBaseWaveform);
    /// <summary>
    /// <para>The speech's voice can be adjusted by changing the parameters;</para>
    ///
    /// <code>
    /// result Speech::setParams(unsigned int aBaseFrequency,
    ///                          float aBaseSpeed,
    ///                          float aBaseDeclination,
    ///                          int aBaseWaveform)
    /// </code>
    ///
    /// <para>The default parameters for these are 1330 for frequecy, 10 for speed, 0.5 for declination and KW_SQUARE as waveform. The easiest way to understand what these parameters do and how they affect the voice is to play with the "speechfilter" demo that is part of the megademo example.</para>
    /// </summary>
    public int setParams(uint aBaseFrequency = 1330, float aBaseSpeed = 10.0f, float aBaseDeclination = 0.5f, int aBaseWaveform = KW_TRIANGLE)
    {
        return Speech_setParamsEx(objhandle, aBaseFrequency, aBaseSpeed, aBaseDeclination, aBaseWaveform);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Speech_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>This function can be used to set the speech to loop.</para>
    ///
    /// <code>
    /// gHeyListen.setLooping(1);
    /// </code>
    ///
    /// <para>Calling this function will not affect "live" sound sources.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Speech_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Speech_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Speech_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Speech_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Speech_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Speech_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Speech_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Speech_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Speech_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Speech_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Speech_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public double getLoopPoint()
    {
        return Speech_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to speech audio sources.</para>
    ///
    /// <code>
    /// gRobot.setFilter(0, &amp;gRobotize);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Speech_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Speech_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a speech sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gHeyListen.stop(); // shut up already!
    /// </code>
    /// </summary>
    public void stop()
    {
        Speech_stop(objhandle);
    }
}

/// <summary>
/// <para>The SoLoud::TedSid is a replayer for TED and SID soundchip register write dumps, based on tedplay (c) 2012 Attila Grosz, used under Unlicense http://unlicense.org/.</para>
/// <para>TED is the soundchip of the commodore plus/4, and SID is the soundchip of the commodore 64.</para>
/// <para>The TED and SID songs are actually complete c64 or plus/4 programs, so in order to avoid running a complete c64 emulator in an audio engine, we only simulate the soundchips at real time. To generate the dumps, you can use the tedsid2dump.exe tool.</para>
/// <para>You can use the Soloud.getInfo() interface to query TED and SID register values while the song is playing. The SID registers are mapped to values 0-31 and the TED registers to 64-69.</para>
/// </summary>
public partial class TedSid : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr TedSid_create();
    public TedSid()
    {
        objhandle = TedSid_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr TedSid_destroy(IntPtr aObjHandle);
    ~TedSid()
    {
        TedSid_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int TedSid_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    /// <summary>
    /// <para>You tell TedSid to load a file with the load function:</para>
    ///
    /// <code>
    /// result load(const char *aFilename);
    /// </code>
    ///
    /// <para>If loading fails, the function returns an error code.</para>
    ///
    /// <code>
    /// SoLoud::TedSid galway;
    /// galway.load("galway.sid.dump");
    /// </code>
    /// </summary>
    public int load(string aFilename)
    {
        return TedSid_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int TedSid_loadToMem(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    /// <summary>
    /// <para>The loadToMem() tells SoLoud to load the whole file to a memory buffer and stream it from there.</para>
    /// </summary>
    public int loadToMem(string aFilename)
    {
        return TedSid_loadToMem(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int TedSid_loadMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    /// <summary>
    /// <para>Alternate way of loading the file is to read from a memory buffer.</para>
    ///
    /// <code>
    /// result loadMem(unsigned char *aMem, int aLength,
    ///                bool aCopy, bool aTakeOwnership);
    /// </code>
    ///
    /// <para>SoLoud function assumes that the pointer and the data pointed will be valid as long as SoLoud needs them. You can use the aCopy parameter to tell SoLoud to take a copy of the data instead of using the pointers directly, and the aTakeOwnership parameter to tell SoLoud to free the pointer when the object is being destroyed.</para>
    /// </summary>
    public int loadMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return TedSid_loadMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int TedSid_loadFileToMem(IntPtr aObjHandle, IntPtr aFile);
    /// <summary>
    /// <para>The loadFileToMem() function performs the memory loading of loadToMem() using SoLoud::File objects, same way as loadFile() does.</para>
    /// </summary>
    public int loadFileToMem(SoloudObject aFile)
    {
        return TedSid_loadFileToMem(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int TedSid_loadFile(IntPtr aObjHandle, IntPtr aFile);
    /// <summary>
    /// <para>The loadFile() can be used to load audio from a SoLoud::File object. This is useful for integrating with virtual filesystems / packfiles, such as PhysFS.</para>
    /// </summary>
    public int loadFile(SoloudObject aFile)
    {
        return TedSid_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        TedSid_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Adjusting the looping of a TedSid sound does not currently have any effect. All music is set to loop by default.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        TedSid_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        TedSid_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        TedSid_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        TedSid_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        TedSid_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        TedSid_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        TedSid_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        TedSid_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        TedSid_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        TedSid_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double TedSid_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public double getLoopPoint()
    {
        return TedSid_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to monotone audio sources.</para>
    ///
    /// <code>
    /// gMusic.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        TedSid_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void TedSid_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a monotone sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gMusic.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        TedSid_stop(objhandle);
    }
}

/// <summary>
/// <para>The SoLoud::Vic is a vic-20 audio emulator. It is used by adjusting registers directly, like on the actual device.</para>
/// <para>This audio source is a bit of a work in progress. All instances of a single Vic object will play exactly the same audio, and registers are set at the object level.</para>
/// </summary>
public partial class Vic : SoloudObject
{
    public const int PAL = 0;
    public const int NTSC = 1;
    public const int BASS = 0;
    public const int ALTO = 1;
    public const int SOPRANO = 2;
    public const int NOISE = 3;
    public const int MAX_REGS = 4;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Vic_create();
    public Vic()
    {
        objhandle = Vic_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Vic_destroy(IntPtr aObjHandle);
    ~Vic()
    {
        Vic_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_setModel(IntPtr aObjHandle, int model);
    /// <summary>
    /// <para>The model of machine, between PAL and NTSC, can be set and queried. PAL is the default.</para>
    ///
    /// <code>
    /// // Set model to NTSC
    /// gMyVic.setModel(SoLoud::Vic::NTSC);
    /// </code>
    /// </summary>
    public void setModel(int model)
    {
        Vic_setModel(objhandle, model);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Vic_getModel(IntPtr aObjHandle);
    /// <summary>
    /// <para>The model of machine, between PAL and NTSC, can be set and queried. PAL is the default.</para>
    ///
    /// <code>
    /// // Set model to NTSC
    /// gMyVic.setModel(SoLoud::Vic::NTSC);
    /// </code>
    /// </summary>
    public int getModel()
    {
        return Vic_getModel(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_setRegister(IntPtr aObjHandle, int reg, byte value);
    /// <summary>
    /// <para>Registers can be set and read. All of the four registers are 8 bits wide.</para>
    ///
    /// <code>
    /// // Increment register 2
    /// unsigned char v = gMyVic.getRegister(2);
    /// gMyVic.setRegister(2, v + 1);
    /// </code>
    /// </summary>
    public void setRegister(int reg, byte value)
    {
        Vic_setRegister(objhandle, reg, value);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial byte Vic_getRegister(IntPtr aObjHandle, int reg);
    /// <summary>
    /// <para>Registers can be set and read. All of the four registers are 8 bits wide.</para>
    ///
    /// <code>
    /// // Increment register 2
    /// unsigned char v = gMyVic.getRegister(2);
    /// gMyVic.setRegister(2, v + 1);
    /// </code>
    /// </summary>
    public byte getRegister(int reg)
    {
        return Vic_getRegister(objhandle, reg);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    ///
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Vic_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Adjusting the looping of a vic sound does not have any effect.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Vic_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Vic_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Vic_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Vic_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Vic_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Vic_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Vic_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Vic_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Vic_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>Adjusting the looping of a vic sound does not have any effect.</para>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Vic_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Vic_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>Adjusting the looping of a vic sound does not have any effect.</para>
    /// </summary>
    public double getLoopPoint()
    {
        return Vic_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to monotone audio sources.</para>
    ///
    /// <code>
    /// gMusic.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Vic_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vic_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a vic sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gMusic.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        Vic_stop(objhandle);
    }
}

/// <summary>
/// <para>Vizsn is another free, but primitive speech synthesizer.</para>
/// <para>This audio source is a bit of a work in progress.</para>
/// </summary>
public partial class Vizsn : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Vizsn_create();
    public Vizsn()
    {
        objhandle = Vizsn_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Vizsn_destroy(IntPtr aObjHandle);
    ~Vizsn()
    {
        Vizsn_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_setText(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aText);
    /// <summary>
    /// <para>The words to be spoken are set with setText(). Calling this while the audio source is in use may cause undefined behavior.</para>
    ///
    /// <code>
    /// // Speak the truth
    /// gTalk.setText("You probably won't understand what I'm saying");
    /// </code>
    /// </summary>
    public void setText(string aText)
    {
        Vizsn_setText(objhandle, aText);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    ///
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Vizsn_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>Adjusting the looping of a monotone sound does not currently have any effect. All music is set to loop by default.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Vizsn_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Vizsn_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Vizsn_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Vizsn_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Vizsn_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Vizsn_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Vizsn_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Vizsn_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gTalk.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Vizsn_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Vizsn_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Vizsn_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public double getLoopPoint()
    {
        return Vizsn_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to monotone audio sources.</para>
    ///
    /// <code>
    /// gTalk.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Vizsn_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Vizsn_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a monotone sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// // Shut up.
    /// gTalk.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        Vizsn_stop(objhandle);
    }
}

/// <summary>
/// <para>The SoLoud::Wav class represents a wave sound effect. The source files may be in various RIFF WAV file formats, FLAC, MP3 or Ogg Vorbis files.</para>
/// <para>The sounds are loaded and converted to float samples, which means that every second of a 44100Hz stereo sound takes about 350kB of memory. The good side is, after loading, the use of these samples are very lightweight, as their processing is mostly just a memory copy.</para>
/// <para>For lengthy samples like background music, you may want to use SoLoud::WavStream instead. The Wav is all about speed, and always decodes the whole sample into memory at load time.</para>
/// </summary>
public partial class Wav : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Wav_create();
    public Wav()
    {
        objhandle = Wav_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr Wav_destroy(IntPtr aObjHandle);
    ~Wav()
    {
        Wav_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Wav_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    /// <summary>
    /// <para>The wav loader takes just one parameter, the file name:</para>
    ///
    /// <code>
    /// void load(const char *aFilename); // File to load
    /// </code>
    ///
    /// <para>If loading fails, the sample will be silent.</para>
    ///
    /// <code>
    /// SoLoud::Wav boom;
    /// boom.load("boom.wav");
    /// </code>
    ///
    /// <para>If the loading function is called while there are instances playing, the result is undefined (most likely a crash).</para>
    /// </summary>
    public int load(string aFilename)
    {
        return Wav_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Wav_loadMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    /// <summary>
    /// <para>Alternate way of loading samples is to read from a memory buffer.</para>
    ///
    /// <code>
    /// result loadMem(unsigned char *aMem, int aLength,
    ///                bool aCopy, bool aTakeOwnership);
    /// </code>
    ///
    /// <para>If loading fails, the sample will be silent.</para>
    ///
    /// <code>
    /// SoLoud::Wav boom;
    /// boom.loadMem(boomMemoryResource, boomMemoryResourceLength);
    /// </code>
    ///
    /// <para>SoLoud function assumes that the pointer and the data pointed will be valid as long as SoLoud needs them. You can use the aCopy parameter to tell SoLoud to take a copy of the data instead of using the pointers directly, and the aTakeOwnership parameter to tell SoLoud to free the pointer when the object is being destroyed.</para>
    /// </summary>
    public int loadMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Wav_loadMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Wav_loadFile(IntPtr aObjHandle, IntPtr aFile);
    /// <summary>
    /// <para>The loadFile() can be used to load audio from a SoLoud::File object. This is useful for integrating with virtual filesystems / packfiles, such as PhysFS.</para>
    /// </summary>
    public int loadFile(SoloudObject aFile)
    {
        return Wav_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Wav_loadRawWave8Ex(IntPtr aObjHandle, IntPtr aMem, uint aLength, float aSamplerate, uint aChannels);
    /// <summary>
    /// <para>It is also possible to turn an array of raw wave data into a SoLoud Wav object using the loadRawWave family of functions. These functions differ primarily by the format of wave data they load - floats, unsigned 8 bit and signed 16 bit samples.</para>
    ///
    /// <code>
    ///     result loadRawWave8(unsigned char *aMem,
    ///                         unsigned int aLength,
    ///                         float aSamplerate = 44100.0f,
    ///                         unsigned int aChannels = 1);
    ///     result loadRawWave16(short *aMem,
    ///                          unsigned int aLength,
    ///                          float aSamplerate = 44100.0f,
    ///                          unsigned int aChannels = 1);
    ///     result loadRawWave(float *aMem,
    ///                        unsigned int aLength,
    ///                        float aSamplerate = 44100.0f,
    ///                        unsigned int aChannels = 1,
    ///                        bool aCopy = false,
    ///                        bool aTakeOwnership = true);
    /// </code>
    ///
    /// <para>The variant that loads float samples additionally lets you decice whether SoLoud should make a copy of the data (such as when the data is in stack), and whether SoLoud should take ownership of the data (i.e, should SoLoud delete the data when the object is destroyed).</para>
    /// </summary>
    public int loadRawWave8(IntPtr aMem, uint aLength, float aSamplerate = 44100.0f, uint aChannels = 1)
    {
        return Wav_loadRawWave8Ex(objhandle, aMem, aLength, aSamplerate, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Wav_loadRawWave16Ex(IntPtr aObjHandle, IntPtr aMem, uint aLength, float aSamplerate, uint aChannels);
    /// <summary>
    /// <para>It is also possible to turn an array of raw wave data into a SoLoud Wav object using the loadRawWave family of functions. These functions differ primarily by the format of wave data they load - floats, unsigned 8 bit and signed 16 bit samples.</para>
    ///
    /// <code>
    ///     result loadRawWave8(unsigned char *aMem,
    ///                         unsigned int aLength,
    ///                         float aSamplerate = 44100.0f,
    ///                         unsigned int aChannels = 1);
    ///     result loadRawWave16(short *aMem,
    ///                          unsigned int aLength,
    ///                          float aSamplerate = 44100.0f,
    ///                          unsigned int aChannels = 1);
    ///     result loadRawWave(float *aMem,
    ///                        unsigned int aLength,
    ///                        float aSamplerate = 44100.0f,
    ///                        unsigned int aChannels = 1,
    ///                        bool aCopy = false,
    ///                        bool aTakeOwnership = true);
    /// </code>
    ///
    /// <para>The variant that loads float samples additionally lets you decice whether SoLoud should make a copy of the data (such as when the data is in stack), and whether SoLoud should take ownership of the data (i.e, should SoLoud delete the data when the object is destroyed).</para>
    /// </summary>
    public int loadRawWave16(IntPtr aMem, uint aLength, float aSamplerate = 44100.0f, uint aChannels = 1)
    {
        return Wav_loadRawWave16Ex(objhandle, aMem, aLength, aSamplerate, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int Wav_loadRawWaveEx(IntPtr aObjHandle, float[] aMem, uint aLength, float aSamplerate, uint aChannels, int aCopy, int aTakeOwnership);
    /// <summary>
    /// <para>It is also possible to turn an array of raw wave data into a SoLoud Wav object using the loadRawWave family of functions. These functions differ primarily by the format of wave data they load - floats, unsigned 8 bit and signed 16 bit samples.</para>
    ///
    /// <code>
    ///     result loadRawWave8(unsigned char *aMem,
    ///                         unsigned int aLength,
    ///                         float aSamplerate = 44100.0f,
    ///                         unsigned int aChannels = 1);
    ///     result loadRawWave16(short *aMem,
    ///                          unsigned int aLength,
    ///                          float aSamplerate = 44100.0f,
    ///                          unsigned int aChannels = 1);
    ///     result loadRawWave(float *aMem,
    ///                        unsigned int aLength,
    ///                        float aSamplerate = 44100.0f,
    ///                        unsigned int aChannels = 1,
    ///                        bool aCopy = false,
    ///                        bool aTakeOwnership = true);
    /// </code>
    ///
    /// <para>The variant that loads float samples additionally lets you decice whether SoLoud should make a copy of the data (such as when the data is in stack), and whether SoLoud should take ownership of the data (i.e, should SoLoud delete the data when the object is destroyed).</para>
    /// </summary>
    public int loadRawWave(float[] aMem, uint aLength, float aSamplerate = 44100.0f, uint aChannels = 1, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Wav_loadRawWaveEx(objhandle, aMem, aLength, aSamplerate, aChannels, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Wav_getLength(IntPtr aObjHandle);
    /// <summary>
    /// <para>The length, in seconds, of this wave can be queried with this function.</para>
    ///
    /// <code>
    /// double t = gRecord.getLength();
    /// </code>
    /// </summary>
    public double getLength()
    {
        return Wav_getLength(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        Wav_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>This function can be used to set the wave to loop.</para>
    ///
    /// <code>
    /// gDrumloop.setLooping(1);
    /// </code>
    ///
    /// <para>Calling this function will not affect "live" sound sources.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        Wav_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Wav_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Wav_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Wav_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        Wav_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Wav_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Wav_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Wav_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Wav_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        Wav_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double Wav_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public double getLoopPoint()
    {
        return Wav_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to wave audio sources.</para>
    ///
    /// <code>
    /// gHipster.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Wav_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void Wav_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a wave sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gHammertime.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        Wav_stop(objhandle);
    }
}

/// <summary>
/// <para>The waveshaper filter is an experimental filter that shapes the input wave by a simple function. It is mostly meant as a voice distortion filter.</para>
/// <para>There is one parameter to adjust the severity of the effect.</para>
///
/// <code>
/// // Set up wave shaper
/// gWaveshaper.setParams(0.1f, 1);
/// // Set the filter as the first filter of the bus
/// gBus.setFilter(0, &amp;gWaveshaper);
/// </code>
/// </summary>
public partial class WaveShaperFilter : SoloudObject
{
    public const int WET = 0;
    public const int AMOUNT = 1;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr WaveShaperFilter_create();
    public WaveShaperFilter()
    {
        objhandle = WaveShaperFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr WaveShaperFilter_destroy(IntPtr aObjHandle);
    ~WaveShaperFilter()
    {
        WaveShaperFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int WaveShaperFilter_setParams(IntPtr aObjHandle, float aAmount);
    /// <summary>
    /// <para>Set the parameters of the filter.</para>
    ///
    /// <code>
    /// gWaveshaper.setParams(0.3f, 1);
    /// </code>
    ///
    /// <para>Changing the parameters via setParams() does not affect "live" sounds. Live parameters can be set, faded or oscillated:</para>
    ///
    /// <code>
    /// gSoloud.fadeFilterParameter(
    ///   gMusicHandle, // Sound handle
    ///   0,            // First filter
    ///   1,            // What to adjust
    ///   0,            // Target value
    ///   3);           // Time in seconds
    /// </code>
    /// </summary>
    public int setParams(float aAmount)
    {
        return WaveShaperFilter_setParams(objhandle, aAmount);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int WaveShaperFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return WaveShaperFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr WaveShaperFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = WaveShaperFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial uint WaveShaperFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return WaveShaperFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float WaveShaperFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return WaveShaperFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial float WaveShaperFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return WaveShaperFilter_getParamMin(objhandle, aParamIndex);
    }
}

/// <summary>
/// <para>The SoLoud::WavStream class represents a wave sound effect that is streamed off disk while it's playing. The source files may be in various RIFF WAV file formats, FLAC, MP3 or Ogg Vorbis files.</para>
/// <para>The sounds are loaded in pieces while they are playing, which takes more processing power than playing samples from memory, but they require much less memory.</para>
/// <para>For short or often used samples, you may want to use SoLoud::Wav instead.</para>
/// </summary>
public partial class WavStream : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr WavStream_create();
    public WavStream()
    {
        objhandle = WavStream_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial IntPtr WavStream_destroy(IntPtr aObjHandle);
    ~WavStream()
    {
        WavStream_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int WavStream_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    /// <summary>
    /// <para>The wav loader takes just one parameter, the file name:</para>
    ///
    /// <code>
    /// result load(const char *aFilename); // File to load
    /// </code>
    ///
    /// <para>If loading fails, the function will return an error code.</para>
    ///
    /// <code>
    /// SoLoud::WavStream muzak;
    /// muzak.load("elevator.ogg");
    /// </code>
    ///
    /// <para>If the loading function is called while there are instances playing, the result is undefined (most likely a crash).</para>
    /// </summary>
    public int load(string aFilename)
    {
        return WavStream_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int WavStream_loadMemEx(IntPtr aObjHandle, IntPtr aData, uint aDataLen, int aCopy, int aTakeOwnership);
    /// <summary>
    /// <para>Alternate way of loading samples is to read from a memory buffer.</para>
    ///
    /// <code>
    /// result loadMem(unsigned char *aMem, int aLength,
    ///                bool aCopy, bool aTakeOwnership);
    /// </code>
    ///
    /// <para>If loading fails, the sample will be silent.</para>
    ///
    /// <code>
    /// SoLoud::WavStream boom;
    /// boom.loadMem(boomMemoryResource, boomMemoryResourceLength);
    /// </code>
    ///
    /// <para>SoLoud function assumes that the pointer and the data pointed will be valid as long as SoLoud needs them. You can use the aCopy parameter to tell SoLoud to take a copy of the data instead of using the pointers directly, and the aTakeOwnership parameter to tell SoLoud to free the pointer when the object is being destroyed.</para>
    /// </summary>
    public int loadMem(IntPtr aData, uint aDataLen, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return WavStream_loadMemEx(objhandle, aData, aDataLen, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int WavStream_loadToMem(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    /// <summary>
    /// <para>The loadToMem() tells SoLoud to load the whole file to a memory buffer and stream it from there. This is similar as to using the Wav object, except that the data is not decoded to raw samples on load.</para>
    /// <para>This can be useful if you expect the media the data resides on to be slow or busy, but don't want to spend the memory for the completely decoded audio data.</para>
    /// </summary>
    public int loadToMem(string aFilename)
    {
        return WavStream_loadToMem(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int WavStream_loadFile(IntPtr aObjHandle, IntPtr aFile);
    /// <summary>
    /// <para>The loadFile() can be used to load audio from a SoLoud::File object. This is useful for integrating with virtual filesystems / packfiles, such as PhysFS.</para>
    /// </summary>
    public int loadFile(SoloudObject aFile)
    {
        return WavStream_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial int WavStream_loadFileToMem(IntPtr aObjHandle, IntPtr aFile);
    /// <summary>
    /// <para>The loadFileToMem() function performs the memory loading of loadToMem() using SoLoud::File objects, same way as loadFile() does.</para>
    /// </summary>
    public int loadFileToMem(SoloudObject aFile)
    {
        return WavStream_loadFileToMem(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double WavStream_getLength(IntPtr aObjHandle);
    /// <summary>
    /// <para>The length, in seconds, of this wav stream can be queried with this function.</para>
    ///
    /// <code>
    /// double t = gRecord.getLength();
    /// </code>
    /// </summary>
    public double getLength()
    {
        return WavStream_getLength(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_setVolume(IntPtr aObjHandle, float aVolume);
    /// <summary>
    /// <para>Set the default volume of the instances created from this audio source.</para>
    ///
    /// <code>
    /// gTinyVoice.setVolume(0.1);
    /// </code>
    /// </summary>
    public void setVolume(float aVolume)
    {
        WavStream_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_setLooping(IntPtr aObjHandle, int aLoop);
    /// <summary>
    /// <para>This function can be used to set the wav stream to loop.</para>
    ///
    /// <code>
    /// soundtrack.setLooping(1);
    /// </code>
    ///
    /// <para>Calling this function will not affect "live" sound sources.</para>
    /// </summary>
    public void setLooping(int aLoop)
    {
        WavStream_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    /// <summary>
    /// <para>Set the minimum and maximum distances for the audio source with set3dMinMaxDistance()</para>
    ///
    /// <code>
    /// void set3dMinMaxDistance(float aMinDistance,
    ///                          float aMaxDistance)
    /// </code>
    ///
    /// <para>Default values are 1 and 1000000.</para>
    /// <para>See the 3d audio concepts chapter for the meaning of these parameters.</para>
    /// </summary>
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        WavStream_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    /// <summary>
    /// <para>Set the attenuation model and rolloff factor with set3dAttenuation()</para>
    ///
    /// <code>
    /// void set3dAttenuation(unsigned int aAttenuationModel,
    ///                       float aAttenuationRolloffFactor)
    /// </code>
    ///
    /// <para>The default values are NO_ATTENUATION and 1.</para>
    ///
    /// Constant
    /// Model
    ///
    /// NO_ATTENUATION
    /// No attenuation
    ///
    /// INVERSE_DISTANCE
    /// Inverse distance attenuation model
    ///
    /// LINEAR_DISTANCE
    /// Linear distance attenuation model
    ///
    /// EXPONENTIAL_DISTANCE
    /// Exponential distance attenuation model
    ///
    /// <para>See the 3d audio concepts chapter for the meaning of the models and the rolloff parameter.</para>
    /// </summary>
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        WavStream_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    /// <summary>
    /// <para>Set the doppler factor with set3dDopplerFactor().</para>
    ///
    /// <code>
    /// void set3dDopplerFactor(float aDopplerFactor)
    /// </code>
    ///
    /// <para>The default value is 1.</para>
    /// </summary>
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        WavStream_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    /// <summary>
    /// <para>Enable or disable listener relativity for a 3d audio source. If a sound is listener-relative, the listener's coordinates are assumed to be (0,0,0) in calculations.</para>
    ///
    /// <code>
    /// void set3dListenerRelative(bool aListenerRelative)
    /// </code>
    ///
    /// <para>Default is disabled. Some custom colliders will depend on this flag to be enabled.</para>
    /// </summary>
    public void set3dListenerRelative(int aListenerRelative)
    {
        WavStream_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    /// <summary>
    /// <para>Enable or disable the distance delay effect for a 3d audio source. Since speed of sound is way slower than speed of light, in reality we might see an explosion before we hear it.</para>
    ///
    /// <code>
    /// void set3dDistanceDelay(bool aDistanceDelay)
    /// </code>
    ///
    /// <para>Default is disabled, as this may be seen as a glitch since most games do not bother simulating this.</para>
    /// <para>Note that this will only affect the start time of the sound.</para>
    /// </summary>
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        WavStream_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    /// <summary>
    /// <para>Set or clear the custom audio collider and collider's user data for a 3d audio source.</para>
    ///
    /// <code>
    /// void set3dCollider(AudioCollider *aCollider,
    ///                    int aUserData)
    ///
    /// </code>
    ///
    /// <para>SoLoud expects the collider to be there until all instances of the sound have stopped. Application is responsible for cleaning up the collider. Several sound sources may use the same collider.</para>
    /// </summary>
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        WavStream_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    /// <summary>
    /// <para>Set a custom attenuator for the instances created from this audio source. use NULL to disable.</para>
    ///
    /// <code>
    /// void set3dAttenuator(AudioAttenuator *aAttenuator)
    /// </code>
    /// </summary>
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        WavStream_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    /// <summary>
    /// <para>Set the inaudible behavior of the sound. By default, if a sound is inaudible, it's paused, and will resume when it becomes audible again. With this function you can tell SoLoud to either kill the sound if it becomes inaudible, or to keep ticking the sound even if it's inaudible.</para>
    ///
    /// <code>
    /// // Keep on talking even if I'm not around
    /// gSpeech.setInaudibleBehavior(true, false);
    /// </code>
    /// </summary>
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        WavStream_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public void setLoopPoint(double aLoopPoint)
    {
        WavStream_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial double WavStream_getLoopPoint(IntPtr aObjHandle);
    /// <summary>
    /// <para>If looping is enabled, the loop point is, by default, the start of the stream. The loop point can be changed, and current loop point can be queried with these functions.</para>
    ///
    /// <code>
    /// double t = snd.getLoopPoint();
    /// snd.setLoopPoint(t + 1); // skip 1 second more when looping
    /// ...
    /// snd.setLoopPoint(0); // loop from start
    /// </code>
    /// </summary>
    public double getLoopPoint()
    {
        return WavStream_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    /// <summary>
    /// <para>As with any other audio source, you can attach filters to wav stream audio sources.</para>
    ///
    /// <code>
    /// gHipster.setFilter(0, &amp;gLofi);
    /// </code>
    /// </summary>
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        WavStream_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static partial void WavStream_stop(IntPtr aObjHandle);
    /// <summary>
    /// <para>You can stop all instances of a wav stream sound source with stop(). This is equivalent of calling soloud.stopAudioSource() with the sound source.</para>
    ///
    /// <code>
    /// gHammertime.stop();
    /// </code>
    /// </summary>
    public void stop()
    {
        WavStream_stop(objhandle);
    }
}