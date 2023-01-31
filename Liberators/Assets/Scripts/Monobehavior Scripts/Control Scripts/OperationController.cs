using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class OperationController : MonoBehaviour
{
    OperationsData opsData;
    OperationPathfinder pathfinder;
    List<GameObject> squads = new List<GameObject>();

    public Sprite playerFlag;
    public Sprite enemyFlag;

    public GameObject squadPrefab;

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = new OperationPathfinder(gameObject.GetComponent<MapController>());
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < squads.Count; i++)
        {
            for (int j = i+1; j < squads.Count; j++)
            {
                checkSkirmish(squads[i], squads[j]);
            }
        }
        Debug.Log(checkVictory());
    }

    void OnEnable()
    {
        //Load in all units
        foreach(SquadData data in OperationSceneHandler.squadDataList)
        {
            GameObject temp = GameObject.Instantiate(squadPrefab);
            temp.GetComponent<SquadController>().loadSquadData(data);
            if (data.team == operationsTeam.PLAYER)
            {
                temp.GetComponent<SpriteRenderer>().sprite = playerFlag;
            }
            if (data.team == operationsTeam.ENEMY)
            {
                temp.GetComponent<SpriteRenderer>().sprite = enemyFlag;
            }
            squads.Add(temp);
        }
        List<Vector3> options = new List<Vector3>();
        //Handle battle results if present
        switch (OperationSceneHandler.battleOutcome)
        {
            case battleOutcome.VICTORY:
                destroySquad(squads[OperationSceneHandler.defenderId]);
                break;

            case battleOutcome.SUCCESS:
                options = availableTranslations(squads[OperationSceneHandler.defenderId].transform.position);
                if (options.Count == 0)
                {
                    destroySquad(squads[OperationSceneHandler.defenderId]);
                    break;
                }
                squads[OperationSceneHandler.defenderId].transform.Translate(options[Random.Range(0, options.Count)]);
                break;

            case battleOutcome.FAILURE:
                options = availableTranslations(squads[OperationSceneHandler.attackerId].transform.position);
                if (options.Count == 0)
                {
                    destroySquad(squads[OperationSceneHandler.attackerId]);
                    break;
                }
                squads[OperationSceneHandler.attackerId].transform.Translate(options[Random.Range(0,options.Count)]);
                break;

            case battleOutcome.ROUTED:
                destroySquad(squads[OperationSceneHandler.attackerId]);
                break;
        }
        //Load operations data
        opsData = OperationSceneHandler.data;
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }

    public void moveSquadToLocation(GameObject squad, Vector2Int cell)
    {
        pathfinder.runCalculations(gameObject.GetComponent<MapController>().gridTilePos(squad.transform.position));
        List<Vector2Int> directions = pathfinder.getPathToCell(cell);
        if (directions == null)
        {
            return;
        }
        squad.GetComponent<SquadController>().moveSquad(directions);
    }

    void checkSkirmish(GameObject unit1, GameObject unit2)
    {
        if ((unit1.GetComponent<SquadController>().getTeam() == operationsTeam.ENEMY && (unit2.GetComponent<SquadController>().getTeam() == operationsTeam.PLAYER || unit2.GetComponent<SquadController>().getTeam() == operationsTeam.ALLY)) || (unit2.GetComponent<SquadController>().getTeam() == operationsTeam.ENEMY && (unit1.GetComponent<SquadController>().getTeam() == operationsTeam.PLAYER || unit1.GetComponent<SquadController>().getTeam() == operationsTeam.ALLY)))
        {
            if (Vector3.Distance(unit1.transform.position, unit2.transform.position) <= 2f / 3f)
            {
                Tilemap operationsTilemap = gameObject.GetComponent<Tilemap>();
                Vector2Int tileCoord = gameObject.GetComponent<MapController>().gridTilePos(unit1.transform.position);
                TerrainTileWorld tile = operationsTilemap.GetTile<TerrainTileWorld>(new Vector3Int(tileCoord.x, tileCoord.y, 0));
                startSkirmish(unit1, unit2, tile);
            }
        }
    }

    void startSkirmish(GameObject squad, GameObject opponent, TerrainTileWorld battlefield)
    {
        OperationSceneHandler.reset();
        OperationSceneHandler.attackerData = squad.GetComponent<SquadController>().getSquadData();
        OperationSceneHandler.attackerId = squads.IndexOf(squad);
        OperationSceneHandler.defenderData = opponent.GetComponent<SquadController>().getSquadData();
        OperationSceneHandler.defenderId = squads.IndexOf(opponent);
        OperationSceneHandler.battleScene = battlefield.getBattleMaps()[Random.Range(0,battlefield.getBattleMaps().Count)];
        foreach (GameObject temp in squads)
        {
            OperationSceneHandler.squadDataList.Add(temp.GetComponent<SquadController>().getSquadData());
        }
        SceneManager.LoadSceneAsync("BattlePrep");
    }

    //Squad Management
    void destroySquad(GameObject target)
    {
        squads.Remove(target);
        GameObject.Destroy(target);
    }

    public GameObject getSquadOfTeam(operationsTeam targetTeam)
    {
        List<GameObject> validTargets = new List<GameObject>();
        foreach (GameObject temp in squads)
        {
            if (temp.GetComponent<SquadController>().team == targetTeam)
            {
                validTargets.Add(temp);
            }
        }
        return validTargets[Random.Range(0, validTargets.Count)];
    }

    //Win-Lose Checks
    bool checkVictory()
    {
        foreach (OperationsReachCondition condition in opsData.getPlayerReachWinConditions())
        {
            return checkReachCondition(condition);
        }
        foreach (OperationsReachCondition condition in opsData.getEnemyReachWinConditions())
        {
            return checkReachCondition(condition);
        }
        return false;
    }

    //Helper Functions
    List<Vector3> availableTranslations(Vector3 coords)
    {
        List<Vector3> possibleCoords = new List<Vector3>();
        possibleCoords.Add(Vector3.up);
        possibleCoords.Add(Vector3.down);
        possibleCoords.Add(Vector3.left);
        possibleCoords.Add(Vector3.right);

        Tilemap operationsTilemap = gameObject.GetComponent<Tilemap>();

        for (int i = possibleCoords.Count - 1; i >= 0; i--)
        {
            Vector3 coord = coords + possibleCoords[i];
            Vector2Int tileCoord = gameObject.GetComponent<MapController>().gridTilePos(coord);
            if (!operationsTilemap.GetTile<TerrainTileWorld>(new Vector3Int(tileCoord.x, tileCoord.y, 0)).isPassable())
            {
                possibleCoords.Remove(possibleCoords[i]);
                continue;
            }
            foreach (GameObject checkSquad in squads)
            {
                if ((coord - checkSquad.transform.position).magnitude <= 0.25)
                {
                    possibleCoords.Remove(possibleCoords[i]);
                    break;
                }
            }
        }

        return possibleCoords;
    }

    bool checkCorners(Vector2Int corner1, Vector2Int corner2, Vector2Int position)
    {
        if ((position.x <= corner1.x && position.x >= corner2.x) || (position.x >= corner1.x && position.x <= corner2.x))
        {
            if ((position.y <= corner1.y && position.y >= corner2.y) || (position.y >= corner1.y && position.y <= corner2.y))
            {
                return true;
            }
        }
        return false;
    }

    //Victory Functions
    bool checkReachCondition(OperationsReachCondition condition)
    {
        foreach (GameObject squad in squads)
        {
            if (checkCorners(condition.getReachCorners()[0], condition.getReachCorners()[0], gameObject.GetComponent<MapController>().gridWorldPos(squad.transform.position)))
            {
                if (squad.GetComponent<SquadController>().getTeam() == condition.getReachTeam())
                {
                    return true;
                }
            }
        }
        return false;
    }
}
