using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTransitionController
{
    public static Weapon equippedWeapon;

    public static Armor equippedArmor;

    public static int characterId;

    public static string origin;

    public static void reset()
    {
        equippedArmor = null;
        equippedWeapon = null;
        characterId = -1;
        origin = null;
    }
}
