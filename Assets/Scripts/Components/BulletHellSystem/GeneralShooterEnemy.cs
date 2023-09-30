using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace DaiVQScript.BulletHellSystem
{
    public class GeneralShooterEnemy : MonoBehaviour
    {
        [Header("Bullet config")]
        public float damage = 5f;
        public float pushForce = 1f;
        public float pushDuration = 0.1f;

        [Header("Vfx")]
        public ParticleSystem vfxMuzzle = null;
        public Utilities.SFxPlayerRepeatSound sfxShoot = null;
        public ParticleSystem prefabVfxBulletHit = null;

        public class EnemyShootThread : GeneralShootThreadAbstract
        {
            [Header("Bullet config")]
            public float damage = 5f;
            public float pushForce = 1f;
            public float pushDuration = 0.1f;

            [Header("Vfx")]
            public ParticleSystem prefabVfxBulletHit = null;

            private System.Action<EnemyShootThread> _onFinishedShooting = null;
            private System.Action<EnemyShootThread> _onAllBulletDisappeared = null;

            public void StartShoot(System.Action<EnemyShootThread> onFinishedShooting = null, System.Action<EnemyShootThread> onAllBulletDisappeared = null)
            {
                _onFinishedShooting = onFinishedShooting;
                _onAllBulletDisappeared = onAllBulletDisappeared;
                TriggerShoot();
            }

            protected override bool HandleOnBulletCollided(BulletHellObjectBase bullet, Collider2D target)
            {
                if (target.gameObject.CompareTag(Tags.Player) && target.TryGetComponent<PlayerNew.Player>(out var player))
                {
                    Vector3 hitPosition = target.transform.position;
                    player.playerController.ApplyPushForce(pushForce * ((Vector2)(hitPosition - bullet.transform.position)).normalized, pushDuration);
                    player.TakeHit(Mathf.RoundToInt(damage));
                    DaiVQObjectPool.Instance.PlayParticleOnce(prefabVfxBulletHit, hitPosition, Quaternion.LookRotation(Vector3.forward, bullet.transform.up));
                    return true;
                }
                else
                {
                    return false;
                }
            }

            protected override void OnFinishedShooting()
            {
                base.OnFinishedShooting();
                _onFinishedShooting?.Invoke(this);
                _onFinishedShooting = null;
            }

            protected override void OnAllBulletDisappeared()
            {
                base.OnAllBulletDisappeared();
                _onAllBulletDisappeared?.Invoke(this);
                _onAllBulletDisappeared = null;
            }
        }

        public GeneralShootConfig shootConfig = default;
        [SerializeField] private bool _isPrePoolBulletOnStart = false;
        private List<ShootThreadWrapper> _listShootThreads = new List<ShootThreadWrapper>();

        public class ShootThreadWrapper
        {
            public EnemyShootThread shootThread;
            public System.Action<EnemyShootThread> onAllBulletDisappeared;
        }

        public void SetUp()
        {
            if (_isPrePoolBulletOnStart && shootConfig.prefabBullet)
            {
                int maxNumBullet = ((shootConfig.bulletLimitAmount >= 0) ? shootConfig.bulletLimitAmount : (shootConfig.numWaves * shootConfig.numWays * shootConfig.numBulletsInPack));
                BulletHellSharedPool.Instance.AddPool(shootConfig.prefabBullet, maxNumBullet);
                BulletHellSharedPool.Instance.PrePoolMaxIterate(shootConfig.prefabBullet, maxNumBullet);
            }
        }

        public void StartShoot(System.Action<EnemyShootThread> onFinishedShooting = null, System.Action<EnemyShootThread> onAllBulletDisappeared = null)
        {
            EnemyShootThread enemyShootThread = new EnemyShootThread();
            enemyShootThread.SetUp(this, (GeneralShootConfig)shootConfig.Clone());

            ShootThreadWrapper shootThreadWrapper = new ShootThreadWrapper()
            {
                shootThread = enemyShootThread,
                onAllBulletDisappeared = onAllBulletDisappeared,
            };
            _listShootThreads.Add(shootThreadWrapper);
            shootThreadWrapper.shootThread.damage = damage;
            shootThreadWrapper.shootThread.pushForce = pushForce;
            shootThreadWrapper.shootThread.pushDuration = pushDuration;
            shootThreadWrapper.shootThread.prefabVfxBulletHit = prefabVfxBulletHit;
            shootThreadWrapper.shootThread.StartShoot(onFinishedShooting, OnThreadHasAllBulletDisappread);

            if (vfxMuzzle != null) vfxMuzzle.Play();
            sfxShoot.Play();
        }

        private void OnThreadHasAllBulletDisappread(EnemyShootThread shootThread)
        {
            for (int i = _listShootThreads.Count - 1; i >= 0; i--)
            {
                if (_listShootThreads[i].shootThread == shootThread)
                {
                    _listShootThreads[i].onAllBulletDisappeared?.Invoke(shootThread);
                    _listShootThreads.RemoveAt(i);
                }
            }
        }

        private void Update()
        {
            for (int i = _listShootThreads.Count - 1; i >= 0; i--)
            {
                _listShootThreads[i].shootThread.Update();
            }
        }

        private void LateUpdate()
        {
            for (int i = _listShootThreads.Count - 1; i >= 0; i--)
            {
                _listShootThreads[i].shootThread.LateUpdate();
            }
        }

        public void CancelAllShoot()
        {
            for (int i = _listShootThreads.Count - 1; i >= 0; i--)
            {
                _listShootThreads[i].shootThread.CancelShoot();
            }
            _listShootThreads.Clear();
        }

        private void OnDestroy()
        {
            CancelAllShoot();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            float waveMainAngle;
            if (shootConfig.shootTarget)
            {
                waveMainAngle = Vector2.SignedAngle(Vector2.right, shootConfig.shootTarget.position - transform.position);
            }
            else if (shootConfig.isUseLocalShootDirection)
            {
                waveMainAngle = Vector2.SignedAngle(Vector2.right, transform.up);
            }
            else
            {
                waveMainAngle = Vector2.SignedAngle(Vector2.right, shootConfig.shootDirection);
            }
            float wayShootAngle;
            Vector3 wayShootDirectionNorm = Vector3.zero;
            for (int wayIndex = 0; wayIndex < shootConfig.numWays; wayIndex++)
            {
                wayShootAngle = waveMainAngle + shootConfig.angleOffset + shootConfig.angleRange * (-0.5f + (wayIndex + 0.5f) / shootConfig.numWays);
                math.sincos(wayShootAngle * Mathf.Deg2Rad, out wayShootDirectionNorm.y, out wayShootDirectionNorm.x);
                Gizmos.DrawLine(transform.position, transform.position + wayShootDirectionNorm * 5f);
            }
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button, Sirenix.OdinInspector.ButtonGroup]
        public void TestShoot()
        {
            StartShoot();
        }

        [Sirenix.OdinInspector.Button, Sirenix.OdinInspector.ButtonGroup]
        public void CancelShoot()
        {
            CancelAllShoot();
        }
#endif
    }
}