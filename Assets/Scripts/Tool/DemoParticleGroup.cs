using System.Collections;
using UnityEngine;

namespace DaiVQScript.Utilities
{
    [ExecuteInEditMode]
    public class DemoParticleGroup : MonoBehaviour
    {
        private static bool _isShowName = true;

        [SerializeField] private bool _isUseCustomLayout = false;
        [SerializeField] private int _numColsCustom = -1;
        [SerializeField] private Vector2 _spacing = Vector2.one;

        [SerializeField] private bool _isAutoUpdateLayoutOnValidate = false;

        [SerializeField] private Vector3 _nameOffset = Vector3.zero;

        private void OnEnable()
        {
            demoParticles = transform.GetComponentsInChildren<DemoParticleOnStartEditor>();
        }

        [Sirenix.OdinInspector.Button]
        private void UpdateLayout()
        {
            demoParticles = transform.GetComponentsInChildren<DemoParticleOnStartEditor>();
            int numCols = _isUseCustomLayout ? (_numColsCustom > 0 ? _numColsCustom : demoParticles.Length) : Mathf.RoundToInt(Mathf.Sqrt(demoParticles.Length));

            int index = 0;
            int rowIndex = 0;
            int colIndex;
            while (index < demoParticles.Length)
            {
                colIndex = index % numCols;

                demoParticles[index].transform.position = transform.position + (Vector3)(new Vector2(colIndex, rowIndex) * _spacing);

                index += 1;
                if (index % numCols == 0) rowIndex += 1;
            }
        }

        private DemoParticleOnStartEditor[] demoParticles;
        [Sirenix.OdinInspector.Button]
        private void Simulate()
        {
            demoParticles = transform.GetComponentsInChildren<DemoParticleOnStartEditor>();
            foreach (var demo in demoParticles) demo.Simulate();
        }

        private void OnDrawGizmos()
        {
            if (_isShowName) foreach (var demo in demoParticles) UnityEditor.Handles.Label(demo.transform.position + _nameOffset, demo.gameObject.name);
        }

        private void OnValidate()
        {
            if (_isAutoUpdateLayoutOnValidate)
            {
                UpdateLayout();
            }
        }

        [Sirenix.OdinInspector.ButtonGroup, Sirenix.OdinInspector.Button]
        private void ShowName() => _isShowName = true;
        [Sirenix.OdinInspector.ButtonGroup, Sirenix.OdinInspector.Button]
        private void HideName() => _isShowName = false;
    }
}