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
    BattleController battleController;
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
        battleController = mainGrid.GetComponentsInChildren<BattleController>()[0];
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
        uiController.hoverUnit(battleController.getUnitFromCoords(gridPosition));
        if (battleController.getActionPhase() == actionPhase.PREPARE)
        {
            GameObject hoveredUnit = battleController.getUnitFromCoords(gridPosition);
            activeHover = hoveredUnit;
            if (hoveredUnit && !uiController.hasPreview())
            {
                attackPreview(battleController.getActiveUnit(), hoveredUnit, battleController.getActiveCombatAbility());
            }
            else if (hoveredUnit && !hoveredUnit.Equals(activeHover))
            {
                uiController.clearPreview();
                attackPreview(battleController.getActiveUnit(), hoveredUnit, battleController.getActiveCombatAbility());
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
        switch (battleController.getTurnPhase())
        {
            case turnPhase.MAIN:
                if (!battleController.getActiveUnit())
                {
                    GameObject targetUnit = battleController.getUnitFromCoords(gridPosition);
                    if (targetUnit)
                    {
                        if (!battleController.getActiveUnit())
                        {
                            if (battleController.getActiveTeam() != targetUnit.GetComponent<UnitController>().getTeam())
                            {
                                return;
                            }
                            uiController.drawButtons(targetUnit.GetComponent<UnitController>().getAbilities(), targetUnit);
                            uiController.validateButtons(targetUnit.GetComponent<UnitController>().getActions()[1]);
                        }
                    }
                }
                if (battleController.getActiveAbility())
                {
                    if (battleController.getActiveAbility().getAbilityType() == actionType.MOVE)
                    {
                        moveAbilityTargeting();
                    }
                    else if (battleController.getActiveAbility().getAbilityType() == actionType.COMBAT)
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
        switch (battleController.getTurnPhase())
        {
            case turnPhase.MAIN:
                switch (battleController.getActionPhase())
                {
                    case actionPhase.PREPARE:
                        //Cancel action
                        battleController.completeAction();
                        break;

                    case actionPhase.EXECUTE:
                        //Skip animation
                        break;

                    case actionPhase.INACTIVE:
                        //Open unit data
                        getUnitData(battleController.getUnitFromCoords(gridPosition));
                        break;

                    default:
                        Debug.Log("Action Phase Error - Secondary");
                        break;
                }
                break;

            case turnPhase.PAUSE:
                uiController.clearPreview();
                battleController.setTurnPhase(turnPhase.MAIN);
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
        if (!activeAbility)
        {
            return;
        }
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        int[] hitStats = battleController.getHitStats(attackerController, defenderController, activeAbility.getAbilityData().getTargetInstruction());
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
        GameObject targetUnit = battleController.getUnitFromCoords(gridPosition);
        if (battleController.getActiveUnit())
        {
            CombatAbility combatAbility = battleController.getActiveCombatAbility();
            TargetInstruction targetInstruction = combatAbility.getAbilityData().getTargetInstruction();
            battleController.combatTargeting(targetUnit, targetInstruction, tilePosition);
        }
    }

    void moveAbilityTargeting()
    {
        if (battleController.getActiveUnit())
        {
            if (battleController.getActiveAbility().getAbilityType() == actionType.MOVE)
            {
                battleController.executeAction(null, tilePosition);
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
        battleController.setTurnPhase(turnPhase.PAUSE);
        uiCanvas.GetComponent<UIController>().displayUnitDataPreview(unit);
    }
}