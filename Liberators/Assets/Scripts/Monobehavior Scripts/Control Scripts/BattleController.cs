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
                //TODO: TEMP CODE
                if (ability.getAbilityType() == ActionType.MOVE)
                {
                    continue;
                }
                AbilityScript script = AbilityEvaluator.Deserialize<AbilityScript>(ability.GetAbilityXMLFile());
                //TEMPORARY MEASURE - REMOVE WHEN BEAMS VALID ABILITY
                Debug.Log(ability);
                Debug.Log(script);
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
                        if (activeOverlay)
                        {
                            direction = activeOverlay.GetComponent<OverlayController>().getOverlayDirection();
                        }
                        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targeting.range.sightRequired, mapController, this, teamLists, direction);
                        List<GameObject> validTargets = rangefinder.generateTargetsNotOfTeam(activeUnit.GetComponent<UnitController>().getUnitPos(), activeTeam, false);
                        Debug.Log(validTargets.Count);
                        if (validTargets.Count <= 0)
                        {
                            active.GetComponent<AIController>().disableActions(activeAbility);
                            continue;
                        }
                        //TODO: Set AI multitargeting
                        GameObject target = activeUnit.GetComponent<AIController>().getGameObjectTarget(selectedAbility as CombatAbility, validTargets);
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
        int[] ranges = GetRanges(range, unit);
        int rangeMax = ranges[0];
        int rangeMin = ranges[1];
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
        foreach (EffectTypeA effect in abilityScript.effects)
        {
            foreach (GameObject target in selectedGameObjectTargets)
            {
                EffectStep(activeUnit, target, effect);
            }
        }
        //TODO: Reimplement Animation Support
        StartAnimation();
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
    public void CombatTargeting(GameObject targetUnit, TargetingType targeting)
    {
        Debug.Log(targeting.GetType().ToString());
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
        if (activeOverlay)
        {
            int[] ranges = GetRanges(targeting.range, source);
            int rangeMax = ranges[0];
            int rangeMin = ranges[1];
            Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targeting.range.sightRequired, mapController, this, teamLists, activeOverlay.GetComponent<OverlayController>().getOverlayDirection());
            List<GameObject> beamTargets = rangefinder.generateTargetsOfTeam(activeUnit.GetComponent<UnitController>().getUnitPos(), GetFilterTeam(targeting.team, activeTeam), true);
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
        int[] ranges = GetRanges(range, source);
        int rangeMax = ranges[0];
        int rangeMin = ranges[1];
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
        BattleTeam targetTeam = GetFilterTeam(team, source.GetComponent<UnitController>().getTeam());
        Debug.Log(targetTeam);
        Debug.Log(source.GetComponent<UnitController>().getTeam());
        Debug.Log(target.GetComponent<UnitController>().getTeam());
        switch (targetTeam)
        {
            case BattleTeam.PLAYER:
            case BattleTeam.ALLY:
                if (target.GetComponent<UnitController>().getTeam() == BattleTeam.PLAYER || target.GetComponent<UnitController>().getTeam() == BattleTeam.ALLY)
                {
                    return true;
                }
                break;

            case BattleTeam.ENEMY:
                if (target.GetComponent<UnitController>().getTeam() == BattleTeam.ENEMY)
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

    public void DamageUnit(GameObject attacker, GameObject defender, DamageEffect effect)
    {
        UnitController attackerController = attacker.GetComponent<UnitController>();
        UnitController defenderController = defender.GetComponent<UnitController>();
        int randomChance = Random.Range(0, 100);
        int[] hitStats = GetHitStats(attackerController, defenderController, effect);
        int totalHit = hitStats[0];
        int totalCrit = hitStats[1];
        if (effect.trueHit || randomChance < totalHit)
        {
            attackerController.attackUnit(defenderController, effect, totalCrit);
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
        source.GetComponent<UnitController>().healUnit(target.GetComponent<UnitController>(), effect);
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

    public void KillDead()
    {
        foreach (KeyValuePair<BattleTeam, List<GameObject>> team in teamLists)
        {
            for (int i = team.Value.Count; i >= 0; i--)
            {
                GameObject unit = team.Value[i];
                if (unit.GetComponent<UnitController>().getHealth()[1] <= 0)
                {
                    KillUnit(unit);
                }
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
        return activeOverlay;
    }

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
    public int FinalRange(int baseRange, MovementAbility ability)
    {
        return Mathf.FloorToInt((baseRange + ability.getFlatMoveBonus()) * ((ability.getPercentMoveBonus() / 100f) + 1f));
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
                rangeMax += source.GetComponent<UnitController>().GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
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
    public void StartAnimation()
    {
        turnPhase = TurnPhase.PAUSE;
        uiCanvas.GetComponent<UIController>().displayBattleAnimation();
        StartCoroutine(PostAnimCleanup());
    }

    IEnumerator PostAnimCleanup()
    {
        yield return new WaitWhile(() => uiCanvas.GetComponent<UIController>().hasAnimation());
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
}