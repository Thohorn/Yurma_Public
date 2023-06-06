using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private BuildingsManager _buildingsManager;

    [SerializeField] private GameObject _builderScreen;
    [SerializeField] private GameObject _desertBuildings;
    [SerializeField] private GameObject _forestBuildings;
    [SerializeField] private GameObject _grassBuildings;
    [SerializeField] private GameObject _waterBuildings;
    [SerializeField] private GameObject _rockBuildings;
    [SerializeField] private GameObject _cityBuildings;
    [SerializeField] private GameObject _notOwnedBuildings;

    // Get all the not owned buildings things
    [SerializeField] private GameObject _notOwnedTitle;
    [SerializeField] private GameObject _claimButton;
    [SerializeField] private TextMeshProUGUI _claimNeededText;

    // Get resource Counters
    [SerializeField] private TextMeshProUGUI _foodCounter;
    [SerializeField] private TextMeshProUGUI _woodCounter;
    [SerializeField] private TextMeshProUGUI _stoneCounter;
    [SerializeField] private TextMeshProUGUI _ironCounter;
    [SerializeField] private TextMeshProUGUI _controlCounter;

    // Get grass counters
    [SerializeField] private TextMeshProUGUI _cornFieldCounter;

    // Get mountain counters
    [SerializeField] private TextMeshProUGUI _quarryCounter;

    // Get the forrest counters
    [SerializeField] private TextMeshProUGUI _foresterCounter;

    // Get the city counters
    [SerializeField] private TextMeshProUGUI _blackSmithCounter;
    [SerializeField] private TextMeshProUGUI _entertainmentCenterCounter;

    [SerializeField] private GameObject _settingsScreen;
    
    // Get the text from next turn button
    [SerializeField] private TextMeshProUGUI _nextTurnText;

    public delegate void FinishedClaiming(GameObject selectedTarget);
    public static event FinishedClaiming finishedClaiming;

    private int _amountNeeded;

    private LobbyManager _lobbyManager;
    private PlayerManager _playerManager;
    private WorldManager _worldManager;

    private void OnEnable()
    {
        GrassManager.onIncreaseFood += AddToFoodCounter;
        MountainManager.onIncreaseStone += AddToStoneCounter;
        ForestManager.onIncreaseWood += AddToWoodCounter;
        CityManager.onIncreaseIron += AddToIronCounter;
        CityManager.onIncreaseEntertainment += AddToControlCounter;
        MouseController.onTileNotInControl += Show_NotOwnedBuildings;
        MouseController.showBuilding += ShowBuildings;
        KeyboardController.onEscapeIsPressed += EscapeIsPressed;
        EventManager.onTrigerReady += GetButtonReady;
    }

    private void OnDisable()
    {
        GrassManager.onIncreaseFood -= AddToFoodCounter;
        MountainManager.onIncreaseStone -= AddToStoneCounter;
        ForestManager.onIncreaseWood -= AddToWoodCounter;
        CityManager.onIncreaseIron -= AddToIronCounter;
        CityManager.onIncreaseEntertainment -= AddToControlCounter;
        MouseController.onTileNotInControl -= Show_NotOwnedBuildings;
        MouseController.showBuilding -= ShowBuildings;
        KeyboardController.onEscapeIsPressed -= EscapeIsPressed;
        EventManager.onTrigerReady += GetButtonReady;
    }

    private void Start()
    {
        // Get the LobbyManager
        _lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        if (_lobbyManager != null)
        {
            Lobby lobby = _lobbyManager.GetJoinedLobby();
            foreach (Player player in lobby.Players)
            {
                // Check if the selected player is this player
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    // Get PlayerManager and WorldManager
                    _playerManager = GameObject.Find(player.Data["PlayerName"].Value).GetComponent<PlayerManager>();
                    _worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
                }
                
            }
        }
    }

    public void ClaimLand()
    {
        // Get the lobby
        _lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        if (_lobbyManager != null)
        {
            Lobby lobby = _lobbyManager.GetJoinedLobby();
            foreach (Player player in lobby.Players)
            {
                // Check if selected player is this player
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    GameObject playerObject = GameObject.Find(player.Data["PlayerName"].Value);
                    if (playerObject != null)
                    {
                        // Get the PlayerManager and the MouseController of selected player
                        PlayerManager playerManager = playerObject.GetComponent<PlayerManager>();
                        MouseController mouseController = playerObject.GetComponent<MouseController>();

                        // Get the current target and remove it from all the players control
                        string playerId = AuthenticationService.Instance.PlayerId;
                        GameObject currentTarget = mouseController.GetCurrentSelectedTarget();
                        Debug.Log("This is currentTarget in ClaimLand: " + currentTarget);
                        Debug.Log("ClaimID is: " + currentTarget.GetComponent<ControlManager>().GetPlayerId());
                        playerManager.RemoveFromPlayerControlServerRpc(currentTarget.name, player.Id);

                        // Add the current target to the selected players control
                        Debug.Log("Current target is " + currentTarget);
                        string newValue = player.Data["PlayerControl"].Value + "," + currentTarget.name;
                        Debug.Log("All the new tiles in control" + newValue);
                        playerManager.AddToPlayerControl(null, currentTarget, null, playerId, false);
                        playerManager.AddToPlayerControl(null, null, newValue, playerId, true);
                        if ( int.TryParse(_controlCounter.text, out int controlValue))
                        {
                            int newControlValue = controlValue - _amountNeeded;
                            _controlCounter.text = newControlValue.ToString();
                        }
                        finishedClaiming?.Invoke(currentTarget);
                    }
                }
            }
        }
    }

    private void GetButtonReady()
    {
        // Get the lobby and check if the player is waiting for the next turn or not.
        // And change the button text depending on if it is true or false.
        _lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        if (_lobbyManager != null)
        {
            Lobby lobby = _lobbyManager.GetJoinedLobby();
            foreach (Player player in lobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    if (player.Data["ReadyForNextTurn"].Value == "false")
                    {
                        _nextTurnText.text = "Waiting";
                    }
                    else if (player.Data["ReadyForNextTurn"].Value == "true")
                    {
                        _nextTurnText.text = "Next Turn";
                    }
                }
            }
        }
    }

    private void EscapeIsPressed()
    {
        if (_builderScreen.activeInHierarchy)
        {
            HideBuildings();
        }
        else if (_settingsScreen.activeInHierarchy)
        {
            HideSettings();
        }
        else
        {
            ShowSettings();
        }
    }

    private void Show_NotOwnedBuildings(string type, GameObject tile)
    {
        // Show information to the player that he does not own this tile.
        HideBuildings();
        _builderScreen.SetActive(true);
        _notOwnedBuildings.SetActive(true);
        _notOwnedTitle.GetComponent<TextMeshProUGUI>().text = "You do not control this " + type + " tile.";

        // Show how much control power is needed to control the tile.
        if (tile.GetComponent<ControlManager>().GetInPlayerControl())
        {
            _amountNeeded = (_playerManager.GetPlayerControl().Count * 20) + 100;
        }
        else
        {
            _amountNeeded = (_playerManager.GetPlayerControl().Count * 20);
        }
        _claimNeededText.text = "You need: " + _amountNeeded + " control to claim this land.";

        if(int.TryParse(_controlCounter.text, out int count))
        {
            if (count >= _amountNeeded)
            {
                _claimButton.SetActive(true);
            }
        }
    }

    public void ShowBuildings(string type)
    {
        // Show the building availabilities depending on the type of tile that is selected.
        HideBuildings();
        _builderScreen.SetActive(true);
        GameObject buildTarget = _buildingsManager.GetBuildTarget();
        if(buildTarget != null)
        {
            switch(type)
            {
                case "Desert":
                    _desertBuildings.SetActive(true);
                    break;
                case "Forest":
                    _forestBuildings.SetActive(true);
                    UpdateCountersForrest(buildTarget.GetComponent<ForestManager>().GetAllValues());
                    break;
                case "Water":
                    _waterBuildings.SetActive(true);
                    break;
                case "Grass":
                    _grassBuildings.SetActive(true);
                    UpdateCountersGrass(buildTarget.GetComponent<GrassManager>().GetAllValues());
                    break;
                case "Rock":
                    _rockBuildings.SetActive(true);
                    UpdateCountersMountain(buildTarget.GetComponent<MountainManager>().GetAllValues());
                    break;
                case "City":
                    _cityBuildings.SetActive(true);
                    break;
                default:
                    Debug.Log("Couldn't find tag for ShowBuildings");
                    break;
            }
        }
    }

    public void HideBuildings()
    {
        _builderScreen.SetActive(false);
        _desertBuildings.SetActive(false);
        _forestBuildings.SetActive(false);
        _waterBuildings.SetActive(false);
        _grassBuildings.SetActive(false);
        _rockBuildings.SetActive(false);
        _cityBuildings.SetActive(false);
        _notOwnedBuildings.SetActive(false);
        _claimButton.SetActive(false);
    }

    public void ShowSettings()
    {
        _settingsScreen.SetActive(true);
    }

    public void HideSettings()
    {
        _settingsScreen.SetActive(false);
    }

    public void UpdateCountersGrass(string[] values)
    {
        _cornFieldCounter.text = values[0];
    }

    public void UpdateCountersMountain(string[] values)
    {
        _quarryCounter.text = values[0];
    }
    public void UpdateCountersForrest(string[] values)
    {
        _foresterCounter.text = values[0];
    }
    public void UpdateCountersCity(string[] values)
    {
        _entertainmentCenterCounter.text = values[0];
        _blackSmithCounter.text = values[1];

    }

    public void AddToFoodCounter(int increaseWith)
    {
        if(int.TryParse(_foodCounter.text, out int count))
        {
            count += increaseWith;
            _foodCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("Food counter is not a number in AddToFoodCounter()");
        }
        
    }

    public void RemoveFromFoodCounter(int decreaseWith)
    {
        if(int.TryParse(_foodCounter.text, out int count))
        {
            count -= decreaseWith;
            _foodCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("Food counter is not a number in RemoveFromFoodCounter()");
        }
    }

    public int GetFoodCounter()
    {
        if(int.TryParse(_foodCounter.text, out int count))
        {
            return count;
        }
        else
        {
            Debug.Log("Food counter is not a number in GetFoodCounter()");
            return -1;
        }
    }

     public void AddToWoodCounter(int increaseWith)
    {
        if(int.TryParse(_woodCounter.text, out int count))
        {
            count += increaseWith;
            _woodCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("Wood counter is not a number in AddToWoodCounter()");
        }
        
    }

    public void RemoveFromWoodCounter(int decreaseWith)
    {
        if(int.TryParse(_woodCounter.text, out int count))
        {
            count -= decreaseWith;
            _woodCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("wood counter is not a number in RemoveFromWoodCounter()");
        }
    }

    public int GetWoodCounter()
    {
        if(int.TryParse(_woodCounter.text, out int count))
        {
            return count;
        }
        else
        {
            Debug.Log("wood counter is not a number in GetWoodCounter()");
            return -1;
        }
    }

    public void AddToStoneCounter(int increaseWith)
    {
        if(int.TryParse(_stoneCounter.text, out int count))
        {
            count += increaseWith;
            _stoneCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("stone counter is not a number in AddToStoneCounter()");
        }
        
    }

    public void RemoveFromStoneCounter(int decreaseWith)
    {
        if(int.TryParse(_stoneCounter.text, out int count))
        {
            count -= decreaseWith;
            _stoneCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("stone counter is not a number in RemoveFrom_StoneCounter()");
        }
    }

    public int GetStoneCounter()
    {
        if(int.TryParse(_stoneCounter.text, out int count))
        {
            return count;
        }
        else
        {
            Debug.Log("Stone counter is not a number in GetStoneCounter()");
            return -1;
        }
    }

    public void AddToIronCounter(int increaseWith)
    {
        if(int.TryParse(_ironCounter.text, out int count))
        {
            count += increaseWith;
            _ironCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("Iron counter is not a number in AddToIronCounter()");
        }
        
    }

    public void RemoveFromIronCounter(int decreaseWith)
    {
        if(int.TryParse(_ironCounter.text, out int count))
        {
            count -= decreaseWith;
            _ironCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("Iron counter is not a number in RemoveFromIronCounter()");
        }
    }

    public int GetIronCounter()
    {
        if(int.TryParse(_ironCounter.text, out int count))
        {
            return count;
        }
        else
        {
            Debug.Log("Iron counter is not a number in GetIronCounter()");
            return -1;
        }
    }

    public void AddToControlCounter(int increaseWith)
    {
        if(int.TryParse(_controlCounter.text, out int count))
        {
            count += increaseWith;
            _controlCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("Control counter is not a number in AddToControlCounter()");
        }
        
    }

    public void RemoveFromControlCounter(int decreaseWith)
    {
        if(int.TryParse(_controlCounter.text, out int count))
        {
            count -= decreaseWith;
            _controlCounter.text = count.ToString();
        }
        else
        {
            Debug.Log("control counter is not a number in RemoveFromControlCounter()");
        }
    }

    public int GetControlCounter()
    {
        if(int.TryParse(_controlCounter.text, out int count))
        {
            return count;
        }
        else
        {
            Debug.Log("Control counter is not a number in GetcontrolCounter()");
            return -1;
        }
    }
}
