using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] Transform playerCameraTransform = null;
    [SerializeField] float speed = 20f;
    [SerializeField] float screenBorderThickness = 10f;
    [SerializeField] Vector2 screenXLimits = Vector2.zero;
    [SerializeField] Vector2 screenZLimits = Vector2.zero;

    // State
    Vector2 previousInput = Vector2.zero;

    // Cached References
    StandardInput standardInput = null;

    [ClientCallback]

    private void Update()
    {
        if (!hasAuthority || !Application.isFocused) { return; }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 position = playerCameraTransform.position;
        
        if (previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;

            Vector2 currentPosition = Mouse.current.position.ReadValue();

            if (currentPosition.y >= Screen.height - screenBorderThickness)
            {
                cursorMovement.z += 1;
            }
            else if (currentPosition.y <= screenBorderThickness)
            {
                cursorMovement.z -= 1;
            }

            if (currentPosition.x >= Screen.width - screenBorderThickness)
            {
                cursorMovement.x += 1;
            }
            else if (currentPosition.x <= screenBorderThickness)
            {
                cursorMovement.x -= 1;
            }

            position += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else
        {
            position += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        position.x = Mathf.Clamp(position.x, screenXLimits.x, screenXLimits.y);
        position.z = Mathf.Clamp(position.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = position;
    }

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        standardInput = new StandardInput();

        standardInput.Player.MoveCamera.performed += context => SetPreviousInput(context.ReadValue<Vector2>());
        standardInput.Player.MoveCamera.canceled += context => SetPreviousInput(context.ReadValue<Vector2>());

        standardInput.Player.Enable();
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }

        standardInput.Player.Disable();
    }

    private void SetPreviousInput(Vector2 input)
    {
        previousInput = input;
    }
}
