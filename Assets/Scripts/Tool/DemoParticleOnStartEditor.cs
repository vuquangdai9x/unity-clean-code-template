using System.Collections;
using UnityEngine;

namespace DaiVQScript.Utilities
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ParticleSystem))]
    public class DemoParticleOnStartEditor : MonoBehaviour
    {
        [SerializeField] private Vector3 _nameOffset = Vector3.zero;
        [SerializeField] private float _simulateTime = 0.1f;

#if UNITY_EDITOR
        [ExecuteInEditMode]
        private void OnEnable()
        {
            Simulate();
        }

        [Sirenix.OdinInspector.Button]
        public void Simulate()
        {
            GetComponent<ParticleSystem>().Simulate(_simulateTime, true);
        }

        [Sirenix.OdinInspector.Button]
        private void SimulateSelected()
        {
            foreach (var selectedObject in UnityEditor.Selection.gameObjects)
            {
                if (selectedObject.scene == null) continue;
                GetComponent<DemoParticleOnStartEditor>()?.Simulate();
            }
        }

        [Sirenix.OdinInspector.Button]
        private void SimulateAll()
        {
            DemoParticleOnStartEditor[] allDemo = FindObjectsOfType<DemoParticleOnStartEditor>();
            foreach (var demo in allDemo) demo.Simulate();
        }
#endif
    }
}