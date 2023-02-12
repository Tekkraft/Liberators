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
        StartCoroutine(BannerTimer());
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
            unit.doPostBattleHealing();
            BattleExitHandler.unitData.Add(unit);
        }
        foreach (UnitEntryData unit in BattleEntryHandler.enemyPlacements.Keys)
        {
            unit.doPostBattleHealing();
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
            PlaceUnit(player, BattleEntryHandler.deployedUnits[player], battleTeam.PLAYER);
        }
        foreach (UnitEntryData enemy in BattleEntryHandler.enemyPlacements.Keys)
        {
            PlaceUnit(enemy, BattleEntryHandler.enemyPlacements[enemy], battleTeam.ENEMY);
        }
        //TODO: ADD ALLY FOREACH
        activeTeam = battleTeam.PLAYER;
        firstTeam = battleTeam.PLAYER;
    }

    //Turn Management
    void NextPhase()
    {
        turnPhase = turnPhase.END;
        uiCanvas.GetComponent<UIController>().resetButtons();
        CompleteAction();
        if (teamLists.ContainsKey(activeTeam))
        {
            List<GameObject> active = teamLists[activeTeam];
            for (int i = active.Count - 1; i >= 0; i--)
            {
                GameObject unit = active[i];
                bool dead = unit.GetComponent<UnitController>().endUnitTurn();
                if (dead)
                {
                    KillUnit(unit);
                }
            }
        }
        activeTeam = NextTeam();
        if (activeTeam == firstTeam)
        {
            turnNumber++;
        }
        CheckEndGame();
        turnPhase = turnPhase.START;
        StartCoroutine(BannerTimer());
        if (activeTeam != battleTeam.PLAYER)
        {
            StartCoroutine(RunAIPhase(teamLists[activeTeam]));
        }
    }

    IEnumerator RunAIPhase(List<GameObject> units)
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
                SetActionState(active, selectedAbility);
                if (activeAbility.getAbilityType() == actionType.COMBAT)
                {
                    AbilityData selectedData = (selectedAbility as CombatAbility).getAbilityData();
                    //TEMPORARY MEASURE - REMOVE WHEN BEAMS VALID ABILITY
                    if (selectedData.getTargetInstruction().getTargetType() == targetType.BEAM)
                    {
                        CompleteAction();
                        continue;
                    }
                    else
                    {
                        TargetInstruction targetInstruction = selectedData.getTargetInstruction();
                        UnitController targetController = activeUnit.GetComponent<UnitController>();
                        int rangeMax = targetInstruction.getMaxRange();
                        if (!targetInstruction.getMaxRangeFixed())
                        {
                            rangeMax += targetController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
                        }
                        int rangeMin = targetInstruction.getMinRange();
                        if (!targetInstruction.getMinRangeFixed())
                        {
                            rangeMax += targetController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
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
                                            CombatTargeting(target, targetInstruction, new Vector2(0, 0));
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
                                CombatTargeting(target, targetInstruction, new Vector2(0, 0));
                            }
                        }
                    }
                }
                else if (activeAbility.getAbilityType() == actionType.MOVE)
                {
                    ExecuteAction(activeUnit, mapController.tileGridPos(activeUnit.GetComponent<AIController>().getMoveTarget(activeAbility as MovementAbility)));
                }
                yield return new WaitForSeconds(0.5f);
            }
            active.GetComponent<AIController>().resetActions();
        }
        yield return new WaitUntil(() => turnPhase == turnPhase.MAIN);
        NextPhase();
    }

    public battleTeam GetActiveTeam()
    {
        return activeTeam;
    }

    IEnumerator BannerTimer()
    {
        int bannerTime = 15;
        uiCanvas.GetComponent<UIController>().changeBanner("Team " + activeTeam, bannerTime);
        for (int i = 0; i < bannerTime; i++)
        {
            yield return new WaitForSeconds(0.1f);
        }
        turnPhase = turnPhase.MAIN;
    }

    public turnPhase GetTurnPhase()
    {
        return turnPhase;
    }

    public void SetTurnPhase(turnPhase phase)
    {
        turnPhase = phase;
    }

    public actionPhase GetActionPhase()
    {
        return actionPhase;
    }

    //Action Management
    public void SetActionState(GameObject unit, Ability ability)
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
            NextPhase();
        }
        if (!unit)
        {
            return;
        }
        activeAbility = ability;
        activeUnit = unit;
        ActionSetup(unit);
    }

    public actionType GetActionState()
    {
        return actionState;
    }

    public void ActionSetup(GameObject targetUnit)
    {
        switch (actionState)
        {
            case actionType.MOVE:
                MovePrepare(targetUnit);
                break;

            case actionType.COMBAT:
                CombatPrepare(targetUnit);
                break;
        }
    }

    public void ExecuteAction(GameObject targetUnit, Vector2 tileDestination)
    {
        switch (actionState)
        {
            case actionType.MOVE:
                MoveAction(tileDestination);
                break;

            case actionType.COMBAT:
                CombatAction(targetUnit);
                break;
        }
        CompleteAction();
    }

    //Action Handling
    public void CompleteAction()
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

    void MovePrepare(GameObject unit)
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

    void MoveAction(Vector2 tileDestination)
    {
        UnitController activeController = activeUnit.GetComponent<UnitController>();
        if (activeController.checkActions(activeAbility.getAPCost()))
        {
            return;
        }
        bool clear = MoveUnit(activeUnit, tileDestination);
        CompleteAction();
        if (clear)
        {
            bool done = activeController.useActions(activeAbility.getAPCost());
        }
        CheckEndGame();
    }

    void CombatPrepare(GameObject unit)
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
            rangeMax += targetController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
        }
        int rangeMin = targetCondition.getMinRange();
        if (!abilityData.getTargetInstruction().getMinRangeFixed())
        {
            rangeMin += targetController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
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

    void CombatAction(GameObject targetUnit)
    {
        if (!activeAbility)
        {
            CompleteAction();
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            CompleteAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        if ((targetCondition.getTargetType() == targetType.TARGET && !targetUnit) || (targetCondition.getTargetType() == targetType.SELF && targetUnit != activeUnit))
        {
            CompleteAction();
            return;
        }
        if (selectedController.checkActions(activeAbility.getAPCost()))
        {
            CompleteAction();
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
                int[] hitStats = GetHitStats(attackerController, defenderController, abilityData.getTargetInstruction());
                if (randomChance < hitStats[0])
                {
                    if (attackerController.GetEquippedWeapons().Item1.GetInstanceWeaponStatus())
                    {
                        data.Add(defenderController.inflictStatus(attackerController.GetEquippedWeapons().Item1.GetInstanceWeaponStatus(), attackerController.gameObject));
                    }
                    List<CombatData> subResults = AttackUnit(activeUnit, target, true);
                    if (subResults != null)
                    {
                        data.AddRange(subResults);
                    }
                }
                else
                {
                    List<CombatData> subResults = AttackUnit(activeUnit, target, false);
                    if (subResults != null)
                    {
                        data.AddRange(subResults);
                    }
                }
            }
        }
        StartAnimation(data);
        CompleteAction();
        selectedController.useActions(activeAbility.getAPCost());
    }

    //Targeting Functions
    public void CombatTargeting(GameObject targetUnit, TargetInstruction targetInstruction, Vector2 tilePosition)
    {
        switch (targetInstruction.getTargetCondition())
        {
            case targetCondition.RANDOMDUPE:
                switch (targetInstruction.getTargetType())
                {
                    case targetType.TARGET:
                        RandomUnitTargets(true);
                        ExecuteAction(targetUnit, tilePosition);
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
                        RandomUnitTargets(false);
                        ExecuteAction(targetUnit, tilePosition);
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
                            AddUnitToTargets(targetUnit);
                            if (GetGameObjectTargets().Count >= GetActiveCombatAbility().getAbilityData().getTargetInstruction().getTargetConditionCount())
                            {
                                ExecuteAction(targetUnit, tilePosition);
                            }
                        }
                        break;

                    case targetType.BEAM:
                        if (activeOverlay)
                        {
                            AbilityCalculator calculator = new AbilityCalculator(GetFilterTeam(targetInstruction, activeTeam), GetActiveCombatAbility(), activeUnit.GetComponent<UnitController>().getUnitPos(), activeOverlay.GetComponent<OverlayController>().getOverlayDirection());
                            List<GameObject> targetLine = calculator.getAffectedUnits(targetInstruction, activeUnit.GetComponent<UnitController>());
                            foreach (GameObject target in targetLine)
                            {
                                AddUnitToTargets(target);
                            }
                            ExecuteAction(targetUnit, tilePosition);
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
                        AllPointTargets();
                        ExecuteAction(targetUnit, tilePosition);
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

    public void AddUnitToTargets(GameObject addition)
    {
        if (!activeAbility)
        {
            CompleteAction();
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            CompleteAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        int rangeMax = abilityData.getTargetInstruction().getMaxRange();
        if (!targetCondition.getMaxRangeFixed())
        {
            rangeMax += selectedController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
        }
        int rangeMin = targetCondition.getMinRange();
        if (!abilityData.getTargetInstruction().getMinRangeFixed())
        {
            rangeMin += selectedController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
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

    public void AddTileToTargets(Vector2Int tile)
    {

    }

    public void AddPointToTargets(Vector2 position)
    {

    }

    public void RandomUnitTargets(bool duplicatesAllowed)
    {
        if (!activeAbility)
        {
            CompleteAction();
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            CompleteAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        int rangeMax = abilityData.getTargetInstruction().getMaxRange();
        if (!targetCondition.getMaxRangeFixed())
        {
            rangeMax += selectedController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
        }
        int rangeMin = targetCondition.getMinRange();
        if (!abilityData.getTargetInstruction().getMinRangeFixed())
        {
            rangeMin += selectedController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
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

    public void RandomTileTargets(bool duplicatesAllowed)
    {

    }

    public void RandomPointTargets(bool duplicatesAllowed)
    {

    }

    public void AllPointTargets()
    {
        if (!activeAbility)
        {
            CompleteAction();
            return;
        }
        if (activeAbility.getAbilityType() != actionType.COMBAT)
        {
            CompleteAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstruction targetCondition = abilityData.getTargetInstruction();
        int rangeMax = abilityData.getTargetInstruction().getMaxRange();
        if (!targetCondition.getMaxRangeFixed())
        {
            rangeMax += selectedController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
        }
        int rangeMin = targetCondition.getMinRange();
        if (!abilityData.getTargetInstruction().getMinRangeFixed())
        {
            rangeMin += selectedController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
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

    public void AllTileTargets()
    {

    }

    public List<GameObject> GetGameObjectTargets()
    {
        return selectedGameObjectTargets;
    }

    public List<Vector2Int> GetTileTargets()
    {
        return selectedTileTargets;
    }

    public List<Vector2> GetPointTargets()
    {
        return selectedPointTargets;
    }

    //Unit Management
    public List<GameObject> GetUnits()
    {
        List<GameObject> allUnits = new List<GameObject>();
        foreach (KeyValuePair<Vector2Int, GameObject> temp in unitList)
        {
            allUnits.Add(temp.Value);
        }
        return allUnits;
    }

    public void AddUnit(GameObject unit)
    {
        unitList.Add(unit.GetComponent<UnitController>().getUnitPos(), unit);
        battleTeam team = unit.GetComponent<UnitController>().getTeam();
        teamLists[team].Add(unit);
    }

    public bool MoveUnit(GameObject unit, Vector2 coords)
    {
        if (activeAbility.getAbilityType() != actionType.MOVE)
        {
            return false;
        }
        MovementAbility calculateAbility = activeAbility as MovementAbility;
        if (GetUnitFromCoords(mapController.gridTilePos(coords)) && GetUnitFromCoords(mapController.gridTilePos(coords)) != gameObject)
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

    public List<GameObject> GetHitUnits(CombatAbility calculateAbility, TargetInstruction targetInstruction, UnitController selectedController, Vector2 direction)
    {
        if (targetInstruction.getTargetType() == targetType.NONE)
        {
            List<GameObject> target = new List<GameObject>();
            target.Add(selectedController.gameObject);
            return target;
        }
        AbilityCalculator calculator = new AbilityCalculator(GetFilterTeam(targetInstruction, activeTeam), calculateAbility, cursorController.getGridPos(), direction);
        List<GameObject> hitUnits = calculator.getAffectedUnits(targetInstruction, selectedController);
        return hitUnits;
    }

    public List<CombatData> AttackUnit(GameObject attacker, GameObject defender, bool overallHit)
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
            List<GameObject> hitUnits = GetHitUnits(calculateAbility, effect.getEffectTarget(), defender.GetComponent<UnitController>(), new Vector2());
            foreach (GameObject target in hitUnits)
            {
                CombatData result = ApplyEffect(attacker, target, effect, overallHit);
                retVal.Add(result);
            }
        }
        return retVal;
    }

    public CombatData ApplyEffect(GameObject attacker, GameObject defender, EffectInstruction effect, bool overallHit)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        int randomChance = Random.Range(0, 100);
        int[] hitStats = GetHitStats(attackerController, defenderController, effect.getEffectTarget());
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
    void CheckEndGame()
    {
        if (EvaluatePlayerRout())
        {
            BattleExitHandler.outcome = battleOutcome.ROUTED;
            MapEnd();
            return;
        }
        if (EvaluateEnemyRout())
        {
            BattleExitHandler.outcome = battleOutcome.VICTORY;
            MapEnd();
            return;
        }
        if (mapData.evaluateDefeatConditions(unitList, turnNumber))
        {
            BattleExitHandler.outcome = battleOutcome.FAILURE;
            MapEnd();
            return;
        }
        if (mapData.evaluateVictoryConditions(unitList, turnNumber))
        {
            BattleExitHandler.outcome = battleOutcome.SUCCESS;
            MapEnd();
            return;
        }
    }

    bool EvaluatePlayerRout()
    {
        return teamLists[battleTeam.ALLY].Count == 0 && teamLists[battleTeam.PLAYER].Count == 0;
    }

    bool EvaluateEnemyRout()
    {
        return teamLists[battleTeam.ENEMY].Count == 0;
    }

    void MapEnd()
    {
        BattleExitHandler.turn_count = turnNumber;
        StopAllCoroutines();
        SceneManager.LoadSceneAsync("BattleEnd");
    }

    //Other Helpers
    public int GetBattleTeamCount()
    {
        return System.Enum.GetNames(typeof(battleTeam)).Length;
    }

    public GameObject GetUnitFromCoords(Vector2Int coords)
    {
        if (unitList.ContainsKey(coords))
        {
            return unitList[coords];
        }
        return null;
    }

    public void KillDead(List<CombatData> combatResults)
    {
        foreach (CombatData data in combatResults)
        {
            if (data.getDefenderKilled() && unitList.ContainsKey(data.getDefender().GetComponent<UnitController>().getUnitPos()))
            {
                KillUnit(data.getDefender());
            }
        }
    }

    public Dictionary<battleTeam, List<GameObject>> GetTeamLists()
    {
        return teamLists;
    }

    public void KillUnit(GameObject deadUnit)
    {
        Vector2Int location = deadUnit.GetComponent<UnitController>().getUnitPos();
        unitList.Remove(location);
        battleTeam team = deadUnit.GetComponent<UnitController>().getTeam();
        teamLists[team].Remove(deadUnit);
        CheckEndGame();
        GameObject.Destroy(deadUnit);
    }

    public Pathfinder GetPathfinder()
    {
        return pathfinder;
    }

    public Ability GetActiveAbility()
    {
        return activeAbility;
    }

    public CombatAbility GetActiveCombatAbility()
    {
        if (activeAbility.getAbilityType() == actionType.COMBAT)
        {
            return activeAbility as CombatAbility;
        }
        return null;
    }

    public MovementAbility GetActiveMovementAbility()
    {
        if (activeAbility.getAbilityType() == actionType.MOVE)
        {
            return activeAbility as MovementAbility;
        }
        return null;
    }

    public GameObject GetActiveUnit()
    {
        return activeUnit;
    }

    public GameObject GetActiveOverlay()
    {
        return activeOverlay;
    }

    public battleTeam GetFilterTeam(TargetInstruction instruction, battleTeam actingTeam)
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
    public int FinalRange(int baseRange, MovementAbility ability)
    {
        return Mathf.FloorToInt((baseRange + ability.getFlatMoveBonus()) * ((ability.getPercentMoveBonus() / 100f) + 1f));
    }

    public int[] GetHitStats(UnitController attackerController, UnitController defenderController, TargetInstruction targetInstruction)
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
            if (attackerController.GetEquippedWeapons().Item1 != null)
            {
                effectiveHit += attackerController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[1];
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
    battleTeam NextTeam()
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
    void PlaceUnit(UnitEntryData unit, Vector2Int tile, battleTeam team)
    {
        GameObject temp = GameObject.Instantiate(unitTemplate);
        temp.SetActive(false);
        temp.GetComponent<UnitController>().setUnitInstance(unit.getUnit());
        temp.GetComponent<UnitController>().equippedMainHandWeapon = unit.getWeapons().Item1;
        temp.GetComponent<UnitController>().equippedOffHandWeapon = unit.getWeapons().Item2;
        temp.GetComponent<UnitController>().equippedArmor = unit.getArmor();
        temp.GetComponent<UnitController>().team = team;
        temp.GetComponent<SpriteRenderer>().sprite = unit.getUnit().getBattleSprite("attack");
        Vector2 unitPos = mapController.tileGridPos(tile);
        temp.GetComponent<Transform>().position = new Vector3(unitPos.x, unitPos.y, -2);
        temp.SetActive(true);
    }

    //Animation Control Functions
    public void StartAnimation(List<CombatData> combatSequence)
    {
        turnPhase = turnPhase.PAUSE;
        uiCanvas.GetComponent<UIController>().displayBattleAnimation(combatSequence);
        StartCoroutine(PostAnimCleanup(combatSequence));
    }

    IEnumerator PostAnimCleanup(List<CombatData> combatSequence)
    {
        yield return new WaitWhile(() => uiCanvas.GetComponent<UIController>().hasAnimation());
        KillDead(combatSequence);
        turnPhase = turnPhase.MAIN;
    }
}