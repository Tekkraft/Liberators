using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    [SerializeField]
    Unit lanaBase;
    [SerializeField]
    Weapon lanaWeapon;
    [SerializeField]
    Armor lanaArmor;
    [SerializeField]
    string lanaSkillTree;
    UnitData lanaData;

    [SerializeField]
    Unit ethanBase;
    UnitData ethanData;

    [SerializeField]
    Unit saeiBase;
    UnitData saeiData;

    [SerializeField]
    Unit vaueBase;
    UnitData vaueData;

    [SerializeField]
    Unit mayBase;
    UnitData mayData;

    [SerializeField]
    Unit runliBase;
    UnitData runliData;

    [SerializeField]
    Unit colinBase;
    UnitData colinData;

    [SerializeField]
    Unit hanaeiBase;
    UnitData hanaeiData;

    [SerializeField]
    List<WeaponData> weaponList;

    [SerializeField]
    List<ArmorData> armorList;

    List<UnitData> characterList;
    /*
    void Awake()
    {
        characterList = new List<UnitData>() { lanaData, ethanData, saeiData, vaueData, mayData, runliData, colinData, hanaeiData };
        foreach (UnitData data in characterList)
        {
            data.skillTree.CheckAllUnlocked();
            data.skillTree.LoadAllUnlockedAbilities();
        }
    }
    */
}
