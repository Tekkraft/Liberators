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
    List<UnitData> units;

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

    [SerializeField]
    string overrideBattlefield;

    public string GetSquadName()
    {
        return squadName;
    }

    public Vector2 GetPosition()
    {
        return squadPosition;
    }

    public List<UnitData> GetUnits()
    {
        return units;
    }

    public List<Vector2Int> GetUnitPositions()
    {
        return unitPositions;
    }

    public OperationsTeam GetTeam()
    {
        return team;
    }

    public OperationsAI GetSquadAI()
    {
        return squadAI;
    }

    public OperationsMoveType GetMovementType()
    {
        return movementType;
    }

    public bool GetSquadAnchored()
    {
        return squadAnchored;
    }

    public string GetOverrideBattlefield()
    {
        return overrideBattlefield;
    }

    public void InitializeData()
    {
        foreach (UnitData data in units)
        {
            data.Initialize();
        }
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