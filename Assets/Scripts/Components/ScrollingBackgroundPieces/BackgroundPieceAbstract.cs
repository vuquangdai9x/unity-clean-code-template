using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace DaiVQScript.UFOSurvivor.Map
{
    public abstract class BackgroundPieceAbstract : MonoBehaviour
    {
        [SerializeField] private SortingGroup _sortingGroup = null;
        public void SetSortingOrder(int order) { if (_sortingGroup) _sortingGroup.sortingOrder = order; }

        private System.Action<BackgroundPieceAbstract> _onDisappeared = null;
        public void Generate(int data, int seed, System.Action<BackgroundPieceAbstract> onDisappeared = null)
        {
            _onDisappeared = onDisappeared;
            Generate(data, seed);
        }
        protected abstract void Generate(int data, int seed);

        public void Disappear()
        {
            OnDisappeared();
            _onDisappeared?.Invoke(this);
        }
        protected virtual void OnDisappeared() { }

        // edge local position
        public abstract float GetLocalEdgeMin();
        public abstract float GetLocalEdgeMax();
        public abstract void SetLocalPositionByEdgeMin(float edgePosition);
        public abstract void SetLocalPositionByEdgeMax(float edgePosition);
    }
}