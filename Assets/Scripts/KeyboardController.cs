using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KeyboardController : NetworkBehaviour
{
    public delegate void OnEscapeIsPressed();
    public static event OnEscapeIsPressed onEscapeIsPressed;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            onEscapeIsPressed?.Invoke();
        }
    }
}
