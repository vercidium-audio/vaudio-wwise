using System;
using System.IO;

namespace vaudio_wwise;

public class WwiseSystem
{
    private const ulong ListenerObjectId = 0;
    private ulong nextSoundObjectId = 1;

    // Wwise aux bus used for the environment reverb (ShortID from Init.json AuxBusses)
    private const uint ReverbAuxBusID = 3744218805;

    // RTPCs wired to RoomVerb on ReverbBus
    private const string RtpcPreDelay    = "Reverb_PreDelay";
    private const string RtpcDecayTime   = "Reverb_DecayTime";
    private const string RtpcHFDamping   = "Reverb_HFDamping";
    private const string RtpcDiffusion   = "Reverb_Diffusion";
    private const string RtpcStereoWidth = "Reverb_StereoWidth";
    private const string RtpcFrontLevel  = "Reverb_FrontLevel";
    private const string RtpcRearLevel   = "Reverb_RearLevel";
    private const string RtpcCenterLevel = "Reverb_CenterLevel";
    private const string RtpcLFELevel    = "Reverb_LFELevel";
    private const string RtpcDryLevel    = "Reverb_DryLevel";
    private const string RtpcERLevel     = "Reverb_ERLevel";
    private const string RtpcReverbLevel = "Reverb_ReverbLevel";

    // Wwise event posted to start a looping sound on a game object
    private const string PlaySoundEvent = "Play_Speech";

    public WwiseSystem()
    {
        AkSoundEngine.LogResult("Init", AkSoundEngine.Init());
        AkSoundEngine.LogResult("InitCommunication", AkSoundEngine.InitCommunication());
        AkSoundEngine.LogResult("RegisterGameObj(Listener)", AkSoundEngine.RegisterGameObj(ListenerObjectId, "Listener"));
        AkSoundEngine.LogResult("AddDefaultListener(Listener)", AkSoundEngine.AddDefaultListener(ListenerObjectId));
    }

    public void UpdateReverb(vaudio.EAXReverbResults eax)
    {
        // Bus-level RTPCs must use the global overload (no game object) — they drive RoomVerb on ReverbBus
        AkSoundEngine.SetRTPCValue(RtpcPreDelay,    Math.Clamp(eax.ReflectionsDelay * 1000f, 0f, 200f));
        AkSoundEngine.SetRTPCValue(RtpcDecayTime,   Math.Clamp(eax.DecayTime, 0.1f, 20f));
        AkSoundEngine.SetRTPCValue(RtpcHFDamping,   Math.Clamp((1f - eax.DecayHFRatio) * 100f, 0f, 100f));
        AkSoundEngine.SetRTPCValue(RtpcDiffusion,   Math.Clamp(eax.Diffusion * 100f, 0f, 100f));
        AkSoundEngine.SetRTPCValue(RtpcStereoWidth, 100f);
        AkSoundEngine.SetRTPCValue(RtpcReverbLevel, LinearToDb(eax.LateReverbGain));
        AkSoundEngine.SetRTPCValue(RtpcERLevel,     LinearToDb(eax.ReflectionsGain));
        AkSoundEngine.SetRTPCValue(RtpcDryLevel,    LinearToDb(eax.Gain));
        AkSoundEngine.SetRTPCValue(RtpcFrontLevel,  0f);
        AkSoundEngine.SetRTPCValue(RtpcRearLevel,   0f);
        AkSoundEngine.SetRTPCValue(RtpcCenterLevel, 0f);
        AkSoundEngine.SetRTPCValue(RtpcLFELevel,    0f);
    }

    static float LinearToDb(float linear)
        => linear > 0f ? Math.Clamp(20f * MathF.Log10(linear), -96f, 0f) : -96f;

    public void LoadSoundData(string filePath)
    {
        string bankDir = Path.GetFullPath(Path.GetDirectoryName(filePath)!);
        string bankName = Path.GetFileNameWithoutExtension(filePath);

        AkSoundEngine.LogResult($"SetBasePath(\"{bankDir}\")", AkSoundEngine.SetBasePath(bankDir));
        AkSoundEngine.LogResult("LoadBankByName(\"Init\")", AkSoundEngine.LoadBankByName("Init", out _));

        AkSoundEngine.LogResult($"LoadBankByName(\"{bankName}\")", AkSoundEngine.LoadBankByName(bankName, out uint bankId));
        Console.WriteLine($"[Wwise] LoadBank \"{bankName}\" bankId={bankId}");
    }

    public WwiseSound CreateSound(vaudio.Vector3F position)
    {
        ulong objectId = nextSoundObjectId++;
        Console.WriteLine($"[Wwise] CreateSound objectId={objectId}");
        AkSoundEngine.LogResult($"RegisterGameObj(Sound_{objectId})", AkSoundEngine.RegisterGameObj(objectId, $"Sound_{objectId}"));
        AkSoundEngine.LogResult($"SetListeners(Sound_{objectId})", AkSoundEngine.SetListeners(objectId, [ListenerObjectId]));

        AkSoundEngine.LogResult($"SetPosition(Sound_{objectId})", AkSoundEngine.SetPosition(objectId,
            new AkSoundEngine.AkVector(position.X, position.Y, position.Z),
            new AkSoundEngine.AkVector(0f, 0f, 1f),
            new AkSoundEngine.AkVector(0f, 1f, 0f)));

        AkSoundEngine.LogResult($"SetGameObjectAuxSendValues(Sound_{objectId})", AkSoundEngine.SetGameObjectAuxSendValues(objectId, ReverbAuxBusID, 1.0f));

        return new WwiseSound(objectId);
    }

    public void SetListenerPosition(vaudio.Vector3F position, float pitch, float yaw)
    {
        float cosPitch = MathF.Cos(pitch);
        float sinPitch = MathF.Sin(pitch);
        float cosYaw   = MathF.Cos(yaw);
        float sinYaw   = MathF.Sin(yaw);

        AkSoundEngine.SetPosition(ListenerObjectId,
            new AkSoundEngine.AkVector(position.X, position.Y, position.Z),
            new AkSoundEngine.AkVector(cosPitch * sinYaw, sinPitch, cosPitch * cosYaw),
            new AkSoundEngine.AkVector(-sinPitch * sinYaw, cosPitch, -sinPitch * cosYaw));
    }

    public void Update()
    {
        AkSoundEngine.RenderAudio();
    }

    public void Cleanup()
    {
        AkSoundEngine.LogResult("UnregisterGameObj(Listener)", AkSoundEngine.UnregisterGameObj(ListenerObjectId));
        AkSoundEngine.Term();
    }
}
