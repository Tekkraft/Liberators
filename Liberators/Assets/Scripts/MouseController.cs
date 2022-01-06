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
        if (!unitSelected)
        {
            if (selectedUnit)
            {
                selectedUnit.GetComponent<UnitController>().destroyMarkers();
            }
            GameObject targetUnit = mapController.getUnitFromCoords(gridPosition);
            if (targetUnit)
            {
                selectedUnit = targetUnit;
                targetUnit.GetComponent<UnitController>().createMarkers();
                unitSelected = true;
                return;
            }
            return;
        }
        else
        {
            unitSelected = !mapController.moveUnit(selectedUnit, mapController.tileGridPos(gridPosition));
        }
    }
}