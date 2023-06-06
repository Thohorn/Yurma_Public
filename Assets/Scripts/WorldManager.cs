using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using UnityEngine;

public class WorldManager : NetworkBehaviour
{
    private static LobbyManager s_lobbyManager;
    private static GameObject s_gameManager;
    private int _mapWidth = 20;
    private int _mapHeight = 20;

    private NetworkVariable<int> _amountOfPlayers = new NetworkVariable<int>(0);

    private GameObject _tile;
    
    private Dictionary<string, GameObject> _tileDictionary = new Dictionary<string, GameObject>();
    private Dictionary<string, Color> _colourDictionary = new Dictionary<string, Color>{ {"0", new Color (0.28f, 0.92f, 0.93f, 0.4f)},
                                                                                        {"1", new Color (0.73f, 0.2f, 0.66f, 0.4f)},
                                                                                        {"2", new Color (0.81f, 0.2f, 0.2f, 0.4f)}};

    private GameObject _currentTarget;

    private List<GameObject> _cityList = new List<GameObject>();

    public delegate void OnNextTurn();
    public static event OnNextTurn onNextTurn;
    private bool _subscribedToHostStuff = false;


    private void OnEnable()
    {
        MouseController.getTarget += GetCurrentTarget;
        MouseController.setTarget += SetCurrentTarget;
        MouseController.getTile += GetFromTileDictionary;
        MouseController.showControl += ShowPlayerControlServerRpc;

        PlayerManager.onPlayerCreated += AddToAmountOfPlayersServerRpc;
        PlayerManager.onPlayerGetId += CreatePlayerID;

        LobbyManager.onLobbyChanged += ShowPlayerControlServerRpc;
    }

