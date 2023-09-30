using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript.UFOSurvivor.Map
{
    public class BackgroundParticle : MonoBehaviour
    {
        [SerializeField] private Transform _container = null;
        [SerializeField] private ParticleSystem[] _particles = null;
        [SerializeField] private float _flySpeedMul = 1f;
        [SerializeField] private Vector2 _cameraPosMul = Vector2.zero;
        private Camera _mainCamera;
        private Transform _mainCameraTransform;
        private ParticleSystem.VelocityOverLifetimeModule[] _particleVelocities;
        private float[] _originVelocitiesY = null;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _mainCameraTransform = _mainCamera.transform;
            _particleVelocities = new ParticleSystem.VelocityOverLifetimeModule[_particles.Length];
            _originVelocitiesY = new float[_particles.Length];
            for (int i = 0; i < _particles.Length; i++)
            {
                _particleVelocities[i] = _particles[i].velocityOverLifetime;
                _originVelocitiesY[i] = _particleVelocities[i].yMultiplier;
            }

            Vector3 position = _container.position;
            position.x = _mainCameraTransform.position.x * _cameraPosMul.x;
            position.y = _mainCameraTransform.position.y * _cameraPosMul.y;
            _container.position = position;
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                _particleVelocities[i].yMultiplier = _originVelocitiesY[i] - (GameplayGlobalData.Instance.PlayerFlySpeed.Value * _flySpeedMul - 1f);
            }
        }
    }
}