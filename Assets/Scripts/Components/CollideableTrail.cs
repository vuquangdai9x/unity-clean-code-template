using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript.Utilities
{
    public class CollideableTrail : MonoBehaviour
    {
        [SerializeField] private EdgeCollider2D _collider = null;

        [SerializeField] private Transform _head = null;
        public Transform Head => _head;

        [SerializeField] private TrailRenderer _trail = null;
        [SerializeField] private ParticleSystem _vfxl = null;
        [SerializeField] private ParticleSystem[] _timeControlledVfxs = null;
        [SerializeField] private float _trailTimeAdditionalOffset = 0.15f;

        [SerializeField] private float _intervalUpdateCollider = 0.1f;

        private System.Action<CollideableTrail, Collider2D> _onTriggerEntered = null;
        private System.Action<CollideableTrail, Collider2D> _onTriggerExited = null;

        private float _nextTimeUpdateCollider;

        [Header("Size")]
        [SerializeField] private float _trailOriginWidth = 1f;
        [SerializeField] private float _vfxlOriginSize = 1f;
        [SerializeField] private float _originEmissionRateOverDistance = 2f;

        public float Radius
        {
            get => _collider.edgeRadius;
            set
            {
                _collider.edgeRadius = value;
                _trail.widthMultiplier = value * _trailOriginWidth;
                _vfxl.transform.localScale = Vector3.one * value * _vfxlOriginSize;

                ParticleSystem.EmissionModule emmission = _vfxl.emission;
                emmission.rateOverDistanceMultiplier = _originEmissionRateOverDistance / (value * _vfxlOriginSize);
            }
        }
        public bool IsAlive => _collider.enabled;

        public void StartEffect(float decayTime, float radius, System.Action<CollideableTrail, Collider2D> onTriggerEntered = null, System.Action<CollideableTrail, Collider2D> onTriggerExited = null)
        {
            transform.position = Vector3.zero;

            _onTriggerEntered = onTriggerEntered;
            _onTriggerExited = onTriggerExited;

            Radius = radius;

            _trail.time = decayTime + _trailTimeAdditionalOffset;

            _vfxl.transform.SetParent(null);
            _vfxl.gameObject.SetActive(false);

            ParticleSystem.MainModule main;
            for (int i = _timeControlledVfxs.Length - 1; i >= 0; i--)
            {
                main = _timeControlledVfxs[i].main;
                main.duration = decayTime;
            }

            _trail.Clear();
            _trail.emitting = true;

            _vfxl.gameObject.SetActive(true);

            _collider.enabled = false;

            this.enabled = true;
            _nextTimeUpdateCollider = Time.time + _intervalUpdateCollider;
        }

        private void OnDestroy()
        {
            if (_vfxl != null) Destroy(_vfxl.gameObject);
        }

        private void Update()
        {
            if (Time.time > _nextTimeUpdateCollider)
            {
                UpdateCollider();
                _nextTimeUpdateCollider = Time.time + _intervalUpdateCollider;
            }
        }

        private void UpdateCollider()
        {
            if (_trail.positionCount >= 1)
            {
                List<Vector2> points = new List<Vector2>(_trail.positionCount + 1);
                for (int i = 0; i < _trail.positionCount; i++)
                {
                    points.Add(_trail.GetPosition(i));
                }
                points.Add(_head.position);
                _collider.SetPoints(points);
                _collider.enabled = true;
            }
            else
            {
                _collider.enabled = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            _onTriggerEntered?.Invoke(this, collision);
        }

        public Vector2 GetClosetPoint(Vector2 point) => _collider.ClosestPoint(point);

        private void OnTriggerExit2D(Collider2D collision)
        {
            _onTriggerExited?.Invoke(this, collision);
        }

        public void StopEffect()
        {
            _onTriggerEntered = null;
            _trail.Clear();
            _trail.emitting = false;
            _vfxl.Stop();
            _collider.enabled = false;
            this.enabled = false;
        }

#if UNITY_EDITOR
        [Header("Editor Tool")]
        [SerializeField] private float _decayTimeEditor = 5f;
        [SerializeField] private float _radiusEditor = 5f;

        [Sirenix.OdinInspector.Button("Start")]
        private void StartEffectEditor()
        {
            StartEffect(_decayTimeEditor, _radiusEditor, OnCollidedEditor);

            void OnCollidedEditor(CollideableTrail trail, Collider2D collider)
            {
                Debug.Log("[CollideableTrail]" + gameObject.name + " collide with " + collider.gameObject.name);
            }
        }
        [Sirenix.OdinInspector.Button("Stop")]
        private void StopEffectEditor()
        {
            StopEffect();
        }
#endif
    }
}