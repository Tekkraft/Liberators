using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    //Special Constants
    public static int reactThreshold = 10; //Minimum threshold for evasion bonuses for ranged attacks. No threshold for melee attacks.
    public static float hitFactor = 2f;
    public static float avoidFactor = 1f;
    public static float critFactor = 2f;

    //Main Variabless
    Dictionary<Vector2Int, GameObject> unitList = new Dictionary<Vector2Int, GameObject>();
    Dictionary<BattleTeam, List<GameObject>> teamLists = new Dictionary<BattleTeam, List<GameObject>>();
    BattleTeam activeTeam;
    BattleTeam firstTeam;
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

    TurnPhase turnPhase = TurnPhase.START;
    ActionPhase actionPhase = ActionPhase.INACTIVE;

    //Action Handlers
    ActionType actionState = ActionType.NONE;
    Ability activeAbility;
    AbilityScript activeAbilityScript;
    GameObject activeUnit;

    //Public Objects
    public GameObject overlayObject;
    public MapData mapData;
    public GameObject unitTemplate;

    Coroutine aiTurn;

    void Awake()
    {
        mapController = gameObject.GetComponent<MapController>();
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        cursor = GameObject.FindGameObjectWithTag("Cursor");
        cursorController = cursor.GetComponent<MouseController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        turnPhase = TurnPhase.START;
        StartCoroutine(BannerTimer());
        //TODO: Change to iterate on enum?
        teamLists.Add(BattleTeam.PLAYER, new List<GameObject>());
        teamLists.Add(BattleTeam.ENEMY, new List<GameObject>());
        teamLists.Add(BattleTeam.ALLY, new List<GameObject>());
        teamLists.Add(BattleTeam.NEUTRAL, new List<GameObject>());
    }

    //Use this to save key data upon victory
    void OnDisable()
    {
        AICoordinator.clearSeenEnemies();
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
            if (player.getUnit().getCurrentHP() <= 0)
            {
                continue;
            }
            PlaceUnit(player, BattleEntryHandler.deployedUnits[player], BattleTeam.PLAYER);
        }
        foreach (UnitEntryData enemy in BattleEntryHandler.enemyPlacements.Keys)
        {
            if (enemy.getUnit().getCurrentHP() <= 0)
            {
                continue;
            }
            PlaceUnit(enemy, BattleEntryHandler.enemyPlacements[enemy], BattleTeam.ENEMY);
        }
        //TODO: ADD ALLY FOREACH
        activeTeam = BattleTeam.PLAYER;
        firstTeam = BattleTeam.PLAYER;
    }

    //Turn Management
    void NextPhase()
    {
        turnPhase = TurnPhase.END;
        actionPhase = ActionPhase.INACTIVE;
        if (aiTurn != null)
        {
            StopCoroutine(aiTurn);
        }
        uiCanvas.GetComponent<UIController>().resetButtons();
        CompleteAction();
        AICoordinator.clearSeenEnemies();
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
        while (teamLists[activeTeam].Count == 0)
        {
            activeTeam = NextTeam();
        }
        if (activeTeam == firstTeam)
        {
            turnNumber++;
        }
        CheckEndGame();
        turnPhase = TurnPhase.START;
        StartCoroutine(BannerTimer());
        if (activeTeam != BattleTeam.PLAYER)
        {
            aiTurn = StartCoroutine(RunAIPhase(teamLists[activeTeam]));
        }
    }

    IEnumerator RunAIPhase(List<GameObject> units)
    {
        while (turnPhase != TurnPhase.MAIN)
        {
            yield return new WaitForSeconds(0.1f);
        }
        rescanEnemies(units);
        foreach (GameObject active in units)
        {
            if (active.GetComponent<UnitController>().getHealth()[1] / active.GetComponent<UnitController>().getHealth()[0] > 0.5f)
            {
                active.GetComponent<AIController>().SetAIMode(AIMode.attack);
            } else
            {
                active.GetComponent<AIController>().SetAIMode(AIMode.flee);
            }
            while (uiCanvas.GetComponent<UIController>().hasAnimation())
            {
                yield return new WaitForSeconds(0.1f);
            }
            int cheapest = int.MaxValue;
            foreach (Ability ability in active.GetComponent<UnitController>().getAbilities())
            {
                //TEMPORARY MEASURE - REMOVE WHEN BEAMS VALID ABILITY
                if (ability.getAbilityType() == ActionType.COMBAT && (ability as CombatAbility).getAbilityData().getTargetInstruction().getTargetType() == TargetType.BEAM)
                {
                    active.GetComponent<AIController>().disableActions(activeAbility);
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
                if (activeAbility.getAbilityType() == ActionType.COMBAT)
                {
                    AbilityData selectedData = (selectedAbility as CombatAbility).getAbilityData();
                    //TEMPORARY MEASURE - REMOVE WHEN BEAMS VALID ABILITY
                    if (selectedData.getTargetInstruction().getTargetType() == TargetType.BEAM)
                    {
                        active.GetComponent<AIController>().disableActions(activeAbility);
                        CompleteAction();
                        continue;
                    }
                    else
                    {
                        TargetInstructionInstance targetInstruction = selectedData.getTargetInstruction();
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
                        List<GameObject> validTargets = rangefinder.generateTargetsNotOfTeam(activeUnit.GetComponent<UnitController>().getUnitPos(), activeTeam, targetInstruction.getTargetType() == TargetType.BEAM);
                        Debug.Log(validTargets.Count);
                        if (validTargets.Count <= 0)
                        {
                            active.GetComponent<AIController>().disableActions(activeAbility);
                            continue;
                        }
                        if (targetInstruction.getTargetCondition() == TargetCondition.SELECTED)
                        {
                            switch (targetInstruction.getTargetType())
                            {
                                case TargetType.POINT:
                                    Debug.Log("Unimplemented Selected Target for AI");
                                    break;

                                case TargetType.TILE:
                                    Debug.Log("Unimplemented Selected Target for AI");
                                    break;

                                case TargetType.TARGET:
                                    while (selectedGameObjectTargets.Count < targetInstruction.getTargetConditionCount())
                                    {
                                        GameObject target = activeUnit.GetComponent<AIController>().getGameObjectTarget(selectedAbility as CombatAbility, validTargets);
                                        if (target)
                                        {
                                            CombatTargeting(target, targetInstruction, new Vector2(0, 0));
                                            active.GetComponent<AIController>().resetActions();
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
                                active.GetComponent<AIController>().resetActions();
                            }
                        }
                    }
                }
                else if (activeAbility.getAbilityType() == ActionType.MOVE)
                {
                    Vector2Int destination = activeUnit.GetComponent<AIController>().getMoveTarget(activeAbility as MovementAbility);
                    if (destination == active.GetComponent<UnitController>().getUnitPos())
                    {
                        active.GetComponent<AIController>().disableActions(activeAbility);
                        continue;
                    }
                    if (destination == new Vector2Int(int.MaxValue, int.MaxValue))
                    {
                        active.GetComponent<AIController>().disableActions(activeAbility);
                        continue;
                    }
                    bool retVal = ExecuteAction(activeUnit, mapController.tileGridPos(destination));
                    if (!retVal)
                    {
                        active.GetComponent<AIController>().blockTiles(destination);
                        continue;
                    }
                    active.GetComponent<AIController>().resetBlockedTiles();
                    active.GetComponent<AIController>().resetActions();
                }
                yield return new WaitForSeconds(0.5f);
            }
            active.GetComponent<AIController>().resetActions();
        }
        yield return new WaitUntil(() => turnPhase == TurnPhase.MAIN);
        NextPhase();
    }

    public BattleTeam GetActiveTeam()
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
        turnPhase = TurnPhase.MAIN;
    }

    public TurnPhase GetTurnPhase()
    {
        return turnPhase;
    }

    public void SetTurnPhase(TurnPhase phase)
    {
        turnPhase = phase;
    }

    public ActionPhase GetActionPhase()
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
            actionState = ActionType.NONE;
        }
        if (actionState == ActionType.END)
        {
            NextPhase();
        }
        if (!unit)
        {
            return;
        }
        SetActiveAbility(ability);
        activeUnit = unit;
        ActionSetup(unit);
    }

    public ActionType GetActionState()
    {
        return actionState;
    }

    public void ActionSetup(GameObject targetUnit)
    {
        switch (actionState)
        {
            case ActionType.MOVE:
                MovePrepare(targetUnit);
                break;

            case ActionType.COMBAT:
                CombatPrepare(targetUnit);
                break;
        }
    }

    public bool ExecuteAction(GameObject targetUnit, Vector2 tileDestination)
    {
        bool retVal = true;
        switch (actionState)
        {
            case ActionType.MOVE:
                retVal = MoveAction(tileDestination);
                break;

            case ActionType.COMBAT:
                CombatAction(targetUnit);
                break;
        }
        CompleteAction();
        return retVal;
    }

    //Action Handling
    public void CompleteAction()
    {
        selectedGameObjectTargets.Clear();
        selectedPointTargets.Clear();
        selectedTileTargets.Clear();
        actionState = ActionType.NONE;
        activeUnit = null;
        actionPhase = ActionPhase.INACTIVE;
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
        if (activeAbility.getAbilityType() != ActionType.MOVE)
        {
            return;
        }
        MovementAbility calculateAbility = activeAbility as MovementAbility;
        if (!unit)
        {
            return;
        }
        actionPhase = ActionPhase.PREPARE;
        UnitController targetController = unit.GetComponent<UnitController>();
        targetController.createMoveMarkers(calculateAbility, MarkerController.Markers.BLUE);
    }

    bool MoveAction(Vector2 tileDestination)
    {
        UnitController activeController = activeUnit.GetComponent<UnitController>();
        if (activeController.checkActions(activeAbility.getAPCost()))
        {
            return false;
        }
        bool clear = MoveUnit(activeUnit, tileDestination);
        CompleteAction();
        if (clear)
        {
            bool done = activeController.useActions(activeAbility.getAPCost());
        }
        CheckEndGame();
        return clear;
    }

    void CombatPrepare(GameObject unit)
    {
        if (!unit || !activeAbility)
        {
            return;
        }
        if (activeAbility.getAbilityType() != ActionType.COMBAT)
        {
            return;
        }
        actionPhase = ActionPhase.PREPARE;
        UnitController targetController = unit.GetComponent<UnitController>();
        RangeElement range;
        switch (activeAbilityScript.targeting[0].GetType().ToString())
        {
            case "UnitTargeting":
                range = (activeAbilityScript.targeting[0] as UnitTargeting).range;
                break;

            case "BeamTargeting":
                range = (activeAbilityScript.targeting[0] as BeamTargeting).range;
                break;

            case "SelfTargeting":
                targetController.createAttackMarkers(new List<Vector2Int>() { targetController.getUnitPos() }, MarkerController.Markers.RED);
                return;

            default:
                return;
        }
        int rangeMax;
        int rangeMin;
        if (range.melee)
        {
            rangeMax = 1;
            rangeMin = 1;
        } else
        {
            rangeMax = range.max;
            if (range.extendMaxRange)
            {
                rangeMax += targetController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
            }
            rangeMin = range.min;
        }
        Vector2 direction = new Vector2(0, 0);
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, range.sightRequired, mapController, this, teamLists, direction);
        if (activeAbilityScript.targeting[0].GetType() == typeof(BeamTargeting))
        {
            GameObject temp = GameObject.Instantiate(overlayObject, targetController.gameObject.transform);
            activeOverlay = temp;
            activeOverlay.GetComponent<OverlayController>().initalize(rangeMax, true);
        }
        else
        {
            switch((activeAbilityScript.targeting[0] as UnitTargeting).team.filter)
            {
                case "enemy":
                    targetController.createAttackMarkers(rangefinder.generateCoordsNotOfTeam(unit.GetComponent<UnitController>().getUnitPos(), activeTeam, false), MarkerController.Markers.RED);
                    break;

                case "ally":
                    targetController.createAttackMarkers(rangefinder.generateCoordsOfTeam(unit.GetComponent<UnitController>().getUnitPos(), activeTeam, false), MarkerController.Markers.GREEN);
                    break;

                case "all":
                    targetController.createAttackMarkers(rangefinder.generateCoordsNotOfTeam(unit.GetComponent<UnitController>().getUnitPos(), BattleTeam.NEUTRAL, false), MarkerController.Markers.RED);
                    break;
            }
            //TODO: ADD OTHER FILTERS
        }

    }

    //TODO: Examine in depth how this function works
    void CombatAction(GameObject targetUnit)
    {
        if (!activeAbility)
        {
            CompleteAction();
            return;
        }
        if (activeAbility.getAbilityType() != ActionType.COMBAT)
        {
            CompleteAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityScript abilityScript;
        if (calculateAbility.GetAbilityXMLFile() != null && calculateAbility.GetAbilityXMLFile() != "")
        {
            abilityScript = AbilityEvaluator.Deserialize<AbilityScript>(calculateAbility.GetAbilityXMLFile());
            if (abilityScript == default)
            {
                CompleteAction();
                return;
            }
        }
        else
        {
            CompleteAction();
            return;
        }
        switch(abilityScript.targeting[0].GetType().ToString())
        {
            case "UnitTargeting":
                if (!targetUnit)
                {
                    CompleteAction();
                    return;
                }
                break;

            case "BeamTargeting":
                break;

            case "SelfTargeting":
                if (targetUnit != activeUnit)
                {
                    CompleteAction();
                    return;
                }
                break;

            default:
                CompleteAction();
                return;
        }
        if (selectedController.checkActions(activeAbility.getAPCost()))
        {
            CompleteAction();
            return;
        }
        actionPhase = ActionPhase.EXECUTE;
        List<CombatData> data = new List<CombatData>();
        foreach (EffectTypeA effect in abilityScript.effects)
        {
            foreach (GameObject target in selectedGameObjectTargets)
            {
                EffectStep(activeUnit, target, effect);
            }
        }
        //TODO: Reimplement Animation Support
        StartAnimation(data);
        CompleteAction();
        selectedController.useActions(activeAbility.getAPCost());
    }

    //Action Helper Functions
    void EffectStep(GameObject source, GameObject target, EffectTypeA effect) {
        switch (effect.GetType().ToString())
        {
            case "DamageEffect":
                DamageUnit(source, target, effect as DamageEffect);
                return;

            case "StatusEffect":
                StatusUnit(source, target, effect as StatusEffect);
                return;

            case "HealEffect":
                HealUnit(source, target, effect as HealEffect);
                return;

            case "SelectEffect":
                //TODO: Implement selection effects
                return;

            default:
                return;
        }
    }

    //Targeting Functions
    public void CombatTargetingA(GameObject targetUnit, TargetingType targeting)
    {
        switch (targeting.GetType().ToString())
        {
            case "UnitTargeting":
                CheckUnitTargeting(targetUnit, targeting as UnitTargeting);
                break;

            case "BeamTargeting":
                CheckBeamTargeting(activeUnit, targeting as BeamTargeting);
                break;

            case "SelfTargeting":
                Debug.LogError("Self targeting not yet implemented.");
                break;

            default:
                Debug.LogError("No such targeting type matches: " + targeting.GetType().ToString());
                break;
        }
    }

    void CheckUnitTargeting(GameObject targetUnit, UnitTargeting targeting)
    {
        if (targeting.count.selectManual)
        {
            if (targetUnit)
            {
                if (RangeCheckUnit(targeting.range, activeUnit, targetUnit) && TeamCheckUnit(targeting.team, activeUnit, targetUnit))
                {
                    AddUnitToTargets(targetUnit);
                }
                if (GetGameObjectTargets().Count >= targeting.count.targets)
                {
                    ExecuteAction(targetUnit, Vector2.zero);
                }
            }
        }
        else
        {
            RandomUnitTargetsA(targeting, activeUnit);
            ExecuteAction(targetUnit, Vector2.zero);
        }
    }

    void CheckBeamTargeting(GameObject targetUnit, BeamTargeting targeting)
    {
        if (activeOverlay)
        {
            int rangeMax;
            int rangeMin;
            rangeMax = targeting.range.max;
            if (targeting.range.extendMaxRange)
            {
                rangeMax += targetUnit.GetComponent<UnitController>().GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
            }
            rangeMin = targeting.range.min;
            Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targeting.range.sightRequired, mapController, this, teamLists, activeOverlay.GetComponent<OverlayController>().getOverlayDirection());
            List<GameObject> beamTargets = rangefinder.generateTargetsOfTeam(activeUnit.GetComponent<UnitController>().getUnitPos(), GetFilterTeamA(targeting.team, activeTeam), true);
            foreach (GameObject target in beamTargets)
            {
                //TODO: Implement first-unit blocking with block tag
                selectedGameObjectTargets.Add(target);
            }
            ExecuteAction(targetUnit, Vector2.zero);
        }
        else
        {
            Debug.LogError("Attempting to check beam with null/no overlay");
        }
    }

    void RandomUnitTargetsA(UnitTargeting targeting, GameObject source)
    {
        RangeElement range = targeting.range;
        int rangeMax;
        int rangeMin;
        if (range.melee)
        {
            rangeMax = 1;
            rangeMin = 1;
        }
        else
        {
            rangeMax = range.max;
            if (range.extendMaxRange)
            {
                rangeMax += source.GetComponent<UnitController>().GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
            }
            rangeMin = range.min;
        }
        Vector2 direction = new Vector2(0, 0);
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, range.sightRequired, mapController, this, teamLists, direction);
        List<GameObject> validTargets = rangefinder.generateTargetsNotOfTeam(source.GetComponent<UnitController>().getUnitPos(), activeTeam, false);
        if (targeting.count.duplicate)
        {
            while (selectedGameObjectTargets.Count <= targeting.count.targets)
            {
                selectedGameObjectTargets.Add(validTargets[Random.Range(0, validTargets.Count)]);
            }
        }
        else
        {
            if (targeting.count.infinite || validTargets.Count <= targeting.count.targets)
            {
                selectedGameObjectTargets.AddRange(validTargets);
            }
            else
            {
                while (selectedGameObjectTargets.Count <= targeting.count.targets)
                {
                    GameObject selected = validTargets[Random.Range(0, validTargets.Count)];
                    selectedGameObjectTargets.Add(selected);
                    validTargets.Remove(selected);
                }
            }
        }
    }

    bool RangeCheckUnit(RangeElement range, GameObject source, GameObject target)
    {
        int rangeMax;
        int rangeMin;
        if (range.melee)
        {
            rangeMax = 1;
            rangeMin = 1;
        }
        else
        {
            rangeMax = range.max;
            if (range.extendMaxRange)
            {
                rangeMax += source.GetComponent<UnitController>().GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
            }
            rangeMin = range.min;
        }
        Vector2 direction = new Vector2(0, 0);
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, range.sightRequired, mapController, this, teamLists, direction);
        bool validLOS = true;
        bool inRange = rangefinder.inRange(source.GetComponent<UnitController>().getUnitPos(), target.GetComponent<UnitController>().getUnitPos(), rangeMax, rangeMin);
        if (range.sightRequired)
        {
            validLOS = !rangefinder.checkLineCollision(source.GetComponent<UnitController>().getUnitPos(), target.GetComponent<UnitController>().getUnitPos());
        }
        return validLOS && inRange;
    }

    bool TeamCheckUnit(TeamElement team, GameObject source, GameObject target)
    {
        BattleTeam targetTeam = GetFilterTeamA(team, source.GetComponent<UnitController>().getTeam());
        switch (targetTeam)
        {
            case BattleTeam.PLAYER:
            case BattleTeam.ALLY:
                if (target.GetComponent<UnitController>().getTeam() == BattleTeam.ENEMY)
                {
                    return true;
                }
                break;

            case BattleTeam.ENEMY:
                if (target.GetComponent<UnitController>().getTeam() == BattleTeam.PLAYER || target.GetComponent<UnitController>().getTeam() == BattleTeam.ALLY)
                {
                    return true;
                }
                break;

            case BattleTeam.NEUTRAL:
                return true;

            default:
                Debug.LogError("Invalid target team: " + targetTeam.ToString());
                break;
        }
        return false;
    }

    public void CombatTargeting(GameObject targetUnit, TargetInstructionInstance targetInstruction, Vector2 tilePosition)
    {
        switch (targetInstruction.getTargetCondition())
        {
            case TargetCondition.RANDOMDUPE:
                switch (targetInstruction.getTargetType())
                {
                    case TargetType.TARGET:
                        RandomUnitTargets(true);
                        ExecuteAction(targetUnit, tilePosition);
                        break;

                    case TargetType.BEAM:
                        Debug.Log("Invalid TargetType - Beam and RandomDupe are incompatible");
                        break;

                    case TargetType.POINT:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.PROJECTILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.NONE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.SELF:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.TILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    default:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;
                }
                break;

            case TargetCondition.RANDOMUNIQUE:
                switch (targetInstruction.getTargetType())
                {
                    case TargetType.TARGET:
                        RandomUnitTargets(false);
                        ExecuteAction(targetUnit, tilePosition);
                        break;

                    case TargetType.BEAM:
                        Debug.Log("Invalid TargetType - Beam and RandomUnique are incompatible");
                        break;

                    case TargetType.POINT:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.PROJECTILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.NONE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.SELF:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.TILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    default:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;
                }
                break;

            case TargetCondition.SELECTED:
                switch (targetInstruction.getTargetType())
                {
                    case TargetType.TARGET:
                        if (targetUnit)
                        {
                            AddUnitToTargets(targetUnit);
                            if (GetGameObjectTargets().Count >= GetActiveCombatAbility().getAbilityData().getTargetInstruction().getTargetConditionCount())
                            {
                                ExecuteAction(targetUnit, tilePosition);
                            }
                        }
                        break;

                    case TargetType.BEAM:
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

                    case TargetType.POINT:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.PROJECTILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.NONE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.SELF:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.TILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    default:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;
                }
                break;

            case TargetCondition.ALL:
                switch (targetInstruction.getTargetType())
                {
                    case TargetType.TARGET:
                        AllPointTargets();
                        ExecuteAction(targetUnit, tilePosition);
                        break;

                    case TargetType.BEAM:
                        Debug.Log("Invalid TargetType - Beam and All are incompatible");
                        break;

                    case TargetType.POINT:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.PROJECTILE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.NONE:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.SELF:
                        Debug.Log("Invalid/Unimplemented TargetType");
                        break;

                    case TargetType.TILE:
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
        if (activeAbility.getAbilityType() != ActionType.COMBAT)
        {
            CompleteAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstructionInstance targetCondition = abilityData.getTargetInstruction();
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
        foreach (TargetFilterInstance filter in targetCondition.getConditionFilters())
        {
            if (filter.getTargetFilter() == TargetFilter.ENEMY && !enemyFilter)
            {
                if (rangefinder.generateTargetsNotOfTeam(selectedController.getUnitPos(), activeTeam, targetCondition.getTargetType() == TargetType.BEAM).Contains(addition))
                {
                    selectedGameObjectTargets.Add(addition);
                }
                enemyFilter = true;
                noFilter = false;
            }
            if (filter.getTargetFilter() == TargetFilter.ALLY && !allyFilter)
            {
                if (rangefinder.generateTargetsOfTeam(selectedController.getUnitPos(), activeTeam, targetCondition.getTargetType() == TargetType.BEAM).Contains(addition))
                {
                    selectedGameObjectTargets.Add(addition);
                }
                allyFilter = true;
                noFilter = false;
            }
        }
        if (noFilter)
        {
            if (rangefinder.generateTargetsOfTeam(selectedController.getUnitPos(), BattleTeam.NEUTRAL, targetCondition.getTargetType() == TargetType.BEAM).Contains(addition))
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
        if (activeAbility.getAbilityType() != ActionType.COMBAT)
        {
            CompleteAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstructionInstance targetCondition = abilityData.getTargetInstruction();
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
        if (activeAbility.getAbilityType() != ActionType.COMBAT)
        {
            CompleteAction();
            return;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityData abilityData = calculateAbility.getAbilityData();
        TargetInstructionInstance targetCondition = abilityData.getTargetInstruction();
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
        BattleTeam team = unit.GetComponent<UnitController>().getTeam();
        teamLists[team].Add(unit);
    }

    public bool MoveUnit(GameObject unit, Vector2 coords)
    {
        if (activeAbility.getAbilityType() != ActionType.MOVE)
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

    public List<GameObject> GetHitUnits(CombatAbility calculateAbility, TargetInstructionInstance targetInstruction, UnitController selectedController, Vector2 direction)
    {
        if (targetInstruction.getTargetType() == TargetType.NONE)
        {
            List<GameObject> target = new List<GameObject>();
            target.Add(selectedController.gameObject);
            return target;
        }
        AbilityCalculator calculator = new AbilityCalculator(GetFilterTeam(targetInstruction, activeTeam), calculateAbility, cursorController.GetGridPos(), direction);
        List<GameObject> hitUnits = calculator.getAffectedUnits(targetInstruction, selectedController);
        return hitUnits;
    }

    public void DamageUnit(GameObject attacker, GameObject defender, DamageEffect effect)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        int randomChance = Random.Range(0, 100);
        int[] hitStats = GetHitStatsA(attackerController, defenderController, effect);
        int totalHit = hitStats[0];
        int totalCrit = hitStats[1];
        if (effect.trueHit || randomChance < totalHit)
        {
            attackerController.attackUnitA(defenderController, effect, totalCrit);
            return;
        } else
        {
            //TODO: DO SOMETHING FOR MISS
            return;
        }
    }

    public void StatusUnit(GameObject source, GameObject target, StatusEffect effect)
    {
        UnitController targetController = target.GetComponent<UnitController>();
        //TODO: Reimplement chance to inflict status
        //TODO: Implement status id lookup
        Status status = new Status();
        targetController.inflictStatus(status, source);
    }

    public void HealUnit(GameObject source, GameObject target, HealEffect effect)
    {
        source.GetComponent<UnitController>().healUnitA(target.GetComponent<UnitController>(), effect);
    }

    public List<CombatData> AttackUnit(GameObject attacker, GameObject defender, bool overallHit)
    {
        List<CombatData> retVal = new List<CombatData>();
        //CANNOT DO BEAM AOEs
        if (activeAbility.getAbilityType() != ActionType.COMBAT)
        {
            return null;
        }
        CombatAbility calculateAbility = activeAbility as CombatAbility;
        AbilityData abilityData = calculateAbility.getAbilityData();
        List<EffectInstructionInstance> effectList = abilityData.getEffectInstructions();
        foreach (EffectInstructionInstance effect in effectList)
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

    public CombatData ApplyEffect(GameObject attacker, GameObject defender, EffectInstructionInstance effect, bool overallHit)
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
            if (effect.getEffectType() == EffectType.STATUS)
            {
                result = new CombatData(attacker, defender, effect.getEffectStatus(), false);
                return result;
            }
            if (effect.getEffectType() == EffectType.DAMAGE)
            {
                result = new CombatData(attacker, defender, effect, false, false, 0, attacker.GetComponent<UnitController>().getStats()[2], false);
                return result;
            }
        }
        if (randomChance < totalHit || !effect.getIndependentHit())
        {
            if (effect.getEffectType() == EffectType.STATUS)
            {
                result = defenderController.inflictStatus(effect.getEffectStatus(), attacker);
            }
            if (effect.getEffectType() == EffectType.DAMAGE)
            {
                result = attackerController.attackUnit(defender.GetComponent<UnitController>(), effect, totalCrit);
            }
            if (effect.getEffectType() == EffectType.HEAL)
            {
                attackerController.healUnit(defender.GetComponent<UnitController>(), effect);
            }
        }
        else
        {
            if (effect.getEffectType() == EffectType.STATUS)
            {
                result = new CombatData(attacker, defender, effect.getEffectStatus(), false);
                return result;
            }
            if (effect.getEffectType() == EffectType.DAMAGE)
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
            BattleExitHandler.outcome = BattleOutcome.ROUTED;
            MapEnd();
            return;
        }
        if (EvaluateEnemyRout())
        {
            BattleExitHandler.outcome = BattleOutcome.VICTORY;
            MapEnd();
            return;
        }
        if (mapData.evaluateDefeatConditions(unitList, turnNumber))
        {
            BattleExitHandler.outcome = BattleOutcome.FAILURE;
            MapEnd();
            return;
        }
        if (mapData.evaluateVictoryConditions(unitList, turnNumber))
        {
            BattleExitHandler.outcome = BattleOutcome.SUCCESS;
            MapEnd();
            return;
        }
    }

    bool EvaluatePlayerRout()
    {
        return teamLists[BattleTeam.ALLY].Count == 0 && teamLists[BattleTeam.PLAYER].Count == 0;
    }

    bool EvaluateEnemyRout()
    {
        return teamLists[BattleTeam.ENEMY].Count == 0;
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
        return System.Enum.GetNames(typeof(BattleTeam)).Length;
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
        rescanEnemies(teamLists[activeTeam]);
    }

    public Dictionary<BattleTeam, List<GameObject>> GetTeamLists()
    {
        return teamLists;
    }

    public void KillUnit(GameObject deadUnit)
    {
        Vector2Int location = deadUnit.GetComponent<UnitController>().getUnitPos();
        unitList.Remove(location);
        BattleTeam team = deadUnit.GetComponent<UnitController>().getTeam();
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
        if (activeAbility.getAbilityType() == ActionType.COMBAT)
        {
            return activeAbility as CombatAbility;
        }
        return null;
    }

    public MovementAbility GetActiveMovementAbility()
    {
        if (activeAbility.getAbilityType() == ActionType.MOVE)
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

    public BattleTeam GetFilterTeam(TargetInstructionInstance instruction, BattleTeam actingTeam)
    {
        foreach (TargetFilterInstance filter in instruction.getConditionFilters())
        {
            if (filter.getTargetFilter() == TargetFilter.ENEMY)
            {
                if (actingTeam == BattleTeam.ENEMY)
                {
                    return BattleTeam.PLAYER;
                } else if (actingTeam == BattleTeam.PLAYER || actingTeam == BattleTeam.ALLY)
                {
                    return BattleTeam.ENEMY;
                }
            } else if (filter.getTargetFilter() == TargetFilter.ALLY)
            {
                if (actingTeam == BattleTeam.PLAYER || actingTeam == BattleTeam.ALLY)
                {
                    return BattleTeam.PLAYER;
                }
                else if (actingTeam == BattleTeam.ENEMY)
                {
                    return BattleTeam.ENEMY;
                }
            }
        }
        return BattleTeam.NEUTRAL;
    }

    public BattleTeam GetFilterTeamA(TeamElement team, BattleTeam actingTeam)
    {
        if (team.filter == "enemy")
        {
            if (actingTeam == BattleTeam.ENEMY)
            {
                return BattleTeam.PLAYER;
            }
            else if (actingTeam == BattleTeam.PLAYER || actingTeam == BattleTeam.ALLY)
            {
                return BattleTeam.ENEMY;
            }
            else
            {
                Debug.LogError("Invalid battle team: " + actingTeam.ToString());
                return BattleTeam.NEUTRAL;
            }
        }
        else if (team.filter == "ally")
        {
            if (actingTeam == BattleTeam.PLAYER || actingTeam == BattleTeam.ALLY)
            {
                return BattleTeam.PLAYER;
            }
            else if (actingTeam == BattleTeam.ENEMY)
            {
                return BattleTeam.ENEMY;
            }
            else
            {
                Debug.LogError("Invalid battle team: " + actingTeam.ToString());
                return BattleTeam.NEUTRAL;
            }
        }
        else if (team.filter == "all")
        {
            return BattleTeam.NEUTRAL;
        } else
        {
            Debug.LogError("Invalid team filter: " + team.filter);
            return BattleTeam.NEUTRAL;
        }
    }

    //Stat Math
    public int FinalRange(int baseRange, MovementAbility ability)
    {
        return Mathf.FloorToInt((baseRange + ability.getFlatMoveBonus()) * ((ability.getPercentMoveBonus() / 100f) + 1f));
    }

    public int[] GetHitStats(UnitController attackerController, UnitController defenderController, TargetInstructionInstance targetInstruction)
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

    public int[] GetHitStatsA(UnitController attackerController, UnitController defenderController, DamageEffect effect)
    {
        int effectiveHit;
        int effectiveAvoid;
        int totalHit;

        //Always calculate avoid for crit
        float rawAvoid;
        if (!effect.melee)
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
        if (effect.trueHit)
        {
            effectiveHit = effect.hit;
            totalHit = effectiveHit;
        }
        else
        {
            effectiveHit = (int)(attackerController.getStats()[5] * hitFactor + effect.hit);
            if (effect.melee)
            {
                effectiveHit = (int)(attackerController.getStats()[6] * hitFactor + effect.hit);
            }
            if (attackerController.GetEquippedWeapons().Item1 != null)
            {
                effectiveHit += attackerController.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[1];
            }
            totalHit = effectiveHit - effectiveAvoid;
        }

        //Calculate crit independently
        int effectiveCrit;
        if (effect.melee)
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
    BattleTeam NextTeam()
    {
        switch (activeTeam)
        {
            case BattleTeam.PLAYER:
                return BattleTeam.ENEMY;

            case BattleTeam.ENEMY:
                return BattleTeam.ALLY;

            case BattleTeam.ALLY:
                return BattleTeam.PLAYER;

            default:
                return BattleTeam.PLAYER;
        }
    }

    //AI Help
    public void rescanEnemies(List<GameObject> units)
    {
        AICoordinator.clearSeenEnemies();
        foreach (GameObject check in units)
        {
            Rangefinder rangefinder = new Rangefinder(int.MaxValue, 0, true, mapController, this, teamLists, Vector2.zero);
            foreach (GameObject target in rangefinder.generateTargetsNotOfTeam(check.transform.position, activeTeam, false))
            {
                if (!AICoordinator.seenEnemies.Contains(target))
                {
                    AICoordinator.seenEnemies.Add(target);
                }
            }
        }
    }

    //Unit Construction
    void PlaceUnit(UnitEntryData unit, Vector2Int tile, BattleTeam team)
    {
        GameObject temp = GameObject.Instantiate(unitTemplate);
        temp.SetActive(false);
        temp.GetComponent<UnitController>().setUnitInstance(unit.getUnit());
        temp.GetComponent<UnitController>().equippedMainHandWeapon = unit.getWeapons().Item1;
        temp.GetComponent<UnitController>().equippedOffHandWeapon = unit.getWeapons().Item2;
        temp.GetComponent<UnitController>().equippedArmor = unit.getArmor();
        temp.GetComponent<UnitController>().team = team;
        temp.GetComponent<SpriteRenderer>().sprite = unit.getUnit().getBattleSprite("idle");
        Vector2 unitPos = mapController.tileGridPos(tile);
        temp.GetComponent<Transform>().position = new Vector3(unitPos.x, unitPos.y, -2);
        temp.SetActive(true);
    }

    //Animation Control Functions
    public void StartAnimation(List<CombatData> combatSequence)
    {
        turnPhase = TurnPhase.PAUSE;
        uiCanvas.GetComponent<UIController>().displayBattleAnimation(combatSequence);
        StartCoroutine(PostAnimCleanup(combatSequence));
    }

    IEnumerator PostAnimCleanup(List<CombatData> combatSequence)
    {
        yield return new WaitWhile(() => uiCanvas.GetComponent<UIController>().hasAnimation());
        KillDead(combatSequence);
        turnPhase = TurnPhase.MAIN;
    }

    //Ability Validation

    //Ability Handling and Parsing
    void SetActiveAbility(Ability ability)
    {
        if (ability == activeAbility)
        {
            return;
        }
        if (ability.GetAbilityXMLFile() != null && ability.GetAbilityXMLFile() != "")
        {
            activeAbility = ability;
            activeAbilityScript = AbilityEvaluator.Deserialize<AbilityScript>(ability.GetAbilityXMLFile());
        }
    }
}