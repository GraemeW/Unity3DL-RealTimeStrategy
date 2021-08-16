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

    private void Start()
    {
        networkPlayer = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
    }

    private void Update()
    {
        if (playerCameraTransform != null) { return; }

        if (networkPlayer != null)
        {
            playerCameraTransform = networkPlayer.GetCameraTransform();
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
