using System;
using System.Runtime.InteropServices;

namespace vaudio_wwise;

public static class AkSoundEngine
{
    private const string DllName = "AkUnitySoundEngine";

    [StructLayout(LayoutKind.Sequential)]
    public struct AkVector
    {
        public float X, Y, Z;

        public AkVector(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
    }

    // AkAuxSendValue is an opaque native struct — size queried at runtime, written via CSharp_AkAuxSendValue_Set
    [DllImport(DllName, EntryPoint = "CSharp_AkAuxSendValue_GetSizeOf")]
    private static extern int AkAuxSendValue_GetSizeOf();

    [DllImport(DllName, EntryPoint = "CSharp_AkAuxSendValue_Set")]
    private static extern void AkAuxSendValue_Set(IntPtr slot, ulong listenerID, uint auxBusID, float controlValue);

    [DllImport(DllName, EntryPoint = "CSharp_RegisterGameObjInternal_WithName")]
    private static extern int RegisterGameObjInternal_WithName(ulong gameObjectId,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    public static int RegisterGameObj(ulong gameObjectId, string name)
        => RegisterGameObjInternal_WithName(gameObjectId, name);

    [DllImport(DllName, EntryPoint = "CSharp_UnregisterGameObjInternal")]
    private static extern int UnregisterGameObjInternal(ulong gameObjectId);

    public static int UnregisterGameObj(ulong gameObjectId)
        => UnregisterGameObjInternal(gameObjectId);

    // PostEvent by name (wide string)
    [DllImport(DllName, EntryPoint = "CSharp_PostEvent__SWIG_7")]
    private static extern uint PostEvent_ByName(
        [MarshalAs(UnmanagedType.LPWStr)] string eventName, ulong gameObjectId);

    public static uint PostEvent(string eventName, ulong gameObjectId)
        => PostEvent_ByName(eventName, gameObjectId);

    // SetRTPCValue by name — scoped to a game object
    [DllImport(DllName, EntryPoint = "CSharp_SetRTPCValue__SWIG_8")]
    private static extern int SetRTPCValue_ByName(
        [MarshalAs(UnmanagedType.LPWStr)] string rtpcName, float value, ulong gameObjectId);

    public static int SetRTPCValue(string rtpcName, float value, ulong gameObjectId)
        => SetRTPCValue_ByName(rtpcName, value, gameObjectId);

    // SetRTPCValue by name — global scope (bus-level RTPCs, no game object)
    [DllImport(DllName, EntryPoint = "CSharp_SetRTPCValue__SWIG_9")]
    private static extern int SetRTPCValue_Global(
        [MarshalAs(UnmanagedType.LPWStr)] string rtpcName, float value);

    public static int SetRTPCValue(string rtpcName, float value)
        => SetRTPCValue_Global(rtpcName, value);

    // SetObjectPosition takes (gameObjectId, position, orientationFront, orientationTop)
    // AkVector matches UnityEngine.Vector3 layout (3 floats)
    [DllImport(DllName, EntryPoint = "CSharp_SetObjectPosition")]
    private static extern int SetObjectPosition(ulong gameObjectId,
        AkVector position, AkVector orientationFront, AkVector orientationTop);

    public static int SetPosition(ulong gameObjectId, AkVector position, AkVector front, AkVector top)
        => SetObjectPosition(gameObjectId, position, front, top);

    // SetListeners — assigns which listener(s) a game object hears from
    [DllImport(DllName, EntryPoint = "CSharp_SetListeners")]
    private static extern int SetListeners(ulong gameObjectId, ulong[] listenerIds, uint count);

    public static int SetListeners(ulong gameObjectId, ulong[] listenerIds)
        => SetListeners(gameObjectId, listenerIds, (uint)listenerIds.Length);

    // AddDefaultListener — makes a game object a default listener
    [DllImport(DllName, EntryPoint = "CSharp_AddDefaultListener")]
    public static extern int AddDefaultListener(ulong listenerObjectId);

    // LoadBank by name (wide string)
    [DllImport(DllName, EntryPoint = "CSharp_LoadBank__SWIG_1")]
    private static extern int LoadBank_ByName(
        [MarshalAs(UnmanagedType.LPWStr)] string bankName, out uint outBankId);

    public static int LoadBankByName(string bankName, out uint outBankId)
        => LoadBank_ByName(bankName, out outBankId);

    // StopPlayingID
    [DllImport(DllName, EntryPoint = "CSharp_StopPlayingID__SWIG_1")]
    private static extern void StopPlayingID_WithTransition(uint playingId, int transitionDuration);

    public static void StopPlayingID(uint playingId, int transitionDuration = 0)
        => StopPlayingID_WithTransition(playingId, transitionDuration);

    // SetGameObjectAuxSendValues — buffer must be laid out using AkAuxSendValue_Set
    [DllImport(DllName, EntryPoint = "CSharp_SetGameObjectAuxSendValues")]
    private static extern int SetGameObjectAuxSendValuesNative(ulong gameObjectId,
        IntPtr auxSendValues, uint numSendValues);

    // AK_INVALID_GAME_OBJECT — tells Wwise the send is not spatialized relative to a specific listener
    private const ulong AK_INVALID_GAME_OBJECT = ulong.MaxValue;

    public static int SetGameObjectAuxSendValues(ulong gameObjectId, uint auxBusID, float controlValue)
    {
        int stride = AkAuxSendValue_GetSizeOf();
        IntPtr buffer = Marshal.AllocHGlobal(stride);
        try
        {
            AkAuxSendValue_Set(buffer, AK_INVALID_GAME_OBJECT, auxBusID, controlValue);
            return SetGameObjectAuxSendValuesNative(gameObjectId, buffer, 1);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // RenderAudio
    [DllImport(DllName, EntryPoint = "CSharp_RenderAudio__SWIG_1")]
    public static extern int RenderAudio();

    [DllImport(DllName, EntryPoint = "CSharp_new_AkInitializationSettings")]
    private static extern IntPtr NewAkInitializationSettings();

    [DllImport(DllName, EntryPoint = "CSharp_delete_AkInitializationSettings")]
    private static extern void DeleteAkInitializationSettings(IntPtr settings);

    [DllImport(DllName, EntryPoint = "CSharp_Init")]
    private static extern int InitNative(IntPtr settings);

    public static int Init()
    {
        IntPtr settings = NewAkInitializationSettings();
        int result = InitNative(settings);
        DeleteAkInitializationSettings(settings);
        return result;
    }

    [DllImport(DllName, EntryPoint = "CSharp_SetBasePath")]
    private static extern int SetBasePathNative([MarshalAs(UnmanagedType.LPWStr)] string basePath);

    public static int SetBasePath(string basePath)
        => SetBasePathNative(basePath);

    [DllImport(DllName, EntryPoint = "CSharp_Term")]
    public static extern void Term();

    [DllImport(DllName, EntryPoint = "CSharp_new_AkCommunicationSettings")]
    private static extern IntPtr NewAkCommunicationSettings();

    [DllImport(DllName, EntryPoint = "CSharp_delete_AkCommunicationSettings")]
    private static extern void DeleteAkCommunicationSettings(IntPtr settings);

    [DllImport(DllName, EntryPoint = "CSharp_InitCommunication")]
    private static extern int InitCommunicationNative(IntPtr settings);

    public static int InitCommunication()
    {
        IntPtr settings = NewAkCommunicationSettings();
        int result = InitCommunicationNative(settings);
        DeleteAkCommunicationSettings(settings);
        return result;
    }

    // AKRESULT 1 == AK_Success
    public static void LogResult(string call, int result)
    {
        if (result != 1)
            Console.Error.WriteLine($"[Wwise] {call} failed: AKRESULT={result}");
        else
            Console.WriteLine($"[Wwise] {call} OK");
    }

    public static void LogPostEvent(string eventName, ulong gameObjectId, uint playingId)
    {
        if (playingId == 0)
            Console.Error.WriteLine($"[Wwise] PostEvent(\"{eventName}\", obj={gameObjectId}) failed: returned AK_INVALID_PLAYING_ID");
        else
            Console.WriteLine($"[Wwise] PostEvent(\"{eventName}\", obj={gameObjectId}) OK: playingId={playingId}");
    }
}
