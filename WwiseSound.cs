using System;

namespace vaudio_wwise;

public class WwiseSound
{
    private readonly ulong objectId;
    private uint playingId;

    internal WwiseSound(ulong objectId)
    {
        this.objectId = objectId;
    }

    public void Play()
    {
        playingId = AkSoundEngine.PostEvent("Play_Speech", objectId);
        AkSoundEngine.LogPostEvent("Play_Speech", objectId, playingId);
    }

    public void UpdatePosition(vaudio.Vector3F pos)
    {
        AkSoundEngine.SetPosition(objectId,
            new AkSoundEngine.AkVector(pos.X, pos.Y, pos.Z),
            new AkSoundEngine.AkVector(0f, 0f, 1f),
            new AkSoundEngine.AkVector(0f, 1f, 0f));
    }

    public void UpdateFilter(vaudio.AudioFilter filter)
    {
        // gainHF 1.0 = fully open, 0.0 = fully closed; Wwise LPF is 0 (open) to 100 (closed)
        float lpfValue = (1f - filter.gainHF) * 100f;
        AkSoundEngine.LogResult($"SetRTPCValue(Speech_LPF={lpfValue:F1}, obj={objectId})", AkSoundEngine.SetRTPCValue("Speech_LPF", lpfValue, objectId));
    }

    public void Stop()
    {
        Console.WriteLine($"[Wwise] StopPlayingID(playingId={playingId})");
        AkSoundEngine.StopPlayingID(playingId);
        AkSoundEngine.LogResult($"UnregisterGameObj(obj={objectId})", AkSoundEngine.UnregisterGameObj(objectId));
    }
}
