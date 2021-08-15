using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    // Tunables
    [SerializeField] RectTransform minimapRect = null;
    [SerializeField] float mapScale = 20f;
    [SerializeField] float offset = -6f;

    // State
    NetworkPlayer networkPlayer = null;
    Transform playerCameraTransform = null;

    private void Update()
    {
        if (playerCameraTransform != null) { return; }

        SetUpNetworkPlayerReference();
        if (networkPlayer != null)
        {
            playerCameraTransform = networkPlayer.GetCameraTransform();
        }
    }

    private void SetUpNetworkPlayerReference()
    {
        // Called after Start
        // Race Condition:  Cannot guarantee client is available within start since it follows from networkmanager Start() routine
        if (networkPlayer == null)
        {
            NetworkConnection networkConnection = NetworkClient.connection;
            if (networkConnection != null)
            {
                NetworkIdentity networkIdentity = networkConnection.identity;
                if (networkIdentity != null) { networkPlayer = networkIdentity.GetComponent<NetworkPlayer>(); }
            }
        }
    }

    private void MoveCamera()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, mousePosition, null, out Vector2 localPoint)) { return; }

        Vector2 lerpRatio = new Vector2(
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width, 
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        Vector3 newCameraPosition = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerpRatio.x), 
            playerCameraTransform.position.y, 
            Mathf.Lerp(-mapScale, mapScale, lerpRatio.y));

        playerCameraTransform.position = newCameraPosition + new Vector3(0f, 0f, offset);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }
}
