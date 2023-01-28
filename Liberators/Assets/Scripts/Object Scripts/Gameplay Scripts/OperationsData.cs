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

    public List<SquadSeralization> getSquads()
    {
        return squadData;
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