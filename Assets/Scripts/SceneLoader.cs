using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quits the game");
    }


    public void SinglePlayerPlay()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void MultiPlayerPlay()
    {
        NetworkManager.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
