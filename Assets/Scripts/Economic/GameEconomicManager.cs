using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.DesignPattern;
using Game.Concept;

namespace Game.Economic
{
    /// <summary>
    /// Ecomomic element is all the value that changed on playing
    /// </summary>
    public class GameEconomicManager : MonoSingleton<GameEconomicManager>
    {
        [SerializeField] private Dictionary<EconomicElementLocal, int> _dictEconomicElementAmountsLocal = new Dictionary<EconomicElementLocal, int>();
        [SerializeField] private Dictionary<EconomicElementServer, int> _dictEconomicElementAmountsServer = new Dictionary<EconomicElementServer, int>();

        // idealy, local and server element should not interrelated

    }
}