using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class OperationPrepController : MonoBehaviour
{
    [SerializeField]
    OperationsData opsData;

    [SerializeField]
    GameObject operationName;

    public List<UnitEntryData> unitList;

    public List<UnitEntryData> enemy1List;

    public List<UnitEntryData> enemy2List;

    // Start is called before the first frame update
    void Start()
    {
        operationName.GetComponent<TextMeshProUGUI>().text = opsData.name;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void startOperation()
    {
        foreach (SquadSeralization squad in opsData.getSquads())
        {
            OperationSceneHandler.squadDataList.Add(new SquadData(10, squad.getUnits(), squad.getUnitPositions(), squad.getTeam(), squad.getPosition(), squad.getSquadName(), squad.getSquadAI(), squad.getSquadAnchored()));
        }
        SquadData player = new SquadData(10, PlayerRoster.GetFullUnitEntryData(), new List<Vector2Int>() { new Vector2Int(0, -12), new Vector2Int(0, -11) }, operationsTeam.PLAYER, new Vector3(-22.5f, -14.5f, -1f), "Player Squad", operationsAI.PLAYER, false);
        OperationSceneHandler.squadDataList.Add(player);
        /*
        SquadData enemy1 = new SquadData(10, enemy1List, new List<Vector2Int>() { new Vector2Int(0, 12), new Vector2Int(0, 11) }, operationsTeam.ENEMY, new Vector3(-22.5f, -13.5f, -1f), "Enemy Squad", operationsAI.ATTACK, false);
        OperationSceneHandler.squadDataList.Add(enemy1);
        */
        OperationSceneHandler.data = opsData;
        SceneManager.LoadSceneAsync("OperationMap");
    }
}
