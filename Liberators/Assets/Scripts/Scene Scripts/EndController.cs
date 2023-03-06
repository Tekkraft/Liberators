using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndController : MonoBehaviour
{
    void OnEnable()
    {
        switch (BattleExitHandler.outcome)
        {
            case BattleOutcome.VICTORY:
                GameObject.Find("Victory Label").GetComponent<TextMeshProUGUI>().text = "Total Victory!";
                break;

            case BattleOutcome.ROUTED:
                GameObject.Find("Victory Label").GetComponent<TextMeshProUGUI>().text = "Routed...";
                break;

            case BattleOutcome.SUCCESS:
                GameObject.Find("Victory Label").GetComponent<TextMeshProUGUI>().text = "Victory!";
                break;

            case BattleOutcome.FAILURE:
                GameObject.Find("Victory Label").GetComponent<TextMeshProUGUI>().text = "Defeat...";
                break;
        }
        OperationSceneHandler.battleOutcome = BattleExitHandler.outcome;
        GameObject.Find("Turn Count Label").GetComponent<TextMeshProUGUI>().text = "Turns: " + BattleExitHandler.turn_count;
    }

    public void exitScreen()
    {
        foreach (UnitEntryData data in BattleExitHandler.unitData)
        {
            if (data.getUnit() != null && data.getUnit().getSkillTree() != null)
            {
                data.getUnit().getSkillTree().gainSkillPoints(1);
            }
        }
        SceneManager.LoadSceneAsync("OperationMap");
    }
}
