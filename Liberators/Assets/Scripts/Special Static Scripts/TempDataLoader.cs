using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDataLoader : MonoBehaviour
{
    [SerializeField]
    FullUnitData lanaData;

    [SerializeField]
    FullUnitData ethanData;

    [SerializeField]
    FullUnitData saeiData;

    [SerializeField]
    FullUnitData vaueData;

    [SerializeField]
    FullUnitData mayData;

    [SerializeField]
    FullUnitData runliData;

    [SerializeField]
    FullUnitData colinData;

    [SerializeField]
    FullUnitData hanaeiData;

    [SerializeField]
    List<WeaponInstance> weapons;

    [SerializeField]
    List<ArmorInstance> armors;

    void Awake()
    {
        List<FullUnitData> dataList = new List<FullUnitData>()
        {
            lanaData,
            ethanData,
            saeiData,
            vaueData,
            mayData,
            runliData,
            colinData,
            hanaeiData
        };
        PlayerRoster.LoadData(dataList);
        List<ItemInstance> items = new List<ItemInstance>();
        foreach (WeaponInstance weapon in weapons)
        {
            items.Add(weapon);
        }
        foreach (ArmorInstance armor in armors)
        {
            items.Add(armor);
        }
        PlayerInventory.LoadInventory(items);
    }
}
