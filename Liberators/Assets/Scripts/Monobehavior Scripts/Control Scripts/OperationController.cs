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

    // Start is called before the first frame update
    void Start()
    {
        pathfinder = new OperationPathfinder(gameObject.GetComponent<Tilemap>());
        pathfinder.runCalculations(new Vector2Int(0, 0));
        squads.Add(GameObject.Find("Player Squad"));
        squads.Add(GameObject.Find("Enemy Squad 0"));
        squads.Add(GameObject.Find("Enemy Squad 1"));
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
            SceneManager.LoadSceneAsync("BattlePrep");
        }
    }
}
