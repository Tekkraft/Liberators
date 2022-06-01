using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour
{
    //Grid Coordinates are +x to top-right, +y to top-left
    Grid mainGrid;
    Dictionary<Vector2Int, GameObject> unitList = new Dictionary<Vector2Int, GameObject>();
    Dictionary<int, List<GameObject>> teamLists = new Dictionary<int, List<GameObject>>();
    int eliminatedTeams = 0;
    List<GameObject> actedUnits = new List<GameObject>();
    int activeTeam = 0;
    int turnNumber = 1;

    Canvas uiCanvas;
    GameObject cursor;
    MouseController cursorController;
    Tilemap mainTilemap;
    public Pathfinder pathfinder;

    //Action Handlers
    public enum actionType { NONE, MOVE, ATTACK, SUPPORT, MISC, WAIT, END };
    actionType actionState = actionType.NONE;
    Ability activeAbility;
    GameObject activeUnit;

    public GameObject abilityCalculator;

    void Awake()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        cursor = GameObject.FindGameObjectWithTag("Cursor");
        cursorController = cursor.GetComponent<MouseController>();
        mainTilemap = mainGrid.GetComponentInChildren<Tilemap>();
        pathfinder = new Pathfinder(new Vector2Int(0, 0), 0, 0, mainTilemap);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Turn " + turnNumber);
        Debug.Log("Team " + activeTeam);
    }

    //Turn Management
    void nextPhase()
    {
        uiCanvas.GetComponent<UIController>().resetButtons();
        completeAction(null);
        List<GameObject> active = teamLists[activeTeam];
        foreach (GameObject unit in active)
        {
            unit.GetComponent<UnitController>().resetActions();
        }
        actedUnits.Clear();
        activeTeam++;
        while (!teamLists.ContainsKey(activeTeam))
        {
            if (teamLists.Count + eliminatedTeams < activeTeam)
            {
                activeTeam = -1;
                turnNumber++;
                Debug.Log("Turn " + turnNumber);
            }
            activeTeam++;
        }
        Debug.Log("Team " + activeTeam);
    }

    public int getActiveTeam()
    {
        return activeTeam;
    }

    //Canvas Management
    public bool mouseOverCanvas(Vector2 mousePos)
    {
        return uiCanvas.GetComponent<UIController>().mouseOverCanvas(mousePos);
    }

    //Action Management
    public void setActionState(GameObject unit, Ability ability)
    {
        if (ability)
        {
            actionState = ability.getAbilityType();
        }
        else
        {
            actionState = actionType.NONE;
        }
        if (actionState == actionType.END)
        {
            nextPhase();
        }
        if (!unit)
        {
            return;
        }
        activeAbility = ability;
        activeUnit = unit;
        actionSetup(unit);
    }

    public actionType getActionState()
    {
        return actionState;
    }

    void actionSetup(GameObject targetUnit)
    {
        switch (actionState)
        {
            case actionType.MOVE:
                movePrepare(targetUnit);
                break;

            case actionType.ATTACK:
                attackPrepare(targetUnit);
                break;

            case actionType.SUPPORT:
                supportPrepare(targetUnit);
                break;
        }
    }

    public void executeAction(GameObject targetUnit)
    {
        switch (actionState)
        {
            case actionType.MOVE:
                moveAction();
                break;

            case actionType.ATTACK:
                attackAction(targetUnit);
                break;

            case actionType.SUPPORT:
                supportAction(targetUnit);
                break;
        }
        completeAction(targetUnit);
    }

    //Action Handling
    public void completeAction(GameObject selectedUnit)
    {
        setActionState(null, null);
        if (selectedUnit)
        {
            selectedUnit.GetComponent<UnitController>().destroyMarkers();
        }
        else
        {
            foreach (KeyValuePair<Vector2Int, GameObject> pair in unitList)
            {
                pair.Value.GetComponent<UnitController>().destroyMarkers();
            }
        }
        cursorController.setSelectedUnit(null);
        uiCanvas.GetComponent<UIController>().clearButtons();
    }

    public void movePrepare(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        cursorController.setSelectedUnit(unit);
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMarkers(finalRange(unit.GetComponent<UnitController>().getStats()[0], activeAbility), activeAbility.getAbilityRanges()[0], MarkerController.Markers.BLUE, true);
    }

    void moveAction()
    {
        UnitController activeController = activeUnit.GetComponent<UnitController>();
        if (activeController.checkActions(activeAbility.getAPCost()))
        {
            return;
        }
        bool clear = moveUnit(activeUnit, tileGridPos(cursorController.getGridPos()));
        completeAction(activeUnit);
        if (clear)
        {
            bool done = activeController.useActions(activeAbility.getAPCost());
        }
    }

    public void attackPrepare(GameObject unit)
    {
        if (!unit || !activeAbility)
        {
            return;
        }
        cursorController.setSelectedUnit(unit);
        UnitController targetController = unit.GetComponent<UnitController>();
        if (activeAbility.getSpecialRules().Contains("SelfCast"))
        {
            targetController.createMarkers(0, 0, MarkerController.Markers.RED, false);
        }
        else
        {
            targetController.createMarkers(targetController.getEquippedWeapon().getWeaponStats()[3] + activeAbility.getAbilityRanges()[0], activeAbility.getAbilityRanges()[1], MarkerController.Markers.RED, false);
        }
        cursorController.setSelectedUnit(unit);
    }

    void attackAction(GameObject targetUnit)
    {
        UnitController selectedController = cursorController.getSelectedUnit().GetComponent<UnitController>();
        if (!activeAbility || !targetUnit)
        {
            return;
        }
        UnitController targetController = targetUnit.GetComponent<UnitController>();
        pathfinder.changeParameters(selectedController.getUnitPos(), selectedController.getEquippedWeapon().getWeaponStats()[3] + activeAbility.getAbilityRanges()[0], activeAbility.getAbilityRanges()[1]);
        pathfinder.calculate(false);
        if (activeAbility.getSpecialRules().Contains("SelfCast") && !pathfinder.checkCoords(targetController.getUnitPos()))
        {
            completeAction(cursorController.getSelectedUnit());
            return;
        }
        if (selectedController.checkActions(activeAbility.getAPCost()) || !pathfinder.checkCoords(targetController.getUnitPos()) || (selectedController.getTeam() == targetController.getTeam() && !activeAbility.getSpecialRules().Contains("SelfCast")))
        {
            completeAction(cursorController.getSelectedUnit());
            return;
        }
        GameObject temp = GameObject.Instantiate(abilityCalculator);
        AbilityCalculatorController calculator = temp.GetComponent<AbilityCalculatorController>();
        calculator.inintalizeCalculator(cursorController.getSelectedUnit(), activeAbility, cursorController.getGridPos());
        List<GameObject> hitUnits = calculator.getAffectedUnits();
        foreach (GameObject target in hitUnits)
        {
            if (target.GetComponent<UnitController>().getTeam() == selectedController.getTeam())
            {
                continue;
            }
            attackUnit(cursorController.getSelectedUnit(), target);
        }
        GameObject.Destroy(temp);
        completeAction(cursorController.getSelectedUnit());
        selectedController.useActions(activeAbility.getAPCost());
    }

    public void supportPrepare(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        if (cursorController.getSelectedUnit())
        {
            cursorController.getSelectedUnit().GetComponent<UnitController>().destroyMarkers();
        }
        cursorController.setSelectedUnit(unit);
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMarkers(0, 0, MarkerController.Markers.GREEN, false);
    }

    void supportAction(GameObject targetUnit)
    {
        UnitController targetController = cursorController.getSelectedUnit().GetComponent<UnitController>();
        if (targetController.checkActions(activeAbility.getAPCost()))
        {
            completeAction(cursorController.getSelectedUnit());
            return;
        }
        targetController.restoreHealth(targetController.getStats()[4]);
        bool done = targetController.useActions(activeAbility.getAPCost());
        completeAction(cursorController.getSelectedUnit());
    }

    //Unit Management
    public List<GameObject> getUnits()
    {
        List<GameObject> allUnits = new List<GameObject>();
        foreach (KeyValuePair<Vector2Int, GameObject> temp in unitList)
        {
            allUnits.Add(temp.Value);
        }
        return allUnits;
    }

    public void addUnit(GameObject unit)
    {
        unitList.Add(unit.GetComponent<UnitController>().getUnitPos(), unit);
        int team = unit.GetComponent<UnitController>().getTeam();
        if (!teamLists.ContainsKey(team))
        {
            teamLists.Add(team, new List<GameObject>());
        }
        teamLists[team].Add(unit);
    }

    public bool moveUnit(GameObject unit, Vector2 coords)
    {
        if (getUnitFromCoords(gridTilePos(coords)) && getUnitFromCoords(gridTilePos(coords)) != gameObject)
        {
            return false;
        }
        Vector2Int key = unit.GetComponent<UnitController>().getUnitPos();
        unitList.Remove(key);
        bool moved = unit.GetComponent<UnitController>().moveUnit(coords, activeAbility);
        if (moved)
        {
            unitList.Add(gridTilePos(coords), unit);
        }
        else
        {
            unitList.Add(key, unit);
        }
        return moved;
    }

    public void attackUnit(GameObject attacker, GameObject defender)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        bool defeated = attackerController.attackUnit(defender, activeAbility);
        if (defeated)
        {
            Vector2Int location = defenderController.getUnitPos();
            unitList.Remove(location);
            int team = defenderController.getTeam();
            teamLists[team].Remove(defender);
            if (teamLists[team].Count <= 0)
            {
                teamLists.Remove(team);
                eliminatedTeams++;
            }
            GameObject.Destroy(defender);
        }
    }

    //Stat Math
    public int finalRange(int baseRange, Ability ability)
    {
        return Mathf.FloorToInt((float)(baseRange + ability.getAbilityRanges()[0]) * (((float)ability.getAbilityRadii()[0] / 100f) + 1f));
    }

    //Grid Management
    public GameObject getUnitFromCoords(Vector2Int coords)
    {
        if (unitList.ContainsKey(coords))
        {
            return unitList[coords];
        }
        return null;
    }

    public Vector2 tileWorldPos(Vector2 worldPos)
    {
        Vector3 localCellCoords = mainGrid.LocalToCell(worldPos);
        Vector3Int localCellInt = new Vector3Int(Mathf.CeilToInt(localCellCoords.x), Mathf.CeilToInt(localCellCoords.y), Mathf.CeilToInt(localCellCoords.z));
        Vector3 localCoords = mainGrid.CellToLocal(localCellInt);
        return new Vector2(localCoords.x, localCoords.y + mainGrid.cellSize.y / 2);
    }

    public Vector2 tileGridPos(Vector2Int gridPos)
    {
        return new Vector2(((float)(gridPos.x - gridPos.y) / 2) * mainGrid.cellSize.x, ((float)(gridPos.x + gridPos.y - 1f) / 2) * mainGrid.cellSize.y);
    }

    public Vector2Int gridTilePos(Vector2 tilePos)
    {
        float tileX = tilePos.x * 2 / mainGrid.cellSize.x;
        float tileY = (tilePos.y + 0.25f) * 2 / mainGrid.cellSize.y;
        int gridX = Mathf.CeilToInt(tileY + tileX);
        int gridY = Mathf.CeilToInt(tileY - tileX);
        return new Vector2Int(gridX, gridY) / 2;
    }

    public Vector2Int gridWorldPos(Vector2 worldPos)
    {
        return gridTilePos(tileWorldPos(worldPos));
    }

    public int gridDistance(Vector2Int gridPos1, Vector2Int gridPos2)
    {
        int xDist = Mathf.Abs(gridPos1.x - gridPos2.x);
        int yDist = Mathf.Abs(gridPos1.y - gridPos2.y);
        return xDist + yDist;
    }

    public int gridDistanceFromTile(Vector2 tilePos1, Vector2 tilePos2)
    {
        return gridDistance(gridTilePos(tilePos1), gridTilePos(tilePos2));
    }

    public int gridDistanceFromWorld(Vector2 worldPos1, Vector2 worldPos2)
    {
        return gridDistance(gridWorldPos(worldPos1), gridWorldPos(worldPos2));
    }
}
