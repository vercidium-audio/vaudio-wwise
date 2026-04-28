namespace vaudio_wwise;

public class WwiseExample
{
    private const ulong SoundEmitterId = 1;

    public void PlaySoundAt(float x, float y, float z)
    {
        // Register the game object that will emit the sound
        AkSoundEngine.RegisterGameObj(SoundEmitterId, "MySoundEmitter");

        // Set its 3D position
        var position = new AkSoundEngine.AkSoundPosition
        {
            Position = new AkSoundEngine.AkVector(x, y, z),
            OrientationFront = new AkSoundEngine.AkVector(0f, 0f, 1f),
            OrientationTop = new AkSoundEngine.AkVector(0f, 1f, 0f)
        };
        AkSoundEngine.SetPosition(SoundEmitterId, ref position);

        // Post the Wwise event by name (or use the AkUniqueID uint for perf)
        uint playingId = AkSoundEngine.PostEvent("Play_MySound", SoundEmitterId);
    }

    public void Cleanup()
    {
        AkSoundEngine.UnregisterGameObj(SoundEmitterId);
    }
}