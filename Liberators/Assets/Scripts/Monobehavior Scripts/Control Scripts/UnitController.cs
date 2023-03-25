using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    //Control Variables
    public GameObject marker;
    Vector2Int unitGridPosition;
    Grid mainGrid;
    MapController mapController;
    BattleController battleController;
    List<GameObject> markerList = new List<GameObject>();
    public BattleTeam team;

    public Unit unitTemplate;
    UnitInstance unitObject;
    string unitName;
    int mov;
    int maxHP;
    int currentHP;
    int str;
    int pot;
    int acu;
    int fin;
    int rea;

    //Unit Properties
    int maxActions = 3;
    int actions = 3;

    public Ability basicMovement;
    public Ability passUnitAbility;
    public Ability endTurnAbility;

    public WeaponInstance equippedMainHandWeapon;
    public WeaponInstance equippedOffHandWeapon;
    public ArmorInstance equippedArmor;

    List<Ability> allAbilities = new List<Ability>();

    List<StatusInstance> statuses = new List<StatusInstance>();

    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        battleController = mainGrid.GetComponentsInChildren<BattleController>()[0];
        unitGridPosition = mapController.gridWorldPos(transform.position);
        battleController.AddUnit(this.gameObject);
        if (unitObject == null)
        {
            unitObject = new UnitInstance(unitTemplate);
        }
        createUnit(unitObject.getStats(), team, unitObject.getCurrentHP());
        unitName = unitObject.getUnitName();
        if (basicMovement)
        {
            allAbilities.Add(basicMovement);
        }
        if (equippedMainHandWeapon != null && !equippedMainHandWeapon.NullCheckBase())
        {
            allAbilities.AddRange(equippedMainHandWeapon.GetInstanceAbilities());
        }
        if (equippedOffHandWeapon != null && !equippedOffHandWeapon.NullCheckBase())
        {
            allAbilities.AddRange(equippedOffHandWeapon.GetInstanceAbilities());
        }
        allAbilities.AddRange(unitObject.getAbilities());
    }

    public void createUnit(int[] unitStats, BattleTeam team, int startingHP)
    {
        maxHP = unitStats[0];
        mov = unitStats[1];
        currentHP = startingHP;
        str = unitStats[2];
        pot = unitStats[3];
        acu = unitStats[4];
        fin = unitStats[5];
        rea = unitStats[6];
        this.team = team;
    }

    public void setUnitInstance(UnitInstance unitInstance)
    {
        unitObject = unitInstance;
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
                if (currentHP <= 0)
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

    public (WeaponInstance, WeaponInstance) GetEquippedWeapons()
    {
        return (equippedMainHandWeapon, equippedOffHandWeapon);
    }

    public ArmorInstance GetEquippedArmor()
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
        return targetController.TakeDamage(damage, effect, equippedMainHandWeapon, critical);
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
        currentHP -= damageTaken;
        unitObject.setCurrentHP(currentHP);
    }

    //Attack Damage
    public BattleDetail TakeDamage(int damage, DamageEffect effect, WeaponInstance attackerWeapon, bool critical)
    {
        //TODO: Reimplement elemental resistances
        int damageTaken = Mathf.FloorToInt(damage);
        if (equippedArmor != null && !equippedArmor.NullCheckBase())
        {
            damageTaken -= GetDefense(effect);
        }
        if (damageTaken < 0)
        {
            damageTaken = 0;
        }
        currentHP -= damageTaken;
        unitObject.setCurrentHP(currentHP);
        return new BattleDetail(damage, currentHP <= 0, critical);
    }

    public BattleDetail RestoreHealth(int healing)
    {
        currentHP += healing;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        unitObject.setCurrentHP(currentHP);
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
        if (equippedMainHandWeapon != null && !equippedMainHandWeapon.NullCheckBase())
        {
            damage += equippedMainHandWeapon.GetInstanceWeaponStats()[0];
        }
        switch (effect.source)
        {
            case "phyiscal":
                damage += effect.value + str;
                break;
            case "magic":
                damage += effect.value + pot;
                break;
            case "adaptive":
                damage += effect.value + Mathf.Max(str,pot);
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
        if (equippedArmor != null && !equippedArmor.NullCheckBase())
        {
            switch (effect.type)
            {
                case "physical":
                    defense += equippedArmor.GetInstanceDefenses()[0];
                    break;
                case "magic":
                    defense += equippedArmor.GetInstanceDefenses()[1];
                    break;
                case "adaptive":
                    defense += Mathf.Min(equippedArmor.GetInstanceDefenses()[0], equippedArmor.GetInstanceDefenses()[1]);
                    break;
            }
        }
        return defense;
    }

    public float GetDamageReduction(DamageElement attackElement)
    {
        if (equippedArmor != null && !equippedArmor.NullCheckBase())
        {
            return equippedArmor.GetInstanceElementResist(attackElement);
        }
        return 1f;
    }

    public string GetName()
    {
        return unitName;
    }

    public UnitInstance GetUnitInstance()
    {
        return unitObject;
    }

    public int[] GetHealth()
    {
        return new int[] { maxHP, currentHP };
    }

    public int[] GetStats()
    {
        return new int[] { mov, maxHP, currentHP, str, pot, acu, fin, rea };
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
