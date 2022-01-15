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
    Vector2 tilePosition = new Vector2(0, 0);
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
            switch (action)
            {
                case MapController.actionType.MOVE:
                    moveAction();
                    break;

                case MapController.actionType.ATTACK:
                    attackAction(targetUnit);
                    break;

                case MapController.actionType.SUPPORT:
                    supportAction();
                    break;
            }
        }
        else
        {
            if (targetUnit)
            {
                if (mapController.getActiveTeam() != targetUnit.GetComponent<UnitController>().getTeam())
                {
                    return;
                }
            }
            switch (action)
            {
                case MapController.actionType.MOVE:
                    movePrepare(targetUnit);
                    break;

                case MapController.actionType.ATTACK:
                    attackPrepare(targetUnit);
                    break;

                case MapController.actionType.SUPPORT:
                    supportPrepare(targetUnit);
                    break;
            }

        }
    }

    void OnCursorAlternate()
    {
        completeAction();
    }

    //Action Handling
    public void completeAction()
    {
        mapController.setActionState(MapController.actionType.NONE);
        if (selectedUnit)
        {
            selectedUnit.GetComponent<UnitController>().destroyMarkers();
        }
        selectedUnit = null;
        unitSelected = false;
    }

    void movePrepare(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        if (selectedUnit)
        {
            selectedUnit.GetComponent<UnitController>().destroyMarkers();
        }
        selectedUnit = unit;
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMarkers(UnitController.MarkerAreas.RADIAL, targetController.getStats()[0], 0, MarkerController.Markers.BLUE);
        unitSelected = true;
    }

    void moveAction()
    {
        UnitController selectedController = selectedUnit.GetComponent<UnitController>();
        if (selectedController.checkActions(1))
        {
            return;
        }
        bool obstructed = !mapController.moveUnit(selectedUnit, mapController.tileGridPos(gridPosition));
        if (!obstructed)
        {
            bool done = selectedController.useActions(1);
            completeAction();
        }
    }

    void attackPrepare(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        if (selectedUnit)
        {
            selectedUnit.GetComponent<UnitController>().destroyMarkers();
        }
        selectedUnit = unit;
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMarkers(targetController.getAttackArea(), targetController.getRange(), 1, MarkerController.Markers.RED);
        unitSelected = true;
    }

    void attackAction(GameObject targetUnit)
    {
        UnitController selectedController = selectedUnit.GetComponent<UnitController>();
        if (!targetUnit || selectedController.checkActions(1))
        {
            return;
        }
        bool success = mapController.attackUnit(selectedUnit, targetUnit);
        if (success)
        {
            completeAction();
            bool done = selectedController.useActions(1);
        }
    }

    void supportPrepare(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        if (selectedUnit)
        {
            selectedUnit.GetComponent<UnitController>().destroyMarkers();
        }
        selectedUnit = unit;
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMarkers(UnitController.MarkerAreas.RADIAL, 0, 0, MarkerController.Markers.GREEN);
        unitSelected = true;
    }

    void supportAction()
    {
        UnitController targetController = selectedUnit.GetComponent<UnitController>();
        if (targetController.checkActions(2))
        {
            completeAction();
            return;
        }
        targetController.restoreHealth(targetController.getStats()[0]);
        bool done = targetController.useActions(2);
        completeAction();
    }
}