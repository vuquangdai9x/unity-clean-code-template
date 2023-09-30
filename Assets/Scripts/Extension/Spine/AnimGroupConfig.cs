using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaiVQScript.AIUnit.Animation
{
    /// <summary>
    /// Config specific animations each track
    /// </summary>
    [System.Serializable]
    public class AnimGroupConfig
    {
        //[SerializeField] private SkeletonAnimation skeletonAnimation = null;
        //[System.Serializable]
        //public class AnimConfig
        //{
        //    [SpineAnimation]
        //    public string animation = string.Empty;
        //    public bool isLoop = true;
        //}
        public float timeScale = 1f;
        [SpineAnimation]
        public string[] animations = null;
        //public List<AnimConfig> group = null;
    }

    [System.Serializable]
    public class AnimAssetGroupConfig
    {
        //[SerializeField] private SkeletonAnimation skeletonAnimation = null;
        //[System.Serializable]
        //public class AnimAssetConfig
        //{
        //    public bool isLoop = true;
        //}
        public float timeScale = 1f;
        public AnimationReferenceAsset[] animations = null;
        //public List<AnimAssetConfig> group = null;
    }
}