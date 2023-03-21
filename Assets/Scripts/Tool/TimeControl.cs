using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Game.DesignPattern;
using DG.Tweening;

namespace Game.Tools
{
    public class TimeControl : MonoSingleton<TimeControl>
    {
        private float _timeScale = 1f;

        public bool isUseMinTime = true;
        private Dictionary<int, float> _dictTimeScales = new Dictionary<int, float>();

        public bool isUseTween = true;
        private Tween _tweenScaleTime = null;
        public float durationTweenScaling = 1f;
        public Ease easeTweenScaling = Ease.Linear;

        public float TimeScale => _timeScale;
        private void OnDictTimeScaleChanged()
        {
            _timeScale = 1f;
            if (isUseMinTime)
            {
                foreach (var timeScale in _dictTimeScales)
                {
                    if (timeScale.Value < _timeScale) _timeScale = timeScale.Value;
                }
            }
            else
            {
                foreach (var timeScale in _dictTimeScales)
                {
                    if (timeScale.Value > _timeScale) _timeScale = timeScale.Value;
                }
            }

            if (_tweenScaleTime.IsActive()) _tweenScaleTime.Kill();
            if (isUseTween)
            {
                _tweenScaleTime = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, _timeScale, durationTweenScaling).SetEase(easeTweenScaling).SetUpdate(true).Play();
            }
            else
            {
                Time.timeScale = _timeScale;
            }
        }

        public void RegisterTimeScale(int id, float timeScale)
        {
            if (_dictTimeScales.ContainsKey(id))
            {
                _dictTimeScales[id] = timeScale;
            }
            else
            {
                _dictTimeScales.Add(id, timeScale);
            }
            OnDictTimeScaleChanged();
        }

        public int RegisterTimeScale(float timeScale)
        {
            int id = 0;
            do
            {
                id = Random.Range(0, int.MaxValue);
            }
            while (_dictTimeScales.ContainsKey(id));
            _dictTimeScales.Add(id, timeScale);
            OnDictTimeScaleChanged();
            return id;
        }

        public void UnregisterTimeScale(int id)
        {
            _dictTimeScales.Remove(id);
            OnDictTimeScaleChanged();
        }

        public void RegisterTimeScale(MonoBehaviour monoBehaviour, float timeScale)
        {
            int id = monoBehaviour.GetHashCode();
            if (_dictTimeScales.ContainsKey(id))
            {
                _dictTimeScales[id] = timeScale;
            }
            else
            {
                _dictTimeScales.Add(id, timeScale);
            }
            OnDictTimeScaleChanged();
        }
        public void UnregisterTimeScale(MonoBehaviour monoBehaviour)
        {
            int id = monoBehaviour.GetHashCode();
            _dictTimeScales.Remove(id);
            OnDictTimeScaleChanged();
        }
        public void ClearAllTimeScaleModify()
        {
            _dictTimeScales.Clear();
            OnDictTimeScaleChanged();
        }

        private void OnEnable()
        {
            OnDictTimeScaleChanged();
        }

        private void OnDisable()
        {
            if (_tweenScaleTime.IsActive()) _tweenScaleTime.Kill();
        }
    }
}