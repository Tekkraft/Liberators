using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartController : MonoBehaviour
{
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

    PlayerUnitDataList dataList;

    void Awake()
    {
        ChangePage(BattleMenuPage.main);
    }

    void OnEnable()
    {
        if (BattlePrepHandler.unitData == null)
        {
            dataList = PlayerUnitDataList.FromJSON(SaveSystem.LoadTeamData());
            BattlePrepHandler.unitData = dataList;
        } else
        {
            dataList = BattlePrepHandler.unitData;
        }
        //TODO: Fix error with invalid/empty/null scene
        //Possibly no issue?
        BattlePrepHandler.battleScene = OperationSceneHandler.battleScene;
        LoadSkillTree();
        LoadInventory();
        OperationSceneHandler.attackerData.unitDataList = BattlePrepHandler.unitData.ToList();
    }

    void OnDisable()
    {
        SaveSystem.SaveTeamData(dataList.ToJSON());
        BattlePrepHandler.unitData = dataList;
        BattlePrepHandler.battleScene = OperationSceneHandler.battleScene;
    }

    public void EnterBattle()
    {
        BattleTransition.playerSpawnLocations = new List<Vector2Int>();
        foreach (Vector2Int place in OperationSceneHandler.attackerData.getPairedUnits().Values)
        {
            BattleTransition.playerSpawnLocations.Add(place);
        }
        BattleTransition.enemyPlacements = OperationSceneHandler.defenderData.getPairedUnits();
        SceneManager.LoadSceneAsync(BattlePrepHandler.battleScene);
    }

    public void EnterSkillTree()
    {
        SkillTreeTransition.characterIndex = characterIndex;
        SkillTreeTransition.unitData = UnitIndexToData(characterIndex);
        SceneManager.LoadSceneAsync("UnitSkillTree");
    }

    public void EnterInventory()
    {
        InventoryTransition.characterIndex = characterIndex;
        InventoryTransition.unitData = UnitIndexToData(characterIndex);
        InventoryTransition.origin = "BattlePrep";
        SceneManager.LoadSceneAsync("UnitInventory");
    }

    public void LoadSkillTree()
    {
        if (SkillTreeTransition.activated)
        {
            characterIndex = SkillTreeTransition.characterIndex;
            UpdateUnitIndexWithData(characterIndex, SkillTreeTransition.unitData);
            LoadUnitPage(SkillTreeTransition.characterIndex);
        }
        SkillTreeTransition.reset();
    }

    public void LoadInventory()
    {
        if (InventoryTransition.activated)
        {
            characterIndex = InventoryTransition.characterIndex;
            UpdateUnitIndexWithData(characterIndex, InventoryTransition.unitData);
            LoadUnitPage(InventoryTransition.characterIndex);
        }
        InventoryTransition.reset();
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
        unitSpecificScreenUIElements[0].GetComponent<TextMeshProUGUI>().text = UnitIndexToData(characterIndex).unitName;
    }

    public void LoadUnitPage(int index)
    {
        if (index < dataList.entries && index >= 0)
        {
            characterIndex = index;
        }
        ChangePage(BattleMenuPage.unit);
        unitSpecificScreenUIElements[0].GetComponent<TextMeshProUGUI>().text = UnitIndexToData(characterIndex).unitName;
    }

    public void LoadUnitOverviewPage()
    {
        ChangePage(BattleMenuPage.overview);
        unitOverviewScreenUIElements[0].GetComponent<TextMeshProUGUI>().text = UnitIndexToData(characterIndex).unitName;
    }

    public void NextUnit()
    {
        characterIndex++;
        if (characterIndex >= dataList.entries)
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
            characterIndex = dataList.entries - 1;
        }
        LoadUnitPage();
    }

    public UnitData UnitIndexToData(int index)
    {
        switch (index)
        {
            case 0:
                return dataList.lanaData;
            case 1:
                return dataList.ethanData;
            case 2:
                return dataList.saeiData;
            case 3:
                return dataList.vaueData;
            case 4:
                return dataList.mayData;
            case 5:
                return dataList.runliData;
            case 6:
                return dataList.colinData;
            case 7:
                return dataList.hanaeiData;
            default:
                return null;
        }
    }

    public void UpdateUnitIndexWithData(int index, UnitData data)
    {
        switch (index)
        {
            case 0:
                dataList.lanaData = data;
                break;
            case 1:
                dataList.ethanData = data;
                break;
            case 2:
                dataList.saeiData = data;
                break;
            case 3:
                dataList.vaueData = data;
                break;
            case 4:
                dataList.mayData = data;
                break;
            case 5:
                dataList.runliData = data;
                break;
            case 6:
                dataList.colinData = data;
                break;
            case 7:
                dataList.hanaeiData = data;
                break;
        }
    }
}
