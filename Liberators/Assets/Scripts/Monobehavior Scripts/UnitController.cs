using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    //Enums
    public enum MarkerAreas { RADIAL, BOX, CROSS };

    //Control Variables
    public GameObject marker;
    Vector2 unitPosition;
    Vector2Int unitGridPosition;
    Grid mainGrid;
    MapController mapController;
    List<GameObject> markerList = new List<GameObject>();
    public int teamNumber = -1;

    public Unit unitObject;
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

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        unitPosition = mapController.tileWorldPos(transform.position);
        unitPosition.y--;
        unitGridPosition = mapController.gridTilePos(unitPosition);
        mapController.addUnit(this.gameObject);
        createUnit(unitObject.getStats(), teamNumber);
        allAbilities.Add(basicMovement);
        allAbilities.AddRange(equippedWeapon.getAbilities());
        allAbilities.AddRange(unitObject.getAbilities());
    }

    // Update is called once per frame
    void Update()
    {

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

    public bool attackUnit(GameObject target, Ability attackAbility)
    {
        int damage = 0;
        if (equippedWeapon)
        {
            damage += equippedWeapon.getWeaponStats()[0];
        }
        switch (attackAbility.getAbilityDamageSource())
        {
            case Ability.damageType.PHYSICAL:
                damage += attackAbility.getAbilityDamage() + str;
                break;
            case Ability.damageType.MAGIC:
                damage += attackAbility.getAbilityDamage() + pot;
                break;
            case Ability.damageType.TRUE:
                damage += attackAbility.getAbilityDamage();
                break;
        }
        return target.GetComponent<UnitController>().takeDamage(damage, attackAbility.getAbilityDamageType());
    }

    public bool takeDamage(int damage, Ability.damageType damageType)
    {
        int damageTaken = damage;
        if (equippedArmor)
        {
            switch (damageType)
            {
                case Ability.damageType.PHYSICAL:
                    damageTaken -= equippedArmor.getDefenses()[0];
                    break;
                case Ability.damageType.MAGIC:
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

    public int[] getStats()
    {
        return new int[] { mov, maxHP, currentHP, str, pot, acu, fin, rea };
    }

    public int getTeam()
    {
        return teamNumber;
    }

    public bool moveUnit(Vector2 destination, Ability moveAbility)
    {
        Vector2Int destinationTile = mapController.gridTilePos(destination);
        if (inRange(moveAbility.getAbilityRangeType(), mapController.finalRange(mov, moveAbility), moveAbility.getAbilityRanges()[1], destinationTile))
        {
            setUnitPos(destination);
            return true;
        }
        return false;
    }

    void setUnitPos(Vector2 worldPos)
    {
        transform.position = worldPos;
        unitGridPosition = mapController.gridWorldPos(transform.position);
        transform.Translate(new Vector3(0, 0.75f, -2));
        destroyMarkers();
    }

    public Vector2Int getUnitPos()
    {
        return unitGridPosition;
    }

    public void createMarkers(MarkerAreas area, int maxRange, int minRange, MarkerController.Markers color)
    {
        for (int i = -maxRange; i <= maxRange; i++)
        {
            for (int j = -maxRange; j <= maxRange; j++)
            {
                if (inRange(area, maxRange, minRange, new Vector2Int(i, j)))
                {
                    GameObject temp = GameObject.Instantiate(marker);
                    Vector2 markerLocation = mapController.tileGridPos(unitGridPosition + new Vector2Int(i, j));
                    temp.GetComponent<MarkerController>().setup(color, markerLocation);
                    markerList.Add(temp);
                }
            }
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

    //Calculates whether a given location is within range of the specified range parameters.
    public bool inRange(MarkerAreas area, int maxRange, int minRange, Vector2Int location)
    {
        switch (area)
        {
            case MarkerAreas.RADIAL:
                int distance = Mathf.Abs(location.x) + Mathf.Abs(location.y);
                return (distance <= maxRange && distance >= minRange);
            case MarkerAreas.BOX:
                return (Mathf.Abs(location.x) >= minRange || Mathf.Abs(location.y) >= minRange) && (Mathf.Abs(location.x) <= maxRange && Mathf.Abs(location.y) <= maxRange);
            case MarkerAreas.CROSS:
                return ((location.x == unitGridPosition.x && Mathf.Abs(location.y) >= minRange) || (location.y == unitGridPosition.y && Mathf.Abs(location.x) >= minRange));
        }
        return false;
    }
}
