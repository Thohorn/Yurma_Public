using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildingsManager : MonoBehaviour
{

    [SerializeField] CanvasManager _canvasManager;

    private GameObject _buildTarget;

    private void OnEnable()
    {
        MouseController.setTargetBuild += SetBuildTarget;
    }

    private void OnDisable()
    {
        MouseController.setTargetBuild -= SetBuildTarget;
    }

    public void SetBuildTarget(GameObject newBuildTarget)
    {
        _buildTarget = newBuildTarget;
    }

    public GameObject GetBuildTarget()
    {
        return _buildTarget;
    }

    public void BuildCornField()
    {
        if (_buildTarget != null)
        {
            // To be able to build the building we need 2 wood and 1 iron.
            // If we do build the building.
            int woodCount = _canvasManager.GetWoodCounter();
            int ironCount = _canvasManager.GetIronCounter();
            if (woodCount > 1 && ironCount > 0)
            {
                // Add the building
                _buildTarget.GetComponent<GrassManager>().IncreaseCornFieldsBy(1);
                // Remove the resources
                _canvasManager.RemoveFromWoodCounter(2);
                _canvasManager.RemoveFromIronCounter(1);
            }
            else
            {
                Debug.Log("Not enough resources");
            }
            // Update the building values the player sees
            _canvasManager.UpdateCountersGrass(_buildTarget.GetComponent<GrassManager>().GetAllValues());

        }
    }

    public void BuildQuarry()
    {
        // To be able to build a quarry we need 2 wood and 1 iron
        int woodCount = _canvasManager.GetWoodCounter();
        int ironCount = _canvasManager.GetIronCounter();
        if(woodCount > 1 && ironCount >0)
        {
            // Add the building
            _buildTarget.GetComponent<MountainManager>().IncreaseQuarriesBy(1);
            // Remove the resources
            _canvasManager.RemoveFromWoodCounter(2);
            _canvasManager.RemoveFromIronCounter(1);
        }
        else
        {
            Debug.Log("Not enough resources");
        }
        // Update the building values the player sees
        _canvasManager.UpdateCountersMountain(_buildTarget.GetComponent<MountainManager>().GetAllValues());
    }

    public void BuildForester()
    {
        // To be able to build a quarry we need 2 food and 1 iron
        int foodCount = _canvasManager.GetFoodCounter();
        int ironCount = _canvasManager.GetIronCounter();
        if(foodCount > 1 && ironCount >0)
        {
            // Add the building
            _buildTarget.GetComponent<ForestManager>().IncreaseForestersBy(1);
            // Remove the resources
            _canvasManager.RemoveFromFoodCounter(2);
            _canvasManager.RemoveFromIronCounter(1);
        }
        else
        {
            Debug.Log("Not enough resources");
        }
        // Update the building values the player sees
        _canvasManager.UpdateCountersForrest(_buildTarget.GetComponent<ForestManager>().GetAllValues());
    }

    public void BuildEntertainmentCenter()
    {
        // To be able to build a quarry we need 5 food
        int foodCount = _canvasManager.GetFoodCounter();
        if(foodCount > 4)
        {
            // Add the building
            _buildTarget.GetComponent<CityManager>().IncreaseEntertainmentCentersBy(1);
            // Remove the resources
            _canvasManager.RemoveFromFoodCounter(5);
        }
        else
        {
            Debug.Log("Not enough resources");
        }
        // Update the building values the player sees
        _canvasManager.UpdateCountersCity(_buildTarget.GetComponent<CityManager>().GetAllValues());
    }

    public void BuildBlackSmith()
    {
        // To be able to build a quarry we need 2 food and 2 wood
        int foodCount = _canvasManager.GetFoodCounter();
        int woodCount = _canvasManager.GetWoodCounter();
        if(foodCount > 1 && woodCount > 1)
        {
            // Add the building
            _buildTarget.GetComponent<CityManager>().IncreaseBlackSmithsBy(1);
            // Remove the resources
            _canvasManager.RemoveFromFoodCounter(2);
            _canvasManager.RemoveFromWoodCounter(2);
        }
        else
        {
            Debug.Log("Not enough resources");
        }
        // Update the building values the player sees
        _canvasManager.UpdateCountersCity(_buildTarget.GetComponent<CityManager>().GetAllValues());
    }

}
