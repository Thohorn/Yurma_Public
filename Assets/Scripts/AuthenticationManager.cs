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
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class AuthenticationManager : MonoBehaviour
{

    private UnityTransport _transport;

    private static Lobby _currentLobby;

    private static bool _authenticated = false;

    private async void Awake()
    {
        if(!_authenticated)
        {
            await UnityServices.InitializeAsync();
            await Authenticate();
        }
    }

    private static async Task Authenticate()
    {

        // Code copied from documentation
        try
        {
            var options = new InitializationOptions();

            #if UNITY_EDITOR
            if (ParrelSync.ClonesManager.IsClone())
            {
                // When using a ParrelSync clone, switch to a different authentication profile to force the clone
                // to sign in as a different anonymous user account.
                string customArgument = ParrelSync.ClonesManager.GetArgument();
                AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
                Debug.Log("Switched profile");
            }
            #endif
            await UnityServices.InitializeAsync(options);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            
            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}"); 
            _authenticated = true;

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    
}
