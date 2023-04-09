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

    List<GameObject> dataOverlays = new List<GameObject>();

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
        //TODO: Likely inefficiency
        for (int i = dataOverlays.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(dataOverlays[i]);
            dataOverlays.Remove(dataOverlays[i]);
        }
        worldPosition = Camera.main.ScreenToWorldPoint(cursorPosition);
        tilePosition = mapController.tileWorldPos(worldPosition);
        Vector3 tileTransform = new Vector3(tilePosition.x, tilePosition.y, -1);
        transform.position = tileTransform;
        gridPosition = mapController.gridTilePos(tilePosition);
        Cursor.visible = mapController.mouseOverCanvas(cursorPosition);
        uiController.unHoverUnit();
        uiController.hoverUnit(battleController.GetUnitFromCoords(gridPosition));
        if (battleController.GetActionPhase() == ActionPhase.PREPARE && battleController.GetActiveAbility().getAbilityType() == ActionType.COMBAT)
        {
            GameObject hoveredUnit = battleController.GetUnitFromCoords(gridPosition);
            if (!hoveredUnit)
            {
                uiController.clearPreview();
                dataOverlays.Clear();
            }
            else
            {
                AbilityPreview(hoveredUnit, battleController.GetActiveAbility());
                foreach (EffectTypeA effect in battleController.GetActiveAbilityScript().effects)
                {
                    if (effect.GetType() == typeof(SelectEffect))
                    {
                        SelectEffect select = effect as SelectEffect;
                        if (select.selector.GetType() == typeof(AOESelector))
                        {
                            AOESelector aoe = select.selector as AOESelector;
                            //TODO: Handle more advanced AOE ranges
                            GameObject temp = GameObject.Instantiate(battleController.GetOverlayObject(), hoveredUnit.gameObject.transform);
                            temp.GetComponent<OverlayController>().initalize(aoe.range.max * 2 + 1, false, hoveredUnit);
                            dataOverlays.Add(temp);
                        }
                    }
                }
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
        if (uiController.mouseOverCanvas(cursorPosition))
        {
            return;
        }
        switch (battleController.GetTurnPhase())
         {
            case TurnPhase.MAIN:
                if (!battleController.GetActiveUnit())
                {
                    GameObject targetUnit = battleController.GetUnitFromCoords(gridPosition);
                    if (targetUnit)
                    {
                        if (!battleController.GetActiveUnit())
                        {
                            if (battleController.GetActiveTeam() != targetUnit.GetComponent<UnitController>().GetTeam())
                            {
                                return;
                            }
                            uiController.drawButtons(targetUnit.GetComponent<UnitController>().getAbilities(), targetUnit);
                            uiController.validateButtons(targetUnit.GetComponent<UnitController>().getActions()[1]);
                        }
                    }
                }
                if (battleController.GetActiveAbility())
                {
                    if (battleController.GetActiveAbility().getAbilityType() == ActionType.MOVE)
                    {
                        MoveAbilityTargeting();
                    }
                    else if (battleController.GetActiveAbility().getAbilityType() == ActionType.COMBAT)
                    {
                        CombatAbilityTargeting();
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
        switch (battleController.GetTurnPhase())
        {
            case TurnPhase.MAIN:
                switch (battleController.GetActionPhase())
                {
                    case ActionPhase.PREPARE:
                        //Cancel action
                        battleController.CompleteAction();
                        break;

                    case ActionPhase.EXECUTE:
                        //Skip animation
                        uiController.terminateBattleAnimation();
                        break;

                    case ActionPhase.INACTIVE:
                        //Open unit data
                        GetUnitData(battleController.GetUnitFromCoords(gridPosition));
                        break;

                    default:
                        Debug.Log("Action Phase Error - Secondary");
                        break;
                }
                break;

            case TurnPhase.PAUSE:
                uiController.clearPreview();
                battleController.SetTurnPhase(TurnPhase.MAIN);
                break;

            default:
                Debug.Log("Turn Phase Error - Secondary");
                break;
        }
    }

    public Vector2Int GetGridPos()
    {
        return gridPosition;
    }

    public Vector2 GetWorldPos()
    {
        return worldPosition;
    }

    public bool MouseOnLeft()
    {
        return (cursorPosition.x / Screen.width) < 0.5;
    }

    public void AbilityPreview(GameObject defender, Ability activeAbility)
    {
        //TODO: Reimplement Attack Preview
        if (!activeAbility)
        {
            return;
        }
        uiController.displayAbilityPreview(defender, activeAbility);
    }

    //Helper Functions
    void CombatAbilityTargeting()
    {
        GameObject targetUnit = battleController.GetUnitFromCoords(gridPosition);
        if (battleController.GetActiveUnit())
        {
            battleController.CombatTargeting(targetUnit, battleController.GetActiveAbilityScript().targeting[0]);
        }
    }

    void MoveAbilityTargeting()
    {
        if (battleController.GetActiveUnit())
        {
            if (battleController.GetActiveAbility().getAbilityType() == ActionType.MOVE)
            {
                battleController.ExecuteAction(null, tilePosition);
            }
        }
    }

    //Pull up unit data UI and disable all main game input
    void GetUnitData(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        battleController.SetTurnPhase(TurnPhase.PAUSE);
        uiCanvas.GetComponent<UIController>().displayUnitDataPreview(unit);
    }
}