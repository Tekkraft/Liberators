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
    List<GameObject> markerList = new List<GameObject>();
    public int teamNumber = -1;

    public Unit unitObject;
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
    public Weapon equippedWeapon;
    public Armor equippedArmor;
    List<Ability> allAbilities = new List<Ability>();

    List<StatusInstance> statuses = new List<StatusInstance>();

    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        unitGridPosition = mapController.gridWorldPos(transform.position);
        mapController.addUnit(this.gameObject);
        createUnit(unitObject.getStats(), teamNumber);
        this.unitName = unitObject.getUnitName();
        allAbilities.Add(basicMovement);
        if (equippedWeapon)
        {
            allAbilities.AddRange(equippedWeapon.getAbilities());
        }
        allAbilities.AddRange(unitObject.getAbilities());
    }

    public void createUnit(int[] unitStats, int teamNumber)
    {
        this.maxHP = unitStats[0];
        this.mov = unitStats[1];
        currentHP = this.maxHP;
        this.str = unitStats[2];
        this.pot = unitStats[3];
        this.acu = unitStats[4];
        this.fin = unitStats[5];
        this.rea = unitStats[6];
        this.teamNumber = teamNumber;
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
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status linkedStatus = statuses[i].getStatus();
            int[] healthOverTime = linkedStatus.getHealthOverTime();
            if (healthOverTime[0] > 0)
            {
                bool lethal = takeDamage(healthOverTime[0], damageType.TRUE);
                if (lethal)
                {
                    return true;
                }
            }
            if (healthOverTime[1] > 0)
            {
                restoreHealth(healthOverTime[1]);
            }
            bool expired = statuses[i].update();
            if (expired)
            {
                statuses.Remove(statuses[i]);
            }
        }
        return false;
    }

    public List<Ability> getAbilities()
    {
        return allAbilities;
    }

    public Weapon getEquippedWeapon()
    {
        return equippedWeapon;
    }

    public Armor getEquippedArmor()
    {
        return equippedArmor;
    }

    public bool attackUnit(UnitController targetController, CombatAbility attackAbility, int critChance)
    {
        int damage = getAttack(attackAbility);
        if (Random.Range(0, 100) < critChance)
        {
            damage = (int) (damage * MapController.critFactor);
        }
        return targetController.takeDamage(damage, attackAbility.getAbilityDamageType());
    }

    public bool takeDamage(int damage, damageType damageType)
    {
        int damageTaken = damage;
        if (equippedArmor)
        {
            switch (damageType)
            {
                case damageType.PHYSICAL:
                    damageTaken -= equippedArmor.getDefenses()[0];
                    break;
                case damageType.MAGIC:
                    damageTaken -= equippedArmor.getDefenses()[1];
                    break;
            }
        }
        if (damageTaken < 0)
        {
            damageTaken = 0;
        }
        currentHP -= damageTaken;
        return currentHP <= 0;
    }

    public void restoreHealth(int healing)
    {
        currentHP += healing;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public void inflictStatus(Status newStatus, GameObject source)
    {
        statuses.Add(new StatusInstance(newStatus, source));
    }

    public int getAttack(CombatAbility attackAbility)
    {
        int damage = 0;
        if (equippedWeapon)
        {
            damage += equippedWeapon.getWeaponStats()[0];
        }
        switch (attackAbility.getAbilityDamageSource())
        {
            case damageType.PHYSICAL:
                damage += attackAbility.getAbilityDamage() + str;
                break;
            case damageType.MAGIC:
                damage += attackAbility.getAbilityDamage() + pot;
                break;
            case damageType.TRUE:
                damage += attackAbility.getAbilityDamage();
                break;
        }
        damage = (int)(damage * ((attackAbility.getAbilityDamageBonus() / 100f) + 1f));
        return damage;
    }

    public int getExpectedDamage(UnitController targetController, CombatAbility attackAbility)
    {
        int damage = 0;
        if (equippedWeapon)
        {
            damage += equippedWeapon.getWeaponStats()[0];
        }
        switch (attackAbility.getAbilityDamageSource())
        {
            case damageType.PHYSICAL:
                damage += attackAbility.getAbilityDamage() + str;
                break;
            case damageType.MAGIC:
                damage += attackAbility.getAbilityDamage() + pot;
                break;
            case damageType.TRUE:
                damage += attackAbility.getAbilityDamage();
                break;
        }
        damage = (int)(damage * ((attackAbility.getAbilityDamageBonus() / 100f) + 1f));
        damage -= targetController.getDefense(attackAbility);
        if (damage < 0)
        {
            damage = 0;
        }
        return damage;
    }

    public int getDefense(CombatAbility attackAbility)
    {
        int defense = 0;
        if (equippedArmor)
        {
            switch (attackAbility.getAbilityDamageType())
            {
                case damageType.PHYSICAL:
                    defense += equippedArmor.getDefenses()[0];
                    break;
                case damageType.MAGIC:
                    defense += equippedArmor.getDefenses()[1];
                    break;
            }
        }
        return defense;
    }

    public string getName()
    {
        return unitName;
    }

    public int[] getStats()
    {
        return new int[] { mov, maxHP, currentHP, str, pot, acu, fin, rea };
    }

    public int getTeam()
    {
        return teamNumber;
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
        mapController.getPathfinder().changeParameters(unitGridPosition, mapController.finalRange(mov, moveAbility), moveAbility.getMinMoveRange());
        mapController.getPathfinder().calculate();
        return mapController.getPathfinder().getValidCoords();
    }
}
