using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndController : MonoBehaviour
{
    void OnEnable()
    {
        if (BattleExitHandler.victory)
        {
            GameObject.Find("Victory Label").GetComponent<TextMeshProUGUI>().text = "Victory!";
            OperationSceneHandler.battleOutcome = battleOutcome.SUCCESS;
        } else
        {
            GameObject.Find("Victory Label").GetComponent<TextMeshProUGUI>().text = "Defeat...";
            OperationSceneHandler.battleOutcome = battleOutcome.FAILURE;
        }
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
