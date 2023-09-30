using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript.Utilities
{
    /// <summary>
    /// Like trail renderer, but new vertex generate is always attached to some specific point of an transform, not based on the transform change velocity.
    /// I recommend place it on the tip of sword, since it use position distance to know when to create new vertices (daivq).
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SwordTrailRenderer : MonoBehaviour
    {
        private Transform _transform = null;

        public class SectionData
        {
            public Vector3[] point;
            public float timeGenerated;
        }

        public bool emitting = true;
        public float time = 2.0f;
        public float minDistance = 0.1f;
        public Vector3[] anchorOffsets = new Vector3[2] { Vector3.zero, Vector3.up };
        public float[] anchorTexMapVs = new float[2] { 0f, 1f };
        public Color[] verticalColors = new Color[2] { Color.white, Color.black };
        public Gradient horizontalGradient = default;

        private Queue<SectionData> _sections; // may change to cyclic array in future for more performance
        private Mesh _mesh = null;
        private bool _isClear = false;

        // cache
        private Vector3 _prevSectionCenterPosition, _currentSectionCenterPosition;
        private Quaternion _currentRotation;
        private Vector3 _currentLossyScale;
        private SectionData _lastSection;
        private Vector3[] _tempPoints;

        private Vector3[] _vertices = null; // use array or list? since array can be destroy and reallocated each update
        private Color[] _colors = null;
        private Vector2[] _uv = null;
        private int[] _triangles = null;

        private void OnEnable()
        {
            _transform = transform;
            _sections = new Queue<SectionData>();
            _isClear = true;
            _mesh = GetComponent<MeshFilter>().mesh;

            _vertices = new Vector3[0];
        }

        public void Clear()
        {
            _isClear = true;
        }

        void LateUpdate()
        {
            int i;
            int anchorOffsetsLength = anchorOffsets.Length;

            _currentSectionCenterPosition = _transform.position;
            _currentRotation = _transform.rotation;
            _currentLossyScale = _transform.lossyScale;

            if (_isClear) // put clear code here, so other component can call Clear() and move to somewhere without create unintented trails
            {
                _isClear = false;

                _tempPoints = new Vector3[anchorOffsetsLength];
                for (i = 0; i < anchorOffsetsLength; i++)
                {
                    _tempPoints[i] = _currentSectionCenterPosition + _currentRotation * Vector3.Scale(anchorOffsets[i], _currentLossyScale);
                }

                _sections.Clear();
                _lastSection = new SectionData() { point = _tempPoints, timeGenerated = Time.time };
                _sections.Enqueue(_lastSection);
            }

            // Remove old sections, but keep the last section alive
            while (_sections.Count > 1 && Time.time > _sections.Peek().timeGenerated + time)
            {
                _sections.Dequeue();
            }

            // Add a new trail section
            if (_sections.Count <= 1 || (_prevSectionCenterPosition - _currentSectionCenterPosition).sqrMagnitude > minDistance * minDistance)
            {
                _lastSection.point = _tempPoints;

                _tempPoints = new Vector3[anchorOffsetsLength];

                _prevSectionCenterPosition = _currentSectionCenterPosition;

                for (i = 0; i < anchorOffsetsLength; i++)
                {
                    _tempPoints[i] = _currentSectionCenterPosition + _currentRotation * Vector3.Scale(anchorOffsets[i], _currentLossyScale);
                }

                _lastSection = new SectionData() { point = _tempPoints, timeGenerated = Time.time };

                _sections.Enqueue(_lastSection);
            }
            else // or else, update last section to match anchors point
            {
                for (i = 0; i < anchorOffsetsLength; i++)
                {
                    _lastSection.point[i] = _currentSectionCenterPosition + _currentRotation * Vector3.Scale(anchorOffsets[i], _currentLossyScale);
                }
            }

            // Rebuild the mesh
            _mesh.Clear();

            if (_sections.Count < 2) return;

            int numVerties = _sections.Count * anchorOffsetsLength;
            int numTriangles = (_sections.Count - 1) * (anchorOffsetsLength - 1) * 2 * 3;

            if (numVerties != _vertices.Length)
            {
                _vertices = new Vector3[numVerties];
                _colors = new Color[numVerties];
                _uv = new Vector2[numVerties];
                _triangles = new int[numTriangles];
            }

            // Use matrix instead of transform.TransformPoint for performance reasons
            Matrix4x4 localSpaceTransformMatrix = _transform.worldToLocalMatrix;

            int sectionIndex = 0;
            float u;
            int verticeIndex;
            Color gradColor;

            foreach (SectionData section in _sections) // queue only allow for-each
            {
                //u = Mathf.Clamp01((Time.time - section.timeGenerated) / time); // don't clamp for performance, but may cause artifact
                u = (Time.time - section.timeGenerated) / time;
                gradColor = horizontalGradient.Evaluate(u);

                // Generate vertices
                for (i = 0; i < anchorOffsetsLength; i++)
                {
                    verticeIndex = sectionIndex * anchorOffsetsLength + i;
                    _vertices[verticeIndex] = localSpaceTransformMatrix.MultiplyPoint(section.point[i]);
                    _uv[verticeIndex] = new Vector2(u, anchorTexMapVs[i]);
                    _colors[verticeIndex] = verticalColors[i] * gradColor;
                }
                sectionIndex++;
            }

            // Generate triangles indices
            sectionIndex = 0;
            int triangleIndiceOffset = 0;
            foreach (SectionData section in _sections)
            {
                if (sectionIndex == 0)
                {
                    sectionIndex++;
                    continue;
                }

                for (i = 1; i < anchorOffsetsLength; i++)
                {
                    _triangles[triangleIndiceOffset + 0] = (sectionIndex - 1) * anchorOffsetsLength + i - 1;
                    _triangles[triangleIndiceOffset + 1] = (sectionIndex) * anchorOffsetsLength + i - 1;
                    _triangles[triangleIndiceOffset + 2] = (sectionIndex - 1) * anchorOffsetsLength + i;

                    _triangles[triangleIndiceOffset + 3] = (sectionIndex - 1) * anchorOffsetsLength + i;
                    _triangles[triangleIndiceOffset + 4] = (sectionIndex) * anchorOffsetsLength + i - 1;
                    _triangles[triangleIndiceOffset + 5] = (sectionIndex) * anchorOffsetsLength + i;

                    triangleIndiceOffset += 6;
                }

                sectionIndex++;
            }

            // Assign to mesh	
            _mesh.vertices = _vertices;
            _mesh.colors = _colors;
            _mesh.uv = _uv;
            _mesh.triangles = _triangles;
        }

        private void OnDrawGizmosSelected()
        {
            if (anchorOffsets != null && anchorOffsets.Length > 0)
            {
                Gizmos.color = new Color(1, 1, 0, 0.5f);
                Vector3 position = transform.position;
                Quaternion rotation = transform.rotation;

                Vector3 prevPos = position + rotation * Vector3.Scale(anchorOffsets[0], _currentLossyScale);
                Gizmos.DrawSphere(prevPos, 0.1f);
                Vector3 pos;

                Gizmos.DrawSphere(prevPos, 0.1f);

                for (int i = 1; i < anchorOffsets.Length; i++)
                {
                    pos = position + rotation * Vector3.Scale(anchorOffsets[i], _currentLossyScale);
                    Gizmos.DrawSphere(pos, 0.1f);
                    Gizmos.DrawLine(prevPos, pos);
                    prevPos = pos;
                }
            }
        }
    }
}

