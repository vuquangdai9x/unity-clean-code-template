using System;
using System.Collections;
using UnityEngine;

namespace DaiVQScript.BulletHellSystem
{
    public class TestMultiShooter : MonoBehaviour
    {
        [SerializeField] private GeneralShooterPlayer _shooterPlayer = null;
        [SerializeField] private BouncingShooterPlayer _bouncingShooterPlayer = null;
        public float cooldown = 1f;
        private float _nextTimeShoot;

        public float totalDamage = 0f;
        public int totalHits = 0;
        public int totalKills = 0;

        public void StartAutoShoot()
        {
            _nextTimeShoot = Time.time + 0.5f;
        }

        private void Update()
        {
            if (Time.time > _nextTimeShoot)
            {
                _shooterPlayer?.StartShoot(null, OnAllBulletDisappeared);
                _bouncingShooterPlayer?.StartShoot(null, OnAllBulletDisappeared);
                _nextTimeShoot = Time.time + cooldown;
            }
        }

        private void OnAllBulletDisappeared(BouncingShooterPlayer.PlayerShootThread shootThread)
        {
            totalDamage += shootThread.CountTotalDamage;
            totalHits += shootThread.CountTotalHits;
            totalKills += shootThread.CountTotalKills;
        }

        private void OnAllBulletDisappeared(GeneralShooterPlayer.PlayerShootThread shootThread)
        {
            totalDamage += shootThread.CountTotalDamage;
            totalHits += shootThread.CountTotalHits;
            totalKills += shootThread.CountTotalKills;
        }
    }
}