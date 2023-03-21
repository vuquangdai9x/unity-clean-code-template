using System.Collections;
using UnityEngine;

namespace Game.DesignPattern
{
    /// <summary>
    /// A component for prepooling
    /// </summary>
    public class PrePoolHandler : MonoBehaviour
    {
        public enum PoolSystem
        {
            POOL_DICT_ARRAY
        }

        [System.Serializable]
        public class PrePoolJob
        {
            public GameObject prefab = null;
            public int amount = 1;
            public int amountSpawnEachFrame = -1;
            public PoolSystem poolSystem = PoolSystem.POOL_DICT_ARRAY;
        }
        public PrePoolJob[] prePoolJobGroupsSequence = null;
        private int _currentSequenceIndex = 0;
        private int _countEndedConcurrentJob = 0;

        public bool triggerPrepoolOnStart = true;
        public bool isPrePoolConcurrently = false;

        private void Start()
        {
            if (triggerPrepoolOnStart) StartPrePool(null);
        }

        public void StartPrePool()
        {
            StartPrePool(null);
        }
        private System.Action _onFinishedAll = null;
        public void StartPrePool(System.Action onFinishedAll)
        {
            _onFinishedAll = onFinishedAll;

            if (isPrePoolConcurrently)
            {
                _countEndedConcurrentJob = 0;
                for (int i = 0; i < prePoolJobGroupsSequence.Length; i++)
                {
                    DoPrePoolJob(prePoolJobGroupsSequence[i], OnConcurrentJobEnded);

                    void OnConcurrentJobEnded()
                    {
                        _countEndedConcurrentJob += 1;
                        if (_countEndedConcurrentJob >= prePoolJobGroupsSequence.Length)
                        {
                            _onFinishedAll?.Invoke();
                            _onFinishedAll = null;
                        }
                    }
                }
            }
            else
            {
                _currentSequenceIndex = 0;
                if (_currentSequenceIndex < prePoolJobGroupsSequence.Length)
                {
                    DoPrePoolJob(prePoolJobGroupsSequence[_currentSequenceIndex], OnEndedSingleJob);
                }
                else
                {
                    _onFinishedAll?.Invoke();
                    _onFinishedAll = null;
                }
            }
        }

        private void OnEndedSingleJob()
        {
            _currentSequenceIndex += 1;
            if (_currentSequenceIndex < prePoolJobGroupsSequence.Length)
            {
                DoPrePoolJob(prePoolJobGroupsSequence[_currentSequenceIndex], OnEndedSingleJob);
            }
            else
            {
                _onFinishedAll?.Invoke();
                _onFinishedAll = null;
            }
        }

        private void DoPrePoolJob(PrePoolJob prePoolJob, System.Action onEnd)
        {
            if (prePoolJob == null)
            {
                onEnd?.Invoke();
                return;
            }

            switch (prePoolJob.poolSystem)
            {
                case PoolSystem.POOL_DICT_ARRAY:
                    ObjectPoolDictArray.Instance.PrePoolingMax(prePoolJob.prefab, prePoolJob.amount, prePoolJob.amountSpawnEachFrame, onEnd);
                    break;
                default:
                    onEnd?.Invoke();
                    break;
            }
        }
    }
}