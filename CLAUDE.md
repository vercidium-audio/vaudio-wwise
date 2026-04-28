Wwise DLL: AkUnitySoundEngine.dll (from Wwise Unity package)
Source: C:\Users\verc\Documents\wwise_unity\windows\Wwise\API\Runtime\Plugins\Windows\x86_64\Debug\AkUnitySoundEngine.dll

Do NOT use AkSoundEngineDLL.dll. It exports functions like RegisterGameObj, PostEvent, etc. only as
C++ mangled names — they cannot be called by plain name via P/Invoke and will throw EntryPointNotFoundException.

AkUnitySoundEngine.dll exposes SWIG-generated C-linkage entry points (CSharp_*) that work with P/Invoke.

SWIG-generated PINVOKE reference file (full signatures for all functions):
  C:\Users\verc\Documents\wwise_unity\windows\Wwise\API\Runtime\Generated\Windows\AkUnitySoundEnginePINVOKE_Windows.cs

---

## Entry point mapping

  CSharp_RegisterGameObjInternal_WithName(ulong, string LPStr)   -> RegisterGameObj
  CSharp_UnregisterGameObjInternal(ulong)                        -> UnregisterGameObj
  CSharp_PostEvent__SWIG_7(string LPWStr, ulong)                 -> PostEvent (by name)
  CSharp_SetRTPCValue__SWIG_8(string LPWStr, float, ulong)       -> SetRTPCValue (by name, scoped to game object)
  CSharp_SetRTPCValue__SWIG_9(string LPWStr, float)              -> SetRTPCValue (by name, GLOBAL scope — use for bus-level RTPCs)
  CSharp_SetObjectPosition(ulong, AkVector, AkVector, AkVector)  -> SetPosition (position, front, top)
  CSharp_SetListeners(ulong, ulong[], uint)                      -> SetListeners
  CSharp_AddDefaultListener(ulong)                               -> AddDefaultListener
  CSharp_LoadBank__SWIG_1(string LPWStr, out uint)               -> LoadBankByName
  CSharp_StopPlayingID__SWIG_1(uint, int)                        -> StopPlayingID
  CSharp_SetGameObjectAuxSendValues(ulong, IntPtr, uint)         -> SetGameObjectAuxSendValues
  CSharp_RenderAudio__SWIG_1()                                   -> RenderAudio
  CSharp_Init(IntPtr)                                            -> Init (pass AkInitializationSettings* — see below)
  CSharp_Term()                                                  -> Term
  CSharp_SetBasePath(string LPWStr)                              -> SetBasePath

---

## Initialization

CSharp_Init does NOT accept IntPtr.Zero. It requires a heap-allocated AkInitializationSettings*
that Wwise fills with defaults. Pattern:

  IntPtr settings = CSharp_new_AkInitializationSettings();  // allocates + fills defaults
  int result = CSharp_Init(settings);
  CSharp_delete_AkInitializationSettings(settings);

SetBasePath must be called before LoadBankByName. Init.bnk must be loaded before any user bank.
LoadBankByName returns a garbage bankId if the file is not found (AKRESULT=66 AK_FileNotFound).

Full init sequence:
  1. CSharp_Init(settings)
  2. CSharp_SetBasePath(bankDir)
  3. CSharp_LoadBank__SWIG_1("Init", out _)
  4. CSharp_LoadBank__SWIG_1("speech", out bankId)

---

## AkVector

AkVector is a [StructLayout(LayoutKind.Sequential)] struct with three floats (X, Y, Z).
It IS blittable and memory-compatible with UnityEngine.Vector3 — safe to pass by value via P/Invoke.

SetObjectPosition takes (gameObjectId, position, orientationFront, orientationTop) as three separate AkVectors,
NOT a single AkSoundPosition struct.

---

## AkAuxSendValue — NOT a blittable struct

AkAuxSendValue is a SWIG opaque type. Do NOT define it as a C# blittable struct and pin it —
the native size differs from ulong+uint+float and you will pass garbage.

Correct pattern (mirrors AkAuxSendArray.cs from the Unity runtime):
  int stride = CSharp_AkAuxSendValue_GetSizeOf();
  IntPtr buffer = Marshal.AllocHGlobal(stride);
  CSharp_AkAuxSendValue_Set(buffer, listenerID, auxBusID, controlValue);
  CSharp_SetGameObjectAuxSendValues(gameObjectId, buffer, 1);
  Marshal.FreeHGlobal(buffer);

