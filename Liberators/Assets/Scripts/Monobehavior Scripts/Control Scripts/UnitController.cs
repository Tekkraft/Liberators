using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    static int baseActions = 3;
    //Control Variables
    public GameObject marker;
    Vector2Int unitGridPosition;
    Grid mainGrid;
    MapController mapController;
    BattleController battleController;
    List<GameObject> markerList = new List<GameObject>();
    public BattleTeam team;

    UnitData unitData;

    //Unit Properties
    int maxActions;
    int actions;

    public Ability basicMovement;
    public Ability passUnitAbility;
    public Ability endTurnAbility;

    public WeaponData equippedMainWeapon;
    public WeaponData equippedSecondaryWeapon;
    public ArmorData equippedArmor;

    bool validMainWeapon = false;
    bool validSecondaryWeapon = false;
    bool validArmor = false;

    List<Ability> allAbilities = new List<Ability>();

    List<StatusInstance> statuses = new List<StatusInstance>();

    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        battleController = mainGrid.GetComponentsInChildren<BattleController>()[0];
        unitGridPosition = mapController.gridWorldPos(transform.position);
        battleController.AddUnit(gameObject);
    }

    public void Initialize(UnitData unitData, WeaponData mainWeapon, WeaponData secondaryWeapon, ArmorData armor)
    {
        this.unitData = unitData;
        equippedMainWeapon = mainWeapon;
        equippedSecondaryWeapon = secondaryWeapon;
        equippedArmor = armor;
        maxActions = baseActions;
        actions = maxActions;
        if (basicMovement)
        {
            allAbilities.Add(basicMovement);
        }
        if (equippedMainWeapon != null && equippedMainWeapon.weaponBaseId != null && equippedMainWeapon.weaponBaseId != "")
        {
            allAbilities.AddRange(equippedMainWeapon.LoadWeaponData().GetAbilities());
            validMainWeapon = true;
        }
        if (equippedSecondaryWeapon != null && equippedSecondaryWeapon.weaponBaseId != null && equippedSecondaryWeapon.weaponBaseId != "")
        {
            allAbilities.AddRange(equippedSecondaryWeapon.LoadWeaponData().GetAbilities());
            validSecondaryWeapon = true;
        }
        if (equippedArmor != null && equippedArmor.armorBaseId != null && equippedArmor.armorBaseId != "")
        {
            validArmor = true;
        }
        if (unitData.skillTree != null)
        {
            unitData.skillTree.LoadAllLearnedAbilities();
            allAbilities.AddRange(unitData.skillTree.GetAllLearnedAbilities());
        }
    }

    public int[] getActions()
    {
        return new int[] { maxActions, actions };
    }

    public bool checkActions(int used)
    {
        return actions < used;
    }

    public bool useActions(int used)
    {
        actions -= used;
        return actions <= 0;
    }

    public void resetActions()
    {
        actions = maxActions;
    }

    //Call only at end of team turn
    public bool endUnitTurn()
    {
        resetActions();
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status linkedStatus = statuses[i].getStatus();
            if (linkedStatus.getHealthOverTime()[0] > 0)
            {
                TakeDamage(linkedStatus.getHealthOverTime()[0], linkedStatus.getHealthOverTimeElement());
                if (unitData.currentHP <= 0)
                {
                    return true;
                }
            }
            if (linkedStatus.getHealthOverTime()[1] > 0)
            {
                RestoreHealth(linkedStatus.getHealthOverTime()[1]);
            }
            bool expired = statuses[i].update();
            if (expired)
            {
                statuses.Remove(statuses[i]);
            }
            if (linkedStatus.getAPMode())
            {
                actions += linkedStatus.getAPChange();
                if (actions < 0)
                {
                    actions = 0;
                }
            }
        }
        return false;
    }

    public List<Ability> getAbilities()
    {
        return allAbilities;
    }

    public (WeaponData, WeaponData) GetEquippedWeapons()
    {
        return (equippedMainWeapon, equippedSecondaryWeapon);
    }

    public ArmorData GetEquippedArmor()
    {
        return equippedArmor;
    }

    public BattleDetail AttackUnit(UnitController targetController, DamageEffect effect, int critChance)
    {
        int damage = GetAttack(effect);
        bool critical = false;
        if (Random.Range(0, 100) < critChance)
        {
            damage = (int)(damage * BattleController.critFactor);
            critical = true;
        }
        return targetController.TakeDamage(damage, effect, equippedMainWeapon, critical);
    }

    public BattleDetail HealUnit(UnitController targetController, HealEffect effect)
    {
        int healing = GetHealing(effect);
        return targetController.RestoreHealth(healing);
    }

    //Passive Damage
    public void TakeDamage(int damage, DamageElement damageElement)
    {
        DamageElement effectElement = damageElement;
        float damageMultiplier = GetDamageReduction(effectElement);
        int damageTaken = Mathf.FloorToInt(damage * damageMultiplier);
        if (damageTaken < 0)
        {
            damageTaken = 0;
        }
        unitData.ChangeCurrentHP(-damageTaken);
    }

    //Attack Damage
    public BattleDetail TakeDamage(int damage, DamageEffect effect, WeaponData attackerWeapon, bool critical)
    {
        //TODO: Reimplement elemental resistances
        int damageTaken = Mathf.FloorToInt(damage);
        if (validArmor)
        {
            damageTaken -= GetDefense(effect);
        }
        if (damageTaken < 0)
        {
            damageTaken = 0;
        }
        unitData.ChangeCurrentHP(-damageTaken);
        return new BattleDetail(damageTaken, unitData.currentHP <= 0, critical);
    }

    public BattleDetail RestoreHealth(int healing)
    {
        unitData.ChangeCurrentHP(healing);
        return new BattleDetail(healing);
    }

    public BattleDetail InflictStatus(Status newStatus, GameObject source)
    {
        statuses.Add(new StatusInstance(newStatus, source));
        if (newStatus.getAPMode())
        {
            actions += newStatus.getAPChange();
            if (actions < 0)
            {
                actions = 0;
            }
        }
        return new BattleDetail(newStatus);
    }

    public int GetHealing(HealEffect effect)
    {
        //TODO: Reimplement stat-based healing bonus
        int healing = effect.value;
        return healing;
    }

    public int GetAttack(DamageEffect effect)
    {
        int damage = 0;
        if (validMainWeapon)
        {
            damage += equippedMainWeapon.LoadWeaponData().GetWeaponStats()[0];
        }
        switch (effect.source)
        {
            case "phyiscal":
                damage += effect.value + GetStat("str");
                break;
            case "magic":
                damage += effect.value + GetStat("pot");
                break;
            case "adaptive":
                damage += effect.value + Mathf.Max(GetStat("str"), GetStat("pot"));
                break;
            case "neutral":
                damage += effect.value;
                break;
        }
        return damage;
    }

    public int GetDefense(DamageEffect effect)
    {
        int defense = 0;
        if (validArmor)
        {
            switch (effect.type)
            {
                case "physical":
                    defense += equippedArmor.LoadArmorData().GetDefenses()[0];
                    break;
                case "magic":
                    defense += equippedArmor.LoadArmorData().GetDefenses()[1];
                    break;
                case "adaptive":
                    defense += Mathf.Min(equippedArmor.LoadArmorData().GetDefenses()[0], equippedArmor.LoadArmorData().GetDefenses()[1]);
                    break;
            }
        }
        return defense;
    }

    public float GetDamageReduction(DamageElement attackElement)
    {
        if (validArmor)
        {
            return equippedArmor.LoadArmorData().GetElementResist(attackElement);
        }
        return 1f;
    }

    public string GetName()
    {
        return unitData.unitName;
    }

    public UnitData GetUnitData()
    {
        return unitData;
    }

    public int GetStat(string stat)
    {
        //TODO: Implement percent-based stat changes
        int movChange = 0;
        int strChange = 0;
        int potChange = 0;
        int acuChange = 0;
        int finChange = 0;
        int reaChange = 0;
        foreach (StatusInstance status in statuses)
        {
            if (status.getStatus().getStatChanges()[1] > movChange)
            {
                movChange = status.getStatus().getStatChanges()[1];
            }
            if (status.getStatus().getStatChanges()[2] > strChange)
            {
                strChange = status.getStatus().getStatChanges()[2];
            }
            if (status.getStatus().getStatChanges()[3] > potChange)
            {
                potChange = status.getStatus().getStatChanges()[3];
            }
            if (status.getStatus().getStatChanges()[4] > acuChange)
            {
                acuChange = status.getStatus().getStatChanges()[4];
            }
            if (status.getStatus().getStatChanges()[5] > finChange)
            {
                finChange = status.getStatus().getStatChanges()[5];
            }
            if (status.getStatus().getStatChanges()[6] > reaChange)
            {
                reaChange = status.getStatus().getStatChanges()[6];
            }
        }
        switch (stat)
        {
            case "mov":
                return Mathf.Max(0,unitData.mov - movChange);
            case "maxHP":
                return unitData.maxHP;
            case "currentHP":
                return unitData.currentHP;
            case "str":
                return Mathf.Max(0, unitData.str - strChange);
            case "pot":
                return Mathf.Max(0, unitData.pot - potChange);
            case "acu":
                return Mathf.Max(0, unitData.acu - acuChange);
            case "fin":
                return Mathf.Max(0, unitData.fin - finChange);
            case "rea":
                return Mathf.Max(0, unitData.rea - reaChange);
            default:
                Debug.LogError("Invalid stat: " + stat);
                return -1;
        }
    }

    public BattleTeam GetTeam()
    {
        return team;
    }

    public bool MoveUnit(Vector2 destination)
    {
        Vector2Int destinationTile = mapController.gridTilePos(destination);
        if (PathfinderValidCoords(battleController.GetActiveAbilityScript()).Contains(destinationTile))
        {
            SetUnitPos(destination);
            return true;
        }
        return false;
    }
     
    void SetUnitPos(Vector2 worldPos)
    {
        transform.position = new Vector3(worldPos.x, worldPos.y, -2);
        unitGridPosition = mapController.gridWorldPos(transform.position);
    }

    public Vector2Int GetUnitPos()
    {
        return unitGridPosition;
    }

    //TODO: Needed? See if can replace/remove
    public List<Vector2Int> PathfinderValidCoords(AbilityScript abilityScript)
    {
        int[] moveRanges = battleController.GetMoveRanges((abilityScript.targeting[0] as TileTargeting).range, gameObject);
        int moveMax = moveRanges[0];
        int moveMin = moveRanges[1];
        Pathfinder pathfinder = new Pathfinder(unitGridPosition, moveMax, moveMin, mapController);
        return pathfinder.getValidCoords();
    }
}
