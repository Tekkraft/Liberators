using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PreviewController : MonoBehaviour
{
    public void setData(GameObject defender, Ability activeAbility, int playerHit, int playerDamage, int playerCrit)
    {
        GameObject.Find("Unit Name").GetComponent<TextMeshProUGUI>().text = defender.GetComponent<UnitController>().GetName();
        GameObject.Find("Ability Name").GetComponent<TextMeshProUGUI>().text = activeAbility.getName();
        GameObject.Find("Player Hit").GetComponent<TextMeshProUGUI>().text = Mathf.Min(playerHit, 100).ToString();
        GameObject.Find("Player Damage").GetComponent<TextMeshProUGUI>().text = playerDamage.ToString();
        GameObject.Find("Player Critical").GetComponent<TextMeshProUGUI>().text = Mathf.Max(playerCrit, 0).ToString();
    }

    public void setData(GameObject unit)
    {
        UnitController unitController = unit.GetComponent<UnitController>();
        GameObject.Find("Unit Name").GetComponent<TextMeshProUGUI>().text = unitController.GetName();
        GameObject.Find("HP Meter").GetComponent<TextMeshProUGUI>().text = unitController.GetStats()[2] + "/" + unitController.GetStats()[1];
        GameObject.Find("AP Meter").GetComponent<TextMeshProUGUI>().text = unitController.getActions()[1] + "/" + unitController.getActions()[0];
        GameObject.Find("Strength Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStats()[3].ToString();
        GameObject.Find("Potential Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStats()[4].ToString();
        GameObject.Find("Acuity Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStats()[5].ToString();
        GameObject.Find("Finesse Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStats()[6].ToString();
        GameObject.Find("Reaction Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStats()[7].ToString();
        GameObject.Find("Movement Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStats()[0].ToString();
        GameObject.Find("Weapon Name").GetComponent<TextMeshProUGUI>().text = unitController.GetEquippedWeapons().Item1.GetInstanceName();
        GameObject.Find("Armor Name").GetComponent<TextMeshProUGUI>().text = unitController.GetEquippedArmor().GetInstanceName();
    }
}