---

## AkInitializationSettings — also NOT blittable

Same pattern as AkAuxSendValue — heap-allocated by Wwise, not a blittable C# struct.
Use CSharp_new_AkInitializationSettings / CSharp_delete_AkInitializationSettings.

---

## RTPC scoping

Bus-level RTPCs (e.g. RoomVerb parameters on an aux bus) MUST use the global SetRTPCValue overload
(CSharp_SetRTPCValue__SWIG_9, no game object argument). Using the game-object-scoped overload
(SWIG_8) with ListenerObjectId=0 silently succeeds (AKRESULT=1) but the bus never sees the value.

Per-sound RTPCs (e.g. EQ filter on a sound object) use the game-object-scoped overload (SWIG_8).

---

## Listener and game object setup

Listener setup requires two calls:
  AkSoundEngine.RegisterGameObj(ListenerObjectId, "Listener");
  AkSoundEngine.AddDefaultListener(ListenerObjectId);

Each sound game object also needs:
  AkSoundEngine.SetListeners(objectId, new ulong[] { ListenerObjectId });

---

## Aux send / reverb routing

SetGameObjectAuxSendValues routes a sound game object to an aux bus.
The aux bus ShortID comes from Init.json (GeneratedSoundBanks/Windows/Init.json) under "AuxBusses[].Id".
ReverbBus ShortID: 3744218805

SoundBank files live in: resource/audio/ (Init.bnk, speech.bnk, Init.json)
Generated from Wwise project: C:\Users\verc\Documents\WwiseProjects\convert_ogg\GeneratedSoundBanks\Windows\

---

## RoomVerb RTPCs (wired in Wwise authoring to ReverbBus effect slots)

These are bus-level — use global SetRTPCValue (no game object):
All RTPCs receive a 0–100 value; Wwise maps each to its native range internally:
  RTPC 0 = native min, RTPC 100 = native max.

  Reverb_PreDelay      (native 0–1000ms)   <- ReflectionsDelay(0–0.3s)*1000/10
  Reverb_DecayTime     (native 0.2–10s)    <- (DecayTime - 0.2) / 9.8 * 100
  Reverb_HFDamping     (native 0.5–10)     <- (DecayHFRatio - 0.5) / 9.5 * 100
  Reverb_Diffusion     (native -100–100%)  <- Diffusion(0–1) * 100
  Reverb_StereoWidth   (native -180–180°)  <- fixed 100
  Reverb_FrontLevel    (native -96.3–96.3dB) <- DbToRtpc(0)
  Reverb_RearLevel     (native -96.3–96.3dB) <- DbToRtpc(0)
  Reverb_CenterLevel   (native -96.3–96.3dB) <- DbToRtpc(0)
  Reverb_LFELevel      (native -96.3–96.3dB) <- DbToRtpc(0)
  Reverb_DryLevel      (native -96.3–96.3dB) <- DbToRtpc(LinearToDb(Gain))
  Reverb_ERLevel       (native -96.3–96.3dB) <- DbToRtpc(LinearToDb(ReflectionsGain))
  Reverb_ReverbLevel   (native -96.3–96.3dB) <- DbToRtpc(LinearToDb(LateReverbGain))

  DbToRtpc(db) = (db + 96.3) / 192.6 * 100

Note: there is a stray RTPC "Reverb_Diffusionew_Game_Parameter" in the project — ignore it, it's a typo artifact.

---

## Filter RTPCs (per-sound, game-object scoped)

  Speech_LPF  (0–100, 0 = fully open, 100 = fully closed)
              <- (1f - filter.gainHF) * 100f
              wired to the Low-Pass Filter property on the 'speech' sound object in Wwise authoring

---

## AKRESULT codes

  1  = AK_Success
  2  = AK_NotInitialized  (Init was not called before other API calls)
  31 = AK_InvalidParameter (e.g. passing IntPtr.Zero to Init)
  66 = AK_FileNotFound    (bank file missing or SetBasePath not called)
  0 from PostEvent = AK_INVALID_PLAYING_ID (event not found or bank not loaded)
