using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CityManager : NetworkBehaviour
{

    public delegate void OnIncreaseEntertainment(int amount);
    public static event OnIncreaseEntertainment onIncreaseEntertainment;
    public delegate void OnIncreaseIron(int amount);
    public static event OnIncreaseIron onIncreaseIron;
    // all the counters
    public NetworkVariable<int> _entertainmentCenter = new NetworkVariable<int>(0);
    public NetworkVariable<int> _blackSmiths = new NetworkVariable<int>(0);



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
        if (this.GetComponent<ControlManager>().GetInPlayerControl())
        {
            onIncreaseEntertainment?.Invoke(_entertainmentCenter.Value);
            onIncreaseIron?.Invoke(_blackSmiths.Value);
        }
    }

    public int[] CollectResources()
    {
        int[] allResources = new int[2];
        allResources[0] = _entertainmentCenter.Value;
        allResources[1] = _blackSmiths.Value;

        return allResources;
    }

    public void SetEntertainmentCenters(int newValue)
    {
        _entertainmentCenter = new NetworkVariable<int>(newValue);
    }

    public int GetEntertainmentCenters()
    {
        return _entertainmentCenter.Value;
    }

    public void IncreaseEntertainmentCentersBy(int amount)
    {
        int newAmountOfEntertainmentCenters = _entertainmentCenter.Value + amount;
        _entertainmentCenter = new NetworkVariable<int>(newAmountOfEntertainmentCenters);
    }

    public void SetBlackSmiths(int newValue)
    {
        _blackSmiths = new NetworkVariable<int>(newValue);
    }

    public int GetBlackSmiths()
    {
        return _blackSmiths.Value;
    }

    public void IncreaseBlackSmithsBy(int amount)
    {
        int newAmountOfBlackSmiths = _blackSmiths.Value + amount;
        _blackSmiths = new NetworkVariable<int>(newAmountOfBlackSmiths);
    }

    public string[] GetAllValues()
    {
        string[] allValues = new string[2];
        allValues[0] = _entertainmentCenter.Value.ToString();
        allValues[1] = _blackSmiths.Value.ToString();
        
        return allValues;
    }
}
