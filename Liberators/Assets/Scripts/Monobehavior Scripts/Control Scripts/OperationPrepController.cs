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
        SquadData player = new SquadData(10, PlayerRoster.GetFullUnitEntryData(), new List<Vector2Int>() { new Vector2Int(1, -12), new Vector2Int(1, -11), new Vector2Int(2, -12), new Vector2Int(2, -11), new Vector2Int(-1, -12), new Vector2Int(-1, -11), new Vector2Int(-2, -12), new Vector2Int(-2, -11) }, OperationsTeam.PLAYER, new Vector3(-22.5f, -14.5f, -1f), "Player Squad", OperationsAI.PLAYER, false);
        OperationSceneHandler.squadDataList.Add(player);
        OperationSceneHandler.data = opsData;
        SceneManager.LoadSceneAsync("OperationMap");
    }
}
