using System.Collections;
using UnityEngine;

namespace DaiVQScript.Utilities
{
    [System.Serializable]
    public abstract class SFxPlayerAbstract
    {
        [SerializeField] protected AudioSource _audioSource = null;
        public virtual void SetUp() { }
        public abstract void Play();
    }


    [System.Serializable]
    public class SFxPlayerSingleSound : SFxPlayerAbstract
    {
        public AudioClip clip = null;
        public float pitch = 1f;
        public float volume = 1f;

        public override void Play()
        {
            if (!_audioSource) return;
            _audioSource.pitch = 1f;
            _audioSource.PlayOneShot(clip, volume);
        }
    }

    [System.Serializable]
    public class SFxPlayerRepeatSound : SFxPlayerAbstract
    {
        [Header("Sound")]
        [SerializeField] private AudioClip _clip = null;

        [Header("Loop")]
        [SerializeField] private float _minDelay = 0.25f;

        [Header("Pitch")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(-1f, 3f);
        [SerializeField] private float _pitchNoiseFrequency = 1f;
        private float _randomNoiseY;

        [Header("Volume")]
        [SerializeField] private Vector2 _volumeRange = new Vector2(0f, 1f);
        [SerializeField] private float _delayGain = 0.1f;
        [SerializeField] private float _volumeDecayEachPlay = 0.25f;
        [SerializeField] private float _volumeGainPerSecond = 0.25f;
        private float _lastTimePlay;
        private float _volumeAfterPlayLastTime;

        public override void SetUp()
        {
            base.SetUp();
            ResetState();
        }

        public override void Play()
        {
            if (!_audioSource || Time.time < _lastTimePlay + _minDelay) return;

            float pitch = Mathf.LerpUnclamped(_pitchRange.x, _pitchRange.y, Mathf.PerlinNoise(Time.time * _pitchNoiseFrequency, _randomNoiseY));
            float volume = (Time.time > _lastTimePlay + _delayGain) ?  Mathf.Min(_volumeAfterPlayLastTime + _volumeGainPerSecond * (Time.time - _lastTimePlay - _delayGain), _volumeRange.y) : _volumeAfterPlayLastTime;

            _audioSource.pitch = pitch;
            _audioSource.PlayOneShot(_clip, volume);

            _lastTimePlay = Time.time;
            _volumeAfterPlayLastTime = Mathf.Max(_volumeRange.x, volume - _volumeDecayEachPlay);
        }

        public void ResetState()
        {
            _randomNoiseY = UnityEngine.Random.Range(-_pitchNoiseFrequency, _pitchNoiseFrequency);
            _lastTimePlay = 0;
            _volumeAfterPlayLastTime = _volumeRange.y;
        }
    }
}