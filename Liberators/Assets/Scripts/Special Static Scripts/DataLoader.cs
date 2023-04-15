using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataLoader : MonoBehaviour
{
    [SerializeField]
    Unit lanaBase;
    [SerializeField]
    string lanaWeaponId;
    [SerializeField]
    string lanaArmorId;
    [SerializeField]
    string lanaSkillTree;

    [SerializeField]
    Unit ethanBase;
    [SerializeField]
    string ethanWeaponId;
    [SerializeField]
    string ethanArmorId;
    [SerializeField]
    string ethanSkillTree;

    [SerializeField]
    Unit saeiBase;
    [SerializeField]
    string saeiWeaponId;
    [SerializeField]
    string saeiArmorId;
    [SerializeField]
    string saeiSkillTree;

    [SerializeField]
    Unit vaueBase;
    [SerializeField]
    string vaueWeaponId;
    [SerializeField]
    string vaueArmorId;
    [SerializeField]
    string vaueSkillTree;

    [SerializeField]
    Unit mayBase;
    [SerializeField]
    string mayWeaponId;
    [SerializeField]
    string mayArmorId;
    [SerializeField]
    string maySkillTree;

    [SerializeField]
    Unit runliBase;
    [SerializeField]
    string runliWeaponId;
    [SerializeField]
    string runliArmorId;
    [SerializeField]
    string runliSkillTree;

    [SerializeField]
    Unit colinBase;
    [SerializeField]
    string colinWeaponId;
    [SerializeField]
    string colinArmorId;
    [SerializeField]
    string colinSkillTree;

    [SerializeField]
    Unit hanaeiBase;
    [SerializeField]
    string hanaeiWeaponId;
    [SerializeField]
    string hanaeiArmorId;
    [SerializeField]
    string hanaeiSkillTree;

    [SerializeField]
    List<string> weaponList;

    [SerializeField]
    List<string> armorList;

    PlayerUnitDataList teamList = new PlayerUnitDataList();

    void Awake()
    {
        teamList.lanaData = LoadUnitData(lanaBase, lanaWeaponId, lanaArmorId, lanaSkillTree);
        teamList.ethanData = LoadUnitData(ethanBase, ethanWeaponId, ethanArmorId, ethanSkillTree);
        teamList.saeiData = LoadUnitData(saeiBase, saeiWeaponId, saeiArmorId, saeiSkillTree);
        teamList.vaueData = LoadUnitData(vaueBase, vaueWeaponId, vaueArmorId, vaueSkillTree);
        teamList.mayData = LoadUnitData(mayBase, mayWeaponId, mayArmorId, maySkillTree);
        teamList.runliData = LoadUnitData(runliBase, runliWeaponId, runliArmorId, runliSkillTree);
        teamList.colinData = LoadUnitData(colinBase, colinWeaponId, colinArmorId, colinSkillTree);
        teamList.hanaeiData = LoadUnitData(hanaeiBase, hanaeiWeaponId, hanaeiArmorId, hanaeiSkillTree);

        SaveSystem.SaveTeamData(teamList.ToJSON());

        foreach (string id in weaponList)
        {
            WeaponData data = new WeaponData(id);
            PlayerInventory.PushItem(data);
        }

        foreach (string id in armorList)
        {
            ArmorData data = new ArmorData(id);
            PlayerInventory.PushItem(data);
        }
    }

    UnitData LoadUnitData(Unit unitBase, string weaponId, string armorId, string skillTreeId)
    {
        UnitData unitData = new UnitData();
        unitData.source = unitBase.name;
        unitData.unitName = unitBase.getUnitName();
        unitData.className = unitBase.getClassName();
        unitData.maxHP = unitBase.getStats()[0];
        unitData.currentHP = unitBase.getStats()[0];
        unitData.mov = unitBase.getStats()[1];
        unitData.str = unitBase.getStats()[2];
        unitData.pot = unitBase.getStats()[3];
        unitData.acu = unitBase.getStats()[4];
        unitData.fin = unitBase.getStats()[5];
        unitData.rea = unitBase.getStats()[6];
        unitData.maxSkillPoints = 1;
        unitData.availableSkillPoints = 1;
        unitData.skillTree = SkillTreeEvaluator.CreateTreeData(skillTreeId + ".xml");
        unitData.skillTree.Initialize();
        unitData.mainWeapon = new WeaponData(weaponId);
        unitData.armor = new ArmorData(armorId);
        unitData.skillTree.CheckAllUnlocked();
        unitData.skillTree.LoadAllLearnedAbilities();
        return unitData;
    }
}

[System.Serializable]
public class PlayerUnitDataList{
    public UnitData lanaData;
    public UnitData ethanData;
    public UnitData saeiData;
    public UnitData vaueData;
    public UnitData mayData;
    public UnitData runliData;
    public UnitData colinData;
    public UnitData hanaeiData;

    public int entries = 8;

    public static PlayerUnitDataList FromJSON(string jsonString)
    {
        return JsonUtility.FromJson<PlayerUnitDataList>(jsonString);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public List<UnitData> ToList()
    {
        return new List<UnitData>() { lanaData, ethanData, saeiData, vaueData, mayData, runliData, colinData, hanaeiData };
    }

    public void FromList(List<UnitData> units)
    {
        lanaData = units[0];
        ethanData = units[1];
        saeiData = units[2];
        vaueData = units[3];
        mayData = units[4];
        runliData = units[5];
        colinData = units[6];
        hanaeiData = units[7];
    }
}