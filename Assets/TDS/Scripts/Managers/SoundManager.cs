using Data;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

public class SoundManager
{
    private int effIdx = 0;
    private List<AudioSource> effSound = new List<AudioSource>();
    private int voiceIdx = 0;
    private List<AudioSource> voiceSound = new List<AudioSource>();
    private List<AudioSource> uiVoiceSound = new List<AudioSource>();
    AudioSource[] audioSources = new AudioSource[(int)Define.Sound.Max];
    AudioMixerGroup[] mixerGroup = new AudioMixerGroup[(int)Define.Sound.Max];
    bool[] muteAudioSource = new bool[(int)Define.Sound.Max]; //페이드인 페이드아웃 사용용도 음소거(옵션에서 음소거 시에는 믹서그룹의 볼륨을 없애자)
    private Dictionary<AudioSource, bool> bgmList = new Dictionary<AudioSource, bool>();
    Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    private GameObject root;
    public bool isFadeOut;
    public AudioClip swapBgmAudioClip;
    private bool isAnotherBgmFadeOut;
    private AudioClip swapAnotherBgmAudioClip;
    private bool isMainOtherBgmOff;

    private const float FadeTime = 1;


    public void Init()
    {
        AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
        var audioMixer = Resources.Load<AudioMixer>("MasterAudioMixer");

        root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);

            string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
            for (int i = 0; i < soundNames.Length - 1; i++)
            {
                var soundName = soundNames[i];
                var mixters = audioMixer.FindMatchingGroups(soundName);
                mixerGroup[i] = mixters[0];

                GameObject go = new GameObject { name = soundName };
                var source = go.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = mixters[0];
                go.transform.parent = root.transform;
                audioSources[i] = source;
            }

            audioSources[(int)Define.Sound.Bgm].loop = true;
            audioSources[(int)Define.Sound.AnotherBgm].loop = true;

            for (int i = 0; i < 10; i++)
            {
                var groups = audioMixer.FindMatchingGroups("Effect");
                GameObject go = new GameObject { name = "Effect" + i };
                var source = go.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = groups[0];
                go.transform.parent = root.transform;
                source.loop = false;
                effSound.Add(source);
            }

            for (int i = 0; i < 4; i++)
            {
                var groups = audioMixer.FindMatchingGroups("Voice");
                GameObject go = new GameObject { name = "Voice" + i };
                var source = go.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = groups[0];
                go.transform.parent = root.transform;
                source.loop = false;
                voiceSound.Add(source);
            }

            for (int i = 0; i < 1; i++)
            {
                var groups = audioMixer.FindMatchingGroups("Voice");
                GameObject go = new GameObject { name = "Voice" + i };
                var source = go.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = groups[0];
                go.transform.parent = root.transform;
                source.loop = false;
                uiVoiceSound.Add(source);
            }

            var bgms = root.Descendants().OfComponent<AudioSource>();
            foreach (var bgm in bgms)
            {
                bgmList.Add(bgm, false);
            }

            MainThreadDispatcher.UpdateAsObservable().Subscribe(_1 =>
            {
                var i = UnsafeUtility.EnumToInt(Define.Sound.Bgm);
                if (muteAudioSource[i])
                    return;

                var bgm = audioSources[i];
                if (isFadeOut)
                {
                    bgm.volume -= Time.deltaTime / FadeTime;
                    if (bgm.volume <= 0)
                    {
                        isFadeOut = false;
                        bgm.clip = swapBgmAudioClip;
                        bgm.Play();
                        swapBgmAudioClip = null;
                    }
                }
                else
                {
                    bgm.volume += Time.deltaTime / FadeTime;
                }
            });


            MainThreadDispatcher.UpdateAsObservable().Subscribe(_1 =>
            {
                var i = UnsafeUtility.EnumToInt(Define.Sound.AnotherBgm);
                if (muteAudioSource[i])
                    return;

                var bgm = audioSources[i];
                if (isAnotherBgmFadeOut)
                {
                    bgm.volume -= Time.deltaTime / FadeTime;
                    if (bgm.volume <= 0)
                    {
                        isAnotherBgmFadeOut = false;
                        bgm.clip = swapAnotherBgmAudioClip;
                        bgm.Play();
                        swapAnotherBgmAudioClip = null;
                    }
                }
                else
                {
                    bgm.volume += Time.deltaTime / FadeTime;
                }
            });

