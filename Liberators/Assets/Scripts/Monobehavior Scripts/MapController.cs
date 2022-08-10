using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapController : MonoBehaviour
{
    //Special Constants
    public static int reactThreshold = 10; //Minimum threshold for evasion bonuses for ranged attacks. No threshold for melee attacks.
    public static float hitFactor = 2;
    public static float avoidFactor = 1;
    public static float critFactor = 2;

    //Main Variabless
    Grid mainGrid;
    Dictionary<Vector2Int, GameObject> unitList = new Dictionary<Vector2Int, GameObject>();
    Dictionary<int, List<GameObject>> teamLists = new Dictionary<int, List<GameObject>>();
    List<List<int>> teamAlignments = new List<List<int>>();
    int eliminatedTeams = 0;
    int activeTeam = 0;
    int turnNumber = 1;

    Canvas uiCanvas;
    GameObject cursor;
    MouseController cursorController;
    Tilemap mainTilemap;
    Pathfinder pathfinder;

    GameObject activeOverlay;

    turnPhase turnPhase = turnPhase.START;
    actionPhase actionPhase = actionPhase.INACTIVE;

    //Action Handlers
    actionType actionState = actionType.NONE;
    Ability activeAbility;
    GameObject activeUnit;

    //Public Objects
    public LayerMask lineOfSightLayer;
    public GameObject overlayObject;
    public MapData mapData;

    void Awake()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        cursor = GameObject.FindGameObjectWithTag("Cursor");
        cursorController = cursor.GetComponent<MouseController>();
        mainTilemap = mainGrid.GetComponentInChildren<Tilemap>();
        pathfinder = new Pathfinder(new Vector2Int(0, 0), 0, 0, mainTilemap);
        teamAlignments = mapData.getTeamAlignments();
    }

    // Start is called before the first frame update
    void Start()
    {
        turnPhase = turnPhase.START;
        StartCoroutine(bannerTimer());
    }

    //Turn Management
    void nextPhase()
    {
        turnPhase = turnPhase.END;
        uiCanvas.GetComponent<UIController>().resetButtons();
        completeAction(null);
        List<GameObject> active = teamLists[activeTeam];
        for (int i = active.Count - 1; i >= 0; i--)
        {
            GameObject unit = active[i];
            unit.GetComponent<UnitController>().resetActions();
            bool dead = unit.GetComponent<UnitController>().endUnitTurn();
            if (dead)
            {
                killUnit(unit);
            }
        }
        activeTeam++;
        while (!teamLists.ContainsKey(activeTeam))
        {
            if (teamLists.Count + eliminatedTeams < activeTeam)
            {
                activeTeam = -1;
                turnNumber++;
            }
            activeTeam++;
        }
        checkEndGame();
        turnPhase = turnPhase.START;
        StartCoroutine(bannerTimer());
        if (mapData.getAITeams().Contains(activeTeam))
        {
            runAIPhase(teamLists[activeTeam]);
        }
    }

    void runAIPhase(List<GameObject> units)
    {
        foreach (GameObject active in units)
        {
            int cheapest = 10;
            foreach (Ability ability in active.GetComponent<UnitController>().getAbilities())
            {
                //TEMPORARY MEASURE - REMOVE WHEN BEAMS VALID ABILITY
                if (ability.getAbilityType() == actionType.COMBAT && (ability as CombatAbility).getAbilityData().getTargetInstruction().getTargetType() == targetType.BEAM)
                {
                    continue;
                }
                if (ability.getAPCost() < cheapest)
                {
                    cheapest = ability.getAPCost();
                }
            }
            while (active.GetComponent<UnitController>().getActions()[1] >= Mathf.Max(1, cheapest))
            {
                Ability selectedAbility = active.GetComponent<AIController>().decideAction(active.GetComponent<UnitController>().getAbilities());
                if (selectedAbility == null)
                {
                    break;
                }
                setActionState(active, selectedAbility);
                if (activeAbility.getAbilityType() == actionType.COMBAT)
                {
                    AbilityData selectedData = (selectedAbility as CombatAbility).getAbilityData();
                    //TEMPORARY MEASURE - REMOVE WHEN BEAMS VALID ABILITY
                    if (selectedData.getTargetInstruction().getTargetType() == targetType.BEAM)
                    {
                        completeAction(active);
                        continue;
                    }
                    else
                    {
                        TargetInstruction targetInstruction = selectedData.getTargetInstruction();
                        UnitController targetController = activeUnit.GetComponent<UnitController>();
                        int rangeMax = targetInstruction.getMaxRange();
                        if (!targetInstruction.getMaxRangeFixed())
                        {
                            rangeMax += targetController.getEquippedWeapon().getWeaponStats()[3];
                        }
                        int rangeMin = targetInstruction.getMinRange();
                        if (!targetInstruction.getMinRangeFixed())
                        {
                            rangeMax += targetController.getEquippedWeapon().getWeaponStats()[3];
                        }
                        Vector2 direction = new Vector2(0, 0);
                        if (activeOverlay)
                        {
                            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
                        }
                        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetInstruction.getLOSRequired(), this, teamLists, direction);
                        List<GameObject> validTargets = rangefinder.generateTargetsNotOfTeam(activeUnit.GetComponent<UnitController>().getUnitPos(), getAlignedTeams(activeTeam), targetInstruction.getTargetType() == targetType.BEAM);
                        GameObject target = activeUnit.GetComponent<AIController>().getGameObjectTarget(selectedAbility as CombatAbility, validTargets);
                        if (target)
                        {
                            executeAction(target, new Vector2(0, 0));
                        }
                    }
                }
                else if (activeAbility.getAbilityType() == actionType.MOVE)
                {
                    executeAction(activeUnit, tileGridPos(activeUnit.GetComponent<AIController>().getMoveTarget(activeAbility as MovementAbility)));
                }
            }
        }
    }

    public int getActiveTeam()
    {
        return activeTeam;
    }

    IEnumerator bannerTimer()
    {
        int bannerTime = 15;
        uiCanvas.GetComponent<UIController>().changeBanner("Team " + activeTeam, bannerTime);
        for (int i = 0; i < bannerTime; i++)
        {
            yield return new WaitForSeconds(0.1f);
        }
        turnPhase = turnPhase.MAIN;
    }

    public turnPhase getTurnPhase()
    {
        return turnPhase;
    }

    public actionPhase getActionPhase()
    {
        return actionPhase;
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

    public void actionSetup(GameObject targetUnit)
    {
        switch (actionState)
        {
            case actionType.MOVE:
                movePrepare(targetUnit);
                break;

            case actionType.COMBAT:
                combatPrepare(targetUnit);
                break;
        }
    }

    public void executeAction(GameObject targetUnit, Vector2 tileDestination)
    {
        switch (actionState)
        {
            case actionType.MOVE:
                moveAction(tileDestination);
                break;

            case actionType.COMBAT:
                combatAction(targetUnit);
                break;
        }
        completeAction(targetUnit);
    }

    //Action Handling
    public void completeAction(GameObject selectedUnit)
    {
        actionState = actionType.NONE;
        activeUnit = null;
        actionPhase = actionPhase.INACTIVE;
        GameObject.Destroy(activeOverlay);
        uiCanvas.GetComponent<UIController>().clearPreview();
        foreach (KeyValuePair<Vector2Int, GameObject> pair in unitList)
        {
            pair.Value.GetComponent<UnitController>().destroyMarkers();
        }
        uiCanvas.GetComponent<UIController>().clearButtons();
    }

    void movePrepare(GameObject unit)
    {
        if (activeAbility.getAbilityType() != actionType.MOVE)
        {
            return;
        }
        MovementAbility calculateAbility = activeAbility as MovementAbility;
        if (!unit)
        {
            return;
        }
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMoveMarkers(calculateAbility, MarkerController.Markers.BLUE);
    }

    void moveAction(Vector2 tileDestination)
    {
        UnitController activeController = activeUnit.GetComponent<UnitController>();
        if (activeController.checkActions(activeAbility.getAPCost()))
        {
            return;
        }
        bool clear = moveUnit(activeUnit, tileDestination);
        completeAction(activeUnit);
        if (clear)
        {
            bool done = activeController.useActions(activeAbility.getAPCost());
        }
        checkEndGame();
    }

    void combatPrepare(GameObject unit)
    {
        if (!unit || !activeAbility)
        {
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        actionPhase = actionPhase.PREPARE;
        UnitController targetController = unit.GetComponent<UnitController>();
        int rangeMax = abilityData.getTargetInstruction().getMaxRange();
        if (!targetCondition.getMaxRangeFixed())
        {
            rangeMax += targetController.getEquippedWeapon().getWeaponStats()[3];
        }
        int rangeMin = targetCondition.getMinRange();
        if (!abilityData.getTargetInstruction().getMinRangeFixed())
        {
            rangeMin += targetController.getEquippedWeapon().getWeaponStats()[3];
        }
        Vector2 direction = new Vector2(0, 0);
        if (activeOverlay)
        {
            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetCondition.getLOSRequired(), this, teamLists, direction);
        if (targetCondition.getTargetType() == targetType.SELF)
        {
            targetController.createAttackMarkers(new List<Vector2Int>() { targetController.getUnitPos() }, MarkerController.Markers.RED);
        }
        else
        {
            if (targetCondition.getTargetType() == targetType.BEAM)
            {
                GameObject temp = GameObject.Instantiate(overlayObject, targetController.gameObject.transform);
                activeOverlay = temp;
                activeOverlay.GetComponent<OverlayController>().initalize(rangeMax, true);
            }
            else
            {
                targetController.createAttackMarkers(rangefinder.generateCoordsNotOfTeam(unit.GetComponent<UnitController>().getUnitPos(), getAlignedTeams(activeTeam), targetCondition.getTargetType() == targetType.BEAM), MarkerController.Markers.RED);
            }
        }
    }

    void combatAction(GameObject targetUnit)
    {
        if (!activeAbility)
        {
            completeAction(activeUnit);
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        Vector2 direction = new Vector2(0, 0);
        if (activeOverlay)
        {
            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
        }
        if ((targetCondition.getTargetType() == targetType.TARGET && !targetUnit) || (targetCondition.getTargetType() == targetType.SELF && targetUnit != activeUnit))
        {
            completeAction(activeUnit);
            return;
        }
        if (selectedController.checkActions(activeAbility.getAPCost()))
        {
            completeAction(activeUnit);
            return;
        }
        actionPhase = actionPhase.EXECUTE;
        List<GameObject> hitUnits = getHitUnits(calculateAbility, targetCondition, selectedController, direction);
        foreach(GameObject temp in hitUnits)
        {
            attackUnit(activeUnit, temp);
        }
        completeAction(activeUnit);
        selectedController.useActions(activeAbility.getAPCost());
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
        if (activeAbility.getAbilityType() != actionType.MOVE)
        {
            return false;
        }
        MovementAbility calculateAbility = activeAbility as MovementAbility;
        if (getUnitFromCoords(gridTilePos(coords)) && getUnitFromCoords(gridTilePos(coords)) != gameObject)
        {
            return false;
        }
        Vector2Int key = unit.GetComponent<UnitController>().getUnitPos();
        unitList.Remove(key);
        bool moved = unit.GetComponent<UnitController>().moveUnit(coords, calculateAbility);
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

    public List<GameObject> getHitUnits(CombatAbility calculateAbility, TargetInstruction targetInstruction, UnitController selectedController, Vector2 direction)
    {
        Debug.Log("START");
        if (targetInstruction.getTargetType() == targetType.NONE)
        {
            List<GameObject> target = new List<GameObject>();
            target.Add(selectedController.gameObject);
            Debug.Log("SINGLE: " + target[0].name);
            return target;
        }
        List<int> teamList = getAllTeams();
        Debug.Log("MULTIPLE");
        foreach (TargetFilter filter in targetInstruction.getConditionFilters())
        {
            if (filter.getTargetFilter() == targetFilter.ENEMY)
            {
                Debug.Log("ENEMIES");
                teamList = getNonAlignedTeams(activeTeam);
            }
            else if (filter.getTargetFilter() == targetFilter.ALLY)
            {
                Debug.Log("ALLIES");
                teamList = getAlignedTeams(activeTeam);
            }
        }
        AbilityCalculator calculator = new AbilityCalculator(teamList, calculateAbility, cursorController.getGridPos(), direction);
        List<GameObject> hitUnits = calculator.getAffectedUnits(targetInstruction, selectedController);
        foreach(GameObject temp in hitUnits)
        {
            Debug.Log(temp.name);
        }
        return hitUnits;
    }

    public void attackUnit(GameObject attacker, GameObject defender)
    {
        //CANNOT DO BEAM AOEs
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        AbilityData abilityData = calculateAbility.getAbilityData();
        List<EffectInstruction> effectList = abilityData.getEffectInstructions();
        foreach (EffectInstruction effect in effectList)
        {
            List<GameObject> hitUnits = getHitUnits(calculateAbility, effect.getEffectTarget(), defender.GetComponent<UnitController>(), new Vector2());
            foreach (GameObject target in hitUnits)
            {
                if (applyEffect(attacker, target, effect))
                {
                    killUnit(target);
                    continue;
                }
            }
        }
    }

    public bool applyEffect(GameObject attacker, GameObject defender, EffectInstruction effect)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        bool defeated = false;
        int randomChance = Random.Range(0, 100);
        int[] hitStats = getHitStats(attackerController, defenderController);
        int totalHit = hitStats[0];
        int totalCrit = hitStats[1];
        if (randomChance < totalHit)
        {
            if (attackerController.getEquippedWeapon().getWeaponStatus())
            {
                defenderController.inflictStatus(attackerController.getEquippedWeapon().getWeaponStatus(), attacker);
            }
            if (effect.getEffectType() == effectType.STATUS)
            {
                defenderController.inflictStatus(effect.getEffectStatus(), attacker);
            }
            if (effect.getEffectType() == effectType.DAMAGE)
            {
                defeated = attackerController.attackUnit(defender.GetComponent<UnitController>(), effect, totalCrit);
            }
        }
        return defeated;
    }

    //End Map Management
    void checkEndGame()
    {
        if (mapData.evaluateDefeatConditions(unitList, teamLists, turnNumber))
        {
            mapDefeat();
            return;
        }
        if (mapData.evaluateVictoryConditions(unitList, teamLists, turnNumber))
        {
            mapVictory();
            return;
        }
    }

    void mapVictory()
    {
        Debug.Log("VICTORY");
    }

    void mapDefeat()
    {
        Debug.Log("DEFEAT");
    }

    //Other Helpers
    public Dictionary<int, List<GameObject>> getTeamLists()
    {
        return teamLists;
    }

    public void killUnit(GameObject deadUnit)
    {
        Vector2Int location = deadUnit.GetComponent<UnitController>().getUnitPos();
        unitList.Remove(location);
        int team = deadUnit.GetComponent<UnitController>().getTeam();
        teamLists[team].Remove(deadUnit);
        if (teamLists[team].Count <= 0)
        {
            teamLists.Remove(team);
            eliminatedTeams++;
        }
        checkEndGame();
        GameObject.Destroy(deadUnit);
    }

    List<int> getAlignedTeams(int team)
    {
        foreach (List<int> temp in teamAlignments)
        {
            if (temp.Contains(team))
            {
                return temp;
            }
        }
        return new List<int>();
    }

    List<int> getNonAlignedTeams(int team)
    {
        List<int> unalignedTeams = new List<int>();
        foreach (List<int> temp in teamAlignments)
        {
            if (!temp.Contains(team))
            {
                unalignedTeams.AddRange(temp);
            }
        }
        return unalignedTeams;
    }

    List<int> getAllTeams()
    {
        List<int> allTeams = new List<int>();
        foreach (List<int> temp in teamAlignments)
        {
            allTeams.AddRange(temp);
        }
        return allTeams;
    }

    public Pathfinder getPathfinder()
    {
        return pathfinder;
    }

    public Ability getActiveAbility()
    {
        return activeAbility;
    }

    public CombatAbility getActiveCombatAbility()
    {
        if (activeAbility.getAbilityType() == actionType.COMBAT)
        {
            return activeAbility as CombatAbility;
        }
        return null;
    }

    public MovementAbility getActiveMovementAbility()
    {
        if (activeAbility.getAbilityType() == actionType.MOVE)
        {
            return activeAbility as MovementAbility;
        }
        return null;
    }

    public GameObject getActiveUnit()
    {
        return activeUnit;
    }

    //Stat Math
    public int finalRange(int baseRange, MovementAbility ability)
    {
        return Mathf.FloorToInt((float)(baseRange + ability.getFlatMoveBonus()) * (((float)ability.getPercentMoveBonus() / 100f) + 1f));
    }

    public int[] getHitStats(UnitController attackerController, UnitController defenderController)
    {
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            return null;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetInstruction = abilityData.getTargetInstruction();
        int effectiveHit = (int)(attackerController.getStats()[5] * hitFactor + targetInstruction.getHitBonus());
        if (targetInstruction.getIsMelee())
        {
            effectiveHit = (int)(attackerController.getStats()[6] * hitFactor + targetInstruction.getHitBonus());
        }
        if (attackerController.equippedWeapon)
        {
            effectiveHit += attackerController.equippedWeapon.getWeaponStats()[1];
        }
        float rawAvoid;
        if (!abilityData.getTargetInstruction().getIsMelee())
        {
            if (defenderController.getStats()[7] < reactThreshold)
            {
                rawAvoid = 0;
            }
            else
            {
                rawAvoid = defenderController.getStats()[5] + (defenderController.getStats()[7] - reactThreshold);
            }
        }
        else
        {
            rawAvoid = defenderController.getStats()[6] + defenderController.getStats()[7];
        }
        int effectiveAvoid = (int)(rawAvoid * avoidFactor);
        int effectiveCrit;
        if (abilityData.getTargetInstruction().getIsMelee())
        {
            //Melee - Use acuity
            effectiveCrit = attackerController.getStats()[5];
        }
        else
        {
            //Ranged - Use finesse
            effectiveCrit = attackerController.getStats()[6];
        }
        int totalHit = effectiveHit - effectiveAvoid;
        int totalCrit = effectiveCrit - effectiveAvoid;
        return new int[] { totalHit, totalCrit };
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
        return new Vector2(localCoords.x + mainGrid.cellSize.x / 2, localCoords.y + mainGrid.cellSize.y / 2);
    }

    public Vector2 tileGridPos(Vector2Int gridPos)
    {
        return new Vector2((float)gridPos.x - mainGrid.cellSize.x / 2, (float)gridPos.y - mainGrid.cellSize.y / 2);
    }

    public Vector2 tileGridPos(Vector2 gridPos)
    {
        return new Vector2((float)gridPos.x - mainGrid.cellSize.x / 2, (float)gridPos.y - mainGrid.cellSize.y / 2);
    }

    public Vector2Int gridTilePos(Vector2 tilePos)
    {
        float tileX = tilePos.x + mainGrid.cellSize.x / 2;
        float tileY = tilePos.y + mainGrid.cellSize.y / 2;
        return new Vector2Int(Mathf.CeilToInt(tileX), Mathf.CeilToInt(tileY));
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
