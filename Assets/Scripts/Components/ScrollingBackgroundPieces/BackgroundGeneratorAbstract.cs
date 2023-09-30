using LonglqScripts;
using LonglqScripts.TowerEnemy;
using System.Collections;
using UnityEngine;

namespace DaiVQScript.UFOSurvivor.Map
{
    public abstract class BackgroundGeneratorAbstract : MonoBehaviour
    {
        protected int _seed { get; private set; }

        #region Longlq  SetUpTowerSpawner Methods
        protected SpawnTowerEnemyControl _spawnTowerEnemyControl;
        public void Awake()
        {
            LonglqChapter.spawnTowerConfigCallback += SetUpSectionEvent;

        }
        private void OnDisable()
        {
            LonglqChapter.spawnTowerConfigCallback -= SetUpSectionEvent;
        }
        public void SetUpSectionEvent(SpawnTowerEnemyControl spawnTowerEnemyControl)
        {
            _spawnTowerEnemyControl = spawnTowerEnemyControl;
        }
        #endregion
        public void SetUp(int seed)
        {
            _seed = seed;
            OnSetUp();
        }

        protected virtual void OnSetUp() { }

        public abstract BackgroundPieceAbstract GetNextPiece();
    }
}