            MainThreadDispatcher.UpdateAsObservable().ThrottleFirstFrame(5).Subscribe(_1 =>
            {
                for (int i = 0; i < muteAudioSource.Length; ++i)
                {
                    if (muteAudioSource[i])
                    {
                        var source = audioSources[i];
                        source.volume -= Time.deltaTime * 5 / FadeTime;
                    }
                }
            });
        }
    }


    void OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        Debug.Log(deviceWasChanged ? "Device was changed" : "Reset was called");
        //if (deviceWasChanged)
        {
            mixerGroup[(int)Define.Sound.Master].audioMixer.GetFloat("MasterVolume", out var volume1);
            mixerGroup[(int)Define.Sound.Master].audioMixer.GetFloat("BgmVolume", out var volume2);
            mixerGroup[(int)Define.Sound.Master].audioMixer.GetFloat("EffectVolume", out var volume3);
            mixerGroup[(int)Define.Sound.Master].audioMixer.GetFloat("VoiceVolume", out var volume4);

            Debug.Log("MasterVolume : " + volume1.ToString());
            Debug.Log("BgmVolume : " + volume2.ToString());
            Debug.Log("EffectVolume : " + volume3.ToString());
            Debug.Log("VoiceVolume : " + volume4.ToString());

            SetSound(Define.Sound.Master, 1);
            SetSound(Define.Sound.Bgm, 1);
            SetSound(Define.Sound.Effect, 1);
            SetSound(Define.Sound.Voice, 1);
        }
    }
    public void Clear()
    {
        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }

        audioClips.Clear();
    }

    public async void PlayAsync(string _path, Define.Sound _type = Define.Sound.Effect, float _pitch = 1.0f)
    {
        if (audioClips.TryGetValue(_path, out var _1))
        {
            Play(_1, _type, _pitch);
            return;
        }

        if (_path.Contains("Sounds/") == false)
            _path = $"Sounds/{_path}";

        var clip = await Managers.Resource.LoadAsync<AudioClip>(_path, "");
        if (_type != Define.Sound.Bgm)
        {
            if (audioClips.TryGetValue(_path, out _) == false)
            {
                audioClips.Add(_path, clip);
            }

            Play(clip, _type, _pitch);
        }
    }

    public AudioSource Play(AudioClip _audioClip, Define.Sound _type = Define.Sound.Effect, float _pitch = 1.0f, bool _mustPlay = false, bool _uiVoiceGroup = false)
    {
        if (_audioClip == null)
            return null;

        if (_type == Define.Sound.Bgm)
        {
            SetFocusAnotherBgm(false);
            var audioSource = audioSources[(int)Define.Sound.Bgm];
            if (_audioClip == audioSource.clip) return null;
            if (audioSource.clip == null)
            {
                audioSource.clip = _audioClip;
                audioSource.Play();
                audioSource.volume = 0;
            }
            else
            {
                isFadeOut = true;
                swapBgmAudioClip = _audioClip;
            }

            return audioSource;
        }
        else if (_type == Define.Sound.Voice)
        {
            if (_uiVoiceGroup)
                return PlayAudioClipUIVoiceSound(_audioClip, _pitch);
            else
                return PlayAudioClipVoiceSound(_audioClip, _pitch, _mustPlay);
        }
        else if (_type == Define.Sound.AnotherBgm)
        {
            SetFocusAnotherBgm(true);
            if (_audioClip == audioSources[(int)Define.Sound.AnotherBgm].clip) return null;
            isAnotherBgmFadeOut = true;
            swapAnotherBgmAudioClip = _audioClip;
        }
        else
        {
            return PlayAudioClipEffectSound(_audioClip, _pitch);
        }

        return null;
    }


    public AudioSource PlayAudioClipEffectSound(AudioClip audioClip, float _pitch, bool loop = false)
    {
        if (null == audioClip)
            return null;

        var count = effSound.Count;
        foreach (var item in effSound)
        {
            if (item.isPlaying == false)
            {
                item.loop = loop;
                item.clip = audioClip;
                item.Play();
                return item;
            }
        }

        var audioSource = effSound[effIdx % count];
        audioSource.Stop();
        audioSource.clip = audioClip;
        audioSource.loop = loop;
        audioSource.Play();
        effIdx = (effIdx + 1) % count;
        return audioSource;
    }

    public AudioSource PlayAudioClipVoiceSound(AudioClip audioClip, float _pitch, bool _mustPlay = false)
    {
        if (null == audioClip)
            return null;

        var count = voiceSound.Count;
        foreach (var item in voiceSound)
        {
            if (item.isPlaying == false)
            {
                item.loop = false;
                item.clip = audioClip;
                item.Play();
                return item;
            }
        }

        if (_mustPlay)
        {
            var audioSource = voiceSound[voiceIdx % count];
            audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.Play();
            voiceIdx = (voiceIdx + 1) % count;
            return audioSource;
        }
        else
        {
            var audioSource = voiceSound[0];
            return audioSource;
        }
    }

    public AudioSource PlayAudioClipUIVoiceSound(AudioClip audioClip, float _pitch)
    {
        if (null == audioClip)
            return null;

        var count = uiVoiceSound.Count;
        foreach (var item in uiVoiceSound)
        {
            if (item.isPlaying == false)
            {
                item.loop = false;
                item.clip = audioClip;
                item.Play();
                return item;
            }
        }

        var audioSource = uiVoiceSound[0];
        return audioSource;
    }

    public bool CheckPlayUiVoiceSount()
    {
        foreach (var item in uiVoiceSound)
        {
            if (item.isPlaying == false)
            {
                return true;
            }
        }
        return false;
    }
    public AudioClip GetClip(Define.Sound _bgm)
    {
        return audioSources[(int)Define.Sound.Bgm].clip;
    }

    public void SetFocusAnotherBgm(bool _isFocus)
    {
        muteAudioSource[(int)Define.Sound.Bgm] = _isFocus;
        muteAudioSource[(int)Define.Sound.AnotherBgm] = !_isFocus;
    }
    public void SetMainOtherBgmMute(bool _isFocus)
    {
        foreach (var item in voiceSound)
        {
            item.mute = _isFocus;
        }
        foreach (var item in effSound)
        {
            item.mute = _isFocus;
        }
        isMainOtherBgmOff = _isFocus;
    }
    public bool GetMainOtherBgmMute()
    {
        return isMainOtherBgmOff;
    }

    public void PauseBgm()
    {
        var audioSources = new List<AudioSource>();
        foreach (var item in bgmList)
        {
            if (item.Key.clip == null)
                continue;

            if (item.Key.isPlaying)
            {
                item.Key.Pause();
                audioSources.Add(item.Key);
            }
        }

        foreach (var audio in audioSources)
        {
            bgmList[audio] = true;
        }

    }

    public void ResumeBgm()
    {
        var audioSources = new List<AudioSource>();
        foreach (var item in bgmList)
        {
            if (item.Value == true)
            {
                item.Key.Play();
                audioSources.Add(item.Key);
            }
        }
        foreach (var audio in audioSources)
        {
            bgmList[audio] = false;
        }
    }

    public void SetBgmMute(Define.Sound soundType)
    {
        muteAudioSource[(int)soundType] = true;
    }

    public void SetBgmMuteOff(Define.Sound soundType)
    {
        muteAudioSource[(int)soundType] = false;
    }

    float Remap(float value, float min1, float max1, float min2, float max2)
    {
        return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
    }

    public void SetSound(Define.Sound soundType, float _volume)
    {
        switch (soundType)
        {
            case Define.Sound.Master:
                {
                    mixerGroup[(int)Define.Sound.Master].audioMixer.GetFloat("MasterVolume", out var volume);
                    mixerGroup[(int)Define.Sound.Master].audioMixer.SetFloat("MasterVolume", _volume == 0 ? -80 : 0);
                }
                break;
            case Define.Sound.Bgm:
            case Define.Sound.AnotherBgm:
                {
                    var value = Remap(_volume, 0, 100f, 0.0001f, 1f);
                    mixerGroup[(int)Define.Sound.Master].audioMixer.GetFloat("BgmVolume", out var volume);
                    mixerGroup[(int)Define.Sound.Master].audioMixer.SetFloat("BgmVolume", Mathf.Log10(value) * 20);
                    mixerGroup[(int)Define.Sound.Master].audioMixer.SetFloat("ScenarioBgmVolume", Mathf.Log10(value) * 20);
                }
                break;
            case Define.Sound.Effect:
                {
                    mixerGroup[(int)Define.Sound.Master].audioMixer.GetFloat("EffectVolume", out var volume);
                    var value = Remap(_volume, 0, 100f, 0.0001f, 1f);
                    mixerGroup[(int)Define.Sound.Master].audioMixer.SetFloat("EffectVolume", Mathf.Log10(value) * 20);
                }
                break;
            case Define.Sound.Voice:
                {
                    mixerGroup[(int)Define.Sound.Master].audioMixer.GetFloat("VoiceVolume", out var volume);
                    var value = Remap(_volume, 0, 100f, 0.0001f, 1f);
                    mixerGroup[(int)Define.Sound.Master].audioMixer.SetFloat("VoiceVolume", Mathf.Log10(value) * 20);
                }
                break;
            case Define.Sound.Max:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(soundType), soundType, null);
        }
    }

    public void StopEffect()
    {
        foreach (var item in effSound)
        {
            if (item.isPlaying)
            {
                item.Stop();
            }
        }
    }
}