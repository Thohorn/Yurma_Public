using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkManagerUI : MonoBehaviour
{
    public static bool host = false;
    public static bool client = false;

    private void Awake()
    {
        // if(host)
        // {
        //     NetworkManager.Singleton.StartHost();
        //     Debug.Log("Started Hosting");
        // }
        // if(client) NetworkManager.Singleton.StartClient();
        // //DontDestroyOnLoad(gameObject);
    }

    public void Host()
    {
        host = true;
    }

    public void Join()
    {
        client = true;
    }
}
