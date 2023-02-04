using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    //Special Constants
    public static int reactThreshold = 10; //Minimum threshold for evasion bonuses for ranged attacks. No threshold for melee attacks.
    public static float hitFactor = 2;
    public static float avoidFactor = 1;
    public static float critFactor = 2;

    //Main Variabless
    Dictionary<Vector2Int, GameObject> unitList = new Dictionary<Vector2Int, GameObject>();
    Dictionary<battleTeam, List<GameObject>> teamLists = new Dictionary<battleTeam, List<GameObject>>();
    battleTeam activeTeam;
    battleTeam firstTeam;
    int turnNumber = 1;

    MapController mapController;
    Canvas uiCanvas;
    GameObject cursor;
    MouseController cursorController;
    Pathfinder pathfinder;

    GameObject activeOverlay;
    List<GameObject> selectedGameObjectTargets = new List<GameObject>();
    List<Vector2Int> selectedTileTargets = new List<Vector2Int>();
    List<Vector2> selectedPointTargets = new List<Vector2>();

    turnPhase turnPhase = turnPhase.START;
    actionPhase actionPhase = actionPhase.INACTIVE;

    //Action Handlers
    actionType actionState = actionType.NONE;
    Ability activeAbility;
    GameObject activeUnit;

    //Public Objects
    public GameObject overlayObject;
    public MapData mapData;
    public GameObject unitTemplate;

    void Awake()
    {
        mapController = gameObject.GetComponent<MapController>();
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        cursor = GameObject.FindGameObjectWithTag("Cursor");
        cursorController = cursor.GetComponent<MouseController>();
        //teamAlignments = mapData.getTeamAlignments();
    }

    // Start is called before the first frame update
    void Start()
    {
        turnPhase = turnPhase.START;
        StartCoroutine(bannerTimer());
        //TODO: Change to iterate on enum?
        teamLists.Add(battleTeam.PLAYER, new List<GameObject>());
        teamLists.Add(battleTeam.ENEMY, new List<GameObject>());
        teamLists.Add(battleTeam.ALLY, new List<GameObject>());
        teamLists.Add(battleTeam.NEUTRAL, new List<GameObject>());
    }

    //Use this to save key data upon victory
    void OnDisable()
    {
        foreach (UnitEntryData unit in BattleEntryHandler.deployedUnits.Keys)
        {
            BattleExitHandler.unitData.Add(unit);
        }
        BattleEntryHandler.reset();
        Cursor.visible = true;
    }

    //Use this to load key data upon start
    void OnEnable()
    {
        BattleExitHandler.reset();
        foreach (UnitEntryData player in BattleEntryHandler.deployedUnits.Keys)
        {
            placeUnit(player, BattleEntryHandler.deployedUnits[player], battleTeam.PLAYER);
        }
        foreach (UnitEntryData enemy in BattleEntryHandler.enemyPlacements.Keys)
        {
            placeUnit(enemy, BattleEntryHandler.enemyPlacements[enemy], battleTeam.ENEMY);
        }
        //TODO: ADD ALLY FOREACH
        activeTeam = battleTeam.PLAYER;
        firstTeam = battleTeam.PLAYER;
    }

    //Turn Management
    void nextPhase()
    {
        turnPhase = turnPhase.END;
        uiCanvas.GetComponent<UIController>().resetButtons();
        completeAction();
        if (teamLists.ContainsKey(activeTeam))
        {
            List<GameObject> active = teamLists[activeTeam];
            for (int i = active.Count - 1; i >= 0; i--)
            {
                GameObject unit = active[i];
                bool dead = unit.GetComponent<UnitController>().endUnitTurn();
                if (dead)
                {
                    killUnit(unit);
                }
            }
        }
        activeTeam = nextTeam();
        if (activeTeam == firstTeam)
        {
            turnNumber++;
        }
        checkEndGame();
        turnPhase = turnPhase.START;
        StartCoroutine(bannerTimer());
        if (activeTeam != battleTeam.PLAYER)
        {
            StartCoroutine(runAIPhase(teamLists[activeTeam]));
        }
    }

    IEnumerator runAIPhase(List<GameObject> units)
    {
        while (turnPhase != turnPhase.MAIN)
        {
            yield return new WaitForSeconds(0.1f);
        }
        foreach (GameObject active in units)
        {
            while (uiCanvas.GetComponent<UIController>().hasAnimation())
            {
                yield return new WaitForSeconds(0.1f);
            }
            int cheapest = int.MaxValue;
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
            while (active.GetComponent<UnitController>().getActions()[1] >= cheapest)
            {
                while (uiCanvas.GetComponent<UIController>().hasAnimation())
                {
                    yield return new WaitForSeconds(0.1f);
                }
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
                        completeAction();
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
                        if (targetInstruction.getIsMelee())
                        {
                            rangeMax = 1;
                            rangeMin = 1;
                        }
                        Vector2 direction = new Vector2(0, 0);
                        if (activeOverlay)
                        {
                            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
                        }
                        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetInstruction.getLOSRequired(), mapController, this, teamLists, direction);
                        List<GameObject> validTargets = rangefinder.generateTargetsNotOfTeam(activeUnit.GetComponent<UnitController>().getUnitPos(), activeTeam, targetInstruction.getTargetType() == targetType.BEAM);
                        if (validTargets.Count <= 0)
                        {
                            active.GetComponent<AIController>().disableActions(activeAbility);
                            continue;
                        }
                        if (targetInstruction.getTargetCondition() == targetCondition.SELECTED)
                        {
                            switch (targetInstruction.getTargetType())
                            {
                                case targetType.POINT:
                                    Debug.Log("Unimplemented Selected Target for AI");
                                    break;

                                case targetType.TILE:
                                    Debug.Log("Unimplemented Selected Target for AI");
                                    break;

                                case targetType.TARGET:
                                    while (selectedGameObjectTargets.Count < targetInstruction.getTargetConditionCount())
                                    {
                                        GameObject target = activeUnit.GetComponent<AIController>().getGameObjectTarget(selectedAbility as CombatAbility, validTargets);
                                        if (target)
                                        {
                                            combatTargeting(target, targetInstruction, new Vector2(0, 0));
                                            break;
                                        }
                                    }
                                    break;

                                default:
                                    Debug.Log("Unimplemented/Invalid Selected Target for AI");
                                    break;
                            }
                        }
                        else
                        {
                            GameObject target = activeUnit.GetComponent<AIController>().getGameObjectTarget(selectedAbility as CombatAbility, validTargets);
                            if (target)
                            {
                                combatTargeting(target, targetInstruction, new Vector2(0, 0));
                            }
                        }
                    }
                }
                else if (activeAbility.getAbilityType() == actionType.MOVE)
                {
                    executeAction(activeUnit, mapController.tileGridPos(activeUnit.GetComponent<AIController>().getMoveTarget(activeAbility as MovementAbility)));
                }
                yield return new WaitForSeconds(0.5f);
            }
            active.GetComponent<AIController>().resetActions();
        }
        yield return new WaitUntil(() => turnPhase == turnPhase.MAIN);
        nextPhase();
    }

    public battleTeam getActiveTeam()
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

    public void setTurnPhase(turnPhase phase)
    {
        turnPhase = phase;
    }

    public actionPhase getActionPhase()
    {
        return actionPhase;
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
        completeAction();
    }

    //Action Handling
    public void completeAction()
    {
        selectedGameObjectTargets.Clear();
        selectedPointTargets.Clear();
        selectedTileTargets.Clear();
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
        actionPhase = actionPhase.PREPARE;
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
        completeAction();
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
        if (targetCondition.getIsMelee())
        {
            rangeMax = 1;
            rangeMin = 1;
        }
        Vector2 direction = new Vector2(0, 0);
        if (activeOverlay)
        {
            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetCondition.getLOSRequired(), mapController, this, teamLists, direction);
        if (targetCondition.getTargetType() == targetType.SELF)
        {
            targetController.createAttackMarkers(new List<Vector2Int>() { targetController.getUnitPos() }, MarkerController.Markers.RED);
        }
        else if (targetCondition.getTargetType() == targetType.BEAM)
        {
            GameObject temp = GameObject.Instantiate(overlayObject, targetController.gameObject.transform);
            activeOverlay = temp;
            activeOverlay.GetComponent<OverlayController>().initalize(rangeMax, true);
        }
        else
        {
            bool noFilter = true;
            bool enemyFilter = false;
            bool allyFilter = false;
            foreach (TargetFilter filter in targetCondition.getConditionFilters())
            {
                if (filter.getTargetFilter() == targetFilter.ENEMY && !enemyFilter)
                {
                    targetController.createAttackMarkers(rangefinder.generateCoordsNotOfTeam(unit.GetComponent<UnitController>().getUnitPos(), activeTeam, targetCondition.getTargetType() == targetType.BEAM), MarkerController.Markers.RED);
                    enemyFilter = true;
                    noFilter = false;
                }
                if (filter.getTargetFilter() == targetFilter.ALLY && !allyFilter)
                {
                    targetController.createAttackMarkers(rangefinder.generateCoordsOfTeam(unit.GetComponent<UnitController>().getUnitPos(), activeTeam, targetCondition.getTargetType() == targetType.BEAM), MarkerController.Markers.GREEN);
                    allyFilter = true;
                    noFilter = false;
                }
            }
            if (noFilter)
            {
                targetController.createAttackMarkers(rangefinder.generateCoordsNotOfTeam(unit.GetComponent<UnitController>().getUnitPos(), battleTeam.NEUTRAL, targetCondition.getTargetType() == targetType.BEAM), MarkerController.Markers.RED);
            }
        }

    }

    void combatAction(GameObject targetUnit)
    {
        if (!activeAbility)
        {
            completeAction();
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            completeAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        if ((targetCondition.getTargetType() == targetType.TARGET && !targetUnit) || (targetCondition.getTargetType() == targetType.SELF && targetUnit != activeUnit))
        {
            completeAction();
            return;
        }
        if (selectedController.checkActions(activeAbility.getAPCost()))
        {
            completeAction();
            return;
        }
        actionPhase = actionPhase.EXECUTE;
        //TEMPORARY CODE - WILL BREAK BEAMS
        List<CombatData> data = new List<CombatData>();
        foreach (GameObject target in selectedGameObjectTargets)
        {
            for (int i = 0; i < abilityData.getAbilityRepeats(); i++)
            {
                UnitController attackerController = activeUnit.GetComponent<UnitController>();
                UnitController defenderController = target.GetComponent<UnitController>();
                int randomChance = Random.Range(0, 100);
                int[] hitStats = getHitStats(attackerController, defenderController, abilityData.getTargetInstruction());
                if (randomChance < hitStats[0])
                {
                    if (attackerController.getEquippedWeapon().getWeaponStatus())
                    {
                        data.Add(defenderController.inflictStatus(attackerController.getEquippedWeapon().getWeaponStatus(), attackerController.gameObject));
                    }
                    List<CombatData> subResults = attackUnit(activeUnit, target, true);
                    if (subResults != null)
                    {
                        data.AddRange(subResults);
                    }
                }
                else
                {
                    List<CombatData> subResults = attackUnit(activeUnit, target, false);
                    if (subResults != null)
                    {
                        data.AddRange(subResults);
                    }
                }
            }
        }
        startAnimation(data);
        completeAction();
        selectedController.useActions(activeAbility.getAPCost());
    }

    //Targeting Functions
    public void combatTargeting(GameObject targetUnit, TargetInstruction targetInstruction, Vector2 tilePosition)
    {
        switch (targetInstruction.getTargetCondition())
        {
            case targetCondition.RANDOMDUPE:
                switch (targetInstruction.getTargetType())
                {
                    case targetType.TARGET:
                        randomUnitTargets(true);
                        executeAction(targetUnit, tilePosition);
                        break;

                    case targetType.BEAM:
                        Debug.Log("Invalid TargetType - Beam and RandomDupe are incompatible");
                        break;

                    case targetType.POINT:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.PROJECTILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.NONE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.SELF:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.TILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    default:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;
                }
                break;

            case targetCondition.RANDOMUNIQUE:
                switch (targetInstruction.getTargetType())
                {
                    case targetType.TARGET:
                        randomUnitTargets(false);
                        executeAction(targetUnit, tilePosition);
                        break;

                    case targetType.BEAM:
                        Debug.Log("Invalid TargetType - Beam and RandomUnique are incompatible");
                        break;

                    case targetType.POINT:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.PROJECTILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.NONE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.SELF:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.TILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    default:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;
                }
                break;

            case targetCondition.SELECTED:
                switch (targetInstruction.getTargetType())
                {
                    case targetType.TARGET:
                        if (targetUnit)
                        {
                            addUnitToTargets(targetUnit);
                            if (getGameObjectTargets().Count >= getActiveCombatAbility().getAbilityData().getTargetInstruction().getTargetConditionCount())
                            {
                                executeAction(targetUnit, tilePosition);
                            }
                        }
                        break;

                    case targetType.BEAM:
                        if (activeOverlay)
                        {
                            AbilityCalculator calculator = new AbilityCalculator(getFilterTeam(targetInstruction, activeTeam), getActiveCombatAbility(), activeUnit.GetComponent<UnitController>().getUnitPos(), activeOverlay.GetComponent<OverlayController>().getOverlayDirection());
                            List<GameObject> targetLine = calculator.getAffectedUnits(targetInstruction, activeUnit.GetComponent<UnitController>());
                            foreach (GameObject target in targetLine)
                            {
                                addUnitToTargets(target);
                            }
                            executeAction(targetUnit, tilePosition);
                        }
                        break;

                    case targetType.POINT:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.PROJECTILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.NONE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.SELF:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.TILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    default:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;
                }
                break;

            case targetCondition.ALL:
                switch (targetInstruction.getTargetType())
                {
                    case targetType.TARGET:
                        allUnitTargets();
                        executeAction(targetUnit, tilePosition);
                        break;

                    case targetType.BEAM:
                        Debug.Log("Invalid TargetType - Beam and All are incompatible");
                        break;

                    case targetType.POINT:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.PROJECTILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.NONE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.SELF:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case targetType.TILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    default:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;
                }
                break;
        }
    }

    public void addUnitToTargets(GameObject addition)
    {
        if (!activeAbility)
        {
            completeAction();
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            completeAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        int rangeMax = abilityData.getTargetInstruction().getMaxRange();
        if (!targetCondition.getMaxRangeFixed())
        {
            rangeMax += selectedController.getEquippedWeapon().getWeaponStats()[3];
        }
        int rangeMin = targetCondition.getMinRange();
        if (!abilityData.getTargetInstruction().getMinRangeFixed())
        {
            rangeMin += selectedController.getEquippedWeapon().getWeaponStats()[3];
        }
        if (targetCondition.getIsMelee())
        {
            rangeMax = 1;
            rangeMin = 1;
        }
        Vector2 direction = new Vector2(0, 0);
        if (activeOverlay)
        {
            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetCondition.getLOSRequired(), mapController, this, teamLists, direction);
        bool noFilter = true;
        bool enemyFilter = false;
        bool allyFilter = false;
        foreach (TargetFilter filter in targetCondition.getConditionFilters())
        {
            if (filter.getTargetFilter() == targetFilter.ENEMY && !enemyFilter)
            {
                if (rangefinder.generateTargetsNotOfTeam(selectedController.getUnitPos(), activeTeam, targetCondition.getTargetType() == targetType.BEAM).Contains(addition))
                {
                    selectedGameObjectTargets.Add(addition);
                }
                enemyFilter = true;
                noFilter = false;
            }
            if (filter.getTargetFilter() == targetFilter.ALLY && !allyFilter)
            {
                if (rangefinder.generateTargetsOfTeam(selectedController.getUnitPos(), activeTeam, targetCondition.getTargetType() == targetType.BEAM).Contains(addition))
                {
                    selectedGameObjectTargets.Add(addition);
                }
                allyFilter = true;
                noFilter = false;
            }
        }
        if (noFilter)
        {
            if (rangefinder.generateTargetsOfTeam(selectedController.getUnitPos(), battleTeam.NEUTRAL, targetCondition.getTargetType() == targetType.BEAM).Contains(addition))
            {
                selectedGameObjectTargets.Add(addition);
            }
        }
    }

    public void addTileToTargets(Vector2Int tile)
    {

    }

    public void addPointToTargets(Vector2 position)
    {

    }

    public void randomUnitTargets(bool duplicatesAllowed)
    {
        if (!activeAbility)
        {
            completeAction();
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            completeAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        int rangeMax = abilityData.getTargetInstruction().getMaxRange();
        if (!targetCondition.getMaxRangeFixed())
        {
            rangeMax += selectedController.getEquippedWeapon().getWeaponStats()[3];
        }
        int rangeMin = targetCondition.getMinRange();
        if (!abilityData.getTargetInstruction().getMinRangeFixed())
        {
            rangeMin += selectedController.getEquippedWeapon().getWeaponStats()[3];
        }
        if (targetCondition.getIsMelee())
        {
            rangeMax = 1;
            rangeMin = 1;
        }
        Vector2 direction = new Vector2(0, 0);
        if (activeOverlay)
        {
            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetCondition.getLOSRequired(), mapController, this, teamLists, direction);
        List<GameObject> potentialTargets = rangefinder.generateTargetsNotOfTeam(selectedController.getUnitPos(), activeTeam, false);
        if (duplicatesAllowed)
        {
            while (selectedGameObjectTargets.Count <= targetCondition.getTargetConditionCount())
            {
                selectedGameObjectTargets.Add(potentialTargets[Random.Range(0, potentialTargets.Count)]);
            }
        }
        else
        {
            if (potentialTargets.Count <= targetCondition.getTargetConditionCount())
            {
                selectedGameObjectTargets.AddRange(potentialTargets);
            }
            else
            {
                while (selectedGameObjectTargets.Count <= targetCondition.getTargetConditionCount())
                {
                    GameObject selected = potentialTargets[Random.Range(0, potentialTargets.Count)];
                    selectedGameObjectTargets.Add(selected);
                    potentialTargets.Remove(selected);
                }
            }
        }
    }

    public void randomTileTargets(bool duplicatesAllowed)
    {

    }

    public void randomPointTargets(bool duplicatesAllowed)
    {

    }

    public void allUnitTargets()
    {
        if (!activeAbility)
        {
            completeAction();
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            completeAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        int rangeMax = abilityData.getTargetInstruction().getMaxRange();
        if (!targetCondition.getMaxRangeFixed())
        {
            rangeMax += selectedController.getEquippedWeapon().getWeaponStats()[3];
        }
        int rangeMin = targetCondition.getMinRange();
        if (!abilityData.getTargetInstruction().getMinRangeFixed())
        {
            rangeMin += selectedController.getEquippedWeapon().getWeaponStats()[3];
        }
        if (targetCondition.getIsMelee())
        {
            rangeMax = 1;
            rangeMin = 1;
        }
        Vector2 direction = new Vector2(0, 0);
        if (activeOverlay)
        {
            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetCondition.getLOSRequired(), mapController, this, teamLists, direction);
        List<GameObject> potentialTargets = rangefinder.generateTargetsNotOfTeam(selectedController.getUnitPos(), activeTeam, false);
        selectedGameObjectTargets.AddRange(potentialTargets);
    }

    public void allTileTargets()
    {

    }

    public List<GameObject> getGameObjectTargets()
    {
        return selectedGameObjectTargets;
    }

    public List<Vector2Int> getTileTargets()
    {
        return selectedTileTargets;
    }

    public List<Vector2> getPointTargets()
    {
        return selectedPointTargets;
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
        battleTeam team = unit.GetComponent<UnitController>().getTeam();
        teamLists[team].Add(unit);
    }

    public bool moveUnit(GameObject unit, Vector2 coords)
    {
        if (activeAbility.getAbilityType() != actionType.MOVE)
        {
            return false;
        }
        MovementAbility calculateAbility = activeAbility as MovementAbility;
        if (getUnitFromCoords(mapController.gridTilePos(coords)) && getUnitFromCoords(mapController.gridTilePos(coords)) != gameObject)
        {
            return false;
        }
        Vector2Int key = unit.GetComponent<UnitController>().getUnitPos();
        unitList.Remove(key);
        bool moved = unit.GetComponent<UnitController>().moveUnit(coords, calculateAbility);
        if (moved)
        {
            unitList.Add(mapController.gridTilePos(coords), unit);
        }
        else
        {
            unitList.Add(key, unit);
        }
        return moved;
    }

    public List<GameObject> getHitUnits(CombatAbility calculateAbility, TargetInstruction targetInstruction, UnitController selectedController, Vector2 direction)
    {
        if (targetInstruction.getTargetType() == targetType.NONE)
        {
            List<GameObject> target = new List<GameObject>();
            target.Add(selectedController.gameObject);
            return target;
        }
        AbilityCalculator calculator = new AbilityCalculator(getFilterTeam(targetInstruction, activeTeam), calculateAbility, cursorController.getGridPos(), direction);
        List<GameObject> hitUnits = calculator.getAffectedUnits(targetInstruction, selectedController);
        return hitUnits;
    }

    public List<CombatData> attackUnit(GameObject attacker, GameObject defender, bool overallHit)
    {
        List<CombatData> retVal = new List<CombatData>();
        //CANNOT DO BEAM AOEs
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            return null;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        AbilityData abilityData = calculateAbility.getAbilityData();
        List<EffectInstruction> effectList = abilityData.getEffectInstructions();
        foreach (EffectInstruction effect in effectList)
        {
            List<GameObject> hitUnits = getHitUnits(calculateAbility, effect.getEffectTarget(), defender.GetComponent<UnitController>(), new Vector2());
            foreach (GameObject target in hitUnits)
            {
                CombatData result = applyEffect(attacker, target, effect, overallHit);
                retVal.Add(result);
            }
        }
        return retVal;
    }

    public CombatData applyEffect(GameObject attacker, GameObject defender, EffectInstruction effect, bool overallHit)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        int randomChance = Random.Range(0, 100);
        int[] hitStats = getHitStats(attackerController, defenderController, effect.getEffectTarget());
        int totalHit = hitStats[0];
        int totalCrit = hitStats[1];
        CombatData result = new CombatData(attacker, defender, effect, false, false, 0, attacker.GetComponent<UnitController>().getStats()[2], false);
        if (!overallHit)
        {
            if (effect.getEffectType() == effectType.STATUS)
            {
                result = new CombatData(attacker, defender, effect.getEffectStatus(), false);
                return result;
            }
            if (effect.getEffectType() == effectType.DAMAGE)
            {
                result = new CombatData(attacker, defender, effect, false, false, 0, attacker.GetComponent<UnitController>().getStats()[2], false);
                return result;
            }
        }
        if (randomChance < totalHit || !effect.getIndependentHit())
        {
            if (effect.getEffectType() == effectType.STATUS)
            {
                result = defenderController.inflictStatus(effect.getEffectStatus(), attacker);
            }
            if (effect.getEffectType() == effectType.DAMAGE)
            {
                result = attackerController.attackUnit(defender.GetComponent<UnitController>(), effect, totalCrit);
            }
            if (effect.getEffectType() == effectType.HEAL)
            {
                attackerController.healUnit(defender.GetComponent<UnitController>(), effect);
            }
        }
        else
        {
            if (effect.getEffectType() == effectType.STATUS)
            {
                result = new CombatData(attacker, defender, effect.getEffectStatus(), false);
                return result;
            }
            if (effect.getEffectType() == effectType.DAMAGE)
            {
                result = new CombatData(attacker, defender, effect, false, false, 0, attacker.GetComponent<UnitController>().getStats()[2], false);
                return result;
            }
        }
        return result;
    }

    //End Map Management
    void checkEndGame()
    {
        if (evaluatePlayerRout())
        {
            BattleExitHandler.outcome = battleOutcome.ROUTED;
            mapEnd();
            return;
        }
        if (evaluateEnemyRout())
        {
            BattleExitHandler.outcome = battleOutcome.VICTORY;
            mapEnd();
            return;
        }
        if (mapData.evaluateDefeatConditions(unitList, turnNumber))
        {
            BattleExitHandler.outcome = battleOutcome.FAILURE;
            mapEnd();
            return;
        }
        if (mapData.evaluateVictoryConditions(unitList, turnNumber))
        {
            BattleExitHandler.outcome = battleOutcome.SUCCESS;
            mapEnd();
            return;
        }
    }

    bool evaluatePlayerRout()
    {
        return teamLists[battleTeam.ALLY].Count == 0 && teamLists[battleTeam.PLAYER].Count == 0;
    }

    bool evaluateEnemyRout()
    {
        return teamLists[battleTeam.ENEMY].Count == 0;
    }

    void mapEnd()
    {
        BattleExitHandler.turn_count = turnNumber;
        StopAllCoroutines();
        SceneManager.LoadSceneAsync("BattleEnd");
    }

    //Other Helpers
    public int getBattleTeamCount()
    {
        return System.Enum.GetNames(typeof(battleTeam)).Length;
    }
    public GameObject getUnitFromCoords(Vector2Int coords)
    {
        if (unitList.ContainsKey(coords))
        {
            return unitList[coords];
        }
        return null;
    }

    public void killDead(List<CombatData> combatResults)
    {
        foreach (CombatData data in combatResults)
        {
            if (data.getDefenderKilled() && unitList.ContainsKey(data.getDefender().GetComponent<UnitController>().getUnitPos()))
            {
                killUnit(data.getDefender());
            }
        }
    }

    public Dictionary<battleTeam, List<GameObject>> getTeamLists()
    {
        return teamLists;
    }

    public void killUnit(GameObject deadUnit)
    {
        Vector2Int location = deadUnit.GetComponent<UnitController>().getUnitPos();
        unitList.Remove(location);
        battleTeam team = deadUnit.GetComponent<UnitController>().getTeam();
        teamLists[team].Remove(deadUnit);
        checkEndGame();
        GameObject.Destroy(deadUnit);
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

    public GameObject getActiveOverlay()
    {
        return activeOverlay;
    }

    public battleTeam getFilterTeam(TargetInstruction instruction, battleTeam actingTeam)
    {
        foreach (TargetFilter filter in instruction.getConditionFilters())
        {
            if (filter.getTargetFilter() == targetFilter.ENEMY)
            {
                if (actingTeam == battleTeam.ENEMY)
                {
                    return battleTeam.PLAYER;
                } else if (actingTeam == battleTeam.PLAYER || actingTeam == battleTeam.ALLY)
                {
                    return battleTeam.ENEMY;
                }
            } else if (filter.getTargetFilter() == targetFilter.ALLY)
            {
                if (actingTeam == battleTeam.PLAYER || actingTeam == battleTeam.ALLY)
                {
                    return battleTeam.PLAYER;
                }
                else if (actingTeam == battleTeam.ENEMY)
                {
                    return battleTeam.ENEMY;
                }
            }
        }
        return battleTeam.NEUTRAL;
    }

    //Stat Math
    public int finalRange(int baseRange, MovementAbility ability)
    {
        return Mathf.FloorToInt((baseRange + ability.getFlatMoveBonus()) * ((ability.getPercentMoveBonus() / 100f) + 1f));
    }

    public int[] getHitStats(UnitController attackerController, UnitController defenderController, TargetInstruction targetInstruction)
    {
        int effectiveHit;
        int effectiveAvoid;
        int totalHit;

        //Always calculate avoid for crit
        float rawAvoid;
        if (!targetInstruction.getIsMelee())
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
        effectiveAvoid = (int)(rawAvoid * avoidFactor);

        //Calculate hit differently if fixed hit
        if (targetInstruction.getFixedHit())
        {
            effectiveHit = targetInstruction.getHitBonus();
            totalHit = effectiveHit;
        }
        else
        {
            effectiveHit = (int)(attackerController.getStats()[5] * hitFactor + targetInstruction.getHitBonus());
            if (targetInstruction.getIsMelee())
            {
                effectiveHit = (int)(attackerController.getStats()[6] * hitFactor + targetInstruction.getHitBonus());
            }
            if (attackerController.equippedWeapon)
            {
                effectiveHit += attackerController.equippedWeapon.getWeaponStats()[1];
            }
            totalHit = effectiveHit - effectiveAvoid;
        }

        //Calculate crit independently
        int effectiveCrit;
        if (targetInstruction.getIsMelee())
        {
            //Melee - Use acuity
            effectiveCrit = attackerController.getStats()[5];
        }
        else
        {
            //Ranged - Use finesse
            effectiveCrit = attackerController.getStats()[6];
        }
        int totalCrit = effectiveCrit - effectiveAvoid;
        return new int[] { totalHit, totalCrit };
    }

    //Turn Control
    battleTeam nextTeam()
    {
        switch (activeTeam)
        {
            case battleTeam.PLAYER:
                return battleTeam.ENEMY;

            case battleTeam.ENEMY:
                return battleTeam.ALLY;

            case battleTeam.ALLY:
                return battleTeam.PLAYER;

            default:
                return battleTeam.PLAYER;
        }
    }

    //Unit Construction
    void placeUnit(UnitEntryData unit, Vector2Int tile, battleTeam team)
    {
        GameObject temp = GameObject.Instantiate(unitTemplate);
        temp.SetActive(false);
        temp.GetComponent<UnitController>().setUnitInstance(unit.getUnit());
        temp.GetComponent<UnitController>().equippedWeapon = unit.getWeapon();
        temp.GetComponent<UnitController>().equippedArmor = unit.getArmor();
        temp.GetComponent<UnitController>().team = team;
        temp.GetComponent<SpriteRenderer>().sprite = unit.getUnit().getBattleSprite("attack");
        Vector2 unitPos = mapController.tileGridPos(tile);
        temp.GetComponent<Transform>().position = new Vector3(unitPos.x, unitPos.y, -2);
        temp.SetActive(true);
    }

    //Animation Control Functions
    public void startAnimation(List<CombatData> combatSequence)
    {
        turnPhase = turnPhase.PAUSE;
        uiCanvas.GetComponent<UIController>().displayBattleAnimation(combatSequence);
        StartCoroutine(postAnimCleanup(combatSequence));
    }

    IEnumerator postAnimCleanup(List<CombatData> combatSequence)
    {
        yield return new WaitWhile(() => uiCanvas.GetComponent<UIController>().hasAnimation());
        killDead(combatSequence);
        turnPhase = turnPhase.MAIN;
    }
}