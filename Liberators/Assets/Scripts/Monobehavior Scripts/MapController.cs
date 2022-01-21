using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //Action Handlers
    public enum actionType { NONE, MOVE, ATTACK, SUPPORT, MISC };
    actionType actionState = actionType.NONE;
    Ability activeAbility;
    GameObject activeUnit;

    void Awake()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        cursor = GameObject.FindGameObjectWithTag("Cursor");
        cursorController = cursor.GetComponent<MouseController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Turn " + turnNumber);
        Debug.Log("Team " + activeTeam);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Turn Management
    void nextPhase()
    {
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
        if (!unit)
        {
            return;
        }
        activeAbility = ability;
        activeUnit = unit;
        if (actionState == actionType.MISC)
        {
            nextPhase();
        }
        else
        {
            actionSetup(unit);
        }
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
        } else
        {
            foreach (GameObject unit in actedUnits)
            {
                unit.GetComponent<UnitController>().destroyMarkers();
            }
        }
        cursorController.setSelectedUnit(null);
    }

    public void movePrepare(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        cursorController.setSelectedUnit(unit);
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMarkers(UnitController.MarkerAreas.RADIAL, targetController.getStats()[0], 0, MarkerController.Markers.BLUE);
    }

    void moveAction()
    {
        UnitController activeController = activeUnit.GetComponent<UnitController>();
        if (activeController.checkActions(1))
        {
            return;
        }
        bool clear = moveUnit(activeUnit, tileGridPos(cursorController.getGridPos()));
        if (clear)
        {
            bool done = activeController.useActions(1);
            completeAction(activeUnit);
        }
    }

    public void attackPrepare(GameObject unit)
    {
        if (!unit)
        {
            return;
        }
        cursorController.setSelectedUnit(unit);
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMarkers(targetController.getAttackArea(), targetController.getRange(), 1, MarkerController.Markers.RED);
        cursorController.setSelectedUnit(unit);
    }

    void attackAction(GameObject targetUnit)
    {
        UnitController selectedController = cursorController.getSelectedUnit().GetComponent<UnitController>();
        if (!targetUnit || selectedController.checkActions(1))
        {
            return;
        }
        bool success = attackUnit(cursorController.getSelectedUnit(), targetUnit);
        if (success)
        {
            completeAction(cursorController.getSelectedUnit());
            bool done = selectedController.useActions(1);
        }
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
        targetController.createMarkers(UnitController.MarkerAreas.RADIAL, 0, 0, MarkerController.Markers.GREEN);
    }

    void supportAction(GameObject targetUnit)
    {
        UnitController targetController = cursorController.getSelectedUnit().GetComponent<UnitController>();
        if (targetController.checkActions(2))
        {
            completeAction(cursorController.getSelectedUnit());
            return;
        }
        targetController.restoreHealth(targetController.getStats()[4]);
        bool done = targetController.useActions(2);
        completeAction(cursorController.getSelectedUnit());
    }

    //Unit Management
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
        bool moved = unit.GetComponent<UnitController>().moveUnit(coords);
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

    public bool attackUnit(GameObject attacker, GameObject defender)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();

        bool inRange = attackerController.inRange(attackerController.getAttackArea(), attackerController.range, 1, defenderController.getUnitPos() - attackerController.getUnitPos());
        if (!inRange || attackerController.getTeam() == defenderController.getTeam())
        {
            return false;
        }

        bool defeated = attackerController.attackUnit(defender);
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
        return true;
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