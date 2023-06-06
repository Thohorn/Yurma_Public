using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Authentication;
using UnityEngine.EventSystems;

public class MouseController : NetworkBehaviour
{
    private GameObject currentTarget;
    private GameObject _currentSelectedTarget;

    public delegate GameObject GetTarget();
    public static event GetTarget getTarget;

    public delegate void SetTarget(GameObject target);
    public static event SetTarget setTarget;

    public delegate void SetTargetBuild(GameObject target);
    public static event SetTargetBuild setTargetBuild;

    public delegate void ShowBuilding(string tag);
    public static event ShowBuilding showBuilding;

    public delegate GameObject GetTile(string name);
    public static event GetTile getTile;

    public delegate void ShowControl();
    public static event ShowControl showControl;

    public delegate void OnTileNotInControl(string type, GameObject tile);
    public static event OnTileNotInControl onTileNotInControl;


    private void OnEnable()
    {
        CanvasManager.finishedClaiming += UpdateBuildingScreen;
    }

    private void OnDisable()
    {
        CanvasManager.finishedClaiming -= UpdateBuildingScreen;
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            currentTarget = getTarget?.Invoke();
            setTarget?.Invoke(TrackHover(currentTarget));

            if (Input.GetMouseButtonDown(0))
            {
                _currentSelectedTarget = currentTarget;
                UpdateBuildingScreen(currentTarget);
            }
        }
    }

    private void UpdateBuildingScreen(GameObject workWithTarget)
    {
        // Checks if the player controls the tile that is pressed.
        // If it does; show the building screen. If not; show that the tile is not owned.
        string tag = workWithTarget.GetComponent<PolygonCollider2D>().tag;
        bool inPlayerControl = workWithTarget.GetComponent<ControlManager>().GetInPlayerControl();
        string targetPlayerId = workWithTarget.GetComponent<ControlManager>().GetPlayerId();
        if(inPlayerControl && targetPlayerId == AuthenticationService.Instance.PlayerId)
        {
            // buildingsManager.SetBuildTarget(workWithTarget);
            Debug.Log("Setting _currentSelectedTarget to: " + workWithTarget);
            setTargetBuild?.Invoke(workWithTarget);
            // canvasManager.ShowBuildings(tag);
            showBuilding?.Invoke(tag);
        }
        else
        {
            onTileNotInControl?.Invoke(tag, workWithTarget);
        }
    }

    private GameObject TrackHover(GameObject currentTarget)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
        if (hit.collider != null)
        {
            if (currentTarget != null)
            {
                UnHoverTile(currentTarget);
            }

            currentTarget = getTile?.Invoke(hit.collider.name);
            if (currentTarget != null)
            {
                HoverTile(currentTarget);
            }
        }
        return currentTarget;
    }

    private void HoverTile(GameObject tile)
    {
        Color hoverColor = tile.GetComponent<SpriteRenderer>().color;
        hoverColor = new Color(0f, 0f, 0f, 0.7f);
        tile.GetComponent<SpriteRenderer>().color  = hoverColor;
    }

    private void UnHoverTile(GameObject tile)
    {
        Color noHoverColor = tile.GetComponent<SpriteRenderer>().color;
        noHoverColor= new Color(0f, 0f, 0f, 0f);
        tile.GetComponent<SpriteRenderer>().color  = noHoverColor;
        showControl?.Invoke();
    }

    public GameObject GetCurrentSelectedTarget()
    {
        Debug.Log("Returning _currentSelectedTarget: " + _currentSelectedTarget);
        return _currentSelectedTarget;
    }
}
