using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICoordinator
{
    public static List<GameObject> seenEnemies = new List<GameObject>();

    public static void clearSeenEnemies()
    {
        seenEnemies = new List<GameObject>();
    }
}
