using System.Collections;
using UnityEngine;

namespace Game.Concept
{
    public abstract class EconomicElementServer : EconomicElement
    {
        [SerializeField] private string _serverKey = string.Empty;
    }
}