using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class OperationController : MonoBehaviour
{
    bool statePaused;
    OperationPathfinder pathfinder;
    List<GameObject> squads = new List<GameObject>();

    public Sprite playerFlag;
    public Sprite enemyFlag;

    public GameObject squadPrefab;

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = new OperationPathfinder(gameObject.GetComponent<Tilemap>());
        pathfinder.runCalculations(new Vector2Int(0, 0));
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
    }

    void OnEnable()
    {
        //Load in all units
        foreach(SquadData data in OperationSceneHandler.squadDataList)
        {
            GameObject temp = GameObject.Instantiate(squadPrefab);
            temp.GetComponent<SquadController>().loadSquadData(data);
            if (data.team == 0)
            {
                temp.GetComponent<SpriteRenderer>().sprite = playerFlag;
            }
            if (data.team == 1)
            {
                temp.GetComponent<SpriteRenderer>().sprite = enemyFlag;
            }
            squads.Add(temp);
        }
        //Handle battle results if present
        switch (OperationSceneHandler.battleOutcome)
        {
            case battleOutcome.VICTORY:
                //NOT WORKING
                squads[OperationSceneHandler.attackerId].transform.Translate(new Vector3(0, 1, 0));
                break;

            case battleOutcome.SUCCESS:
                squads[OperationSceneHandler.attackerId].transform.Translate(new Vector3(0, 1, 0));
                break;

            case battleOutcome.FAILURE:
                squads[OperationSceneHandler.attackerId].transform.Translate(new Vector3(0, 1, 0));
                break;

            case battleOutcome.ROUTED:
                //NOT WORKING
                squads[OperationSceneHandler.attackerId].transform.Translate(new Vector3(0, 1, 0));
                break;
        }
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
        if (unit1.transform.position == unit2.transform.position)
        {
            if (unit1.GetComponent<SquadController>().getTeam() == 0)
            {
                startSkirmish(unit1, unit2);
            } else if (unit2.GetComponent<SquadController>().getTeam() == 0)
            {
                startSkirmish(unit2, unit1);
            }
        }
    }

    void startSkirmish(GameObject squad, GameObject opponent)
    {
        OperationSceneHandler.reset();
        OperationSceneHandler.attackerData = squad.GetComponent<SquadController>().getSquadData();
        OperationSceneHandler.attackerId = squads.IndexOf(squad);
        OperationSceneHandler.defenderData = opponent.GetComponent<SquadController>().getSquadData();
        OperationSceneHandler.defenderId = squads.IndexOf(opponent);
        foreach (GameObject temp in squads)
        {
            OperationSceneHandler.squadDataList.Add(temp.GetComponent<SquadController>().getSquadData());
        }
        SceneManager.LoadSceneAsync("BattlePrep");
    }

    List<Vector3> availableCoords(Vector3 coords)
    {
        List<Vector3> possibleCoords = new List<Vector3>();
        possibleCoords.Add(coords + Vector3.up);
        possibleCoords.Add(coords + Vector3.down);
        possibleCoords.Add(coords + Vector3.left);
        possibleCoords.Add(coords + Vector3.right);

        return possibleCoords;
    }
}
