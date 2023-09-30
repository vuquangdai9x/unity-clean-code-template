using System;
using System.Collections;
using UnityEngine;

namespace DaiVQScript.BulletHellSystem
{
    public class SingleGeneralShooterPlayer : SingleGeneralShooterAbstract
    {
        [Header("Bullet config")]
        public float damage = 5f;
        public float critRatio = 0f;
        public float pushForce = 1f;
        public float pushDuration = 0.25f;

        [Header("Vfx")]
        public ParticleSystem _prefabVfxBulletHit = null;

        //[Header("Statistic")]
        public float CountTotalDamage { get; private set; } = 0f;
        public int CountTotalHits { get; private set; } = 0;
        public int CountTotalKills { get; private set; } = 0;

        //protected override void OnStartShoot()
        //{
        //    base.OnStartShoot();
        //    CountTotalDamage = 0;
        //    CountTotalHits = 0;
        //    CountTotalKills = 0;
        //}

        //protected override bool HandleOnBulletCollided(BulletHellObjectBase bullet, Collider2D target)
        //{
        //    if (target.TryGetComponent<ITakeDamage>(out ITakeDamage targetTakeDamage))
        //    {
        //        Vector3 hitPosition = target.transform.position;
        //        Vector2 direction = bullet.transform.up;
        //        DaiVQObjectPool.Instance.PlayParticleOnce(_prefabVfxBulletHit, hitPosition, Quaternion.LookRotation(Vector3.forward, direction));
        //        float realDamage = targetTakeDamage.TakedDamage(damage, hitPosition, critRatio, OnTargetKilled);
        //        if (pushDuration > 0.01f) targetTakeDamage.ApplyForce(direction * pushForce, pushDuration);
        //        CountTotalDamage += realDamage;
        //        CountTotalHits++;
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //private void OnTargetKilled(Transform target)
        //{
        //    CountTotalKills++;
        //}
    }
}