//using Spine.Unity;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace DaiVQScript.AIUnit.Animation
//{
//    [RequireComponent(typeof(SkeletonAnimation))]
//    public class SpineAnimManager : MonoBehaviour
//    {
//        public SkeletonAnimation skeletonAnimation = null;
//        public float defaultMixDuration = 0.2f;

//        //[System.Serializable]
//        //public struct TrackGroup
//        //{
//        //    public string name;
//        //    [SpineAnimation]
//        //    public string[] animations;
//        //}

//        [System.Serializable]
//        public struct AnimAssetConfig
//        {
//            public AnimationReferenceAsset animAsset;
//            public bool isLoop;
//        }

//        [System.Serializable]
//        public struct TrackAssetGroup
//        {
//            public string name;
//            public AnimAssetConfig[] animConfigs;
//        }

//        //private TrackGroup[] trackGroups = null;
//        public TrackAssetGroup[] trackAssetGroups = null;

//        private Dictionary<string, int> _dictAnimTrackIndex = null;
//        private Dictionary<string, AnimAssetConfig> _dictAnimConfig = null;
//        private Dictionary<AnimationReferenceAsset, int> _dictAnimAssetTrackIndex = null;
//        private Dictionary<AnimationReferenceAsset, AnimAssetConfig> _dictAnimAssetConfig = null;

//        private bool[] _haveAnimInTracks = null;

//        [Header("Common Anim")]
//        public AnimAssetGroupConfig animIdle = null;
//        public void DoIdle()
//        {
//            PlayAnimGroup(animIdle);
//        }

//        private void Awake()
//        {
//            skeletonAnimation = GetComponent<SkeletonAnimation>();
//            SetupAnimDict();
//            _haveAnimInTracks = new bool[trackAssetGroups.Length]; // default false
//        }

//        private void SetupAnimDict()
//        {
//            _dictAnimTrackIndex = new Dictionary<string, int>();
//            _dictAnimAssetTrackIndex = new Dictionary<AnimationReferenceAsset, int>();
//            _dictAnimConfig = new Dictionary<string, AnimAssetConfig>();
//            _dictAnimAssetConfig = new Dictionary<AnimationReferenceAsset, AnimAssetConfig>();
//            for (int i = 0; i < trackAssetGroups.Length; i++)
//            {
//                foreach (AnimAssetConfig animConfig in trackAssetGroups[i].animConfigs)
//                {
//                    _dictAnimTrackIndex.Add(animConfig.animAsset.Animation.Name, i);
//                    _dictAnimConfig.Add(animConfig.animAsset.Animation.Name, animConfig);
//                    _dictAnimAssetTrackIndex.Add(animConfig.animAsset, i);
//                    _dictAnimAssetConfig.Add(animConfig.animAsset, animConfig);
//                }
//            }
//        }

//        public enum PlayAnimGroupMode
//        {
//            REPLACE, // stop all track that not in group
//            ADDITIVE
//        }
//        public void PlayAnimGroup(AnimGroupConfig animGroupConfig, PlayAnimGroupMode playMode = PlayAnimGroupMode.REPLACE)
//        {
//            if (animGroupConfig.animations == null || animGroupConfig.animations.Length == 0) return;

//            bool[] isTrackUsed = new bool[trackAssetGroups.Length]; // default false

//            foreach (string animName in animGroupConfig.animations)
//            {
//                int trackIndex = 0;
//                if (_dictAnimTrackIndex.TryGetValue(animName, out trackIndex))
//                {
//                    if (!isTrackUsed[trackIndex])
//                    {
//                        isTrackUsed[trackIndex] = true;

//                        _haveAnimInTracks[trackIndex] = true;

//                        if (trackIndex >= skeletonAnimation.state.Tracks.Count || skeletonAnimation.state.GetCurrent(trackIndex) == null || skeletonAnimation.state.GetCurrent(trackIndex).Animation.Name.Equals(animName))
//                        {
//                            bool isLoop = false;
//                            if (_dictAnimConfig.TryGetValue(animName, out var animAssetConfig))
//                            {
//                                isLoop = animAssetConfig.isLoop;
//                            }

//                            if (isLoop)
//                            {
//                                skeletonAnimation.AnimationState.SetAnimation(trackIndex, animName, true);
//                            }
//                            else
//                            {
//                                skeletonAnimation.AnimationState.SetAnimation(trackIndex, animName, false);
//                                skeletonAnimation.AnimationState.AddEmptyAnimation(trackIndex, defaultMixDuration, 0f);
//                            }
//                        }
//                    }
//                }
//            }

//            skeletonAnimation.AnimationState.TimeScale = animGroupConfig.timeScale;

