using System;
using System.Collections;
using UnityEngine;

namespace DaiVQScript.BulletHellSystem
{
    [System.Serializable]
    public class GeneralShootConfig : ICloneable
    {
        public Transform shootTarget = null;
        public bool isUseLocalShootDirection = false;
        public Vector2 shootDirection = Vector2.up;

        public BulletHellObjectBase prefabBullet = null;
        public int bulletLimitAmount = -1;

        [Header("Shoot Wave")]
        public int numWaves = 1;
        public float delayBetweenWaves = 0f;

        public Vector2 speedRange = new Vector2(5f, 5f);
        public float speedRandomness = 0f;

        public Vector2 accelerateRange = new Vector2(0f, 0f);
        public float accelerateRandomness = 0f;

        public Vector2 turnAngleRange = new Vector2(0f, 0f);
        public float turnAngleRandomness = 0f;

        [Header("Way")]
        public int numWays = 1;
        public float delayBetweenWays = 0f;
        public bool isShuffleWayIndex = false;

        public float angleOffset = 0f;
        public float angleRange = 360f;
        public float angleRandomness = 0f;

        public bool isFlipTurnSpeedByOddWay = false;
        public bool isFlipTurnSpeedByLeftRight = false;

        [Header("Bullet Pack")]
        public int numBulletsInPack = 1;
        public bool isShuffleBulletIndex = false;

        public float packSpacingVariance = 0f;
        public float packSpacingRandomness = 0f;

        public float packSpreadAngleVariance = 0f;
        public float packSpreadAngleRandomness = 0f;

        public float packSpeedVariance = 0f;
        public float packSpeedRandomness = 0f;

        public float packAccelerateVariance = 0f;
        public float packAccelerateRandomness = 0f;

        public float packTurnAngleSpeedVariance = 0f;
        public float packTurnAngleSpeedRandomness = 0f;

        [Header("Time")]
        public float autoReleaseTime = 5f;
        public int maxNumBulletShootPerFrame = 10;

        [Header("Scale")]
        public Vector3 initScale = Vector3.one;
        public Vector3 scaleSpeed = Vector3.zero;

        [Header("Rotate Around Anchor")]
        public BulletHellObjectBase.MoveAroundAnchorMode moveAroundAnchorMode = BulletHellObjectBase.MoveAroundAnchorMode.CONST;
        public Vector2 moveAroundRadius = Vector2.zero;
        public Vector2 moveAroundRadiusSpeed = Vector2.zero;
        public float moveAroundFreqency = 1f;
        public float moveAroundArcRadian = 0f;
        public float moveAroundArcRadianSpeed = 0f;

        [Header("Global Force")]
        public Vector2 localDirectionalForce = Vector2.zero;

        public object Clone() => this.MemberwiseClone();
    }
}