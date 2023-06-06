using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ForestManager : NetworkBehaviour
{

    public delegate void OnIncreaseWood(int amount);
    public static event OnIncreaseWood onIncreaseWood;
    // all the counters
    public NetworkVariable<int> _foresters = new NetworkVariable<int>(0);



    private void OnEnable()
    {
        WorldManager.onNextTurn += GatherResources;
    }

    private void OnDisable()
    {
        WorldManager.onNextTurn -= GatherResources;
    }

    private void GatherResources()
    {
        if(this.GetComponent<ControlManager>().GetInPlayerControl())
        {
            onIncreaseWood?.Invoke(_foresters.Value);
        }
    }

    public int[] CollectResources()
    {
        int[] allResources = new int[1];
        allResources[0] = _foresters.Value;

        return allResources;
    }

    public void SetForeseters(int newValue)
    {
        _foresters = new NetworkVariable<int>(newValue);
    }

    public void IncreaseForestersBy(int amount)
    {
        int newAmountOfForesters = _foresters.Value + amount;
        _foresters = new NetworkVariable<int>(newAmountOfForesters);
    }

    public int GetForesters()
    {
        return _foresters.Value;
    }

    public string[] GetAllValues()
    {
        string[] allValues = new string[1];
        allValues[0] = _foresters.Value.ToString();
        
        return allValues;
    }
}
