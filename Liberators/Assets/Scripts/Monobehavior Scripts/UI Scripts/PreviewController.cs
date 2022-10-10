using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PreviewController : MonoBehaviour
{
    public void setData(GameObject defender, Ability activeAbility, int playerHit, int playerDamage, int playerCrit)
    {
        GameObject.Find("Unit Name").GetComponent<TextMeshProUGUI>().text = defender.GetComponent<UnitController>().getName();
        GameObject.Find("Ability Name").GetComponent<TextMeshProUGUI>().text = activeAbility.getName();
        GameObject.Find("Player Hit").GetComponent<TextMeshProUGUI>().text = playerHit.ToString();
        GameObject.Find("Player Damage").GetComponent<TextMeshProUGUI>().text = playerDamage.ToString();
        GameObject.Find("Player Critical").GetComponent<TextMeshProUGUI>().text = playerCrit.ToString();
    }

    public void setData(GameObject unit)
    {
        UnitController unitController = unit.GetComponent<UnitController>();
        GameObject.Find("Unit Name").GetComponent<TextMeshProUGUI>().text = unitController.getName();
        GameObject.Find("HP Meter").GetComponent<TextMeshProUGUI>().text = unitController.getStats()[2] + "/" + unitController.getStats()[1];
        GameObject.Find("AP Meter").GetComponent<TextMeshProUGUI>().text = unitController.getActions()[1] + "/" + unitController.getActions()[0];
        GameObject.Find("Strength Stat").GetComponent<TextMeshProUGUI>().text = unitController.getStats()[3].ToString();
        GameObject.Find("Potential Stat").GetComponent<TextMeshProUGUI>().text = unitController.getStats()[4].ToString();
        GameObject.Find("Acuity Stat").GetComponent<TextMeshProUGUI>().text = unitController.getStats()[5].ToString();
        GameObject.Find("Finesse Stat").GetComponent<TextMeshProUGUI>().text = unitController.getStats()[6].ToString();
        GameObject.Find("Reaction Stat").GetComponent<TextMeshProUGUI>().text = unitController.getStats()[7].ToString();
        GameObject.Find("Movement Stat").GetComponent<TextMeshProUGUI>().text = unitController.getStats()[0].ToString();
        GameObject.Find("Weapon Name").GetComponent<TextMeshProUGUI>().text = unitController.getEquippedWeapon().getName();
        GameObject.Find("Armor Name").GetComponent<TextMeshProUGUI>().text = unitController.getEquippedArmor().getName();
    }
}
