using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeExitHandler
{
    public static SkillTreeInstance activeTree;
    public static int characterIndex = -1;
    public static bool activated = false;

    public static void reset()
    {
        activeTree = null;
        characterIndex = -1;
        activated = false;
    }
}
