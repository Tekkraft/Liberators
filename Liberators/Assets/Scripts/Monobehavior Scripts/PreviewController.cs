using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PreviewController : MonoBehaviour
{
    public GameObject unitName;
    public GameObject abilityName;
    public GameObject playerHit;
    public GameObject playerDamage;
    public GameObject playerCrit;

    public void setData(GameObject attacker, GameObject defender, Ability activeAbility, int playerHit, int playerDamage, int playerCrit)
    {
        this.unitName.GetComponent<TextMeshProUGUI>().text = defender.GetComponent<UnitController>().getName();
        this.abilityName.GetComponent<TextMeshProUGUI>().text = activeAbility.getName();
        this.playerHit.GetComponent<TextMeshProUGUI>().text = playerHit.ToString();
        this.playerDamage.GetComponent<TextMeshProUGUI>().text = playerDamage.ToString();
        this.playerCrit.GetComponent<TextMeshProUGUI>().text = playerCrit.ToString();
    }
}
