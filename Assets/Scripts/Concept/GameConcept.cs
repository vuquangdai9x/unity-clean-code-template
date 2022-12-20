using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Concept
{
    /// <summary>
    /// Base class for all the things in the game
    /// </summary>
    //[CreateAssetMenu(fileName = "GameConcept", menuName = "Game/Concept/GameConcept")]
    public abstract class GameConcept : ScriptableObject
    {
        [SerializeField] private int _id = -1;
        public int Id => _id;

        [SerializeField] private string _conceptName = string.Empty;
        public string ConceptName => _conceptName;

        [SerializeField] private string _description = string.Empty;
        public string Description => _description;
    }
}