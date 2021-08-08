using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    // Tunables
    [SerializeField] LayerMask layerMask = new LayerMask();
    [SerializeField] RectTransform unitSelectionArea = null;

    // Cached References
    Camera mainCamera = null;
    NetworkPlayer networkPlayer = null;
    StandardInput standardInput = null;

    // State
    public List<Unit> SelectedUnits { get; } = new List<Unit>();
    private Vector2 startPosition;
    bool selectSticky = false;

    private void Awake()
    {
        standardInput = new StandardInput();
        standardInput.Player.SelectSticky.performed += context => SelectSticky(true);
        standardInput.Player.SelectSticky.canceled += context => SelectSticky(false);
    }

    private void OnEnable()
    {
        standardInput.Player.Enable();
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.clientOnGameOver += ClientHandleGameOver;
    }

    private void OnDisable()
    {
        standardInput.Player.Disable();
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.clientOnGameOver -= ClientHandleGameOver;
    }

    private void SelectSticky(bool enable)
    {
        selectSticky = enable;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        SetUpNetworkPlayerReference();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!selectSticky)
            {
                DeselectUnits();
            }

            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea();
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

    private void StartSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(true);
        startPosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
    }
    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;
        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if (Mathf.Approximately(unitSelectionArea.sizeDelta.magnitude, 0f))
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) { return; }
            if (!hit.collider.TryGetComponent(out Unit unit)) { return; }
            if (!unit.hasAuthority) { return; }

            SelectUnits(unit);
        }
        else
        {
            Vector2 minPosition = unitSelectionArea.anchoredPosition - unitSelectionArea.sizeDelta / 2;
            Vector2 maxPosition = unitSelectionArea.anchoredPosition + unitSelectionArea.sizeDelta / 2;

            foreach (Unit unit in networkPlayer.GetUnits())
            {
                if (SelectedUnits.Contains(unit)) { continue; }

                Vector2 unitPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
                float xPos = unitPosition.x;
                float yPos = unitPosition.y;

                if (xPos > minPosition.x && xPos < maxPosition.x && yPos > minPosition.y && yPos < maxPosition.y)
                {
                    SelectUnits(unit);
                }
            }
        }
        unitSelectionArea.sizeDelta = Vector2.zero;
    }

    private void SelectUnits(Unit unit)
    {
        SelectedUnits.Add(unit);
        foreach (Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Select();
        }
    }

    private void DeselectUnits()
    {
        foreach (Unit selectedUnit in SelectedUnits)
        {
            selectedUnit.Deselect();
        }
        SelectedUnits.Clear();
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
