using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTransitionController
{
    public static WeaponInstance equippedMainHandWeapon;

    public static WeaponInstance equippedOffHandWeapon;

    public static ArmorInstance equippedArmor;

    public static int characterIndex;

    public static BattleMenuPage menuPage;

    public static string origin;

    public static bool activated;

    public static void reset()
    {
        equippedArmor = null;
        equippedMainHandWeapon = null;
        equippedOffHandWeapon = null;
        characterIndex = 0;
        origin = null;
        menuPage = BattleMenuPage.main;
        activated = false;
    }
}
