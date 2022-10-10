using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Ability", menuName = "Movement Ability", order = 45)]
public class MovementAbility : Ability
{
    [SerializeField]
    int minMoveRange;

    [SerializeField]
    int flatMoveBonus;

    [SerializeField]
    int percentMoveBonus;

    [SerializeField]
    bool ignoresTerrain;

    public int getMinMoveRange()
    {
        return minMoveRange;
    }

    public int getFlatMoveBonus()
    {
        return flatMoveBonus;
    }

    public int getPercentMoveBonus()
    {
        return percentMoveBonus;
    }

    public bool getTerrainIgnore()
    {
        return ignoresTerrain;
    }
}
