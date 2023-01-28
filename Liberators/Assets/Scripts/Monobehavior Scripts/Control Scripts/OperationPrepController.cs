using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OperationPrepController : MonoBehaviour
{
    public List<UnitEntryData> unitList;

    public List<UnitEntryData> enemy1List;

    public List<UnitEntryData> enemy2List;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void startOperation()
    {
        SquadData player = new SquadData(10, unitList, new List<Vector2Int>() { new Vector2Int(0, -12), new Vector2Int(0, -11) }, 0, new Vector3(0.5f, 0.5f, -1f), "Player Squad", operationsAI.PLAYER);
        OperationSceneHandler.squadDataList.Add(player);
        SquadData enemy1 = new SquadData(10, enemy1List, new List<Vector2Int>() { new Vector2Int(0, 12), new Vector2Int(0, 11) }, 1, new Vector3(3.5f, 5.5f, -1f), "Enemy Squad 0", operationsAI.WAIT);
        OperationSceneHandler.squadDataList.Add(enemy1);
        SquadData enemy2 = new SquadData(10, enemy2List, new List<Vector2Int>() { new Vector2Int(1, 12), new Vector2Int(-1, 12) }, 1, new Vector3(2.5f, -2.5f, -1f), "Enemy Squad 1", operationsAI.WANDER);
        OperationSceneHandler.squadDataList.Add(enemy2);
        SquadData enemy3 = new SquadData(10, enemy2List, new List<Vector2Int>() { new Vector2Int(0, 12), new Vector2Int(0, 10) }, 1, new Vector3(6.5f, -5.5f, -1f), "Enemy Squad 2", operationsAI.ATTACK);
        OperationSceneHandler.squadDataList.Add(enemy3);
        SceneManager.LoadSceneAsync("OperationMap");
    }
}
