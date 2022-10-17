using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartController : MonoBehaviour
{
    //-1: Home/Overview
    //Rest proceed in array order 0-7
    int characterState = -1;

    public List<UnitEntryData> characterUnitData;

    public GameObject overviewCanvas;
    public GameObject unitCanvas;

    void Awake()
    {
        overviewCanvas.GetComponent<Canvas>().enabled = true;
        unitCanvas.GetComponent<Canvas>().enabled = false;
        displayBattleOverview();
        for (int i = 0; i < characterUnitData.Count; i++)
        {
            UnitEntryData data = characterUnitData[i];
            if (data.getSkillTree() == null && data.getUnit() != null)
            {
                characterUnitData[i] = new UnitEntryData(data.getUnit(), data.getWeapon(), data.getArmor());
            }
        }
    }

    void OnEnable()
    {
        loadSkillTree();
    }

    void OnDisable()
    {

    }

    public void enterBattle()
    {
        BattleEntryHandler.deployedUnits = characterUnitData;
        SceneManager.LoadSceneAsync("PrototypeBattle");
    }

    public void enterSkillTree()
    {
        SkillTreeEntryHandler.unitId = characterState;
        SkillTreeEntryHandler.activeTree = characterUnitData[characterState].getSkillTree();
        SceneManager.LoadSceneAsync("UnitSkillTree");
    }

    public void loadSkillTree()
    {
        if (SkillTreeExitHandler.unitId == -1)
        {
            return;
        }
        setCharacterState(SkillTreeExitHandler.unitId);
        characterUnitData[characterState].setSkillTree(SkillTreeExitHandler.activeTree);
    }

    public void setCharacterState(int stateId)
    {
        characterState = stateId;
        if (characterState == -1)
        {
            overviewCanvas.GetComponent<Canvas>().enabled = true;
            unitCanvas.GetComponent<Canvas>().enabled = false;
            displayBattleOverview();
        }
        else if (characterState >= 0 && characterState <= 7)
        {
            overviewCanvas.GetComponent<Canvas>().enabled = false;
            unitCanvas.GetComponent<Canvas>().enabled = true;
            displayUnitOverview();
        }
    }

    public int getCharacterState()
    {
        return characterState;
    }

    void displayBattleOverview()
    {

    }

    void displayUnitOverview()
    {
        GameObject.Find("UC Unit Name").GetComponent<TextMeshProUGUI>().text = characterUnitData[characterState].getUnit().getUnitName();
        if (characterUnitData[characterState].getWeapon())
        {
            GameObject.Find("UC Weapon Name").GetComponent<TextMeshProUGUI>().text = characterUnitData[characterState].getWeapon().getName();
        }
        else
        {
            GameObject.Find("UC Weapon Name").GetComponent<TextMeshProUGUI>().text = "No Equipped Weapon";
        }
        if (characterUnitData[characterState].getArmor())
        {
            GameObject.Find("UC Armor Name").GetComponent<TextMeshProUGUI>().text = characterUnitData[characterState].getArmor().getName();
        }
        else
        {
            GameObject.Find("UC Armor Name").GetComponent<TextMeshProUGUI>().text = "No Equipped Armor";
        }
    }
}
