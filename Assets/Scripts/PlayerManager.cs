using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    private static LobbyManager _lobbyManager;
    private static WorldManager _worldManager;
    private List<GameObject> playerControlList = new List<GameObject>();

    public delegate void OnSetCanvas(Camera playerCam);
    public static event OnSetCanvas onSetCanvas;

    public delegate void OnPlayerCreated();
    public static event OnPlayerCreated onPlayerCreated;

    public delegate int OnPlayerGetId();
    public static event OnPlayerGetId onPlayerGetId;

    public delegate string OnPlayerGetName(GameObject player);
    public static event OnPlayerGetName onPlayerGetName;

    public delegate void OnCreatePlayerCard();
    public static event OnCreatePlayerCard onCreatePlayerCard;

    private static string _playerName;
    private static int? _playerID;

    public void OnEnable()
    {
        LobbyManager.onPlayerCreate += CreatePlayer;
        LobbyManager.onLobbyChanged += UpdateLocalPlayerData;
        EventManager.onTrigerReady += GetReady;
    }

    private void OnDisable()
    {
        LobbyManager.onPlayerCreate -= CreatePlayer;
        LobbyManager.onLobbyChanged -= UpdateLocalPlayerData;
        EventManager.onTrigerReady -= GetReady;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if(!IsOwner)
        {
            gameObject.SetActive(false);
        }
        else
        {
            _lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
            _worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
            CreatePlayer();
        }
    }

    public void CreatePlayer()
    {
        Debug.Log("Creating player initiated");
        if(_playerID == null)
            {
                Debug.Log("Firtst step for creating player done");
                _playerID = onPlayerGetId?.Invoke();
                Debug.Log("Second step for creating player done");
                _playerName = onPlayerGetName?.Invoke(gameObject);
                Debug.Log("Third step for creating player done");
                onPlayerCreated?.Invoke();
                Debug.Log("Fourth step for creating player done");
                this.gameObject.name = _playerName;
                Debug.Log("CreatePlayerFunction finished, got ID: " + _playerID + " name: " + _playerName);
            }
        else
        {
            Debug.Log("Player is not created, _playerID is not null");
        }
    }

    private void GetReady()
    {
        Lobby lobby = _lobbyManager.GetJoinedLobby();
        foreach (Player player in lobby.Players)
        {
            if (player.Id == AuthenticationService.Instance.PlayerId)
            {
                if (player.Data["ReadyForNextTurn"].Value == "false")
                {
                    _lobbyManager.UpdatePlayerData("ReadyForNextTurn", "true", player.Id);
                    Debug.Log("Setting ReadyForNextTurn to true");
                }
                else if (player.Data["ReadyForNextTurn"].Value == "true")
                {
                    _lobbyManager.UpdatePlayerData("ReadyForNextTurn", "false", player.Id);
                    Debug.Log("Setting ReadyForNextTurn to false");
                }
            }
        }
    }

    private void UpdateLocalPlayerData()
    {
        Lobby lobby = _lobbyManager.GetJoinedLobby();
        foreach (Player player in lobby.Players)
        {
            if (player.Id == AuthenticationService.Instance.PlayerId)
            {
                string lobbyValue = player.Data["PlayerControl"].Value;
                string[] tileStrings = lobbyValue.Split(",");
                foreach (string tileString in tileStrings)
                {
                    GameObject tile = GameObject.Find(tileString);
                    if (tile != null && !playerControlList.Contains(tile))
                    {
                        AddToPlayerControl(null, tile, null, null, false);
                    }
                }
            }
        }
    }

    public int GetPlayerID()
    {
        return _playerID.Value;
    }

    public void SetPlayerName(string playerName)
    {
        _playerName = playerName;
    }

    public string GetPlayerName()
    {
        return _playerName;
    }

    public void AddToPlayerControl(GameObject[] tiles, GameObject singleTile, string newValue, string playerId, bool updateLobby)
    {
        if (updateLobby)
        {
            _lobbyManager.UpdatePlayerData("PlayerControl", newValue, playerId);
        }
        else
        {
            playerControlList.Add(singleTile);
            singleTile.GetComponent<ControlManager>().SetInPlayerControl(true, this.name, playerId);
            Debug.Log(singleTile.name + " set to true for player: " + playerId);
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void RemoveFromPlayerControlServerRpc(string tileName, string playerId)
    {
        RemoveFromPlayerControlClientRpc(tileName, playerId);
    }

    [ClientRpc]
    private void RemoveFromPlayerControlClientRpc(string tileName, string playerId)
    {
        Debug.Log("Trying to remove tile from player control");
        GameObject tile = _worldManager.GetFromTileDictionary(tileName);
        Debug.Log("tile to work with is: " + tile.name);
        if(playerId != AuthenticationService.Instance.PlayerId)
        {
            Lobby lobby = _lobbyManager.GetJoinedLobby();
            foreach (Player player in lobby.Players)
            {
                Debug.Log("Going through each player");
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    Debug.Log("Player id is the same as this one");
                    string oldValue = player.Data["PlayerControl"].Value;
                    string[] tileStrings = oldValue.Split(",");
                    string newValue = "";
                    foreach (string tileString in tileStrings)
                    {
                        if (tileString != tile.name)
                        {
                            if (newValue == "")
                            {
                                newValue = tileString;
                            }
                            else
                            {
                                newValue = newValue + "," + tileString;
                            }
                        }
                    }
                    _lobbyManager.UpdatePlayerData("PlayerControl", newValue, player.Id);
                    Debug.Log("New playerControl is: " + newValue);
                }
            }
            tile.GetComponent<ControlManager>().SetInPlayerControl(false, null, null);
            if(playerControlList.Contains(tile))
            {
                playerControlList.Remove(tile);
            }
        }
    }

    public List<GameObject> GetPlayerControl()
    {
        return playerControlList;
    }
}
