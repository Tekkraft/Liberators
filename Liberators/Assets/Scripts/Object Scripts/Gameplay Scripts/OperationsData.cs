using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Operations Data File", menuName = "OpsData Object", order = 41)]
public class OperationsData : ScriptableObject
{
    [SerializeField]
    string operationsName;

    [SerializeField]
    List<SquadSeralization> squadData;

    [SerializeField]
    List<OperationsReachCondition> playerReachConditions;

    [SerializeField]
    List<OperationsReachCondition> enemyReachConditions;

    public string getOperationsName()
    {
        return operationsName;
    }

    public List<SquadSeralization> getSquads()
    {
        return squadData;
    }

    public List<OperationsReachCondition> getPlayerReachWinConditions()
    {
        return playerReachConditions;
    }

    public List<OperationsReachCondition> getEnemyReachWinConditions()
    {
        return playerReachConditions;
    }
}

[System.Serializable]
public class SquadSeralization
{
    [SerializeField]
    string squadName;

    [SerializeField]
    Vector2 squadPosition;

    [SerializeField]
    List<UnitEntryData> units;

    [SerializeField]
    List<Vector2Int> unitPositions;

    [SerializeField]
    OperationsTeam team;

    [SerializeField]
    OperationsAI squadAI;

    [SerializeField]
    OperationsMoveType movementType;

    [SerializeField]
    bool squadAnchored;

    public string getSquadName()
    {
        return squadName;
    }

    public Vector2 getPosition()
    {
        return squadPosition;
    }

    public List<UnitEntryData> getUnits()
    {
        return units;
    }

    public List<Vector2Int> getUnitPositions()
    {
        return unitPositions;
    }

    public OperationsTeam getTeam()
    {
        return team;
    }

    public OperationsAI getSquadAI()
    {
        return squadAI;
    }

    public OperationsMoveType getMovementType()
    {
        return movementType;
    }

    public bool getSquadAnchored()
    {
        return squadAnchored;
    }
}

[System.Serializable]
public class OperationsReachCondition
{
    [SerializeField]
    Vector2Int corner1;

    [SerializeField]
    Vector2Int corner2;

    [SerializeField]
    OperationsTeam team;

    public List<Vector2Int> getReachCorners()
    {
        return new List<Vector2Int>() { corner1, corner2 };
    }

    public OperationsTeam getReachTeam()
    {
        return team;
    }
}