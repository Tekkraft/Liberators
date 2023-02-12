using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTransitionController
{
    public static WeaponInstance equippedWeapon;

    public static ArmorInstance equippedArmor;

    public static int characterIndex;

    public static BattleMenuPage menuPage;

    public static string origin;

    public static void reset()
    {
        equippedArmor = null;
        equippedWeapon = null;
        characterIndex = 0;
        origin = null;
        menuPage = BattleMenuPage.main;
    }
}
