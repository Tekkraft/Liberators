using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    //Static Variables
    static float timeToCrossTile = 1.5f;

    //Instance Variables
    public float baseSpeed;
    public List<UnitEntryData> unitList;
    public List<Vector2Int> spawnLocations;
    bool movementActive = false;
    public int team;

    Grid mainGrid;
    MapController mapController;

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public SquadData getSquadData()
    {
        //TODO: Possible overlap problem if transform snapping not addressed
        Vector3 snapTransform = transform.position = new Vector3(Mathf.RoundToInt(transform.position.x + 0.5f) - 0.5f, Mathf.RoundToInt(transform.position.y + 0.5f) - 0.5f, -1);
        return new SquadData(baseSpeed, unitList, spawnLocations, team, snapTransform, gameObject.name);
    }

    public void loadSquadData(SquadData data)
    {
        baseSpeed = data.baseSpeed;
        unitList = data.unitList;
        foreach (UnitEntryData dataEntry in unitList)
        {
            if (dataEntry.getUnit() == null)
            {
                dataEntry.reconstruct();
            }
        }
        spawnLocations = data.spawnLocations;
        team = data.team;
        transform.position = new Vector3(data.position.x, data.position.y, -1);
        gameObject.name = data.name;
    }

    public void moveSquad(List<Vector2Int> directions)
    {
        if (!movementActive)
        {
            StartCoroutine(stepSquadMove(directions));
        }
    }

    IEnumerator stepSquadMove(List<Vector2Int> directions)
    {
        movementActive = true;
        foreach (Vector2Int move in directions)
        {
            Vector3 moveData = new Vector3(move.x, move.y, 0);
            float movement = 0;
            Vector2Int gridApprox = mapController.gridTilePos(transform.position);
            while (movement <= 0.999f)
            {
                //TODO: Possible edge case around EXTREME frame rate delay where moveStep > 1?
                float terrainMove = (mapController.getTileAtPos(new Vector3Int(gridApprox.x, gridApprox.y, 0)) as TerrainTileWorld).getMovementFactor();
                float moveSpeed = timeToCrossTile / (baseSpeed / terrainMove);
                Vector3 moveStep = (moveData / moveSpeed) * Time.deltaTime;
                transform.Translate(moveStep);
                movement += moveStep.magnitude;
                yield return new WaitForEndOfFrame();
            }
            transform.position = new Vector3(Mathf.RoundToInt(transform.position.x + 0.5f) - 0.5f, Mathf.RoundToInt(transform.position.y + 0.5f) - 0.5f, -1);
        }
        movementActive = false;
    }

    public List<UnitEntryData> getUnitList()
    {
        return unitList;
    }

    public int getTeam()
    {
        return team;
    }
}

public class SquadData
{
    public float baseSpeed;
    public List<UnitEntryData> unitList;
    public List<Vector2Int> spawnLocations;
    public int team;
    public Vector2 position;
    public string name;

    public SquadData(float baseSpeed, List<UnitEntryData> unitList, List<Vector2Int> spawnLocations, int team, Vector2 position, string name)
    {
        this.baseSpeed = baseSpeed;
        this.unitList = unitList;
        this.spawnLocations = spawnLocations;
        this.team = team;
        this.position = position;
        this.name = name;
    }

    public Dictionary<UnitEntryData, Vector2Int> getPairedUnits()
    {
        Dictionary<UnitEntryData, Vector2Int> retVal = new Dictionary<UnitEntryData, Vector2Int>();
        for (int i = 0; i < unitList.Count && i < spawnLocations.Count; i++)
        {
            retVal.Add(unitList[i], spawnLocations[i]);
        }
        return retVal;
    }
}