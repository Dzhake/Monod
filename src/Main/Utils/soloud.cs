
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
/// Bindings for the Soloud library.
/// </summary>
public partial class Soloud : SoloudObject
{
    /// <summary>
    /// Represents the 'default' value for the argument. Seriously, couldn't you just use default values?
    /// </summary>
    public const uint AUTO = 0;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Soloud_create();
    public Soloud()
    {
        objhandle = Soloud_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Soloud_destroy(IntPtr aObjHandle);
    ~Soloud()
    {
        Soloud_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_initEx(IntPtr aObjHandle, SoloudInitFlags aFlags, SoloudBackend aBackend, uint aSamplerate, uint aBufferSize, uint aChannels);
    public int init(SoloudInitFlags aFlags = SoloudInitFlags.CLIP_ROUNDOFF, SoloudBackend aBackend = SoloudBackend.AUTO, uint aSamplerate = AUTO, uint aBufferSize = AUTO, uint aChannels = 2)
    {
        return Soloud_initEx(objhandle, aFlags, aBackend, aSamplerate, aBufferSize, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_deinit(IntPtr aObjHandle);
    public void deinit()
    {
        Soloud_deinit(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getVersion(IntPtr aObjHandle);
    public uint getVersion()
    {
        return Soloud_getVersion(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Soloud_getErrorString(IntPtr aObjHandle, int aErrorCode);
    public string getErrorString(int aErrorCode)
    {
        IntPtr p = Soloud_getErrorString(objhandle, aErrorCode);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getBackendId(IntPtr aObjHandle);
    public uint getBackendId()
    {
        return Soloud_getBackendId(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Soloud_getBackendString(IntPtr aObjHandle);
    public string getBackendString()
    {
        IntPtr p = Soloud_getBackendString(objhandle);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getBackendChannels(IntPtr aObjHandle);
    public uint getBackendChannels()
    {
        return Soloud_getBackendChannels(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getBackendSamplerate(IntPtr aObjHandle);
    public uint getBackendSamplerate()
    {
        return Soloud_getBackendSamplerate(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getBackendBufferSize(IntPtr aObjHandle);
    public uint getBackendBufferSize()
    {
        return Soloud_getBackendBufferSize(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_setSpeakerPosition(IntPtr aObjHandle, uint aChannel, float aX, float aY, float aZ);
    public int setSpeakerPosition(uint aChannel, float aX, float aY, float aZ)
    {
        return Soloud_setSpeakerPosition(objhandle, aChannel, aX, aY, aZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_getSpeakerPosition(IntPtr aObjHandle, uint aChannel, float[] aX, float[] aY, float[] aZ);
    public int getSpeakerPosition(uint aChannel, float[] aX, float[] aY, float[] aZ)
    {
        return Soloud_getSpeakerPosition(objhandle, aChannel, aX, aY, aZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_playEx(IntPtr aObjHandle, IntPtr aSound, float aVolume, float aPan, int aPaused, uint aBus);
    public uint play(SoloudObject aSound, float aVolume = -1.0f, float aPan = 0.0f, int aPaused = 0, uint aBus = 0)
    {
        return Soloud_playEx(objhandle, aSound.objhandle, aVolume, aPan, aPaused, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_playClockedEx(IntPtr aObjHandle, double aSoundTime, IntPtr aSound, float aVolume, float aPan, uint aBus);
    public uint playClocked(double aSoundTime, SoloudObject aSound, float aVolume = -1.0f, float aPan = 0.0f, uint aBus = 0)
    {
        return Soloud_playClockedEx(objhandle, aSoundTime, aSound.objhandle, aVolume, aPan, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_play3dEx(IntPtr aObjHandle, IntPtr aSound, float aPosX, float aPosY, float aPosZ, float aVelX, float aVelY, float aVelZ, float aVolume, int aPaused, uint aBus);
    public uint play3d(SoloudObject aSound, float aPosX, float aPosY, float aPosZ, float aVelX = 0.0f, float aVelY = 0.0f, float aVelZ = 0.0f, float aVolume = 1.0f, int aPaused = 0, uint aBus = 0)
    {
        return Soloud_play3dEx(objhandle, aSound.objhandle, aPosX, aPosY, aPosZ, aVelX, aVelY, aVelZ, aVolume, aPaused, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_play3dClockedEx(IntPtr aObjHandle, double aSoundTime, IntPtr aSound, float aPosX, float aPosY, float aPosZ, float aVelX, float aVelY, float aVelZ, float aVolume, uint aBus);
    public uint play3dClocked(double aSoundTime, SoloudObject aSound, float aPosX, float aPosY, float aPosZ, float aVelX = 0.0f, float aVelY = 0.0f, float aVelZ = 0.0f, float aVolume = 1.0f, uint aBus = 0)
    {
        return Soloud_play3dClockedEx(objhandle, aSoundTime, aSound.objhandle, aPosX, aPosY, aPosZ, aVelX, aVelY, aVelZ, aVolume, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_playBackgroundEx(IntPtr aObjHandle, IntPtr aSound, float aVolume, int aPaused, uint aBus);
    public uint playBackground(SoloudObject aSound, float aVolume = -1.0f, int aPaused = 0, uint aBus = 0)
    {
        return Soloud_playBackgroundEx(objhandle, aSound.objhandle, aVolume, aPaused, aBus);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_seek(IntPtr aObjHandle, uint aVoiceHandle, double aSeconds);
    public int seek(uint aVoiceHandle, double aSeconds)
    {
        return Soloud_seek(objhandle, aVoiceHandle, aSeconds);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_stop(IntPtr aObjHandle, uint aVoiceHandle);
    public void stop(uint aVoiceHandle)
    {
        Soloud_stop(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_stopAll(IntPtr aObjHandle);
    public void stopAll()
    {
        Soloud_stopAll(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_stopAudioSource(IntPtr aObjHandle, IntPtr aSound);
    public void stopAudioSource(SoloudObject aSound)
    {
        Soloud_stopAudioSource(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_countAudioSource(IntPtr aObjHandle, IntPtr aSound);
    public int countAudioSource(SoloudObject aSound)
    {
        return Soloud_countAudioSource(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setFilterParameter(IntPtr aObjHandle, uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aValue);
    public void setFilterParameter(uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aValue)
    {
        Soloud_setFilterParameter(objhandle, aVoiceHandle, aFilterId, aAttributeId, aValue);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getFilterParameter(IntPtr aObjHandle, uint aVoiceHandle, uint aFilterId, uint aAttributeId);
    public float getFilterParameter(uint aVoiceHandle, uint aFilterId, uint aAttributeId)
    {
        return Soloud_getFilterParameter(objhandle, aVoiceHandle, aFilterId, aAttributeId);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_fadeFilterParameter(IntPtr aObjHandle, uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aTo, double aTime);
    public void fadeFilterParameter(uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aTo, double aTime)
    {
        Soloud_fadeFilterParameter(objhandle, aVoiceHandle, aFilterId, aAttributeId, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_oscillateFilterParameter(IntPtr aObjHandle, uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aFrom, float aTo, double aTime);
    public void oscillateFilterParameter(uint aVoiceHandle, uint aFilterId, uint aAttributeId, float aFrom, float aTo, double aTime)
    {
        Soloud_oscillateFilterParameter(objhandle, aVoiceHandle, aFilterId, aAttributeId, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Soloud_getStreamTime(IntPtr aObjHandle, uint aVoiceHandle);
    public double getStreamTime(uint aVoiceHandle)
    {
        return Soloud_getStreamTime(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Soloud_getStreamPosition(IntPtr aObjHandle, uint aVoiceHandle);
    public double getStreamPosition(uint aVoiceHandle)
    {
        return Soloud_getStreamPosition(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_getPause(IntPtr aObjHandle, uint aVoiceHandle);
    public int getPause(uint aVoiceHandle)
    {
        return Soloud_getPause(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getVolume(IntPtr aObjHandle, uint aVoiceHandle);
    public float getVolume(uint aVoiceHandle)
    {
        return Soloud_getVolume(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getOverallVolume(IntPtr aObjHandle, uint aVoiceHandle);
    public float getOverallVolume(uint aVoiceHandle)
    {
        return Soloud_getOverallVolume(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getPan(IntPtr aObjHandle, uint aVoiceHandle);
    public float getPan(uint aVoiceHandle)
    {
        return Soloud_getPan(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getSamplerate(IntPtr aObjHandle, uint aVoiceHandle);
    public float getSamplerate(uint aVoiceHandle)
    {
        return Soloud_getSamplerate(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_getProtectVoice(IntPtr aObjHandle, uint aVoiceHandle);
    public int getProtectVoice(uint aVoiceHandle)
    {
        return Soloud_getProtectVoice(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getActiveVoiceCount(IntPtr aObjHandle);
    public uint getActiveVoiceCount()
    {
        return Soloud_getActiveVoiceCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getVoiceCount(IntPtr aObjHandle);
    public uint getVoiceCount()
    {
        return Soloud_getVoiceCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_isValidVoiceHandle(IntPtr aObjHandle, uint aVoiceHandle);
    public int isValidVoiceHandle(uint aVoiceHandle)
    {
        return Soloud_isValidVoiceHandle(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getRelativePlaySpeed(IntPtr aObjHandle, uint aVoiceHandle);
    public float getRelativePlaySpeed(uint aVoiceHandle)
    {
        return Soloud_getRelativePlaySpeed(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getPostClipScaler(IntPtr aObjHandle);
    public float getPostClipScaler()
    {
        return Soloud_getPostClipScaler(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getGlobalVolume(IntPtr aObjHandle);
    public float getGlobalVolume()
    {
        return Soloud_getGlobalVolume(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getMaxActiveVoiceCount(IntPtr aObjHandle);
    public uint getMaxActiveVoiceCount()
    {
        return Soloud_getMaxActiveVoiceCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_getLooping(IntPtr aObjHandle, uint aVoiceHandle);
    public int getLooping(uint aVoiceHandle)
    {
        return Soloud_getLooping(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Soloud_getLoopPoint(IntPtr aObjHandle, uint aVoiceHandle);
    public double getLoopPoint(uint aVoiceHandle)
    {
        return Soloud_getLoopPoint(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setLoopPoint(IntPtr aObjHandle, uint aVoiceHandle, double aLoopPoint);
    public void setLoopPoint(uint aVoiceHandle, double aLoopPoint)
    {
        Soloud_setLoopPoint(objhandle, aVoiceHandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setLooping(IntPtr aObjHandle, uint aVoiceHandle, int aLooping);
    public void setLooping(uint aVoiceHandle, int aLooping)
    {
        Soloud_setLooping(objhandle, aVoiceHandle, aLooping);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_setMaxActiveVoiceCount(IntPtr aObjHandle, uint aVoiceCount);
    public int setMaxActiveVoiceCount(uint aVoiceCount)
    {
        return Soloud_setMaxActiveVoiceCount(objhandle, aVoiceCount);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setInaudibleBehavior(IntPtr aObjHandle, uint aVoiceHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(uint aVoiceHandle, int aMustTick, int aKill)
    {
        Soloud_setInaudibleBehavior(objhandle, aVoiceHandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setGlobalVolume(IntPtr aObjHandle, float aVolume);
    public void setGlobalVolume(float aVolume)
    {
        Soloud_setGlobalVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setPostClipScaler(IntPtr aObjHandle, float aScaler);
    public void setPostClipScaler(float aScaler)
    {
        Soloud_setPostClipScaler(objhandle, aScaler);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setPause(IntPtr aObjHandle, uint aVoiceHandle, int aPause);
    public void setPause(uint aVoiceHandle, int aPause)
    {
        Soloud_setPause(objhandle, aVoiceHandle, aPause);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setPauseAll(IntPtr aObjHandle, int aPause);
    public void setPauseAll(int aPause)
    {
        Soloud_setPauseAll(objhandle, aPause);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_setRelativePlaySpeed(IntPtr aObjHandle, uint aVoiceHandle, float aSpeed);
    public int setRelativePlaySpeed(uint aVoiceHandle, float aSpeed)
    {
        return Soloud_setRelativePlaySpeed(objhandle, aVoiceHandle, aSpeed);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setProtectVoice(IntPtr aObjHandle, uint aVoiceHandle, int aProtect);
    public void setProtectVoice(uint aVoiceHandle, int aProtect)
    {
        Soloud_setProtectVoice(objhandle, aVoiceHandle, aProtect);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setSamplerate(IntPtr aObjHandle, uint aVoiceHandle, float aSamplerate);
    public void setSamplerate(uint aVoiceHandle, float aSamplerate)
    {
        Soloud_setSamplerate(objhandle, aVoiceHandle, aSamplerate);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setPan(IntPtr aObjHandle, uint aVoiceHandle, float aPan);
    public void setPan(uint aVoiceHandle, float aPan)
    {
        Soloud_setPan(objhandle, aVoiceHandle, aPan);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setPanAbsoluteEx(IntPtr aObjHandle, uint aVoiceHandle, float aLVolume, float aRVolume, float aLBVolume, float aRBVolume, float aCVolume, float aSVolume);
    public void setPanAbsolute(uint aVoiceHandle, float aLVolume, float aRVolume, float aLBVolume = 0, float aRBVolume = 0, float aCVolume = 0, float aSVolume = 0)
    {
        Soloud_setPanAbsoluteEx(objhandle, aVoiceHandle, aLVolume, aRVolume, aLBVolume, aRBVolume, aCVolume, aSVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setVolume(IntPtr aObjHandle, uint aVoiceHandle, float aVolume);
    public void setVolume(uint aVoiceHandle, float aVolume)
    {
        Soloud_setVolume(objhandle, aVoiceHandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setDelaySamples(IntPtr aObjHandle, uint aVoiceHandle, uint aSamples);
    public void setDelaySamples(uint aVoiceHandle, uint aSamples)
    {
        Soloud_setDelaySamples(objhandle, aVoiceHandle, aSamples);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_fadeVolume(IntPtr aObjHandle, uint aVoiceHandle, float aTo, double aTime);
    public void fadeVolume(uint aVoiceHandle, float aTo, double aTime)
    {
        Soloud_fadeVolume(objhandle, aVoiceHandle, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_fadePan(IntPtr aObjHandle, uint aVoiceHandle, float aTo, double aTime);
    public void fadePan(uint aVoiceHandle, float aTo, double aTime)
    {
        Soloud_fadePan(objhandle, aVoiceHandle, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_fadeRelativePlaySpeed(IntPtr aObjHandle, uint aVoiceHandle, float aTo, double aTime);
    public void fadeRelativePlaySpeed(uint aVoiceHandle, float aTo, double aTime)
    {
        Soloud_fadeRelativePlaySpeed(objhandle, aVoiceHandle, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_fadeGlobalVolume(IntPtr aObjHandle, float aTo, double aTime);
    public void fadeGlobalVolume(float aTo, double aTime)
    {
        Soloud_fadeGlobalVolume(objhandle, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_schedulePause(IntPtr aObjHandle, uint aVoiceHandle, double aTime);
    public void schedulePause(uint aVoiceHandle, double aTime)
    {
        Soloud_schedulePause(objhandle, aVoiceHandle, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_scheduleStop(IntPtr aObjHandle, uint aVoiceHandle, double aTime);
    public void scheduleStop(uint aVoiceHandle, double aTime)
    {
        Soloud_scheduleStop(objhandle, aVoiceHandle, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_oscillateVolume(IntPtr aObjHandle, uint aVoiceHandle, float aFrom, float aTo, double aTime);
    public void oscillateVolume(uint aVoiceHandle, float aFrom, float aTo, double aTime)
    {
        Soloud_oscillateVolume(objhandle, aVoiceHandle, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_oscillatePan(IntPtr aObjHandle, uint aVoiceHandle, float aFrom, float aTo, double aTime);
    public void oscillatePan(uint aVoiceHandle, float aFrom, float aTo, double aTime)
    {
        Soloud_oscillatePan(objhandle, aVoiceHandle, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_oscillateRelativePlaySpeed(IntPtr aObjHandle, uint aVoiceHandle, float aFrom, float aTo, double aTime);
    public void oscillateRelativePlaySpeed(uint aVoiceHandle, float aFrom, float aTo, double aTime)
    {
        Soloud_oscillateRelativePlaySpeed(objhandle, aVoiceHandle, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_oscillateGlobalVolume(IntPtr aObjHandle, float aFrom, float aTo, double aTime);
    public void oscillateGlobalVolume(float aFrom, float aTo, double aTime)
    {
        Soloud_oscillateGlobalVolume(objhandle, aFrom, aTo, aTime);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setGlobalFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setGlobalFilter(uint aFilterId, SoloudObject aFilter)
    {
        Soloud_setGlobalFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_setVisualizationEnable(IntPtr aObjHandle, int aEnable);
    public void setVisualizationEnable(int aEnable)
    {
        Soloud_setVisualizationEnable(objhandle, aEnable);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Soloud_calcFFT(IntPtr aObjHandle);
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
    internal static partial IntPtr Soloud_getWave(IntPtr aObjHandle);
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
    internal static partial float Soloud_getApproximateVolume(IntPtr aObjHandle, uint aChannel);
    public float getApproximateVolume(uint aChannel)
    {
        return Soloud_getApproximateVolume(objhandle, aChannel);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_getLoopCount(IntPtr aObjHandle, uint aVoiceHandle);
    public uint getLoopCount(uint aVoiceHandle)
    {
        return Soloud_getLoopCount(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_getInfo(IntPtr aObjHandle, uint aVoiceHandle, uint aInfoKey);
    public float getInfo(uint aVoiceHandle, uint aInfoKey)
    {
        return Soloud_getInfo(objhandle, aVoiceHandle, aInfoKey);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Soloud_createVoiceGroup(IntPtr aObjHandle);
    public uint createVoiceGroup()
    {
        return Soloud_createVoiceGroup(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_destroyVoiceGroup(IntPtr aObjHandle, uint aVoiceGroupHandle);
    public int destroyVoiceGroup(uint aVoiceGroupHandle)
    {
        return Soloud_destroyVoiceGroup(objhandle, aVoiceGroupHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_addVoiceToGroup(IntPtr aObjHandle, uint aVoiceGroupHandle, uint aVoiceHandle);
    public int addVoiceToGroup(uint aVoiceGroupHandle, uint aVoiceHandle)
    {
        return Soloud_addVoiceToGroup(objhandle, aVoiceGroupHandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_isVoiceGroup(IntPtr aObjHandle, uint aVoiceGroupHandle);
    public int isVoiceGroup(uint aVoiceGroupHandle)
    {
        return Soloud_isVoiceGroup(objhandle, aVoiceGroupHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_isVoiceGroupEmpty(IntPtr aObjHandle, uint aVoiceGroupHandle);
    public int isVoiceGroupEmpty(uint aVoiceGroupHandle)
    {
        return Soloud_isVoiceGroupEmpty(objhandle, aVoiceGroupHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_update3dAudio(IntPtr aObjHandle);
    public void update3dAudio()
    {
        Soloud_update3dAudio(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Soloud_set3dSoundSpeed(IntPtr aObjHandle, float aSpeed);
    public int set3dSoundSpeed(float aSpeed)
    {
        return Soloud_set3dSoundSpeed(objhandle, aSpeed);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float Soloud_get3dSoundSpeed(IntPtr aObjHandle);
    public float get3dSoundSpeed()
    {
        return Soloud_get3dSoundSpeed(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dListenerParametersEx(IntPtr aObjHandle, float aPosX, float aPosY, float aPosZ, float aAtX, float aAtY, float aAtZ, float aUpX, float aUpY, float aUpZ, float aVelocityX, float aVelocityY, float aVelocityZ);
    public void set3dListenerParameters(float aPosX, float aPosY, float aPosZ, float aAtX, float aAtY, float aAtZ, float aUpX, float aUpY, float aUpZ, float aVelocityX = 0.0f, float aVelocityY = 0.0f, float aVelocityZ = 0.0f)
    {
        Soloud_set3dListenerParametersEx(objhandle, aPosX, aPosY, aPosZ, aAtX, aAtY, aAtZ, aUpX, aUpY, aUpZ, aVelocityX, aVelocityY, aVelocityZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dListenerPosition(IntPtr aObjHandle, float aPosX, float aPosY, float aPosZ);
    public void set3dListenerPosition(float aPosX, float aPosY, float aPosZ)
    {
        Soloud_set3dListenerPosition(objhandle, aPosX, aPosY, aPosZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dListenerAt(IntPtr aObjHandle, float aAtX, float aAtY, float aAtZ);
    public void set3dListenerAt(float aAtX, float aAtY, float aAtZ)
    {
        Soloud_set3dListenerAt(objhandle, aAtX, aAtY, aAtZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dListenerUp(IntPtr aObjHandle, float aUpX, float aUpY, float aUpZ);
    public void set3dListenerUp(float aUpX, float aUpY, float aUpZ)
    {
        Soloud_set3dListenerUp(objhandle, aUpX, aUpY, aUpZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dListenerVelocity(IntPtr aObjHandle, float aVelocityX, float aVelocityY, float aVelocityZ);
    public void set3dListenerVelocity(float aVelocityX, float aVelocityY, float aVelocityZ)
    {
        Soloud_set3dListenerVelocity(objhandle, aVelocityX, aVelocityY, aVelocityZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dSourceParametersEx(IntPtr aObjHandle, uint aVoiceHandle, float aPosX, float aPosY, float aPosZ, float aVelocityX, float aVelocityY, float aVelocityZ);
    public void set3dSourceParameters(uint aVoiceHandle, float aPosX, float aPosY, float aPosZ, float aVelocityX = 0.0f, float aVelocityY = 0.0f, float aVelocityZ = 0.0f)
    {
        Soloud_set3dSourceParametersEx(objhandle, aVoiceHandle, aPosX, aPosY, aPosZ, aVelocityX, aVelocityY, aVelocityZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dSourcePosition(IntPtr aObjHandle, uint aVoiceHandle, float aPosX, float aPosY, float aPosZ);
    public void set3dSourcePosition(uint aVoiceHandle, float aPosX, float aPosY, float aPosZ)
    {
        Soloud_set3dSourcePosition(objhandle, aVoiceHandle, aPosX, aPosY, aPosZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dSourceVelocity(IntPtr aObjHandle, uint aVoiceHandle, float aVelocityX, float aVelocityY, float aVelocityZ);
    public void set3dSourceVelocity(uint aVoiceHandle, float aVelocityX, float aVelocityY, float aVelocityZ)
    {
        Soloud_set3dSourceVelocity(objhandle, aVoiceHandle, aVelocityX, aVelocityY, aVelocityZ);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dSourceMinMaxDistance(IntPtr aObjHandle, uint aVoiceHandle, float aMinDistance, float aMaxDistance);
    public void set3dSourceMinMaxDistance(uint aVoiceHandle, float aMinDistance, float aMaxDistance)
    {
        Soloud_set3dSourceMinMaxDistance(objhandle, aVoiceHandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dSourceAttenuation(IntPtr aObjHandle, uint aVoiceHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dSourceAttenuation(uint aVoiceHandle, uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Soloud_set3dSourceAttenuation(objhandle, aVoiceHandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_set3dSourceDopplerFactor(IntPtr aObjHandle, uint aVoiceHandle, float aDopplerFactor);
    public void set3dSourceDopplerFactor(uint aVoiceHandle, float aDopplerFactor)
    {
        Soloud_set3dSourceDopplerFactor(objhandle, aVoiceHandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_mix(IntPtr aObjHandle, float[] aBuffer, uint aSamples);
    public void mix(float[] aBuffer, uint aSamples)
    {
        Soloud_mix(objhandle, aBuffer, aSamples);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Soloud_mixSigned16(IntPtr aObjHandle, IntPtr aBuffer, uint aSamples);
    public void mixSigned16(IntPtr aBuffer, uint aSamples)
    {
        Soloud_mixSigned16(objhandle, aBuffer, aSamples);
    }
}

public partial class BassboostFilter : SoloudObject
{
    public const int WET = 0;
    public const int BOOST = 1;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr BassboostFilter_create();
    public BassboostFilter()
    {
        objhandle = BassboostFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr BassboostFilter_destroy(IntPtr aObjHandle);
    ~BassboostFilter()
    {
        BassboostFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int BassboostFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return BassboostFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr BassboostFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = BassboostFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint BassboostFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return BassboostFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float BassboostFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return BassboostFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float BassboostFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return BassboostFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int BassboostFilter_setParams(IntPtr aObjHandle, float aBoost);
    public int setParams(float aBoost)
    {
        return BassboostFilter_setParams(objhandle, aBoost);
    }
}

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
    internal static partial IntPtr BiquadResonantFilter_create();
    public BiquadResonantFilter()
    {
        objhandle = BiquadResonantFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr BiquadResonantFilter_destroy(IntPtr aObjHandle);
    ~BiquadResonantFilter()
    {
        BiquadResonantFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int BiquadResonantFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return BiquadResonantFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr BiquadResonantFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = BiquadResonantFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint BiquadResonantFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return BiquadResonantFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float BiquadResonantFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return BiquadResonantFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float BiquadResonantFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return BiquadResonantFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int BiquadResonantFilter_setParams(IntPtr aObjHandle, int aType, float aFrequency, float aResonance);
    public int setParams(int aType, float aFrequency, float aResonance)
    {
        return BiquadResonantFilter_setParams(objhandle, aType, aFrequency, aResonance);
    }
}

public partial class Bus : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Bus_create();
    public Bus()
    {
        objhandle = Bus_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Bus_destroy(IntPtr aObjHandle);
    ~Bus()
    {
        Bus_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Bus_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Bus_playEx(IntPtr aObjHandle, IntPtr aSound, float aVolume, float aPan, int aPaused);
    public uint play(SoloudObject aSound, float aVolume = 1.0f, float aPan = 0.0f, int aPaused = 0)
    {
        return Bus_playEx(objhandle, aSound.objhandle, aVolume, aPan, aPaused);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Bus_playClockedEx(IntPtr aObjHandle, double aSoundTime, IntPtr aSound, float aVolume, float aPan);
    public uint playClocked(double aSoundTime, SoloudObject aSound, float aVolume = 1.0f, float aPan = 0.0f)
    {
        return Bus_playClockedEx(objhandle, aSoundTime, aSound.objhandle, aVolume, aPan);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Bus_play3dEx(IntPtr aObjHandle, IntPtr aSound, float aPosX, float aPosY, float aPosZ, float aVelX, float aVelY, float aVelZ, float aVolume, int aPaused);
    public uint play3d(SoloudObject aSound, float aPosX, float aPosY, float aPosZ, float aVelX = 0.0f, float aVelY = 0.0f, float aVelZ = 0.0f, float aVolume = 1.0f, int aPaused = 0)
    {
        return Bus_play3dEx(objhandle, aSound.objhandle, aPosX, aPosY, aPosZ, aVelX, aVelY, aVelZ, aVolume, aPaused);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Bus_play3dClockedEx(IntPtr aObjHandle, double aSoundTime, IntPtr aSound, float aPosX, float aPosY, float aPosZ, float aVelX, float aVelY, float aVelZ, float aVolume);
    public uint play3dClocked(double aSoundTime, SoloudObject aSound, float aPosX, float aPosY, float aPosZ, float aVelX = 0.0f, float aVelY = 0.0f, float aVelZ = 0.0f, float aVolume = 1.0f)
    {
        return Bus_play3dClockedEx(objhandle, aSoundTime, aSound.objhandle, aPosX, aPosY, aPosZ, aVelX, aVelY, aVelZ, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Bus_setChannels(IntPtr aObjHandle, uint aChannels);
    public int setChannels(uint aChannels)
    {
        return Bus_setChannels(objhandle, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_setVisualizationEnable(IntPtr aObjHandle, int aEnable);
    public void setVisualizationEnable(int aEnable)
    {
        Bus_setVisualizationEnable(objhandle, aEnable);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_annexSound(IntPtr aObjHandle, uint aVoiceHandle);
    public void annexSound(uint aVoiceHandle)
    {
        Bus_annexSound(objhandle, aVoiceHandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Bus_calcFFT(IntPtr aObjHandle);
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
    internal static partial IntPtr Bus_getWave(IntPtr aObjHandle);
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
    internal static partial float Bus_getApproximateVolume(IntPtr aObjHandle, uint aChannel);
    public float getApproximateVolume(uint aChannel)
    {
        return Bus_getApproximateVolume(objhandle, aChannel);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Bus_getActiveVoiceCount(IntPtr aObjHandle);
    public uint getActiveVoiceCount()
    {
        return Bus_getActiveVoiceCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Bus_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Bus_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Bus_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Bus_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Bus_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Bus_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Bus_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Bus_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Bus_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Bus_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Bus_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Bus_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Bus_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Bus_stop(IntPtr aObjHandle);
    public void stop()
    {
        Bus_stop(objhandle);
    }
}

public partial class DCRemovalFilter : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr DCRemovalFilter_create();
    public DCRemovalFilter()
    {
        objhandle = DCRemovalFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr DCRemovalFilter_destroy(IntPtr aObjHandle);
    ~DCRemovalFilter()
    {
        DCRemovalFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int DCRemovalFilter_setParamsEx(IntPtr aObjHandle, float aLength);
    public int setParams(float aLength = 0.1f)
    {
        return DCRemovalFilter_setParamsEx(objhandle, aLength);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int DCRemovalFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return DCRemovalFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr DCRemovalFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = DCRemovalFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint DCRemovalFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return DCRemovalFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float DCRemovalFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return DCRemovalFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float DCRemovalFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return DCRemovalFilter_getParamMin(objhandle, aParamIndex);
    }
}

public partial class EchoFilter : SoloudObject
{
    public const int WET = 0;
    public const int DELAY = 1;
    public const int DECAY = 2;
    public const int FILTER = 3;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr EchoFilter_create();
    public EchoFilter()
    {
        objhandle = EchoFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr EchoFilter_destroy(IntPtr aObjHandle);
    ~EchoFilter()
    {
        EchoFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int EchoFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return EchoFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr EchoFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = EchoFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint EchoFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return EchoFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float EchoFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return EchoFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float EchoFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return EchoFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int EchoFilter_setParamsEx(IntPtr aObjHandle, float aDelay, float aDecay, float aFilter);
    public int setParams(float aDelay, float aDecay = 0.7f, float aFilter = 0.0f)
    {
        return EchoFilter_setParamsEx(objhandle, aDelay, aDecay, aFilter);
    }
}

public partial class FFTFilter : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FFTFilter_create();
    public FFTFilter()
    {
        objhandle = FFTFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FFTFilter_destroy(IntPtr aObjHandle);
    ~FFTFilter()
    {
        FFTFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int FFTFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return FFTFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FFTFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = FFTFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint FFTFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return FFTFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float FFTFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return FFTFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float FFTFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return FFTFilter_getParamMin(objhandle, aParamIndex);
    }
}

public partial class FlangerFilter : SoloudObject
{
    public const int WET = 0;
    public const int DELAY = 1;
    public const int FREQ = 2;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FlangerFilter_create();
    public FlangerFilter()
    {
        objhandle = FlangerFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FlangerFilter_destroy(IntPtr aObjHandle);
    ~FlangerFilter()
    {
        FlangerFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int FlangerFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return FlangerFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FlangerFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = FlangerFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint FlangerFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return FlangerFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float FlangerFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return FlangerFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float FlangerFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return FlangerFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int FlangerFilter_setParams(IntPtr aObjHandle, float aDelay, float aFreq);
    public int setParams(float aDelay, float aFreq)
    {
        return FlangerFilter_setParams(objhandle, aDelay, aFreq);
    }
}

public partial class FreeverbFilter : SoloudObject
{
    public const int WET = 0;
    public const int FREEZE = 1;
    public const int ROOMSIZE = 2;
    public const int DAMP = 3;
    public const int WIDTH = 4;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FreeverbFilter_create();
    public FreeverbFilter()
    {
        objhandle = FreeverbFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FreeverbFilter_destroy(IntPtr aObjHandle);
    ~FreeverbFilter()
    {
        FreeverbFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int FreeverbFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return FreeverbFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr FreeverbFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = FreeverbFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint FreeverbFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return FreeverbFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float FreeverbFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return FreeverbFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float FreeverbFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return FreeverbFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int FreeverbFilter_setParams(IntPtr aObjHandle, float aMode, float aRoomSize, float aDamp, float aWidth);
    public int setParams(float aMode, float aRoomSize, float aDamp, float aWidth)
    {
        return FreeverbFilter_setParams(objhandle, aMode, aRoomSize, aDamp, aWidth);
    }
}

public partial class LofiFilter : SoloudObject
{
    public const int WET = 0;
    public const int SAMPLERATE = 1;
    public const int BITDEPTH = 2;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr LofiFilter_create();
    public LofiFilter()
    {
        objhandle = LofiFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr LofiFilter_destroy(IntPtr aObjHandle);
    ~LofiFilter()
    {
        LofiFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int LofiFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return LofiFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr LofiFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = LofiFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint LofiFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return LofiFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float LofiFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return LofiFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float LofiFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return LofiFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int LofiFilter_setParams(IntPtr aObjHandle, float aSampleRate, float aBitdepth);
    public int setParams(float aSampleRate, float aBitdepth)
    {
        return LofiFilter_setParams(objhandle, aSampleRate, aBitdepth);
    }
}

public partial class Monotone : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Monotone_create();
    public Monotone()
    {
        objhandle = Monotone_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Monotone_destroy(IntPtr aObjHandle);
    ~Monotone()
    {
        Monotone_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Monotone_setParamsEx(IntPtr aObjHandle, int aHardwareChannels, int aWaveform);
    public int setParams(int aHardwareChannels, int aWaveform = 0/*SoLoud:Misc::WAVE_SQUARE*/)
    {
        return Monotone_setParamsEx(objhandle, aHardwareChannels, aWaveform);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Monotone_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    public int load(string aFilename)
    {
        return Monotone_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Monotone_loadMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    public int loadMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Monotone_loadMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Monotone_loadFile(IntPtr aObjHandle, IntPtr aFile);
    public int loadFile(SoloudObject aFile)
    {
        return Monotone_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Monotone_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Monotone_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Monotone_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Monotone_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Monotone_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Monotone_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Monotone_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Monotone_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Monotone_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Monotone_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Monotone_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Monotone_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Monotone_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Monotone_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Monotone_stop(IntPtr aObjHandle);
    public void stop()
    {
        Monotone_stop(objhandle);
    }
}

public partial class Noise : SoloudObject
{
    public const int WHITE = 0;
    public const int PINK = 1;
    public const int BROWNISH = 2;
    public const int BLUEISH = 3;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Noise_create();
    public Noise()
    {
        objhandle = Noise_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Noise_destroy(IntPtr aObjHandle);
    ~Noise()
    {
        Noise_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_setOctaveScale(IntPtr aObjHandle, float aOct0, float aOct1, float aOct2, float aOct3, float aOct4, float aOct5, float aOct6, float aOct7, float aOct8, float aOct9);
    public void setOctaveScale(float aOct0, float aOct1, float aOct2, float aOct3, float aOct4, float aOct5, float aOct6, float aOct7, float aOct8, float aOct9)
    {
        Noise_setOctaveScale(objhandle, aOct0, aOct1, aOct2, aOct3, aOct4, aOct5, aOct6, aOct7, aOct8, aOct9);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_setType(IntPtr aObjHandle, int aType);
    public void setType(int aType)
    {
        Noise_setType(objhandle, aType);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Noise_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Noise_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Noise_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Noise_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Noise_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Noise_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Noise_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Noise_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Noise_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Noise_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Noise_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Noise_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Noise_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Noise_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Noise_stop(IntPtr aObjHandle);
    public void stop()
    {
        Noise_stop(objhandle);
    }
}

public partial class Openmpt : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Openmpt_create();
    public Openmpt()
    {
        objhandle = Openmpt_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Openmpt_destroy(IntPtr aObjHandle);
    ~Openmpt()
    {
        Openmpt_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Openmpt_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    public int load(string aFilename)
    {
        return Openmpt_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Openmpt_loadMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    public int loadMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Openmpt_loadMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Openmpt_loadFile(IntPtr aObjHandle, IntPtr aFile);
    public int loadFile(SoloudObject aFile)
    {
        return Openmpt_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Openmpt_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Openmpt_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Openmpt_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Openmpt_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Openmpt_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Openmpt_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Openmpt_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Openmpt_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Openmpt_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Openmpt_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Openmpt_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Openmpt_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Openmpt_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Openmpt_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Openmpt_stop(IntPtr aObjHandle);
    public void stop()
    {
        Openmpt_stop(objhandle);
    }
}

public partial class Queue : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Queue_create();
    public Queue()
    {
        objhandle = Queue_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Queue_destroy(IntPtr aObjHandle);
    ~Queue()
    {
        Queue_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Queue_play(IntPtr aObjHandle, IntPtr aSound);
    public int play(SoloudObject aSound)
    {
        return Queue_play(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint Queue_getQueueCount(IntPtr aObjHandle);
    public uint getQueueCount()
    {
        return Queue_getQueueCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Queue_isCurrentlyPlaying(IntPtr aObjHandle, IntPtr aSound);
    public int isCurrentlyPlaying(SoloudObject aSound)
    {
        return Queue_isCurrentlyPlaying(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Queue_setParamsFromAudioSource(IntPtr aObjHandle, IntPtr aSound);
    public int setParamsFromAudioSource(SoloudObject aSound)
    {
        return Queue_setParamsFromAudioSource(objhandle, aSound.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Queue_setParamsEx(IntPtr aObjHandle, float aSamplerate, uint aChannels);
    public int setParams(float aSamplerate, uint aChannels = 2)
    {
        return Queue_setParamsEx(objhandle, aSamplerate, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Queue_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Queue_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Queue_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Queue_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Queue_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Queue_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Queue_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Queue_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Queue_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Queue_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Queue_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Queue_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Queue_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Queue_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Queue_stop(IntPtr aObjHandle);
    public void stop()
    {
        Queue_stop(objhandle);
    }
}

public partial class RobotizeFilter : SoloudObject
{
    public const int WET = 0;
    public const int FREQ = 1;
    public const int WAVE = 2;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr RobotizeFilter_create();
    public RobotizeFilter()
    {
        objhandle = RobotizeFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr RobotizeFilter_destroy(IntPtr aObjHandle);
    ~RobotizeFilter()
    {
        RobotizeFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int RobotizeFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return RobotizeFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr RobotizeFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = RobotizeFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint RobotizeFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return RobotizeFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float RobotizeFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return RobotizeFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float RobotizeFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return RobotizeFilter_getParamMin(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void RobotizeFilter_setParams(IntPtr aObjHandle, float aFreq, int aWaveform);
    public void setParams(float aFreq, int aWaveform)
    {
        RobotizeFilter_setParams(objhandle, aFreq, aWaveform);
    }
}

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
    internal static partial IntPtr Sfxr_create();
    public Sfxr()
    {
        objhandle = Sfxr_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Sfxr_destroy(IntPtr aObjHandle);
    ~Sfxr()
    {
        Sfxr_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_resetParams(IntPtr aObjHandle);
    public void resetParams()
    {
        Sfxr_resetParams(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Sfxr_loadParams(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    public int loadParams(string aFilename)
    {
        return Sfxr_loadParams(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Sfxr_loadParamsMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    public int loadParamsMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Sfxr_loadParamsMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Sfxr_loadParamsFile(IntPtr aObjHandle, IntPtr aFile);
    public int loadParamsFile(SoloudObject aFile)
    {
        return Sfxr_loadParamsFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Sfxr_loadPreset(IntPtr aObjHandle, int aPresetNo, int aRandSeed);
    public int loadPreset(int aPresetNo, int aRandSeed)
    {
        return Sfxr_loadPreset(objhandle, aPresetNo, aRandSeed);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Sfxr_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Sfxr_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Sfxr_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Sfxr_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Sfxr_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Sfxr_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Sfxr_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Sfxr_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Sfxr_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Sfxr_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Sfxr_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Sfxr_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Sfxr_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Sfxr_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Sfxr_stop(IntPtr aObjHandle);
    public void stop()
    {
        Sfxr_stop(objhandle);
    }
}

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
    internal static partial IntPtr Speech_create();
    public Speech()
    {
        objhandle = Speech_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Speech_destroy(IntPtr aObjHandle);
    ~Speech()
    {
        Speech_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Speech_setText(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aText);
    public int setText(string aText)
    {
        return Speech_setText(objhandle, aText);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Speech_setParamsEx(IntPtr aObjHandle, uint aBaseFrequency, float aBaseSpeed, float aBaseDeclination, int aBaseWaveform);
    public int setParams(uint aBaseFrequency = 1330, float aBaseSpeed = 10.0f, float aBaseDeclination = 0.5f, int aBaseWaveform = KW_TRIANGLE)
    {
        return Speech_setParamsEx(objhandle, aBaseFrequency, aBaseSpeed, aBaseDeclination, aBaseWaveform);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Speech_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Speech_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Speech_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Speech_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Speech_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Speech_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Speech_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Speech_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Speech_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Speech_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Speech_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Speech_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Speech_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Speech_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Speech_stop(IntPtr aObjHandle);
    public void stop()
    {
        Speech_stop(objhandle);
    }
}

public partial class TedSid : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr TedSid_create();
    public TedSid()
    {
        objhandle = TedSid_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr TedSid_destroy(IntPtr aObjHandle);
    ~TedSid()
    {
        TedSid_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int TedSid_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    public int load(string aFilename)
    {
        return TedSid_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int TedSid_loadToMem(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    public int loadToMem(string aFilename)
    {
        return TedSid_loadToMem(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int TedSid_loadMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    public int loadMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return TedSid_loadMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int TedSid_loadFileToMem(IntPtr aObjHandle, IntPtr aFile);
    public int loadFileToMem(SoloudObject aFile)
    {
        return TedSid_loadFileToMem(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int TedSid_loadFile(IntPtr aObjHandle, IntPtr aFile);
    public int loadFile(SoloudObject aFile)
    {
        return TedSid_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        TedSid_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        TedSid_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        TedSid_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        TedSid_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        TedSid_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        TedSid_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        TedSid_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        TedSid_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        TedSid_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        TedSid_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        TedSid_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double TedSid_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return TedSid_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        TedSid_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void TedSid_stop(IntPtr aObjHandle);
    public void stop()
    {
        TedSid_stop(objhandle);
    }
}

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
    internal static partial IntPtr Vic_create();
    public Vic()
    {
        objhandle = Vic_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Vic_destroy(IntPtr aObjHandle);
    ~Vic()
    {
        Vic_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_setModel(IntPtr aObjHandle, int model);
    public void setModel(int model)
    {
        Vic_setModel(objhandle, model);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Vic_getModel(IntPtr aObjHandle);
    public int getModel()
    {
        return Vic_getModel(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_setRegister(IntPtr aObjHandle, int reg, byte value);
    public void setRegister(int reg, byte value)
    {
        Vic_setRegister(objhandle, reg, value);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial byte Vic_getRegister(IntPtr aObjHandle, int reg);
    public byte getRegister(int reg)
    {
        return Vic_getRegister(objhandle, reg);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Vic_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Vic_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Vic_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Vic_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Vic_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Vic_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Vic_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Vic_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Vic_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Vic_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Vic_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Vic_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Vic_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Vic_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vic_stop(IntPtr aObjHandle);
    public void stop()
    {
        Vic_stop(objhandle);
    }
}

public partial class Vizsn : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Vizsn_create();
    public Vizsn()
    {
        objhandle = Vizsn_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Vizsn_destroy(IntPtr aObjHandle);
    ~Vizsn()
    {
        Vizsn_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_setText(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aText);
    public void setText(string aText)
    {
        Vizsn_setText(objhandle, aText);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Vizsn_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Vizsn_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Vizsn_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Vizsn_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Vizsn_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Vizsn_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Vizsn_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Vizsn_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Vizsn_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Vizsn_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Vizsn_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Vizsn_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Vizsn_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Vizsn_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Vizsn_stop(IntPtr aObjHandle);
    public void stop()
    {
        Vizsn_stop(objhandle);
    }
}

public partial class Wav : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Wav_create();
    public Wav()
    {
        objhandle = Wav_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr Wav_destroy(IntPtr aObjHandle);
    ~Wav()
    {
        Wav_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Wav_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    public int load(string aFilename)
    {
        return Wav_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Wav_loadMemEx(IntPtr aObjHandle, IntPtr aMem, uint aLength, int aCopy, int aTakeOwnership);
    public int loadMem(IntPtr aMem, uint aLength, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Wav_loadMemEx(objhandle, aMem, aLength, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Wav_loadFile(IntPtr aObjHandle, IntPtr aFile);
    public int loadFile(SoloudObject aFile)
    {
        return Wav_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Wav_loadRawWave8Ex(IntPtr aObjHandle, IntPtr aMem, uint aLength, float aSamplerate, uint aChannels);
    public int loadRawWave8(IntPtr aMem, uint aLength, float aSamplerate = 44100.0f, uint aChannels = 1)
    {
        return Wav_loadRawWave8Ex(objhandle, aMem, aLength, aSamplerate, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Wav_loadRawWave16Ex(IntPtr aObjHandle, IntPtr aMem, uint aLength, float aSamplerate, uint aChannels);
    public int loadRawWave16(IntPtr aMem, uint aLength, float aSamplerate = 44100.0f, uint aChannels = 1)
    {
        return Wav_loadRawWave16Ex(objhandle, aMem, aLength, aSamplerate, aChannels);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int Wav_loadRawWaveEx(IntPtr aObjHandle, float[] aMem, uint aLength, float aSamplerate, uint aChannels, int aCopy, int aTakeOwnership);
    public int loadRawWave(float[] aMem, uint aLength, float aSamplerate = 44100.0f, uint aChannels = 1, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return Wav_loadRawWaveEx(objhandle, aMem, aLength, aSamplerate, aChannels, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Wav_getLength(IntPtr aObjHandle);
    public double getLength()
    {
        return Wav_getLength(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        Wav_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        Wav_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        Wav_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        Wav_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        Wav_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        Wav_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        Wav_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        Wav_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        Wav_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        Wav_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        Wav_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double Wav_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return Wav_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        Wav_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void Wav_stop(IntPtr aObjHandle);
    public void stop()
    {
        Wav_stop(objhandle);
    }
}

public partial class WaveShaperFilter : SoloudObject
{
    public const int WET = 0;
    public const int AMOUNT = 1;

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr WaveShaperFilter_create();
    public WaveShaperFilter()
    {
        objhandle = WaveShaperFilter_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr WaveShaperFilter_destroy(IntPtr aObjHandle);
    ~WaveShaperFilter()
    {
        WaveShaperFilter_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int WaveShaperFilter_setParams(IntPtr aObjHandle, float aAmount);
    public int setParams(float aAmount)
    {
        return WaveShaperFilter_setParams(objhandle, aAmount);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int WaveShaperFilter_getParamCount(IntPtr aObjHandle);
    public int getParamCount()
    {
        return WaveShaperFilter_getParamCount(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr WaveShaperFilter_getParamName(IntPtr aObjHandle, uint aParamIndex);
    public string getParamName(uint aParamIndex)
    {
        IntPtr p = WaveShaperFilter_getParamName(objhandle, aParamIndex);
        return Marshal.PtrToStringAnsi(p);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial uint WaveShaperFilter_getParamType(IntPtr aObjHandle, uint aParamIndex);
    public uint getParamType(uint aParamIndex)
    {
        return WaveShaperFilter_getParamType(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float WaveShaperFilter_getParamMax(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMax(uint aParamIndex)
    {
        return WaveShaperFilter_getParamMax(objhandle, aParamIndex);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial float WaveShaperFilter_getParamMin(IntPtr aObjHandle, uint aParamIndex);
    public float getParamMin(uint aParamIndex)
    {
        return WaveShaperFilter_getParamMin(objhandle, aParamIndex);
    }
}

public partial class WavStream : SoloudObject
{

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr WavStream_create();
    public WavStream()
    {
        objhandle = WavStream_create();
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial IntPtr WavStream_destroy(IntPtr aObjHandle);
    ~WavStream()
    {
        WavStream_destroy(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int WavStream_load(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    public int load(string aFilename)
    {
        return WavStream_load(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int WavStream_loadMemEx(IntPtr aObjHandle, IntPtr aData, uint aDataLen, int aCopy, int aTakeOwnership);
    public int loadMem(IntPtr aData, uint aDataLen, int aCopy = 0/*false*/, int aTakeOwnership = 1/*true*/)
    {
        return WavStream_loadMemEx(objhandle, aData, aDataLen, aCopy, aTakeOwnership);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int WavStream_loadToMem(IntPtr aObjHandle, [MarshalAs(UnmanagedType.LPStr)] string aFilename);
    public int loadToMem(string aFilename)
    {
        return WavStream_loadToMem(objhandle, aFilename);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int WavStream_loadFile(IntPtr aObjHandle, IntPtr aFile);
    public int loadFile(SoloudObject aFile)
    {
        return WavStream_loadFile(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial int WavStream_loadFileToMem(IntPtr aObjHandle, IntPtr aFile);
    public int loadFileToMem(SoloudObject aFile)
    {
        return WavStream_loadFileToMem(objhandle, aFile.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double WavStream_getLength(IntPtr aObjHandle);
    public double getLength()
    {
        return WavStream_getLength(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_setVolume(IntPtr aObjHandle, float aVolume);
    public void setVolume(float aVolume)
    {
        WavStream_setVolume(objhandle, aVolume);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_setLooping(IntPtr aObjHandle, int aLoop);
    public void setLooping(int aLoop)
    {
        WavStream_setLooping(objhandle, aLoop);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_set3dMinMaxDistance(IntPtr aObjHandle, float aMinDistance, float aMaxDistance);
    public void set3dMinMaxDistance(float aMinDistance, float aMaxDistance)
    {
        WavStream_set3dMinMaxDistance(objhandle, aMinDistance, aMaxDistance);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_set3dAttenuation(IntPtr aObjHandle, uint aAttenuationModel, float aAttenuationRolloffFactor);
    public void set3dAttenuation(uint aAttenuationModel, float aAttenuationRolloffFactor)
    {
        WavStream_set3dAttenuation(objhandle, aAttenuationModel, aAttenuationRolloffFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_set3dDopplerFactor(IntPtr aObjHandle, float aDopplerFactor);
    public void set3dDopplerFactor(float aDopplerFactor)
    {
        WavStream_set3dDopplerFactor(objhandle, aDopplerFactor);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_set3dListenerRelative(IntPtr aObjHandle, int aListenerRelative);
    public void set3dListenerRelative(int aListenerRelative)
    {
        WavStream_set3dListenerRelative(objhandle, aListenerRelative);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_set3dDistanceDelay(IntPtr aObjHandle, int aDistanceDelay);
    public void set3dDistanceDelay(int aDistanceDelay)
    {
        WavStream_set3dDistanceDelay(objhandle, aDistanceDelay);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_set3dColliderEx(IntPtr aObjHandle, IntPtr aCollider, int aUserData);
    public void set3dCollider(SoloudObject aCollider, int aUserData = 0)
    {
        WavStream_set3dColliderEx(objhandle, aCollider.objhandle, aUserData);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_set3dAttenuator(IntPtr aObjHandle, IntPtr aAttenuator);
    public void set3dAttenuator(SoloudObject aAttenuator)
    {
        WavStream_set3dAttenuator(objhandle, aAttenuator.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_setInaudibleBehavior(IntPtr aObjHandle, int aMustTick, int aKill);
    public void setInaudibleBehavior(int aMustTick, int aKill)
    {
        WavStream_setInaudibleBehavior(objhandle, aMustTick, aKill);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_setLoopPoint(IntPtr aObjHandle, double aLoopPoint);
    public void setLoopPoint(double aLoopPoint)
    {
        WavStream_setLoopPoint(objhandle, aLoopPoint);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial double WavStream_getLoopPoint(IntPtr aObjHandle);
    public double getLoopPoint()
    {
        return WavStream_getLoopPoint(objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_setFilter(IntPtr aObjHandle, uint aFilterId, IntPtr aFilter);
    public void setFilter(uint aFilterId, SoloudObject aFilter)
    {
        WavStream_setFilter(objhandle, aFilterId, aFilter.objhandle);
    }

    [LibraryImport(SoloudDllName.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    internal static partial void WavStream_stop(IntPtr aObjHandle);
    public void stop()
    {
        WavStream_stop(objhandle);
    }
}