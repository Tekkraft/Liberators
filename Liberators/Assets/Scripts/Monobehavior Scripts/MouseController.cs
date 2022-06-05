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
    Canvas uiCanvas;
    UIController uiController;

    Vector2 cursorPosition = new Vector2(0, 0);
    public Vector2 tilePosition = new Vector2(0, 0);
    Vector2Int gridPosition = new Vector2Int(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        uiController = uiCanvas.GetComponent<UIController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(cursorPosition);
        worldPos = mapController.tileWorldPos(worldPos);
        tilePosition = new Vector3(worldPos.x, worldPos.y, -1);
        transform.position = tilePosition;
        gridPosition = mapController.gridTilePos(tilePosition);
        Cursor.visible = mapController.mouseOverCanvas(cursorPosition);
        uiController.unHoverUnit();
        uiController.hoverUnit(mapController.getUnitFromCoords(gridPosition));
    }

    //Input Handling
    void OnCursorMovement(InputValue value)
    {
        cursorPosition = value.Get<Vector2>();
    }

    void OnCursorPrimary()
    {
        MapController.actionType action = mapController.getActionState();
        GameObject targetUnit = mapController.getUnitFromCoords(gridPosition);
        if (unitSelected)
        {
            mapController.executeAction(targetUnit);
        }
        else
        {
            if (targetUnit)
            {
                if (mapController.getActiveTeam() != targetUnit.GetComponent<UnitController>().getTeam())
                {
                    return;
                }
                uiController.drawButtons(targetUnit.GetComponent<UnitController>().getAbilities(), targetUnit);
                uiController.validateButtons(targetUnit.GetComponent<UnitController>().getActions()[1]);
            }
        }
    }

    void OnCursorAlternate()
    {
        mapController.completeAction(selectedUnit);
    }

    public void setSelectedUnit(GameObject unit)
    {
        selectedUnit = unit;
        if (selectedUnit)
        {
            unitSelected = true;
        }
        else
        {
            unitSelected = false;
        }
    }

    public GameObject getSelectedUnit()
    {
        return selectedUnit;
    }

    public Vector2Int getGridPos()
    {
        return gridPosition;
    }
}