//            if (playMode == PlayAnimGroupMode.REPLACE)
//            {
//                for (int i = 0; i < trackAssetGroups.Length; i++)
//                {
//                    if (!isTrackUsed[i] && _haveAnimInTracks[i])
//                    {
//                        _haveAnimInTracks[i] = false;
//                        skeletonAnimation.AnimationState.SetEmptyAnimation(i, defaultMixDuration);
//                    }
//                }
//            }
//        }

//        public void PlayAnimGroup(AnimAssetGroupConfig animGroupConfig, PlayAnimGroupMode playMode = PlayAnimGroupMode.REPLACE)
//        {
//            if (animGroupConfig.animations == null || animGroupConfig.animations.Length == 0) return;

//            bool[] isTrackUsed = new bool[trackAssetGroups.Length]; // default false

//            foreach (AnimationReferenceAsset animAsset in animGroupConfig.animations)
//            {
//                if (_dictAnimAssetTrackIndex.TryGetValue(animAsset, out int trackIndex))
//                {
//                    if (!isTrackUsed[trackIndex])
//                    {
//                        isTrackUsed[trackIndex] = true;

//                        _haveAnimInTracks[trackIndex] = true;

//                        if (trackIndex >= skeletonAnimation.state.Tracks.Count || skeletonAnimation.state.GetCurrent(trackIndex) == null || skeletonAnimation.state.GetCurrent(trackIndex).Animation != animAsset.Animation)
//                        {
//                            bool isLoop = false;
//                            if (_dictAnimAssetConfig.TryGetValue(animAsset, out var animAssetConfig))
//                            {
//                                isLoop = animAssetConfig.isLoop;
//                            }

//                            if (isLoop)
//                            {
//                                skeletonAnimation.AnimationState.SetAnimation(trackIndex, animAsset, true);
//                            }
//                            else
//                            {
//                                skeletonAnimation.AnimationState.SetAnimation(trackIndex, animAsset, false);
//                                skeletonAnimation.AnimationState.AddEmptyAnimation(trackIndex, defaultMixDuration, 0f);
//                            }
//                        }
//                    }
//                }
//            }

//            skeletonAnimation.AnimationState.TimeScale = animGroupConfig.timeScale;

//            if (playMode == PlayAnimGroupMode.REPLACE)
//            {
//                for (int i = 0; i < trackAssetGroups.Length; i++)
//                {
//                    if (!isTrackUsed[i] && _haveAnimInTracks[i])
//                    {
//                        _haveAnimInTracks[i] = false;
//                        skeletonAnimation.AnimationState.SetEmptyAnimation(i, defaultMixDuration);
//                    }
//                }
//            }
//        }

//        public void AppendAnim(AnimAssetGroupConfig animGroupConfig)
//        {
//            if (animGroupConfig.animations == null || animGroupConfig.animations.Length == 0) return;

//            bool[] isTrackUsed = new bool[trackAssetGroups.Length]; // default false

//            foreach (AnimationReferenceAsset animAsset in animGroupConfig.animations)
//            {
//                if (_dictAnimAssetTrackIndex.TryGetValue(animAsset, out int trackIndex))
//                {
//                    if (!isTrackUsed[trackIndex])
//                    {
//                        isTrackUsed[trackIndex] = true;
//                        _haveAnimInTracks[trackIndex] = true;

//                        bool isLoop = false;
//                        if (_dictAnimAssetConfig.TryGetValue(animAsset, out var animAssetConfig))
//                        {
//                            isLoop = animAssetConfig.isLoop;
//                        }

//                        if (isLoop)
//                        {
//                            skeletonAnimation.AnimationState.AddAnimation(trackIndex, animAsset, true, 0f);
//                        }
//                        else
//                        {
//                            skeletonAnimation.AnimationState.AddAnimation(trackIndex, animAsset, false, 0f);
//                            skeletonAnimation.AnimationState.AddEmptyAnimation(trackIndex, defaultMixDuration, 0f);
//                        }
//                    }
//                }
//            }
//        }

//        public void ClearAll()
//        {
//            skeletonAnimation.AnimationState.TimeScale = 1f;
//            skeletonAnimation.ClearState();
//        }

//#if UNITY_EDITOR
//        [Header("Editor Tool")]
//        [SerializeField] private AnimGroupConfig groupConfig = default(AnimGroupConfig);
//        [SerializeField] private PlayAnimGroupMode playMode = PlayAnimGroupMode.REPLACE;
//        [ContextMenu("UpdateCurrentTrack")]
//        private void UpdateCurrentTrack()
//        {
//            PlayAnimGroup(groupConfig, playMode);
//        }
//#endif
//    }
//}