    private void OnDisable()
    {
        MouseController.getTarget -= GetCurrentTarget;
        MouseController.setTarget -= SetCurrentTarget;
        MouseController.getTile -= GetFromTileDictionary;
        MouseController.showControl -= ShowPlayerControlServerRpc;

        PlayerManager.onPlayerCreated -= AddToAmountOfPlayersServerRpc;
        PlayerManager.onPlayerGetId -= CreatePlayerID;

        LobbyManager.onLobbyChanged -= ShowPlayerControlServerRpc;

        if(IsServer)
        {
            Debug.Log("Disable the host stuff");
            LobbyManager.onLobbyChanged -= CheckTurnStatusServerRpc;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        s_lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
    }

    private void Update()
    {
        if(!_subscribedToHostStuff && IsServer)
        {
            Debug.Log("IsHost in Update");
            LobbyManager.onLobbyChanged += CheckTurnStatusServerRpc;
            _subscribedToHostStuff = true;
        }
    }

    private int CreatePlayerID()
    {
        return (_amountOfPlayers.Value + 1);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddToAmountOfPlayersServerRpc()
    {
        _amountOfPlayers.Value++;
        Debug.Log("AddToAmountOfPlayersServerRpc: " + _amountOfPlayers.Value);
    }

    public void SetMapWidth(int newValue)
    {
        _mapWidth = newValue;
    }

    public int GetMapWidth()
    {
        return _mapWidth;
    }

    public void SetMapHeight(int newValue)
    {
        _mapHeight = newValue;
    }

    public int GetMapHeight()
    {
        return _mapHeight;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAmountOfPlayersServerRpc(int newValue)
    {
        _amountOfPlayers.Value = newValue;
    }

    public int GetAmountOfPlayers()
    {
        return _amountOfPlayers.Value;
    }

    public void SetCurrentTarget(GameObject target)
    {
        _currentTarget = target;
    }

    public GameObject GetCurrentTarget()
    {
        return _currentTarget;
    }

    public void AddToTileDictionary(string id, GameObject tile)
    {
        if (_tileDictionary.ContainsKey(id))
        {
            Debug.Log("The dictionary already contains key: " + id);
        }
        else
        {
            _tileDictionary.Add(id, tile);
            Debug.Log("tile: " + id + " was added to the dict.");
        }
    }

    public GameObject GetFromTileDictionary(string id)
    {
        if (_tileDictionary.ContainsKey(id))
        {
            _tile = _tileDictionary[id];
        }
        else
        {
            Debug.Log("This id (" + id + ") is not in the dictionary");
        }

        return _tile;
    }

    public void AddToCityList(GameObject city)
    {
        _cityList.Add(city);
    }

    public void RemoveFromCityList(GameObject removeThis)
    {
        _cityList.Remove(removeThis);
    }

    public List<GameObject> GetCityList()
    {
        return _cityList;
    }

    [ServerRpc (RequireOwnership = false)]
    public void ShowPlayerControlServerRpc()
    {
        ShowPlayerControlClientRpc();
    }

    [ClientRpc]
    private void ShowPlayerControlClientRpc()
    {
        Lobby lobby = s_lobbyManager.GetJoinedLobby();
        foreach (Player player in lobby.Players)
        {
            string[] tileStrings = player.Data["PlayerControl"].Value.Split(",");
            foreach(string tileString in tileStrings)
            {
                if (tileString == "")
                {
                    // Debug.Log("tilestring is empty");
                }
                else
                {
                    Color controlColor = GetFromTileDictionary(tileString).GetComponent<SpriteRenderer>().color;
                    controlColor = _colourDictionary[player.Data["PlayerColour"].Value];
                    GetFromTileDictionary(tileString).GetComponent<SpriteRenderer>().color = controlColor;
                }
            }
        }
    }

    [ServerRpc]
    public void DistributeStartingAreaServerRpc()
    {
        Lobby lobby = s_lobbyManager.GetJoinedLobby();
        foreach (Player player in lobby.Players)
        {
            GameObject chosenCity = _cityList[Random.Range(0, _cityList.Count)];
            if(int.TryParse(chosenCity.name, out int tileNumber))
            {
                GameObject[] tiles = new GameObject[]  {chosenCity,
                                                        GetFromTileDictionary((tileNumber - 1).ToString()),
                                                        GetFromTileDictionary((tileNumber - 20).ToString()),
                                                        GetFromTileDictionary((tileNumber - 21).ToString()),
                                                        GetFromTileDictionary((tileNumber + 1).ToString()),
                                                        GetFromTileDictionary((tileNumber + 19).ToString()),
                                                        GetFromTileDictionary((tileNumber + 20).ToString())};
            
                string newValue = "";
                foreach (GameObject tile in tiles)
                {
                    if (newValue == "")
                    {
                        newValue = tile.name;
                    }
                    else
                    {
                        newValue = newValue + "," + tile.name;
                    }
                    DistributeStartingAreaClientRpc(player.Data["PlayerName"].Value, chosenCity.name.ToString(), tile.name, newValue, player.Id, false);
                }
                Debug.Log("Tiles in player control: " + newValue);
                Debug.Log("added for player: " + player.Data["PlayerName"].Value);
                DistributeStartingAreaClientRpc(player.Data["PlayerName"].Value, chosenCity.name.ToString(), null, newValue, player.Id, true);
            }
            RemoveFromCityList(chosenCity);
        }
        ShowPlayerControlServerRpc();
    }

    [ClientRpc]
    private void DistributeStartingAreaClientRpc(string playerName, string cityName, string tileName, string newValue, string playerId, bool updateLobby)
    {
        GameObject playerObject = GameObject.Find(playerName);
        if (playerObject != null)
        {
            PlayerManager playerManager = playerObject.GetComponent<PlayerManager>();
            if (updateLobby)
            {
                playerManager.AddToPlayerControl(null, null, newValue, playerId, true);
                Debug.Log("Updated lobby for: " + playerName);
            }
            else
            {
                GameObject tile = GameObject.Find(tileName);
                if (tile != null)
                {
                    playerManager.AddToPlayerControl(null, tile, newValue, playerId, false);
                    Debug.Log("Activated tile: " + tile.name + " for player: " + playerName);
                }
            }
        }
    }

    [ServerRpc]
    private void CheckTurnStatusServerRpc()
    {
        List<string> checkThis = new List<string>();
        Lobby lobby = s_lobbyManager.GetJoinedLobby();
        foreach (Player player in lobby.Players)
        {
            checkThis.Add(player.Data["ReadyForNextTurn"].Value);
        }
        if (!checkThis.Contains("false"))
        {
            CheckTurnStatusClientRpc();
        }
    }

    [ClientRpc]
    private void CheckTurnStatusClientRpc()
    {
        s_gameManager = GameObject.Find("GameManager");
        Debug.Log("CheckTurnStatusClientRpc");
        s_gameManager.GetComponent<EventManager>().GetReadyForNextTurn();
        onNextTurn?.Invoke();
    }
}
