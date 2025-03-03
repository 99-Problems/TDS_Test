using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class SoundEventDispatcher : MonoBehaviour
{
    public enum ESOUND_TYPE
    {
        ENTRY_BAM,
        ENTRY_BAM2,
        
        EVENT_MARBLE_ADD_COMPLETE_CYCLE,
        EVENT_MARBLE_BACK_MOVE,
        EVENT_MARBLE_NEXT_MOVE,
        EVENT_MARBLE_ONE_MORE,
        EVENT_MARBLE_START_ROLL1,
        EVENT_MARBLE_START_ROLL2,
        EVENT_MARBLE_START_ROLL3,
        EVENT_MARBLE_RECOVERY,
        
    }

    public bool useOverlap = true;
    public SerializableDictionarySoundTypeAudioClip dicSoundList = new SerializableDictionarySoundTypeAudioClip();

    static Subject<ESOUND_TYPE> soundEvent = new Subject<ESOUND_TYPE>();
    private AudioSource audioSource;

    public static Subject<ESOUND_TYPE> SoundEvent
    {
        get => soundEvent;
    }

    void Start()
    {
        soundEvent.Subscribe(_ =>
        {
            if (dicSoundList.TryGetValue(_, out var audioClip))
            {
                if (audioSource && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }

                //audioSource = Managers.Sound.PlayAudioClipEffectSound(audioClip, 1);
            }
        }).AddTo(this);
    }
}