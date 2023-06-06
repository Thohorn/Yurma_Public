using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class CameraSystem : NetworkBehaviour
{

    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
    private bool _useEdgeScrolling = false;
    private bool _useDragPan = false;
    private float _orthographicSizeMax = 50;
    private float _orthographicSizeMin = 1;


    private bool _dragPanMoveActive;
    private Vector2 _lastMousePosition;

    private float _targetOrthographicSize = 5;


    private void Update()
    {
        // Because this is multiplayer check if the local user is the owner of the object
        if(!IsOwner) return;
        CameraMovement();
        if(_useEdgeScrolling) CameraMovementEdgeScrolling();
        if(_useDragPan) CameraMovementDragPan();
        CameraZoom();
    }

    private void CameraMovement()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDir.y = +1f;
        if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDir.y = -1f;
        if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDir.x = -1f;
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDir.x = +1f;

        Vector3 moveDir = transform.up * inputDir.y + transform.right * inputDir.x;

        float moveSpeed = _cinemachineVirtualCamera.m_Lens.OrthographicSize;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void CameraMovementEdgeScrolling()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        int edgeScrollSize = 20;
        if (Input.mousePosition.x < edgeScrollSize) inputDir.x = -1f;
        if (Input.mousePosition.y < edgeScrollSize) inputDir.y = -1f;
        if (Input.mousePosition.x > Screen.width - edgeScrollSize) inputDir.x = +1f;
        if (Input.mousePosition.y > Screen.height - edgeScrollSize) inputDir.y = +1f;

        Vector3 moveDir = transform.up * inputDir.y + transform.right * inputDir.x;

        float moveSpeed = _cinemachineVirtualCamera.m_Lens.OrthographicSize;;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

    }

    private void CameraMovementDragPan()
    {
        Vector3 inputDir = new Vector3(0, 0, 0);

        if (Input.GetMouseButtonDown(1))
        {
            _dragPanMoveActive = true;
            _lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            _dragPanMoveActive = false;
        }

        if (_dragPanMoveActive)
        {
            Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - _lastMousePosition;

            float dragPanSpeed = 1.2f;
            inputDir.x = -mouseMovementDelta.x * dragPanSpeed;
            inputDir.y = -mouseMovementDelta.y * dragPanSpeed;

            _lastMousePosition = Input.mousePosition;
        }
        Vector3 moveDir = transform.up * inputDir.y + transform.right * inputDir.x;

        float moveSpeed = 5f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void CameraZoom()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            _targetOrthographicSize -= 5;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            _targetOrthographicSize += 5;
        }

        _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize, _orthographicSizeMin, _orthographicSizeMax);

        float zoomSpeed = 5f;
        _cinemachineVirtualCamera.m_Lens.OrthographicSize =
            Mathf.Lerp(_cinemachineVirtualCamera.m_Lens.OrthographicSize, _targetOrthographicSize, Time.deltaTime * zoomSpeed);
    }
}
