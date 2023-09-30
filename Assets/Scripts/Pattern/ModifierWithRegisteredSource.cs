using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript.Utilities
{
    [System.Serializable]
    public class BoolModifierWithRegisteredSource
    {
        private List<int> _sources = new List<int>();
        private bool _value;
        public bool Value => _value;
        private System.Action _onChanged;

        public BoolModifierWithRegisteredSource(){}
        public BoolModifierWithRegisteredSource(System.Action onChanged)
        {
            _onChanged = onChanged;
        }

        public void AddModifier(Object @object) => AddModifier(@object.GetInstanceID());
        public void AddModifier(int id)
        {
            if (!_sources.Contains(id))
            {
                _sources.Add(id);
                _value = _sources.Count > 0;
                _onChanged?.Invoke();
            }
        }

        public void RemoveModifier(Object @object) => RemoveModifier(@object.GetInstanceID());
        public void RemoveModifier(int id)
        {
            _sources.Remove(id);
            _value = _sources.Count > 0;
            _onChanged?.Invoke();
        }
    }

    public class Vector2AddModifierWithRegisteredSource
    {
        protected Dictionary<int, Vector2> _source = new Dictionary<int, Vector2>();
        protected Vector2 _value = Vector2.zero;
        public Vector2 Value => _value;

        public void AddModifier(Object @object, Vector2 value) => AddModifier(@object.GetInstanceID(), value);
        public void AddModifier(int id, Vector2 value)
        {
            if (_source.ContainsKey(id))
            {
                _source[id] = value;
            }
            else
            {
                _source.Add(id, value);
            }
            UpdateModifier();
        }
        public void RemoveModifier(Object @object) => RemoveModifier(@object.GetInstanceID());
        public void RemoveModifier(int id)
        {
            if (_source.Remove(id))
            {
                UpdateModifier();
            }
        }

        private void UpdateModifier()
        {
            _value = Vector2.zero;
            foreach (var value in _source.Values)
            {
                _value += value;
            }
        }
    }

    public abstract class FloatModifierWithRegisteredSource
    {
        protected Dictionary<int, float> _source = new Dictionary<int, float>();
        protected float _value;
        public float Value => _value;

        protected FloatModifierWithRegisteredSource()
        {
            _value = InitValue;
        }

        public void AddModifier(Object @object, float value) => AddModifier(@object.GetInstanceID(), value);
        public void AddModifier(int id, float value)
        {
            if (_source.ContainsKey(id))
            {
                _source[id] = value;
            }
            else
            {
                _source.Add(id, value);
            }
            UpdateModifier();
        }
        public void RemoveModifier(Object @object) => RemoveModifier(@object.GetInstanceID());
        public void RemoveModifier(int id)
        {
            if (_source.Remove(id))
            {
                UpdateModifier();
            }
        }

        protected abstract float InitValue { get; }
        protected abstract void UpdateModifier();
    }

    [System.Serializable]
    public class FloatMulModifierWithRegisteredSource : FloatModifierWithRegisteredSource
    {
        protected override float InitValue => 1f;
        protected override void UpdateModifier()
        {
            _value = 1f;
            foreach (var value in _source.Values)
            {
                _value *= value;
            }
        }
    }

    [System.Serializable]
    public class FloatAddModifierWithRegisteredSource : FloatModifierWithRegisteredSource
    {
        protected override float InitValue => 0f;
        protected override void UpdateModifier()
        {
            _value = 0f;
            foreach (var value in _source.Values)
            {
                _value += value;
            }
        }
    }

    [System.Serializable]
    public class FloatMinModifierWithRegisteredSource : FloatModifierWithRegisteredSource
    {
        private float _init;
        protected override float InitValue => _init;
        protected override void UpdateModifier()
        {
            _value = _init;
            foreach (var value in _source.Values)
            {
                if (value < _value) _value = value;
            }
        }

        public FloatMinModifierWithRegisteredSource(float init = float.MaxValue)
        {
            _value = _init = init;
        }
    }

    [System.Serializable]
    public class FloatMaxModifierWithRegisteredSource : FloatModifierWithRegisteredSource
    {
        private float _init;
        protected override float InitValue => _init;
        protected override void UpdateModifier()
        {
            _value = _init;
            foreach (var value in _source.Values)
            {
                if (value > _value) _value = value;
            }
        }

        public FloatMaxModifierWithRegisteredSource(float init = float.MinValue)
        {
            _value = _init = init;
        }
    }

    [System.Serializable]
    public class ComponentOverrideWithRegisteredSource<T> where T : Component
    {
        [System.Serializable]
        public struct Data
        {
            public T component;
            public float priority;
        }

        protected Dictionary<int, Data> _source = new Dictionary<int, Data>();

        private T _componentHighestPriority = null;
        public T ValueHighestPriority => _componentHighestPriority;
        private System.Action _onChangedValueHighestPriority;

        public ComponentOverrideWithRegisteredSource() { }
        public ComponentOverrideWithRegisteredSource(System.Action onChanged)
        {
            _onChangedValueHighestPriority = onChanged;
        }

        public void AddModifier(Object @object, T component, float priority) => AddModifier(@object.GetInstanceID(), component, priority);
        public void AddModifier(int id, T component, float priority)
        {
            if (_source.ContainsKey(id))
            {
                _source[id] = new Data { component = component, priority = priority };
            }
            else
            {
                _source.Add(id, new Data { component = component, priority = priority });
            }
            UpdateModifier();
        }

        public void RemoveModifier(Object @object) => RemoveModifier(@object.GetInstanceID());
        public void RemoveModifier(int id)
        {
            if (_source.Remove(id))
            {
                UpdateModifier();
            }
        }

        private void UpdateModifier()
        {
            T componentMaxPriority = null;
            float maxPriority = float.MinValue;
            foreach (var pair in _source)
            {
                if (pair.Value.priority > maxPriority)
                {
                    maxPriority = pair.Value.priority;
                    componentMaxPriority = pair.Value.component;
                }
            }
            if (_componentHighestPriority != componentMaxPriority)
            {
                _componentHighestPriority = componentMaxPriority;
                _onChangedValueHighestPriority?.Invoke();
            }
        }
    }

    [System.Serializable]
    public class ModifierFloatCountdown
    {
        private MonoBehaviour _context;
        private FloatModifierWithRegisteredSource _modifier;
        private Dictionary<int, Coroutine> _corCountdowns = new Dictionary<int, Coroutine>();

        public ModifierFloatCountdown(MonoBehaviour context, FloatModifierWithRegisteredSource modifier)
        {
            _context = context;
            _modifier = modifier;
        }

        public void Add(int source, float value, float duration)
        {
            _modifier.AddModifier(source, value);
            if (_corCountdowns.TryGetValue(source, out Coroutine cor))
            {
                _context.StopCoroutine(cor);
                _corCountdowns[source] = _context.StartCoroutine(IECountdown(source, duration));
            }
            else
            {
                _corCountdowns.Add(source, _context.StartCoroutine(IECountdown(source, duration)));
            }
        }

        private IEnumerator IECountdown(int source, float duration)
        {
            float timeEnd = Time.time + duration;
            while (Time.time < timeEnd) yield return null;
            _corCountdowns.Remove(source);
            _modifier.RemoveModifier(source);
        }
    }

    [System.Serializable]
    public class ModifierBoolCountdown
    {
        private MonoBehaviour _context;
        private BoolModifierWithRegisteredSource _modifier;
        private Dictionary<int, Coroutine> _corCountdowns = new Dictionary<int, Coroutine>();

        public ModifierBoolCountdown(MonoBehaviour context, BoolModifierWithRegisteredSource modifier)
        {
            _context = context;
            _modifier = modifier;
        }

        public void Add(int source, float duration)
        {
            _modifier.AddModifier(source);
            if (_corCountdowns.TryGetValue(source, out Coroutine cor))
            {
                _context.StopCoroutine(cor);
                _corCountdowns[source] = _context.StartCoroutine(IECountdown(source, duration));
            }
            else
            {
                _corCountdowns.Add(source, _context.StartCoroutine(IECountdown(source, duration)));
            }
        }

        private IEnumerator IECountdown(int source, float duration)
        {
            float timeEnd = Time.time + duration;
            while (Time.time < timeEnd) yield return null;
            _corCountdowns.Remove(source);
            _modifier.RemoveModifier(source);
        }
    }
}