using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory
{
    public static List<ItemInstance> inventory = new List<ItemInstance>();

    public static void LoadInventory(List<ItemInstance> itemList)
    {
        inventory = itemList;
    }

    public static List<ItemInstance> GetInventory()
    {
        return inventory;
    }

    public static List<WeaponInstance> GetWeapons()
    {
        List<WeaponInstance> weapons = new List<WeaponInstance>();
        foreach (ItemInstance item in inventory)
        {
            if (item.GetType() == typeof(WeaponInstance))
            {
                weapons.Add(item as WeaponInstance);
            }
        }
        return weapons;
    }

    public static List<ArmorInstance> GetArmors()
    {
        List<ArmorInstance> armors = new List<ArmorInstance>();
        foreach (ItemInstance item in inventory)
        {
            if (item.GetType() == typeof(ArmorInstance))
            {
                armors.Add(item as ArmorInstance);
            }
        }
        return armors;
    }

    public static void PullItem(ItemInstance item)
    {
        inventory.Remove(item);
    }

    public static void PushItem(ItemInstance item)
    {
        inventory.Add(item);
    }
}