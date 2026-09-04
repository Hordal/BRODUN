// AudioManager.cs — BGM 22곡 / SFX 95종 / 앰비언스 (문서 06-3). 개별 볼륨(접근성).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace BroDungeon.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("믹서 그룹 (개별 볼륨 - 문서 06-6 접근성)")]
        public AudioMixer mixer;
        public AudioSource bgmSource;
        public AudioSource ambienceSource;
        public int sfxVoices = 16;

        [Header("클립 등록 (id → clip)")]
        public List<NamedClip> bgmClips = new List<NamedClip>();
        public List<NamedClip> sfxClips = new List<NamedClip>();

        readonly Dictionary<string, AudioClip> _bgm = new Dictionary<string, AudioClip>();
        readonly Dictionary<string, AudioClip> _sfx = new Dictionary<string, AudioClip>();
        AudioSource[] _sfxPool;
        int _sfxIndex;

        [System.Serializable] public struct NamedClip { public string id; public AudioClip clip; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            foreach (var c in bgmClips) if (c.clip) _bgm[c.id] = c.clip;
            foreach (var c in sfxClips) if (c.clip) _sfx[c.id] = c.clip;

            _sfxPool = new AudioSource[sfxVoices];
            for (int i = 0; i < sfxVoices; i++) _sfxPool[i] = gameObject.AddComponent<AudioSource>();
        }

        public void PlayBGM(string id, bool loop = true)
        {
            if (!_bgm.TryGetValue(id, out var clip) || bgmSource == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;
            bgmSource.clip = clip; bgmSource.loop = loop; bgmSource.Play();
        }

        public void PlaySFX(string id, float volume = 1f)
        {
            if (!_sfx.TryGetValue(id, out var clip)) return;
            var src = _sfxPool[_sfxIndex];
            _sfxIndex = (_sfxIndex + 1) % _sfxPool.Length;
            src.PlayOneShot(clip, volume);
        }

        public void PlayAmbience(string id)
        {
            if (!_bgm.TryGetValue(id, out var clip) || ambienceSource == null) return;
            ambienceSource.clip = clip; ambienceSource.loop = true; ambienceSource.Play();
        }

        // 접근성: 개별 볼륨 (선형 0~1 → dB)
        public void SetVolume(string mixerParam, float linear)
        {
            if (mixer == null) return;
            float db = linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
            mixer.SetFloat(mixerParam, db);
        }
    }
}
