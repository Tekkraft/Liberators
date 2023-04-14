using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    //Static Variables
    static float timeToCrossTile = 1.5f;

    //Instance Variables
    float baseSpeed;
    List<UnitData> unitDataList;
    PlayerUnitDataList playerUnitDataList;
    List<Vector2Int> spawnLocations;
    OperationsTeam team;
    OperationsMoveType movementType;
    bool squadAnchored = false;
    string overrideBattlefield;

    Grid mainGrid;
    MapController mapController;
    OperationController operationController;

    //Temp Variables
    Vector2Int currentMove = Vector2Int.zero;
    float moveProgress = 0;
    Coroutine activeMove;
    List<Vector2Int> queuedMove = new List<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        operationController = mainGrid.GetComponentsInChildren<OperationController>()[0];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public SquadData getSquadData()
    {
        //TODO: Possible overlap problem if transform snapping not addressed
        Vector3 snapTransform = transform.position = new Vector3(Mathf.RoundToInt(transform.position.x + 0.5f) - 0.5f, Mathf.RoundToInt(transform.position.y + 0.5f) - 0.5f, -1);
        return new SquadData(baseSpeed, unitDataList, spawnLocations, team, snapTransform, gameObject.name, gameObject.GetComponent<SquadAIController>().squadAI, squadAnchored, overrideBattlefield);
    }

    public void loadSquadData(SquadData data)
    {
        baseSpeed = data.baseSpeed;
        unitDataList = data.unitDataList;
        spawnLocations = data.spawnLocations;
        team = data.team;
        transform.position = new Vector3(data.position.x, data.position.y, -1);
        gameObject.name = data.name;
        gameObject.GetComponent<SquadAIController>().squadAI = data.squadAI;
    }

    public void moveSquad(List<Vector2Int> directions)
    {
        if (activeMove == null)
        {
            activeMove = StartCoroutine(stepSquadMove(directions));
        } else
        {
            StopCoroutine(activeMove);
            activeMove = null;
            activeMove = StartCoroutine(finishMove());
            //TODO: FIX ISSUE WITH SETTING NEW MOVE
            //queuedMove = directions;
        }
    }

    IEnumerator stepSquadMove(List<Vector2Int> directions)
    {
        foreach (Vector2Int move in directions)
        {
            currentMove = move;
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
                moveProgress = movement;
                yield return new WaitForEndOfFrame();
            }
            transform.position = new Vector3(Mathf.RoundToInt(transform.position.x + 0.5f) - 0.5f, Mathf.RoundToInt(transform.position.y + 0.5f) - 0.5f, -1);
        }
        currentMove = Vector2Int.zero;
        activeMove = null;
    }

    IEnumerator finishMove()
    {
        Vector3 moveData = new Vector3(currentMove.x, currentMove.y, 0);
        float movement = moveProgress;
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
        activeMove = null;
        if (queuedMove.Count > 0)
        {
            moveSquad(new List<Vector2Int>(queuedMove));
            queuedMove.Clear();
        }
    }

    public bool IsMoving()
    {
        return activeMove != null;
    }

    public List<UnitData> GetUnitList()
    {
        return unitDataList;
    }

    public PlayerUnitDataList GetPlayerUnitDataList()
    {
        return playerUnitDataList;
    }

    public OperationsTeam GetTeam()
    {
        return team;
    }

    public OperationController GetOperationController()
    {
        return operationController;
    }

    public bool IsSquadAnchored()
    {
        return squadAnchored;
    }

    public string GetOverrideBattlefield()
    {
        return overrideBattlefield;
    }
}

public class SquadData
{
    public float baseSpeed { get; }
    public List<UnitData> unitDataList { get; set; }
    public List<Vector2Int> spawnLocations { get; }
    public OperationsTeam team { get; }
    public Vector2 position { get; }
    public string name { get; }
    public OperationsAI squadAI { get; }
    public bool squadAnchored { get; }
    public string overrideBattlefield { get; }

    public SquadData(float baseSpeed, List<UnitData> unitDataList, List<Vector2Int> spawnLocations, OperationsTeam team, Vector2 position, string name, OperationsAI squadAI, bool squadAnchored, string overrideBattlefield)
    {
        this.baseSpeed = baseSpeed;
        this.unitDataList = unitDataList;
        this.spawnLocations = spawnLocations;
        this.team = team;
        this.position = position;
        this.name = name;
        this.squadAI = squadAI;
        this.squadAnchored = squadAnchored;
        this.overrideBattlefield = overrideBattlefield;
    }

    public Dictionary<UnitData, Vector2Int> getPairedUnits()
    {
        Dictionary<UnitData, Vector2Int> retVal = new Dictionary<UnitData, Vector2Int>();
        for (int i = 0; i < unitDataList.Count && i < spawnLocations.Count; i++)
        {
            retVal.Add(unitDataList[i], spawnLocations[i]);
        }
        return retVal;
    }
}