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
        tilePosition = mapController.tileWorldPos(worldPosition);
        Vector3 tileTransform = new Vector3(tilePosition.x, tilePosition.y, -1);
        transform.position = tileTransform;
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
                attackPreview(mapController.getActiveUnit(), hoveredUnit, mapController.getActiveCombatAbility());
            }
            else if (hoveredUnit && !hoveredUnit.Equals(activeHover))
            {
                uiController.clearPreview();
                attackPreview(mapController.getActiveUnit(), hoveredUnit, mapController.getActiveCombatAbility());
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
        switch (mapController.getTurnPhase())
        {
            case turnPhase.MAIN:
                if (!mapController.getActiveUnit())
                {
                    GameObject targetUnit = mapController.getUnitFromCoords(gridPosition);
                    if (targetUnit)
                    {
                        if (!mapController.getActiveUnit())
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
                if (mapController.getActiveAbility())
                {
                    if (mapController.getActiveAbility().getAbilityType() == actionType.MOVE)
                    {
                        moveAbilityTargeting();
                    }
                    else if (mapController.getActiveAbility().getAbilityType() == actionType.COMBAT)
                    {
                        combatAbilityTargeting();
                    }
                }
                break;

            default:
                Debug.Log("Turn Phase Issue - Primary");
                break;
        }
    }

    void OnCursorAlternate()
    {
        switch (mapController.getTurnPhase())
        {
            case turnPhase.MAIN:
                switch (mapController.getActionPhase())
                {
                    case actionPhase.PREPARE:
                        //Cancel action
                        mapController.completeAction();
                        break;

                    case actionPhase.EXECUTE:
                        //Skip animation
                        break;

                    case actionPhase.INACTIVE:
                        //Open unit data
                        getUnitData(mapController.getUnitFromCoords(gridPosition));
                        break;

                    default:
                        Debug.Log("Action Phase Error - Secondary");
                        break;
                }
                break;

            case turnPhase.PAUSE:
                uiController.clearPreview();
                mapController.setTurnPhase(turnPhase.MAIN);
                break;

            default:
                Debug.Log("Turn Phase Error - Secondary");
                break;
        }
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
        int[] hitStats = mapController.getHitStats(attackerController, defenderController, activeAbility.getAbilityData().getTargetInstruction());
        GameObject activePreview = uiCanvas.GetComponent<UIController>().displayPreview(defender, activeAbility, hitStats[0], attackerController.getExpectedDamage(defenderController, activeAbility.getAbilityData()), hitStats[1]);
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

    //Helper Functions
    void combatAbilityTargeting()
    {
        GameObject targetUnit = mapController.getUnitFromCoords(gridPosition);
        if (mapController.getActiveUnit())
        {
            CombatAbility combatAbility = mapController.getActiveCombatAbility();
            TargetInstruction targetInstruction = combatAbility.getAbilityData().getTargetInstruction();
            mapController.combatTargeting(targetUnit, targetInstruction, tilePosition);
        }
    }

    void moveAbilityTargeting()
    {
        if (mapController.getActiveUnit())
        {
            if (mapController.getActiveAbility().getAbilityType() == actionType.MOVE)
            {
                mapController.executeAction(null, tilePosition);
            }
        }
    }

    //Pull up unit data UI and disable all main game input
    void getUnitData(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        mapController.setTurnPhase(turnPhase.PAUSE);
        uiCanvas.GetComponent<UIController>().displayUnitDataPreview(unit);
    }
}