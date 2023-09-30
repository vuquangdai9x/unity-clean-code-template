using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace DaiVQScript
{
    [System.Serializable]
    public struct SoundInfo
    {
        [HideInInspector] public string name;
        public AudioClip audioClip;
        public string[] tags;
    }

    [CreateAssetMenu(fileName = "Sound Bank", menuName = "DaiVQ/SoundBank")]
    public class SoundBank : ScriptableObject
    {
        private const string NULL_TAG = "_";
        private static readonly int NULL_TAG_HASH = NULL_TAG.GetHashCode();

        [SerializeField] private SoundInfo[] _soundInfos = null;
        public IList<SoundInfo> SoundInfos => System.Array.AsReadOnly<SoundInfo>(_soundInfos);

        private Dictionary<int, List<AudioClip>> _dictionaryTagHashAudios = null;
        private Dictionary<string, List<AudioClip>> _dictionaryTagAudios = null;
        public int NumOfTags => (_dictionaryTagHashAudios != null) ? _dictionaryTagHashAudios.Count : 0;
        public Dictionary<string, List<AudioClip>> DictionaryTagAudios => _dictionaryTagAudios;

        [ContextMenu("Update audio items")]
        private void UpdateAudioItem()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");
            SoundInfo[] newSoundInfos = new SoundInfo[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                newSoundInfos[i].audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                newSoundInfos[i].name = newSoundInfos[i].audioClip.name;
            }

            System.Array.Sort(newSoundInfos, ComparerSoundNameAscend);

            int oldSoundIndex = 0;
            int newSoundIndex = 0;
            int compareNameResult = 0;
            while (oldSoundIndex < _soundInfos.Length && newSoundIndex < newSoundInfos.Length)
            {
                compareNameResult = ComparerSoundNameAscend(_soundInfos[oldSoundIndex], newSoundInfos[newSoundIndex]);
                if (compareNameResult == 0)
                {
                    newSoundInfos[newSoundIndex] = _soundInfos[oldSoundIndex];
                    newSoundIndex += 1;
                    oldSoundIndex += 1;
                }
                else if (compareNameResult > 0)
                {
                    newSoundIndex += 1;
                }
                else
                {
                    oldSoundIndex += 1;
                }
            }

            _soundInfos = newSoundInfos;

            UpdateDictionaryTagAudios();

            EditorUtility.SetDirty(this);
        }

        private int ComparerSoundNameAscend(SoundInfo s1, SoundInfo s2) => string.Compare(s1.name, s2.name);

        [ContextMenu("Update Dictionary Tag-Audios")]
        private void UpdateDictionaryTagAudios()
        {
            _dictionaryTagHashAudios = new Dictionary<int, List<AudioClip>>();
            _dictionaryTagAudios = new Dictionary<string, List<AudioClip>>();

            List<AudioClip> nullAudioClipList = new List<AudioClip>();
            _dictionaryTagHashAudios.Add(NULL_TAG_HASH, nullAudioClipList);
            _dictionaryTagAudios.Add(NULL_TAG, nullAudioClipList);

            int tagHash;
            foreach (SoundInfo soundInfo in _soundInfos)
            {
                if (soundInfo.tags != null && soundInfo.tags.Length > 0)
                {
                    foreach (string tag in soundInfo.tags)
                    {
                        tagHash = tag.GetHashCode();
                        List<AudioClip> listAudios = null;
                        if (_dictionaryTagHashAudios.ContainsKey(tagHash))
                        {
                            listAudios = _dictionaryTagHashAudios[tagHash];
                            //if (_dictionaryTagHashAudios[tagHash] == null) _dictionaryTagHashAudios[tagHash] = listAudios;
                            //if (_dictionaryTagAudios[tag] == null) _dictionaryTagAudios[tag] = listAudios;
                        }
                        else
                        {
                            listAudios = new List<AudioClip>();
                            _dictionaryTagHashAudios.Add(tagHash, listAudios);
                            _dictionaryTagAudios.Add(tag, listAudios);
                        }
                        listAudios.Add(soundInfo.audioClip);
                    }
                }
                else
                {
                    nullAudioClipList.Add(soundInfo.audioClip);
                }
            }
        }

        public List<AudioClip> GetAudioClipsWithTags(string[] tags)
        {
            if (_dictionaryTagHashAudios == null) UpdateDictionaryTagAudios();

            List<AudioClip> listAudios = new List<AudioClip>();
            foreach (string tag in tags)
            {
                int tagHash = tag.GetHashCode();
                List<AudioClip> foundAudios = null;
                if (_dictionaryTagHashAudios.ContainsKey(tagHash))
                {
                    foundAudios = _dictionaryTagHashAudios[tagHash];
                }
                if (foundAudios != null)
                {
                    foreach (AudioClip audioClip in foundAudios)
                    {
                        listAudios.AddDistinct(audioClip);
                    }
                }
            }
            return listAudios;
        }
    }
}

#endif