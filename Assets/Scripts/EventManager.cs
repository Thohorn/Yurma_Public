using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public delegate void OnTrigerReady();
    public static event OnTrigerReady onTrigerReady;


    public void TriggerNextTurn()
    {
        // onNextTurn?.Invoke();
    }

    public void GetReadyForNextTurn()
    {
        onTrigerReady?.Invoke();
    }
}

