using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class GrassManager : NetworkBehaviour
{
    public delegate void OnIncreaseFood(int amount);
    public static event OnIncreaseFood onIncreaseFood;
    // all the counters
    public NetworkVariable<int> _cornFields = new NetworkVariable<int>(0);



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
            onIncreaseFood?.Invoke(_cornFields.Value);
        }
    }

    public int[] CollectResources()
    {
        int[] allResources = new int[1];
        allResources[0] = _cornFields.Value;

        return allResources;
    }

    public void SetCornFields(int newValue)
    {
        _cornFields = new NetworkVariable<int>(newValue);
    }

    public void IncreaseCornFieldsBy(int amount)
    {
        int newAmountOfCornFields = _cornFields.Value + amount;
        _cornFields = new NetworkVariable<int>(newAmountOfCornFields);
    }

    public int GetCornFields()
    {
        return _cornFields.Value;
    }

    public string[] GetAllValues()
    {
        string[] allValues = new string[1];
        allValues[0] = _cornFields.Value.ToString();
        
        return allValues;
    }
}
