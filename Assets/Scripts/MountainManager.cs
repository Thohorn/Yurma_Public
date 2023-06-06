using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MountainManager : NetworkBehaviour
{

    public delegate void OnIncreaseStone(int amount);
    public static event OnIncreaseStone onIncreaseStone;
    // all the counters
    public NetworkVariable<int> _quarries = new NetworkVariable<int>(0);


    // All the resources that will be gathered
    //private int food = 0;

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
            onIncreaseStone?.Invoke(_quarries.Value);
        }
    }

    public int[] CollectResources()
    {
        int[] allResources = new int[1];
        allResources[0] = _quarries.Value;

        return allResources;
    }

    public void SetQuarries(int newValue)
    {
        _quarries = new NetworkVariable<int>(newValue);
    }

    public void IncreaseQuarriesBy(int amount)
    {
        int newAmountOf_Quarries = _quarries.Value + amount;
        _quarries = new NetworkVariable<int>(newAmountOf_Quarries);
    }

    public int GetQuarries()
    {
        return _quarries.Value;
    }

    public string[] GetAllValues()
    {
        string[] allValues = new string[1];
        allValues[0] = _quarries.Value.ToString();
        
        return allValues;
    }
}
