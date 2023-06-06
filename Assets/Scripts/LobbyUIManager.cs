using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI _gameName;
    [SerializeField] GameObject _lobbyPlayerTile;
    [SerializeField] GameObject _lobbyPlayerName;
    [SerializeField] GameObject _lobbyChosenColour;
    [SerializeField] GameObject _lobbyColour;
    [SerializeField] GameObject _lobbyColourDropdown;
    [SerializeField] Transform _canvasT;
    [SerializeField] Canvas _canvasC;
    [SerializeField] List<GameObject> _lobbyPlayerTileList;

    [SerializeField] GameObject _loadScreen;
    [SerializeField] GameObject _gameInfoScreen;
    [SerializeField] GameObject _startGameButton;

    private int _amountOfPlayers = 0;

    private static GameObject s_lobbyManager;
    private GameObject _lobbyCanvas;
    private GameObject _playerNameTag;
    private List<GameObject> _playerTileList = new List<GameObject>();
    private bool _startGameButtonBuild = false;



    private void OnEnable()
    {
        LobbyManager.onLoading += LoadingScreen;
        LobbyManager.onLobbyChanged += UpdateLobbyPlayerTileServerRpc;
    }

    private void OnDisable()
    {
        LobbyManager.onLoading -= LoadingScreen;
        LobbyManager.onLobbyChanged -= UpdateLobbyPlayerTileServerRpc;
    }

    private void Start()
    {
        s_lobbyManager = GameObject.Find("LobbyManager");
    }

    public void ShowGameName(string name)
    {
        //_gameName.text = name;
    }

    private void LoadingScreen(bool loading)
    {
        // if(loading)
        // {
        //     _gameInfoScreen.SetActive(false);
        //     _loadScreen.SetActive(true);
        // }
        // else
        // {
        //     _gameInfoScreen.SetActive(true);
        //     _loadScreen.SetActive(false);
        // }
    }





    [ServerRpc(RequireOwnership = false)]
    private void UpdateLobbyPlayerTileServerRpc()
    {
        UpdatePlayerTileClientRpc();
    }


    [ClientRpc]
    private void UpdatePlayerTileClientRpc()
    {
        Lobby lobby = s_lobbyManager.GetComponent<LobbyManager>().GetJoinedLobby();
        // Update the colour dropdown for each player
        foreach (Player player in lobby.Players)
        {
            foreach (GameObject tile in _playerTileList)
            {
                if (tile.name == player.Id.ToString())
                {
                    GameObject dropdown = tile.transform.GetChild(1).transform.GetChild(1).gameObject;
                    TMP_Dropdown drop = dropdown.GetComponent<TMP_Dropdown>();
                    string newValue = player.Data["PlayerColour"].Value;
                    if (int.TryParse(newValue, out int intValue))
                    {
                        drop.value = intValue;
                    }
                    else
                    {
                        Debug.Log("Couldn't parse in UpdatePlayerDataLobbyClinetRpc");
                    }
                }
            }
        }

        // Create the start game button only for the host.
        if (this.IsHost && _startGameButtonBuild == false)
        {
            _startGameButton.SetActive(true);
            _startGameButtonBuild = true;
        }

        // If there are more players in the lobby than there are playercards make another playercard based on the extra player
        if (_amountOfPlayers < lobby.Players.Count)
        {
            _amountOfPlayers++;
            if (lobby != null)
            {
                int i = 0;
                foreach (Player player in lobby.Players)
                {
                    if (i < _playerTileList.Count && _playerTileList[i].name == player.Id.ToString())
                    {
                        i++;
                    }
                    else
                    {
                        Debug.Log("CreateLobbyPlayerTileClientRPC is happening with: " + player.Data["PlayerName"].Value);
                        // Create the basic playerTile
                        Vector3 newPosition = _lobbyPlayerTile.transform.position;
                        newPosition.x = newPosition.x + ((i) * 230);
                        GameObject playerTile = Instantiate(_lobbyPlayerTile, newPosition, Quaternion.identity);
                        playerTile.transform.SetParent(_canvasT.transform, false);
                        // Giving the tile  a name so we can find it back later
                        playerTile.name = player.Id.ToString();
                        // Add the name tag to the playertile
                        _playerNameTag = Instantiate(_lobbyPlayerName);
                        _playerNameTag.transform.SetParent(playerTile.transform, false);
                        // Fill in the tag for the name
                        _playerNameTag.GetComponent<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;
                        // Add the colour to the playerTile
                        GameObject colourGroup = Instantiate(_lobbyColour);
                        colourGroup.transform.SetParent(playerTile.transform, false);
                        // Add the colour dropdown to the colour group
                        GameObject colourDropdown = Instantiate(_lobbyColourDropdown);
                        colourDropdown.transform.SetParent(playerTile.transform.GetChild(1).transform, false);
                        // Add the chosen colour to the playertile and make it work with dropdown
                        GameObject chosenColour = Instantiate(_lobbyChosenColour);
                        chosenColour.transform.SetParent(playerTile.transform.GetChild(1).transform.GetChild(1).transform, false);
                        colourDropdown.GetComponent<TMP_Dropdown>().captionImage = chosenColour.GetComponent<Image>();
                        if (int.TryParse(player.Data["PlayerColour"].Value, out int newInt))
                        {
                            colourDropdown.GetComponent<TMP_Dropdown>().value = newInt;
                        }

                        if(player.Id == AuthenticationService.Instance.PlayerId)
                        {
                            colourDropdown.GetComponent<TMP_Dropdown>().interactable = true;
                        }

                        _playerTileList.Add(playerTile);
                        i++;
                        Debug.Log("CreateLobbyPlayerTileClientRPC has happened with: " + player.Data["PlayerName"].Value);
                    }
                }
            }
            else
            {
                Debug.Log("Lobby is null");
            }
        }
    }



    public void OnDropDownValueChanged(TMP_Dropdown dropDown)
    {
        // If the dropdown value is changed for the current player, also change the value in the lobby options
        if (dropDown.interactable == true)
        {
            Lobby lobby = s_lobbyManager.GetComponent<LobbyManager>().GetJoinedLobby();
            foreach (Player player in lobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    s_lobbyManager.GetComponent<LobbyManager>().UpdatePlayerData("PlayerColour", dropDown.value.ToString(), player.Id);
                }
            }

        }
    }
}