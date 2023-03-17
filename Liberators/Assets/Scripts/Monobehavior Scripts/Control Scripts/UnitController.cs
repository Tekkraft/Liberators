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
                takeDamage(linkedStatus.getHealthOverTime()[0], linkedStatus.getHealthOverTimeElement());
                if (currentHP <= 0)
                {
                    return true;
                }
            }
            if (linkedStatus.getHealthOverTime()[1] > 0)
            {
                restoreHealth(linkedStatus.getHealthOverTime()[1]);
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

    public void attackUnit(UnitController targetController, DamageEffect effect, int critChance)
    {
        int damage = getAttack(effect);
        if (Random.Range(0, 100) < critChance)
        {
            damage = (int)(damage * BattleController.critFactor);
        }
        targetController.takeDamage(damage, effect, equippedMainHandWeapon);
    }

    public void healUnit(UnitController targetController, HealEffect effect)
    {
        int healing = getHealing(effect);
        targetController.restoreHealth(healing);
    }

    //Passive Damage
    public KeyValuePair<int, int> takeDamage(int damage, DamageElement damageElement)
    {
        int startingHP = currentHP;
        DamageElement effectElement = damageElement;
        float damageMultiplier = getDamageReduction(effectElement);
        int damageTaken = Mathf.FloorToInt(damage * damageMultiplier);
        if (damageTaken < 0)
        {
            damageTaken = 0;
        }
        currentHP -= damageTaken;
        unitObject.setCurrentHP(currentHP);
        return new KeyValuePair<int, int>(damageTaken, startingHP);
    }

    //Attack Damage
    public void takeDamage(int damage, DamageEffect effect, WeaponInstance attackerWeapon)
    {
        //TODO: Reimplement elemental resistances
        int damageTaken = Mathf.FloorToInt(damage);
        if (equippedArmor != null && !equippedArmor.NullCheckBase())
        {
            damageTaken -= getDefense(effect);
        }
        if (damageTaken < 0)
        {
            damageTaken = 0;
        }
        currentHP -= damageTaken;
        unitObject.setCurrentHP(currentHP);
    }

    public void restoreHealth(int healing)
    {
        currentHP += healing;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
        unitObject.setCurrentHP(currentHP);
    }

    public void inflictStatus(Status newStatus, GameObject source)
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
    }

    public int getHealing(HealEffect effect)
    {
        //TODO: Reimplement stat-based healing bonus
        int healing = effect.value;
        return healing;
    }

    public int getAttack(DamageEffect effect)
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

    public int getDefense(DamageEffect effect)
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

    public float getDamageReduction(DamageElement attackElement)
    {
        if (equippedArmor != null && !equippedArmor.NullCheckBase())
        {
            return equippedArmor.GetInstanceElementResist(attackElement);
        }
        return 1f;
    }

    public string getName()
    {
        return unitName;
    }

    public UnitInstance getUnitInstance()
    {
        return unitObject;
    }

    public int[] getHealth()
    {
        return new int[] { maxHP, currentHP };
    }

    public int[] getStats()
    {
        return new int[] { mov, maxHP, currentHP, str, pot, acu, fin, rea };
    }

    public BattleTeam getTeam()
    {
        return team;
    }

    public bool moveUnit(Vector2 destination, MovementAbility moveAbility)
    {
        Vector2Int destinationTile = mapController.gridTilePos(destination);
        if (pathfinderValidCoords(moveAbility).Contains(destinationTile))
        {
            setUnitPos(destination);
            return true;
        }
        return false;
    }
     
    void setUnitPos(Vector2 worldPos)
    {
        transform.position = new Vector3(worldPos.x, worldPos.y, -2);
        unitGridPosition = mapController.gridWorldPos(transform.position);
        destroyMarkers();
    }

    public Vector2Int getUnitPos()
    {
        return unitGridPosition;
    }

    //TODO: Move marker drawing to UIController, not UnitController
    public void createMoveMarkers(MovementAbility activeAbility, MarkerController.Markers color)
    {
        List<Vector2Int> coords = pathfinderValidCoords(activeAbility);
        foreach (Vector2Int gridPos in coords)
        {
            GameObject temp = GameObject.Instantiate(marker);
            Vector2 markerLocation = mapController.tileGridPos(gridPos);
            temp.GetComponent<MarkerController>().setup(color, markerLocation);
            markerList.Add(temp);
        }
    }

    public void createAttackMarkers(List<Vector2Int> coords, MarkerController.Markers color)
    {
        foreach (Vector2Int gridPos in coords)
        {
            GameObject temp = GameObject.Instantiate(marker);
            Vector2 markerLocation = mapController.tileGridPos(gridPos);
            temp.GetComponent<MarkerController>().setup(color, markerLocation);
            markerList.Add(temp);
        }
    }

    public void destroyMarkers()
    {
        for (int i = 0; i < markerList.Count; i = 0)
        {
            GameObject temp = markerList[0];
            markerList.Remove(temp);
            temp.GetComponent<MarkerController>().removeMarker();
        }
    }

    public List<Vector2Int> pathfinderValidCoords(MovementAbility moveAbility)
    {
        mapController.getPathfinder().changeParameters(unitGridPosition, battleController.FinalRange(mov, moveAbility), moveAbility.getMinMoveRange());
        mapController.getPathfinder().calculate();
        return mapController.getPathfinder().getValidCoords();
    }
}
