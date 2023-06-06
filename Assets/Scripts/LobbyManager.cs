using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    // variables needed for creating a game
    private static string _gameName;
    private static int _maxAmountOfPlayers;

    // Variables needed for both creating and joining a game
    private static string _playerName;

    // Lobby bool
    private static bool _buildLobby;
    private static bool _isHost;
    private static bool _isClient;

    private static string _lobbyId;

    private float _lobbyUpdateTimer = 1.1f;


    public Lobby JoinedLobby
    {
        get { return s_joinedLobby; }
        set
        {
            s_joinedLobby = value;
            onLobbyChanged?.Invoke();
        }   
    }

    private static Lobby s_joinedLobby;

    public delegate void OnLobbyChanged();
    public static event OnLobbyChanged onLobbyChanged;

    public delegate void OnLoading(bool loading);
    public static event OnLoading onLoading;

    public delegate void OnPlayerCreate();
    public static event OnPlayerCreate onPlayerCreate;

    public delegate void OnBuildingLobby(string gameName);
    public static event OnBuildingLobby onBuildingLobby;

    private UnityTransport _transport;


    private void OnEnable()
    {
        PlayerManager.onPlayerGetName += GivePlayerName;
    }

    private void OnDisable()
    {
        PlayerManager.onPlayerGetName -= GivePlayerName;
    }

    private void Awake()
    {
        _transport = FindObjectOfType<UnityTransport>();
        DontDestroyOnLoad(gameObject);
    }

    private  void Update()
    {
        if(_buildLobby)
        {
            if(_isHost)
            {
                if(NetworkManager.Singleton.IsHost == false)
                {
                    // Show the loading screen
                }
                else if(NetworkManager.Singleton.IsHost == true)
                {
                    // Disable loading screen
                    onBuildingLobby?.Invoke(_gameName);
                    onPlayerCreate?.Invoke();
                    //gameObject.GetComponent<LobbyUIManager>().ShowGameName(_gameName);
                    StartCoroutine(PingCoroutine(_lobbyId, 15));
                    Debug.Log("We are in a host lobby");
                    _buildLobby = false;
                }
            }
            else if (_isClient)
            {
                if(NetworkManager.Singleton.IsClient == false)
                {
                    Debug.Log("Client not running, joined lobby is: " + this.JoinedLobby);
                }
                else if(NetworkManager.Singleton.IsClient == true && this.JoinedLobby != null)
                {
                    //gameObject.GetComponent<LobbyUIManager>().ShowGameName(_gameName);
                    onPlayerCreate?.Invoke();
                    Debug.Log("We are in a client lobby");
                    Debug.Log("The lobby is called : " + this.JoinedLobby.Name);
                    Debug.Log("It has " + this.JoinedLobby.Players.Count + " players");
                    _buildLobby = false;
                }
            }
        }

        UpdateLobby();
    }

    private async void UpdateLobby()
    {
        if(this.JoinedLobby != null)
        {
            _lobbyUpdateTimer -= Time.deltaTime;
            if(_lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                _lobbyUpdateTimer = lobbyUpdateTimerMax;

                try
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(this.JoinedLobby.Id);
                    this.JoinedLobby = lobby;
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
            }
        }
    }

    public async void UpdatePlayerData(string key, string newData, string playerId)
    {
        UpdatePlayerOptions options = new UpdatePlayerOptions();
        options.Data = new Dictionary<string, PlayerDataObject>() {
            {key, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newData)}};
        Lobby lobby = await LobbyService.Instance.UpdatePlayerAsync(this.JoinedLobby.Id, playerId, options);
        this.JoinedLobby = lobby;
    }


    public Lobby GetJoinedLobby()
    {
        return this.JoinedLobby;
    }

    public string GivePlayerName(GameObject player)
    {
        return _playerName;
    }


    public void SetGameName(GameObject inputGameName)
    {
        _gameName =  inputGameName.GetComponent<TextMeshProUGUI>().text;
        Debug.Log(_gameName);
    }

    public void SetAmountOfplayers(GameObject inputAmountOfPlayers)
    {
        int amount;
        if(int.TryParse(inputAmountOfPlayers.GetComponent<TextMeshProUGUI>().text, out amount))
        {
            _maxAmountOfPlayers = amount;
            Debug.Log(_maxAmountOfPlayers);
        }
        else
        {
            Debug.Log("amount of players is not a valid number it is :" + amount);
        }
    }

    public void SetPlayerName(GameObject inputPlayerName)
    {
        _playerName = inputPlayerName.GetComponent<TextMeshProUGUI>().text;
        
        Debug.Log("Player name in setPlayerName is: " + _playerName);
    }

    public void SetLobby(bool newLobby)
    {
        _buildLobby = newLobby;
    }

    public async void CreateLobby()
    {
        var a = await RelayService.Instance.CreateAllocationAsync(_maxAmountOfPlayers);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        Debug.Log("GameName in createLobby: " + _gameName);
        Debug.Log("PlayerName In CreateLobby: " + _playerName);
        Debug.Log("Maximum players in createLobby: " + _maxAmountOfPlayers);

        // Create player data in the lobby so we can use it later
        CreateLobbyOptions hostOptions = new CreateLobbyOptions {
            Player = new Player {
                Data = new Dictionary<string, PlayerDataObject> {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName)},
                    { "PlayerColour", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")},
                    { "PlayerControl", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "")},
                    { "ReadyForNextTurn", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false")}
                }
            },
            // Put the lobby joincode in J so we can join as a client
            Data = new Dictionary<string, DataObject> { { "J", new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
        };
        //options.IsPrivate = false;
                            
        Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(_gameName, _maxAmountOfPlayers, hostOptions);

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        this.JoinedLobby = lobby;
        _buildLobby = true;
        _isHost = true;
        _lobbyId = lobby.Id;

        NetworkManager.Singleton.StartHost();
        //sceneloader.GoToLobby();
    }

    public async void JoinLobby()
    {
        Debug.Log("PlayerName In JoinLobby: " + _playerName);
        // Create player data in the lobby so we can use it later
        QuickJoinLobbyOptions quickJoinOptions = new QuickJoinLobbyOptions {
            Player = new Player {
                Data = new Dictionary<string, PlayerDataObject> {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName)},
                    { "PlayerColour", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0")},
                    { "PlayerControl", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "")},
                    { "ReadyForNextTurn", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "false")}
                }
            }
        };

        var lobby = await Lobbies.Instance.QuickJoinLobbyAsync(quickJoinOptions);

        // Grab the joincode
        var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data["J"].Value);

        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
        Debug.Log("The lobbies name is: " + lobby.Name);

        this.JoinedLobby = lobby;
        _buildLobby = true;
        _isClient = true;

        NetworkManager.Singleton.StartClient();

        //sceneloader.GoToLobby();
    }

    private static IEnumerator PingCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while(true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy() 
    {
        StopAllCoroutines();
        if(_isHost)
        {
            Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
        }
    }
}
