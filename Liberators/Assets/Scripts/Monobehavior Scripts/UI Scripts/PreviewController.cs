using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PreviewController : MonoBehaviour
{
    public void setData(GameObject defender, Ability activeAbility)
    {
        GameObject.Find("Target Name").GetComponent<TextMeshProUGUI>().text = defender.GetComponent<UnitController>().GetName();
        GameObject.Find("Ability Name").GetComponent<TextMeshProUGUI>().text = activeAbility.getName();
    }

    public void setData(GameObject unit)
    {
        UnitController unitController = unit.GetComponent<UnitController>();
        GameObject.Find("Unit Name").GetComponent<TextMeshProUGUI>().text = unitController.GetName();
        GameObject.Find("HP Meter").GetComponent<TextMeshProUGUI>().text = unitController.GetStat("currentHP") + "/" + unitController.GetStat("maxHP");
        GameObject.Find("AP Meter").GetComponent<TextMeshProUGUI>().text = unitController.getActions()[1] + "/" + unitController.getActions()[0];
        GameObject.Find("Strength Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStat("str").ToString();
        GameObject.Find("Potential Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStat("pot").ToString();
        GameObject.Find("Acuity Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStat("acu").ToString();
        GameObject.Find("Finesse Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStat("fin").ToString();
        GameObject.Find("Reaction Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStat("rea").ToString();
        GameObject.Find("Movement Stat").GetComponent<TextMeshProUGUI>().text = unitController.GetStat("mov").ToString();
        GameObject.Find("Weapon Name").GetComponent<TextMeshProUGUI>().text = unitController.GetEquippedWeapons().Item1.LoadWeaponData().GetName();
        GameObject.Find("Armor Name").GetComponent<TextMeshProUGUI>().text = unitController.GetEquippedArmor().LoadArmorData().GetName();
    }
}
