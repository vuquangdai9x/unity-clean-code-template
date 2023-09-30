using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript.UFOSurvivor.Map
{
    public class BackgroundControl : MonoBehaviour
    {
        private const int INIT_SORTING_ORDER = 9999;

        private int _seed;

        [SerializeField] private float _flySpeedMul = 1f;
        [SerializeField] private Vector2 _cameraPosMul = Vector2.zero;
        [SerializeField] private Transform _pieceContainer = null;
        private float _pieceContainerPositionZ;
        private Camera _mainCamera;
        private Transform _mainCameraTransform;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _mainCameraTransform = _mainCamera.transform;
            _pieceContainerPositionZ = _pieceContainer.position.z;
            _sortingOrder = INIT_SORTING_ORDER;
        }

        private void LateUpdate()
        {
            _pieceContainer.position = new Vector3(
                _mainCameraTransform.position.x * _cameraPosMul.x,
                _mainCameraTransform.position.y * _cameraPosMul.y,
                _pieceContainerPositionZ
                );
        }

        [Header("Spawn Piece")]
        [SerializeField] private float _rPositionYVisibleMin = -1.1f;
        [SerializeField] private float _rPositionYVisibleMax = 1.1f;
        private Queue<BackgroundPieceAbstract> _queueBackgroundPiece = new Queue<BackgroundPieceAbstract>(5);
        private BackgroundPieceAbstract _lastSpawnPiece;
        private BackgroundGeneratorAbstract _defaultGenerator;
        private BackgroundGeneratorAbstract _currentGenerator = null;
        private int _sortingOrder;

        public void SetUp(int seed, BackgroundGeneratorAbstract defaultGenerator = null)
        {
            _seed = seed;
            _defaultGenerator = defaultGenerator;
        }

        public void ChangeDefaultGenerator() => ChangeGenerator(_defaultGenerator);
        public void ChangeGenerator(BackgroundGeneratorAbstract currentGenerator)
        {
            _currentGenerator = currentGenerator;
            _currentGenerator.SetUp(_seed);
        }

        private void Update()
        {
            if (_queueBackgroundPiece.Count > 0)
            {
                foreach (var piece in _queueBackgroundPiece)
                {
                    piece.transform.localPosition += (_flySpeedMul / _pieceContainer.localScale.y) * GameplayGlobalData.Instance.PlayerFlySpeed.Value * Vector3.down * Time.deltaTime;
                }

                if (_queueBackgroundPiece.Peek().GetLocalEdgeMax() < _rPositionYVisibleMin * UIPosToWorld.worldRect.height / 2f)
                {
                    // disappear
                    BackgroundPieceAbstract piece = _queueBackgroundPiece.Dequeue();
                    if (piece == _lastSpawnPiece) _lastSpawnPiece = null;
                    piece.Disappear();
                }
            }

            if (_currentGenerator)
            {
                BackgroundPieceAbstract prevSpawnPiece = _lastSpawnPiece;
                if (!prevSpawnPiece || prevSpawnPiece.GetLocalEdgeMax() < _rPositionYVisibleMax * UIPosToWorld.worldRect.height / 2f)
                {
                    _lastSpawnPiece = _currentGenerator.GetNextPiece();
                    _queueBackgroundPiece.Enqueue(_lastSpawnPiece);
                    _lastSpawnPiece.transform.SetParent(_pieceContainer);
                    _lastSpawnPiece.transform.localScale = Vector3.one;
                    _lastSpawnPiece.SetLocalPositionByEdgeMin(prevSpawnPiece ? prevSpawnPiece.GetLocalEdgeMax() : (_rPositionYVisibleMin * UIPosToWorld.worldRect.height / 2f));
                    _sortingOrder -= 1;
                    _lastSpawnPiece.SetSortingOrder(_sortingOrder);
                }
            }
        }
    }
}