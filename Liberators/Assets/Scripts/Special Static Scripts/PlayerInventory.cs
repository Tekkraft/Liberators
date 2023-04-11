using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory
{
    public static List<ItemData> inventory = new List<ItemData>();

    public static void LoadInventory(List<ItemData> itemList)
    {
        inventory = itemList;
    }

    public static List<ItemData> GetInventory()
    {
        return inventory;
    }

    public static List<WeaponData> GetWeapons()
    {
        List<WeaponData> weapons = new List<WeaponData>();
        foreach (ItemData item in inventory)
        {
            if (item.itemType == "weapon")
            {
                weapons.Add(item as WeaponData);
            }
        }
        return weapons;
    }

    public static List<ArmorData> GetArmors()
    {
        List<ArmorData> armors = new List<ArmorData>();
        foreach (ItemData item in inventory)
        {
            if (item.itemType == "armor")
            {
                armors.Add(item as ArmorData);
            }
        }
        return armors;
    }

    public static void PullItem(ItemData item)
    {
        inventory.Remove(item);
    }

    public static void PushItem(ItemData item)
    {
        inventory.Add(item);
    }
}