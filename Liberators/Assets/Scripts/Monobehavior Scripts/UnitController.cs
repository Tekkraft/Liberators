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
        if (equippedWeapon)
        {
            allAbilities.AddRange(equippedWeapon.getAbilities());
        }
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
        mapController.pathfinder.changeParameters(unitGridPosition, mapController.finalRange(mov, moveAbility), moveAbility.getAbilityRanges()[1]);
        mapController.pathfinder.calculate(true);
        if (mapController.pathfinder.checkCoords(destinationTile))
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
        destroyMarkers();
    }

    public Vector2Int getUnitPos()
    {
        return unitGridPosition;
    }

    public void createMarkers(int maxRange, int minRange, MarkerController.Markers color, bool passableState)
    {
        mapController.pathfinder.changeParameters(unitGridPosition, maxRange, minRange);
        mapController.pathfinder.calculate(passableState);
        List<Vector2Int> coords = mapController.pathfinder.getValidCoords();
        foreach(Vector2Int gridPos in coords)
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
}
