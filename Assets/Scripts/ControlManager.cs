using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class ControlManager : NetworkBehaviour
{
    NetworkManager _networkManager;
    private NetworkVariable<bool> _inPlayerControl = new NetworkVariable<bool>(false);
    private string _playerOwner;
    private string _playerId;

    public void SetInPlayerControl(bool newValue, string playerName, string playerId)
    {
        SetInPlayerControlServerRpc(newValue);
        SetPlayerOwnerServerRpc(playerName);
        SetPlayerIdValueServerRpc(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetInPlayerControlServerRpc(bool newValue)
    {
        _inPlayerControl.Value = newValue;
        Debug.Log("Is now in player control: " + _inPlayerControl.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerOwnerServerRpc(string playerName)
    {
        SetplayerOwnerClientRpc(playerName);
    }

    [ClientRpc]
    private void SetplayerOwnerClientRpc(string playerName)
    {
        _playerOwner = playerName;
        Debug.Log("Player owner is now: " + _playerOwner);
    }

    [ServerRpc(RequireOwnership=false)]
    private void SetPlayerIdValueServerRpc(string playerId)
    {
        SetPlayerIdValueClientRpc(playerId);
    }

    [ClientRpc]
    private void SetPlayerIdValueClientRpc(string playerId)
    {
        _playerId = playerId;
        Debug.Log("Player ID is now: " + _playerId);
    }

    public bool GetInPlayerControl()
    {
        return _inPlayerControl.Value;
    }

    public string GetPlayerOwner()
    {
        return _playerOwner;
    }

    public string GetPlayerId()
    {
        return _playerId;
    }
}
