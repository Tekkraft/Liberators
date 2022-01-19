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

    public int range = 4;
    public MarkerAreas attackArea = MarkerAreas.RADIAL;

    public Unit unitObject;
    int movementRange;
    int maxHP;
    int currentHP;
    int str;
    int pot;
    int acu;
    int fin;
    int rea;

    //Unit Properties
    int maxActions = 2;
    int actions = 2;

    public Weapon equippedWeapon;

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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void createUnit(int[] unitStats, int teamNumber)
    {
        this.maxHP = unitStats[0];
        this.movementRange = unitStats[1];
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

    public bool attackUnit(GameObject target)
    {
        return target.GetComponent<UnitController>().takeDamage(str);
    }

    public bool takeDamage(int damage)
    {
        currentHP -= damage;
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
        return new int[] { movementRange, maxHP, currentHP, str, pot, acu, fin, rea };
    }

    public int getRange()
    {
        return range;
    }

    public MarkerAreas getAttackArea()
    {
        return attackArea;
    }

    public int getTeam()
    {
        return teamNumber;
    }

    public bool moveUnit(Vector2 destination)
    {
        Vector2Int destinationTile = mapController.gridTilePos(destination);
        int distance = mapController.gridDistance(destinationTile, unitGridPosition);
        if (distance <= movementRange)
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

    public bool inRange(MarkerAreas area, int maxRange, int minRange, Vector2Int location)
    {
        switch (area)
        {
            case MarkerAreas.RADIAL:
                int distance = mapController.gridDistance(location + unitGridPosition, unitGridPosition);
                return (distance <= maxRange && distance >= minRange);
            case MarkerAreas.BOX:
                return (Mathf.Abs(location.x) >= minRange && Mathf.Abs(location.y) >= minRange);
            case MarkerAreas.CROSS:
                return ((location.x == 0 && Mathf.Abs(location.y) >= minRange) || (location.y == 0 && Mathf.Abs(location.x) >= minRange));
        }
        return false;
    }
}
