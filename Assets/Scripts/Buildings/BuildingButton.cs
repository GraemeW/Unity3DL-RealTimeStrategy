using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Tunables
    [SerializeField] Building building = null;
    [SerializeField] Image iconImage = null;
    [SerializeField] TextMeshProUGUI priceText = null;
    [SerializeField] LayerMask floorMask = new LayerMask();
    [SerializeField] Color canPlaceColor = Color.green;
    [SerializeField] Color cannotPlaceColor = Color.red;

    // Cached References
    Camera mainCamera = null;
    NetworkPlayer networkPlayer = null;
    GameObject buildingPreviewInstance = null;
    Renderer buildingRendererInstance = null;
    BoxCollider buildingCollider = null;

    private void Start()
    {
        mainCamera = Camera.main;
        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
        buildingCollider = building.GetComponent<BoxCollider>();

        networkPlayer = NetworkClient.connection.identity.GetComponent<NetworkPlayer>();
    }

    private void Update()
    {
        if (buildingPreviewInstance == null) { return; }

        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        if (networkPlayer.GetResources() < building.GetPrice()) { return; }

        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(buildingPreviewInstance == null) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            networkPlayer.CmdTryPlaceBuilding(building.GetID(), hit.point);
        }

        Destroy(buildingPreviewInstance);
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }

        buildingPreviewInstance.transform.position = hit.point;

        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        Color previewColor = networkPlayer.CanPlaceBuilding(buildingCollider, hit.point) ? canPlaceColor : cannotPlaceColor;

        buildingRendererInstance.material.SetColor("_BaseColor", previewColor);
    }
}
