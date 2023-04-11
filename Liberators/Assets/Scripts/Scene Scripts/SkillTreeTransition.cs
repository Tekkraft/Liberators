using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeTransition
{
    public static UnitData unitData;
    public static int characterIndex = -1;
    public static bool activated = false;

    public static void reset()
    {
        unitData = null;
        characterIndex = -1;
        activated = false;
    }
}
