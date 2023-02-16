using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartController : MonoBehaviour
{
    List<UnitEntryData> characterUnitData = new List<UnitEntryData>();
    int characterIndex = 0;

    [SerializeField]
    GameObject mainScreen;

    [SerializeField]
    GameObject unitListScreen;

    [SerializeField]
    GameObject unitSpecificScreen;

    [SerializeField]
    GameObject unitOverviewScreen;

    //0 - Title
    [SerializeField]
    List<GameObject> mainScreenUIElements;

    //0 - Title
    [SerializeField]
    List<GameObject> unitListScreenUIElements;

    //0 - Title
    [SerializeField]
    List<GameObject> unitSpecificScreenUIElements;

    //0 - Title
    [SerializeField]
    List<GameObject> unitOverviewScreenUIElements;

    void Awake()
    {
        ChangePage(BattleMenuPage.main);
    }

    void OnEnable()
    {
        if (OperationSceneHandler.attackerData != null)
        {
            BattlePrepHandler.data = OperationSceneHandler.attackerData.unitList;
        }
        //TODO: Fix error with invalid/empty/null scene
        //Possibly no issue?
        BattlePrepHandler.battleScene = OperationSceneHandler.battleScene;
        if (BattlePrepHandler.data != null)
        {
            characterUnitData = BattlePrepHandler.data;
        }
        for (int i = 0; i < characterUnitData.Count; i++)
        {
            UnitEntryData data = characterUnitData[i];
            if (data.getUnit() == null)
            {
                data.reconstruct();
            }
        }
        LoadSkillTree();
        LoadInventory();
        BattlePrepHandler.data = characterUnitData;
        OperationSceneHandler.attackerData.unitList = BattlePrepHandler.data;
    }

    void OnDisable()
    {
        BattlePrepHandler.data = characterUnitData;
        BattlePrepHandler.battleScene = OperationSceneHandler.battleScene;
    }

    public void EnterBattle()
    {
        BattleEntryHandler.deployedUnits = OperationSceneHandler.attackerData.getPairedUnits();
        BattleEntryHandler.enemyPlacements = OperationSceneHandler.defenderData.getPairedUnits();
        SceneManager.LoadSceneAsync(BattlePrepHandler.battleScene);
    }

    public void EnterSkillTree()
    {
        SkillTreeEntryHandler.characterIndex = characterIndex;
        SkillTreeEntryHandler.activeTree = characterUnitData[characterIndex].getUnit().getSkillTree();
        SceneManager.LoadSceneAsync("UnitSkillTree");
    }

    public void EnterInventory()
    {
        InventoryTransitionController.characterIndex = characterIndex;
        InventoryTransitionController.equippedMainHandWeapon = characterUnitData[characterIndex].getWeapons().Item1;
        InventoryTransitionController.equippedOffHandWeapon = characterUnitData[characterIndex].getWeapons().Item2;
        InventoryTransitionController.equippedArmor = characterUnitData[characterIndex].getArmor();
        InventoryTransitionController.origin = "BattlePrep";
        SceneManager.LoadSceneAsync("UnitInventory");
    }

    public void LoadSkillTree()
    {
        if (SkillTreeExitHandler.activated)
        {
            characterIndex = SkillTreeExitHandler.characterIndex;
            characterUnitData[characterIndex].getUnit().updateSkillTree(SkillTreeExitHandler.activeTree);
            LoadUnitPage(SkillTreeExitHandler.characterIndex);
            SkillTreeExitHandler.reset();
        }
    }

    public void LoadInventory()
    {
        if (InventoryTransitionController.activated)
        {
            characterIndex = InventoryTransitionController.characterIndex;
            characterUnitData[characterIndex].setArmor(InventoryTransitionController.equippedArmor);
            characterUnitData[characterIndex].setWeapon(InventoryTransitionController.equippedMainHandWeapon, true);
            characterUnitData[characterIndex].setWeapon(InventoryTransitionController.equippedOffHandWeapon, false);
            LoadUnitPage(InventoryTransitionController.characterIndex);
        }
        InventoryTransitionController.reset();
    }

    void ChangePage(BattleMenuPage newPage)
    {
        switch (newPage)
        {
            case BattleMenuPage.main:
                mainScreen.GetComponent<RectTransform>().localPosition = Vector3.zero;
                unitListScreen.GetComponent<RectTransform>().localPosition = new Vector3(0, mainScreen.GetComponent<RectTransform>().sizeDelta.y * 1.1f, 0);
                unitSpecificScreen.GetComponent<RectTransform>().localPosition = new Vector3(0, -unitSpecificScreen.GetComponent<RectTransform>().sizeDelta.y * 1.1f, 0);
                unitOverviewScreen.GetComponent<RectTransform>().localPosition = new Vector3(unitOverviewScreen.GetComponent<RectTransform>().sizeDelta.x * 1.1f, 0, 0);
                break;

            case BattleMenuPage.unit:
                mainScreen.GetComponent<RectTransform>().localPosition = new Vector3(-mainScreen.GetComponent<RectTransform>().sizeDelta.x * 1.1f, 0, 0);
                unitListScreen.GetComponent<RectTransform>().localPosition = new Vector3(0, unitListScreen.GetComponent<RectTransform>().sizeDelta.y * 1.1f, 0);
                unitSpecificScreen.GetComponent<RectTransform>().localPosition = Vector3.zero;
                unitOverviewScreen.GetComponent<RectTransform>().localPosition = new Vector3(unitOverviewScreen.GetComponent<RectTransform>().sizeDelta.x * 1.1f, 0, 0);
                break;

            case BattleMenuPage.list:
                mainScreen.GetComponent<RectTransform>().localPosition = new Vector3(-mainScreen.GetComponent<RectTransform>().sizeDelta.x * 1.1f, 0, 0);
                unitListScreen.GetComponent<RectTransform>().localPosition = Vector3.zero;
                unitSpecificScreen.GetComponent<RectTransform>().localPosition = new Vector3(0, -unitSpecificScreen.GetComponent<RectTransform>().sizeDelta.y * 1.1f, 0);
                unitOverviewScreen.GetComponent<RectTransform>().localPosition = new Vector3(unitOverviewScreen.GetComponent<RectTransform>().sizeDelta.x * 1.1f, 0, 0);
                break;

            case BattleMenuPage.overview:
                mainScreen.GetComponent<RectTransform>().localPosition = new Vector3(-mainScreen.GetComponent<RectTransform>().sizeDelta.x * 1.1f, 0, 0);
                unitListScreen.GetComponent<RectTransform>().localPosition = new Vector3(0, unitListScreen.GetComponent<RectTransform>().sizeDelta.y * 1.1f, 0);
                unitSpecificScreen.GetComponent<RectTransform>().localPosition = new Vector3(0, -unitSpecificScreen.GetComponent<RectTransform>().sizeDelta.y * 1.1f, 0);
                unitOverviewScreen.GetComponent<RectTransform>().localPosition = Vector3.zero;
                break;
        }
    }

    public void LoadMainPage()
    {
        ChangePage(BattleMenuPage.main);
    }

    public void LoadUnitListPage()
    {
        ChangePage(BattleMenuPage.list);
    }

    public void LoadUnitPage()
    {
        ChangePage(BattleMenuPage.unit);
        unitSpecificScreenUIElements[0].GetComponent<TextMeshProUGUI>().text = characterUnitData[characterIndex].getUnit().getUnitName();
    }

    public void LoadUnitPage(int index)
    {
        if (index < characterUnitData.Count && index >= 0)
        {
            characterIndex = index;
        }
        ChangePage(BattleMenuPage.unit);
        unitSpecificScreenUIElements[0].GetComponent<TextMeshProUGUI>().text = characterUnitData[characterIndex].getUnit().getUnitName();
    }

    public void LoadUnitOverviewPage()
    {
        ChangePage(BattleMenuPage.overview);
        unitOverviewScreenUIElements[0].GetComponent<TextMeshProUGUI>().text = characterUnitData[characterIndex].getUnit().getUnitName();
    }

    public void NextUnit()
    {
        characterIndex++;
        if (characterIndex >= characterUnitData.Count)
        {
            characterIndex = 0;
        }
        LoadUnitPage();
    }

    public void PreviousUnit()
    {
        characterIndex--;
        if (characterIndex < 0)
        {
            characterIndex = characterUnitData.Count - 1;
        }
        LoadUnitPage();
    }
}
