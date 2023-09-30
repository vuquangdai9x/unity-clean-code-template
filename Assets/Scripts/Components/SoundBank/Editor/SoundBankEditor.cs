using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace DaiVQScript
{
    [CustomEditor(typeof(SoundBank))]
    public class SoundBankEditor : Editor
    {
        private string _searchedTags = string.Empty;
        private List<AudioClip> _searchedAudioClips = null;

        private bool _isShownTag = false;
        private string _tagsList = string.Empty;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SoundBank soundBank = (SoundBank)target;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Num of tags: " + soundBank.NumOfTags);
            if (_isShownTag)
            {
                if (GUILayout.Button("Hide Tag"))
                {
                    _isShownTag = false;
                }
            }
            else
            {
                if (GUILayout.Button("Show Tag"))
                {
                    _isShownTag = true;

                    _tagsList = "";

                    Dictionary<string, List<AudioClip>> dictTags = soundBank.DictionaryTagAudios;
                    if (dictTags != null)
                    {
                        foreach (KeyValuePair<string, List<AudioClip>> keyValuePair in dictTags)
                        {
                            _tagsList += keyValuePair.Key + ": " + keyValuePair.Value.Count + " | ";
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            if (_isShownTag)
            {
                GUILayout.Label(_tagsList);
            }

            // search
            GUILayout.Label("Search sound by tags, separated by a comma (,):");
            _searchedTags = EditorGUILayout.TextField("Tags: ", _searchedTags);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Search"))
            {
                string[] tags = _searchedTags.Split(',');
                for (int i = 0; i < tags.Length; i++) tags[i] = tags[i].Trim();

                string message = "Start find audio with tags: ";
                foreach (string tag in tags)
                {
                    message += tag + " | ";
                }

                Debug.Log(message);
                _searchedAudioClips = soundBank.GetAudioClipsWithTags(tags);
            }

            if (GUILayout.Button("Clear"))
            {
                _searchedAudioClips = null;
            }
            GUILayout.EndHorizontal();

            if (_searchedAudioClips != null)
            {
                foreach (AudioClip audioClip in _searchedAudioClips)
                {
                    EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), false);
                }
            }
        }
    }
}

#endif