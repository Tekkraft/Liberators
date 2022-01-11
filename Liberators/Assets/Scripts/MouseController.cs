using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    public Sprite positiveCursor;
    public Sprite negativeCursor;
    public Sprite neutralCursor;
    Grid mainGrid;
    MapController mapController;
    GameObject selectedUnit;
    bool unitSelected;

    Vector2 cursorPosition = new Vector2(0, 0);
    Vector2 tilePosition = new Vector2(0, 0);
    Vector2Int gridPosition = new Vector2Int(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(cursorPosition);
        worldPos = mapController.tileWorldPos(worldPos);
        tilePosition = new Vector3(worldPos.x, worldPos.y, -1);
        transform.position = tilePosition;
        gridPosition = mapController.gridTilePos(tilePosition);
    }

    void OnCursorMovement(InputValue value)
    {
        cursorPosition = value.Get<Vector2>();
    }

    void OnCursorPrimary()
    {
        GameObject targetUnit = mapController.getUnitFromCoords(gridPosition);
        if (!unitSelected)
        {
            if (selectedUnit)
            {
                selectedUnit.GetComponent<UnitController>().destroyMarkers();
            }
            if (targetUnit)
            {
                selectedUnit = targetUnit;
                UnitController targetController = targetUnit.GetComponent<UnitController>();
                targetController.createMarkers(UnitController.MarkerAreas.RADIAL, targetController.getStats()[0], 0, MarkerController.Markers.BLUE);
                unitSelected = true;
                return;
            }
            return;
        }
        else
        {
            if (targetUnit)
            {
                mapController.attackUnit(selectedUnit,targetUnit);
            }
            else
            {
                unitSelected = !mapController.moveUnit(selectedUnit, mapController.tileGridPos(gridPosition));
            }
        }
    }

    void OnCursorAlternate()
    {
        completeAction();
    }

    void completeAction()
    {
        mapController.setActionState(MapController.actionType.NONE);
        if (selectedUnit)
        {
            selectedUnit.GetComponent<UnitController>().destroyMarkers();
        }
        selectedUnit = null;
        unitSelected = false;
    }
}