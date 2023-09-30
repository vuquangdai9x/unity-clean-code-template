using System.Collections;
using UnityEngine;

namespace DaiVQScript.UFOSurvivor.Map
{
    public class BackgroundPieceSimple : BackgroundPieceAbstract
    {
        private Transform _transform;
        public float length = 3f;

        protected override void Generate(int data, int seed)
        {
            _transform = transform;
            // do nothing
        }

        protected override void OnDisappeared()
        {
            base.OnDisappeared();
            gameObject.SetActive(false);
        }

        public override float GetLocalEdgeMin() => _transform.localPosition.y;
        public override float GetLocalEdgeMax() => _transform.localPosition.y + length;
        public override void SetLocalPositionByEdgeMin(float edgePosition) => _transform.localPosition = new Vector3(0, edgePosition, 0);
        public override void SetLocalPositionByEdgeMax(float edgePosition) => _transform.localPosition = new Vector3(0, edgePosition - length, 0);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + Vector3.up * length / 2f, new Vector3(20f, length, 0.1f));
        }
    }
}