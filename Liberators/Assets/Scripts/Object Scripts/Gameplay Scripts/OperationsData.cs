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
    operationsTeam team;

    [SerializeField]
    operationsAI squadAI;

    [SerializeField]
    operationsMoveType movementType;

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

    public operationsTeam getTeam()
    {
        return team;
    }

    public operationsAI getSquadAI()
    {
        return squadAI;
    }

    public operationsMoveType getMovementType()
    {
        return movementType;
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
    operationsTeam team;

    public List<Vector2Int> getReachCorners()
    {
        return new List<Vector2Int>() { corner1, corner2 };
    }

    public operationsTeam getReachTeam()
    {
        return team;
    }
}