using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OperationPrepController : MonoBehaviour
{
    public List<UnitEntryData> unitList;

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
        SquadData player = new SquadData(10, unitList, 0, new Vector3(0.5f, 0.5f, -1f), "Player Squad");
        OperationSceneHandler.squadDataList.Add(player);
        SquadData enemy1 = new SquadData(10, new List<UnitEntryData>(), 1, new Vector3(3.5f, 5.5f, -1f), "Enemy Squad 0");
        OperationSceneHandler.squadDataList.Add(enemy1);
        SquadData enemy2 = new SquadData(10, new List<UnitEntryData>(), 1, new Vector3(2.5f, -2.5f, -1f), "Enemy Squad 1");
        OperationSceneHandler.squadDataList.Add(enemy2);
        SceneManager.LoadSceneAsync("OperationMap");
    }
}
