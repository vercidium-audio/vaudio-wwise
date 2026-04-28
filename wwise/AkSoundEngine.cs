using System.Runtime.InteropServices;

namespace vaudio_wwise;

public static class AkSoundEngine
{
    private const string DllName = "AkSoundEngine";

    [StructLayout(LayoutKind.Sequential)]
    public struct AkVector
    {
        public float X, Y, Z;

        public AkVector(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AkSoundPosition
    {
        public AkVector Position;
        public AkVector OrientationFront;
        public AkVector OrientationTop;
    }

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint PostEvent(string eventName, ulong gameObjectId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int RegisterGameObj(ulong gameObjectId, string name);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int UnregisterGameObj(ulong gameObjectId);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetPosition(ulong gameObjectId, ref AkSoundPosition position);
}