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
    Vector2 worldPosition = new Vector2(0, 0);
    public Vector2 tilePosition = new Vector2(0, 0);
    Vector2Int gridPosition = new Vector2Int(0, 0);
    GameObject activeHover;

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
        worldPosition = Camera.main.ScreenToWorldPoint(cursorPosition);
        Vector2 tilePos = mapController.tileWorldPos(worldPosition);
        tilePosition = new Vector3(tilePos.x, tilePos.y, -1);
        transform.position = tilePosition;
        gridPosition = mapController.gridTilePos(tilePosition);
        Cursor.visible = mapController.mouseOverCanvas(cursorPosition);
        uiController.unHoverUnit();
        uiController.hoverUnit(mapController.getUnitFromCoords(gridPosition));

        if (mapController.getActionPhase() == actionPhase.PREPARE)
        {
            GameObject hoveredUnit = mapController.getUnitFromCoords(gridPosition);
            activeHover = hoveredUnit;
            if (hoveredUnit && !uiController.hasPreview())
            {
                attackPreview(selectedUnit, hoveredUnit, mapController.getActiveCombatAbility());
            }
            else if (hoveredUnit && !hoveredUnit.Equals(activeHover))
            {
                uiController.clearPreview();
                attackPreview(selectedUnit, hoveredUnit, mapController.getActiveCombatAbility());
            }
            else if (!hoveredUnit)
            {
                uiController.clearPreview();
            }
        }
    }

    //Input Handling
    void OnCursorMovement(InputValue value)
    {
        cursorPosition = value.Get<Vector2>();
    }

    void OnCursorPrimary()
    {
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
        Debug.Log(gridPosition);
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

    public Vector2 getWorldPos()
    {
        return worldPosition;
    }

    public bool mouseOnLeft()
    {
        return (cursorPosition.x / Screen.width) < 0.5;
    }

    public void attackPreview(GameObject attacker, GameObject defender, CombatAbility activeAbility)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        int[] hitStats = mapController.getHitStats(attackerController, defenderController);
        GameObject activePreview = uiCanvas.GetComponent<UIController>().displayPreview(attacker, defender, activeAbility, hitStats[0], attackerController.getExpectedDamage(defenderController, activeAbility), hitStats[1]);
        RectTransform previewTransform = activePreview.GetComponent<RectTransform>();
        if (!mouseOnLeft())
        {
            previewTransform.anchorMax = new Vector2(0, 0.5f);
            previewTransform.anchorMin = new Vector2(0, 0.5f);
            previewTransform.anchoredPosition = new Vector2(previewTransform.sizeDelta.x / 2 + 20f, 0);
        }
        else
        {
            previewTransform.anchorMax = new Vector2(1, 0.5f);
            previewTransform.anchorMin = new Vector2(1, 0.5f);
            previewTransform.anchoredPosition = new Vector2(-(previewTransform.sizeDelta.x / 2 + 20f), 0);
        }
    }

}