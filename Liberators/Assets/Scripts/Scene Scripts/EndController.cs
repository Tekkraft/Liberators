using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndController : MonoBehaviour
{
    void OnEnable()
    {
        if (BattleExitHandler.victory)
        {
            GameObject.Find("Victory Label").GetComponent<TextMeshProUGUI>().text = "Victory!";
        } else
        {
            GameObject.Find("Victory Label").GetComponent<TextMeshProUGUI>().text = "Defeat...";
        }
        GameObject.Find("Turn Count Label").GetComponent<TextMeshProUGUI>().text = "Turns: " + BattleExitHandler.turn_count;
    }
}
