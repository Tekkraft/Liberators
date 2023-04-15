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
    List<UnitData> allUnits = new List<UnitData>();
    BattleTeam activeTeam;
    BattleTeam firstTeam;
    int turnNumber = 1;

    MapController mapController;
    Canvas uiCanvas;
    GameObject cursor;

    GameObject targetingOverlay;
    List<GameObject> selectedGameObjectTargets = new List<GameObject>();
    List<Vector2Int> selectedTileTargets = new List<Vector2Int>();
    List<Vector2> selectedPointTargets = new List<Vector2>();

    TurnPhase turnPhase = TurnPhase.START;
    ActionPhase actionPhase = ActionPhase.INACTIVE;

    PlayerUnitDataList playerData;

    //Action Handlers
    ActionType actionState = ActionType.NONE;
    Ability activeAbility;
    AbilityScript activeAbilityScript;
    Dictionary<string, GameObject> gameobjectRefs = new Dictionary<string, GameObject>();
    Dictionary<string, int> intRefs = new Dictionary<string, int>();
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
        foreach (UnitData unit in allUnits)
        {
            PostBattleEffects(unit);
        }
        Cursor.visible = true;
        SaveSystem.SaveTeamData(playerData.ToJSON());
    }

    //Use this to load key data upon start
    void OnEnable()
    {
        playerData = PlayerUnitDataList.FromJSON(SaveSystem.LoadTeamData());
        int index = 0;
        foreach (UnitData player in playerData.ToList())
        {
            allUnits.Add(player);
            if (player.currentHP <= 0)
            {
                continue;
            }
            PlaceUnit(player, BattleTransition.playerSpawnLocations[index], BattleTeam.PLAYER);
            index++;
        }
        foreach (UnitData enemy in BattleTransition.enemyPlacements.Keys)
        {
            allUnits.Add(enemy);
            if (enemy.currentHP <= 0)
            {
                continue;
            }
            PlaceUnit(enemy, BattleTransition.enemyPlacements[enemy], BattleTeam.ENEMY);
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
        RescanEnemies(units);
        foreach (GameObject active in units)
        {
            if ((float) active.GetComponent<UnitController>().GetStat("currentHP") / active.GetComponent<UnitController>().GetStat("maxHP") > 0.5f)
            {
                if (active.GetComponent<AIController>().GetAIMode() == AIMode.hold)
                {
                    active.GetComponent<AIController>().SetAIMode(AIMode.hold);
                }
                else
                {
                    active.GetComponent<AIController>().SetAIMode(AIMode.attack);
                }
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
                //TODO: TEMP CODE
                if (ability.getAbilityType() == ActionType.MOVE)
                {
                    continue;
                }
                AbilityScript script = AbilityEvaluator.Deserialize<AbilityScript>(ability.GetAbilityXMLFile());
                //TEMPORARY MEASURE - REMOVE WHEN BEAMS VALID ABILITY
                if (ability.getAbilityType() == ActionType.COMBAT && script.targeting[0].GetType() == typeof(BeamTargeting))
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
                SetActiveAbility(selectedAbility);
                if (activeAbility.getAbilityType() == ActionType.COMBAT)
                {
                    //TEMPORARY MEASURE - REMOVE WHEN BEAMS VALID ABILITY
                    if (activeAbilityScript.targeting[0].GetType() == typeof(BeamTargeting))
                    {
                        active.GetComponent<AIController>().disableActions(activeAbility);
                        CompleteAction();
                        continue;
                    }
                    else
                    {
                        //TODO: Implement Self-targeting
                        UnitController targetController = activeUnit.GetComponent<UnitController>();
                        UnitTargeting targeting = activeAbilityScript.targeting[0] as UnitTargeting;
                        int[] ranges = GetRanges(targeting.range, activeUnit);
                        int rangeMax = ranges[0];
                        int rangeMin = ranges[1];
                        Vector2 direction = new Vector2(0, 0);
                        if (targetingOverlay)
                        {
                            direction = targetingOverlay.GetComponent<OverlayController>().getOverlayDirection();
                        }
                        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targeting.range.sightRequired, mapController, this, teamLists, direction);
                        List<GameObject> validTargets = rangefinder.generateTargetsNotOfTeam(activeUnit.GetComponent<UnitController>().GetUnitPos(), activeTeam, false);
                        if (validTargets.Count <= 0)
                        {
                            active.GetComponent<AIController>().disableActions(activeAbility);
                            continue;
                        }
                        //TODO: Set AI multitargeting
                        GameObject target = activeUnit.GetComponent<AIController>().getGameObjectTarget(activeAbilityScript, validTargets);
                        if (target)
                        {
                            CombatTargeting(target, targeting);
                            active.GetComponent<AIController>().resetActions();
                            break;
                        }
                        
                    }
                }
                else if (activeAbility.getAbilityType() == ActionType.MOVE)
                {
                    Vector2Int destination = activeUnit.GetComponent<AIController>().getMoveTarget(activeAbilityScript);
                    if (destination == active.GetComponent<UnitController>().GetUnitPos())
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
                AbilityPrepare(targetUnit);
                //MovePrepare(targetUnit);
                break;

            case ActionType.COMBAT:
                AbilityPrepare(targetUnit);
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
                AbilityAction(targetUnit);
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
        GameObject.Destroy(targetingOverlay);
        uiCanvas.GetComponent<UIController>().clearPreview();
        uiCanvas.GetComponent<UIController>().clearButtons();
        uiCanvas.GetComponent<UIController>().clearMarkers();
        gameobjectRefs.Clear();
        intRefs.Clear();
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
            activeController.useActions(activeAbility.getAPCost());
        }
        CheckEndGame();
        return clear;
    }

    void AbilityPrepare(GameObject unit)
    {
        if (!unit || !activeAbility)
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

            case "TileTargeting":
                range = (activeAbilityScript.targeting[0] as TileTargeting).range;
                break;

            case "SelfTargeting":
                uiCanvas.GetComponent<UIController>().drawMarkers(new List<Vector2Int>() { targetController.GetUnitPos() }, MarkerController.Markers.RED);
                return;

            default:
                Debug.LogError("Unrecognized Targeting Type: " + activeAbilityScript.targeting[0].GetType().ToString());
                CompleteAction();
                return;
        }
        int[] ranges = GetRanges(range, unit);
        int rangeMax = ranges[0];
        int rangeMin = ranges[1];
        Vector2 direction = new Vector2(0, 0);
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, range.sightRequired, mapController, this, teamLists, direction);
        switch (activeAbilityScript.targeting[0].GetType().ToString())
        {
            case "BeamTargeting":
                GameObject temp = GameObject.Instantiate(overlayObject, targetController.gameObject.transform);
                targetingOverlay = temp;
                targetingOverlay.GetComponent<OverlayController>().initalize(rangeMax, true, cursor);
                break;

            case "UnitTargeting":
                switch ((activeAbilityScript.targeting[0] as UnitTargeting).team.filter)
                {
                    case "enemy":
                        uiCanvas.GetComponent<UIController>().drawMarkers(rangefinder.generateCoordsNotOfTeam(unit.GetComponent<UnitController>().GetUnitPos(), activeTeam, false), MarkerController.Markers.RED);
                        break;

                    case "ally":
                        uiCanvas.GetComponent<UIController>().drawMarkers(rangefinder.generateCoordsOfTeam(unit.GetComponent<UnitController>().GetUnitPos(), activeTeam, false), MarkerController.Markers.GREEN);
                        break;

                    case "all":
                        uiCanvas.GetComponent<UIController>().drawMarkers(rangefinder.generateCoordsNotOfTeam(unit.GetComponent<UnitController>().GetUnitPos(), BattleTeam.NEUTRAL, false), MarkerController.Markers.RED);
                        break;
                }
                //TODO: ADD OTHER FILTERS
                break;

            case "SelfTargeting":
                Debug.LogError("Command should be impossible");
                CompleteAction();
                break;

            case "TileTargeting":
                ModeElement mode = (activeAbilityScript.targeting[0] as TileTargeting).mode;
                switch (mode.filter)
                {
                    case "move":

                        int[] moveRanges = GetMoveRanges(range, unit);
                        int moveMax = moveRanges[0];
                        int moveMin = moveRanges[1];
                        Pathfinder pathfinder = new Pathfinder(unit.GetComponent<UnitController>().GetUnitPos(), moveMax, moveMin, mapController);
                        List<Vector2Int> baseValidCoords = pathfinder.getValidCoords();
                        if (baseValidCoords.Count <= 0)
                        {
                            CompleteAction();
                            return;
                        }
                        List<Vector2Int> validCoords = baseValidCoords;
                        //Add unit filtering
                        //Add and/or filtering combo modes
                        switch (mode.state)
                        {
                            case "occupied":
                                for (int i = validCoords.Count - 1; i >= 0; i--)
                                {
                                    if (unitList.ContainsKey(validCoords[i]))
                                    {
                                        validCoords.Remove(validCoords[i]);
                                    }
                                }
                                break;

                            case "empty":
                                for (int i = validCoords.Count - 1; i >= 0; i--)
                                {
                                    if (unitList.ContainsKey(validCoords[i]))
                                    {
                                        validCoords.Remove(validCoords[i]);
                                    }
                                }
                                break;

                            case "any":
                                break;

                            default:
                                Debug.LogError("Invalid mode state: " + mode.state);
                                break;
                        }
                        uiCanvas.GetComponent<UIController>().drawMarkers(validCoords, MarkerController.Markers.BLUE);
                        break;

                    default:
                        Debug.LogError("Unrecognized TileTargeting mode: " + mode.filter);
                        break;
                }
                break;

            default:
                Debug.LogError("Unrecognized Targeting Type: " + activeAbilityScript.targeting[0].GetType().ToString());
                CompleteAction();
                return;
        }
    }

    //TODO: Examine in depth how this function works
    void AbilityAction(GameObject targetUnit)
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
        gameobjectRefs.Add("caster", activeUnit);
        UnitController selectedController = activeUnit.GetComponent<UnitController>();
        AbilityScript abilityScript;
        if (activeAbility.GetAbilityXMLFile() != null && activeAbility.GetAbilityXMLFile() != "")
        {
            abilityScript = AbilityEvaluator.Deserialize<AbilityScript>(activeAbility.GetAbilityXMLFile());
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
        List<BattleStep> steps = new List<BattleStep>();
        steps.Add(new BattleStep());
        steps[0].AddDetail(activeUnit, new BattleDetail("attack"));
        foreach (EffectTypeA effect in abilityScript.effects)
        {
            foreach (GameObject target in selectedGameObjectTargets)
            {
                steps = EffectStep(activeUnit, target, effect, steps, 0);
            }
        }
        //TODO: Reimplement Animation Support
        StartAnimation(steps);
        CompleteAction();
        selectedController.useActions(activeAbility.getAPCost());
    }

    //Action Helper Functions
    List<BattleStep> EffectStep(GameObject source, GameObject target, EffectTypeA effect, List<BattleStep> steps, int layer) {
        if (layer >= steps.Count)
        {
            steps.Add(new BattleStep());
        }
        switch (effect.GetType().ToString())
        {
            case "DamageEffect":
                steps[layer].AddDetail(target, DamageUnit(source, target, effect as DamageEffect));
                return steps;

            case "StatusEffect":
                steps[layer].AddDetail(target, StatusUnit(source, target, effect as StatusEffect));
                return steps;

            case "HealEffect":
                steps[layer].AddDetail(target, HealUnit(source, target, effect as HealEffect));
                return steps;

            case "SelectEffect":
                return SelectUnits(source, target, effect as SelectEffect, steps, layer + 1);

            default:
                Debug.LogError("Unrecognized effect type: " + effect.GetType().ToString());
                return steps;
        }
    }

    //Targeting Functions
    public void CombatTargeting(GameObject targetUnit, TargetingType targeting)
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
                    selectedGameObjectTargets.Add(targetUnit);
                }
                //TODO: Multi-nonduplicate-manual-targeting will fail when insufficient valid targets
                //Until fixed, NO non-duplicate manual targeting conditions
                if (selectedGameObjectTargets.Count >= targeting.count.targets)
                {
                    ExecuteAction(targetUnit, Vector2.zero);
                }
            }
        }
        else
        {
            RandomUnitTargets(targeting, activeUnit);
            ExecuteAction(targetUnit, Vector2.zero);
        }
    }

    void CheckBeamTargeting(GameObject source, BeamTargeting targeting)
    {
        if (targetingOverlay)
        {
            int[] ranges = GetRanges(targeting.range, source);
            int rangeMax = ranges[0];
            int rangeMin = ranges[1];
            Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targeting.range.sightRequired, mapController, this, teamLists, targetingOverlay.GetComponent<OverlayController>().getOverlayDirection());
            List<GameObject> beamTargets = rangefinder.generateTargetsOfTeam(activeUnit.GetComponent<UnitController>().GetUnitPos(), GetFilterTeam(targeting.team, activeTeam), true);
            foreach (GameObject target in beamTargets)
            {
                //TODO: Implement first-unit blocking with block tag
                selectedGameObjectTargets.Add(target);
            }
            ExecuteAction(source, Vector2.zero);
        }
        else
        {
            Debug.LogError("Attempting to check beam with null/no overlay");
        }
    }

    void RandomUnitTargets(UnitTargeting targeting, GameObject source)
    {
        int[] ranges = GetRanges(targeting.range, source);
        int rangeMax = ranges[0];
        int rangeMin = ranges[1];
        Vector2 direction = new Vector2(0, 0);
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targeting.range.sightRequired, mapController, this, teamLists, direction);
        List<GameObject> validTargets = rangefinder.generateTargetsNotOfTeam(source.GetComponent<UnitController>().GetUnitPos(), activeTeam, false);
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
        int[] ranges = GetRanges(range, source);
        int rangeMax = ranges[0];
        int rangeMin = ranges[1];
        Vector2 direction = new Vector2(0, 0);
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, range.sightRequired, mapController, this, teamLists, direction);
        bool validLOS = true;
        bool inRange = rangefinder.inRange(source.GetComponent<UnitController>().GetUnitPos(), target.GetComponent<UnitController>().GetUnitPos(), rangeMax, rangeMin);
        if (range.sightRequired)
        {
            validLOS = !rangefinder.checkLineCollision(source.GetComponent<UnitController>().GetUnitPos(), target.GetComponent<UnitController>().GetUnitPos());
        }
        return validLOS && inRange;
    }

    bool TeamCheckUnit(TeamElement team, GameObject source, GameObject target)
    {
        BattleTeam targetTeam = GetFilterTeam(team, source.GetComponent<UnitController>().GetTeam());
        switch (targetTeam)
        {
            case BattleTeam.PLAYER:
            case BattleTeam.ALLY:
                if (target.GetComponent<UnitController>().GetTeam() == BattleTeam.PLAYER || target.GetComponent<UnitController>().GetTeam() == BattleTeam.ALLY)
                {
                    return true;
                }
                break;

            case BattleTeam.ENEMY:
                if (target.GetComponent<UnitController>().GetTeam() == BattleTeam.ENEMY)
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
        unitList.Add(unit.GetComponent<UnitController>().GetUnitPos(), unit);
        BattleTeam team = unit.GetComponent<UnitController>().GetTeam();
        teamLists[team].Add(unit);
    }

    //Effect Step Effectors
    public bool MoveUnit(GameObject unit, Vector2 coords)
    {
        if (activeAbility.getAbilityType() != ActionType.MOVE)
        {
            return false;
        }
        if (GetUnitFromCoords(mapController.gridTilePos(coords)) && GetUnitFromCoords(mapController.gridTilePos(coords)) != gameObject)
        {
            return false;
        }
        Vector2Int key = unit.GetComponent<UnitController>().GetUnitPos();
        unitList.Remove(key);
        bool moved = unit.GetComponent<UnitController>().MoveUnit(coords);
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

    BattleDetail DamageUnit(GameObject attacker, GameObject defender, DamageEffect effect)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        int randomChance = Random.Range(0, 100);
        int[] hitStats = GetHitStats(attackerController, defenderController, effect);
        int totalHit = hitStats[0];
        int totalCrit = hitStats[1];
        if (effect.trueHit || randomChance < totalHit)
        {
            BattleDetail detail = attackerController.AttackUnit(defenderController, effect, totalCrit);
            return detail;
        } else
        {
            return new BattleDetail();
        }
    }

    BattleDetail StatusUnit(GameObject source, GameObject target, StatusEffect effect)
    {
        UnitController targetController = target.GetComponent<UnitController>();
        //TODO: Reimplement chance to inflict status
        //TODO: Implement status id lookup
        Status status = new Status();
        return targetController.InflictStatus(status, source);
    }

    BattleDetail HealUnit(GameObject source, GameObject target, HealEffect effect)
    {
        return source.GetComponent<UnitController>().HealUnit(target.GetComponent<UnitController>(), effect);
    }

    List<BattleStep> SelectUnits(GameObject source, GameObject target, SelectEffect effect, List<BattleStep> steps, int layer)
    {
        //TODO: Implement actual select effects
        List<GameObject> newTargets = new List<GameObject>();
        List<EffectTypeA> newEffects = new List<EffectTypeA>();
        switch (effect.selector.GetType().ToString())
        {
            case "AOESelector":
                newTargets = AOESelector(target, effect.selector as AOESelector);
                newEffects = (effect.selector as AOESelector).effects;
                break;

            default:
                Debug.LogError("Unrecognized select effect selector: " + effect.selector.GetType().ToString());
                break;
        }
        List<BattleStep> newSteps = steps;
        foreach (EffectTypeA newEffect in newEffects)
        {
            foreach (GameObject newTarget in newTargets)
            {
                newSteps = EffectStep(source, newTarget, newEffect, newSteps, layer);
            }
        }
        return steps;
    }

    //Select Element Selectors
    List<GameObject> AOESelector(GameObject parent, AOESelector selector)
    {
        //TODO: Add non-circle AOE selectors
        //TODO: Add system for include/exclude parent
        int[] ranges = GetRanges(selector.range, parent);
        int rangeMax = ranges[0];
        int rangeMin = ranges[1];
        Vector2 direction = new Vector2(0, 0);
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, selector.range.sightRequired, mapController, this, teamLists, direction);
        BattleTeam target = GetFilterTeam(selector.team, parent.GetComponent<UnitController>().GetTeam());
        List<GameObject> validTargets = rangefinder.generateTargetsOfTeam(parent.GetComponent<UnitController>().GetUnitPos(), target, false);
        return validTargets;
    }

    //End Map Management
    void CheckEndGame()
    {
        if (EvaluatePlayerRout())
        {
            BattleTransition.outcome = BattleOutcome.ROUTED;
            MapEnd();
            return;
        }
        if (EvaluateEnemyRout())
        {
            BattleTransition.outcome = BattleOutcome.VICTORY;
            MapEnd();
            return;
        }
        if (mapData.evaluateDefeatConditions(unitList, turnNumber))
        {
            BattleTransition.outcome = BattleOutcome.FAILURE;
            MapEnd();
            return;
        }
        if (mapData.evaluateVictoryConditions(unitList, turnNumber))
        {
            BattleTransition.outcome = BattleOutcome.SUCCESS;
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
        BattleTransition.turn_count = turnNumber;
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

    public void KillDead()
    {
        foreach (KeyValuePair<BattleTeam, List<GameObject>> team in teamLists)
        {
            for (int i = team.Value.Count - 1; i >= 0; i--)
            {
                GameObject unit = team.Value[i];
                if (unit.GetComponent<UnitController>().GetStat("currentHP") <= 0)
                {
                    KillUnit(unit);
                }
            }
        }
        RescanEnemies(teamLists[activeTeam]);
    }

    public Dictionary<BattleTeam, List<GameObject>> GetTeamLists()
    {
        return teamLists;
    }

    public void KillUnit(GameObject deadUnit)
    {
        Vector2Int location = deadUnit.GetComponent<UnitController>().GetUnitPos();
        unitList.Remove(location);
        BattleTeam team = deadUnit.GetComponent<UnitController>().GetTeam();
        teamLists[team].Remove(deadUnit);
        CheckEndGame();
        GameObject.Destroy(deadUnit);
    }

    public Ability GetActiveAbility()
    {
        return activeAbility;
    }

    public AbilityScript GetActiveAbilityScript()
    {
        return activeAbilityScript;
    }

    public GameObject GetActiveUnit()
    {
        return activeUnit;
    }

    public GameObject GetActiveOverlay()
    {
        return targetingOverlay;
    }

    public GameObject GetOverlayObject()
    {
        return overlayObject;
    }

    //Returns the target team for the given TeamElement and acting team
    //EG: filter "enemy" gives the opposite of the acting team
    public BattleTeam GetFilterTeam(TeamElement team, BattleTeam actingTeam)
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
    public int[] GetMoveRanges(RangeElement range, GameObject source)
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
                rangeMax += source.GetComponent<UnitController>().GetStat("mov");
            }
            rangeMin = range.min;
        }

        return new int[] { rangeMax, rangeMin };
    }

    public int[] GetRanges(RangeElement range, GameObject source)
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
                rangeMax += source.GetComponent<UnitController>().GetEquippedWeapons().Item1.LoadWeaponData().GetWeaponStats()[3];
            }
            rangeMin = range.min;
        }

        return new int[] { rangeMax, rangeMin };
    }

    public int[] GetHitStats(UnitController attackerController, UnitController defenderController, DamageEffect effect)
    {
        int effectiveHit;
        int effectiveAvoid;
        int totalHit;

        //Always calculate avoid for crit
        float rawAvoid;
        if (!effect.melee)
        {
            if (defenderController.GetStat("rea") < reactThreshold)
            {
                rawAvoid = 0;
            }
            else
            {
                rawAvoid = defenderController.GetStat("acu") + (defenderController.GetStat("rea") - reactThreshold);
            }
        }
        else
        {
            rawAvoid = defenderController.GetStat("fin") + defenderController.GetStat("rea");
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
            effectiveHit = (int)(attackerController.GetStat("acu") * hitFactor + effect.hit);
            if (effect.melee)
            {
                effectiveHit = (int)(attackerController.GetStat("fin") * hitFactor + effect.hit);
            }
            if (attackerController.GetEquippedWeapons().Item1 != null)
            {
                effectiveHit += attackerController.GetEquippedWeapons().Item1.LoadWeaponData().GetWeaponStats()[1];
            }
            totalHit = effectiveHit - effectiveAvoid;
        }

        //Calculate crit independently
        int effectiveCrit;
        if (effect.melee)
        {
            //Melee - Use acuity
            effectiveCrit = attackerController.GetStat("acu");
        }
        else
        {
            //Ranged - Use finesse
            effectiveCrit = attackerController.GetStat("fin");
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
    public void RescanEnemies(List<GameObject> units)
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
    void PlaceUnit(UnitData unit, Vector2Int tile, BattleTeam team)
    {
        GameObject temp = GameObject.Instantiate(unitTemplate);
        temp.SetActive(false);
        temp.GetComponent<UnitController>().Initialize(unit, unit.mainWeapon, unit.secondaryWeapon, unit.armor);
        temp.GetComponent<UnitController>().team = team;
        temp.GetComponent<SpriteRenderer>().sprite = unit.GetBattleSprite("idle");
        Vector2 unitPos = mapController.tileGridPos(tile);
        temp.GetComponent<Transform>().position = new Vector3(unitPos.x, unitPos.y, -2);
        temp.SetActive(true);
    }

    //Animation Control Functions
    public void StartAnimation(List<BattleStep> steps)
    {
        turnPhase = TurnPhase.PAUSE;
        uiCanvas.GetComponent<UIController>().displayBattleAnimation(steps);
        StartCoroutine(PostAnimCleanup());
    }

    IEnumerator PostAnimCleanup()
    {
        yield return new WaitWhile(() => uiCanvas.GetComponent<UIController>().hasAnimation());
        //TODO: Separate from animation loop?
        KillDead();
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
        //TODO: Implement fail state for setting abilities and remove the following line
        activeAbility = ability;
    }

    void PostBattleEffects(UnitData unit)
    {
        if (unit.currentHP > 0)
        {
            unit.ChangeCurrentHP(Mathf.FloorToInt(unit.maxHP * 0.1f));
            unit.maxSkillPoints++;
            unit.availableSkillPoints++;
        }
    }
}