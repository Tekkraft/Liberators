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

    public string GetOperationsName()
    {
        return operationsName;
    }

    public void InitializeSquads()
    {
        foreach (SquadSeralization squad in squadData)
        {
            squad.InitializeData();
        }
    }

    public List<SquadSeralization> GetSquads()
    {
        return squadData;
    }

    public List<OperationsReachCondition> GetPlayerReachWinConditions()
    {
        return playerReachConditions;
    }

    public List<OperationsReachCondition> GetEnemyReachWinConditions()
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
    List<OperationsUnitData> unitBases;

    List<UnitData> unitData;

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

    public List<OperationsUnitData> GetUnitBases()
    {
        return unitBases;
    }

    public List<UnitData> GetUnitData()
    {
        return unitData;
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
        foreach (OperationsUnitData data in unitBases)
        {
            UnitData unit = new UnitData();
            unit.Initialize(data.GetUnitBase(), data.GetMainWeaponId(), data.GetSecondaryWeaponId(), data.GetArmorId());
            for (int i = 0; i < data.GetBonusLevels(); i++)
            {
                unit.maxHP += Mathf.CeilToInt(unit.maxHP * 0.1f);
                unit.currentHP = unit.maxHP;
                unit.str += Mathf.CeilToInt(unit.str * 0.1f);
                unit.pot += Mathf.CeilToInt(unit.pot * 0.1f);
                unit.fin += Mathf.CeilToInt(unit.fin * 0.1f);
                unit.acu += Mathf.CeilToInt(unit.acu * 0.1f);
                unit.rea += Mathf.CeilToInt(unit.rea * 0.1f);
                for (int j = 0; j < 3; j++)
                {
                    int stat = Random.Range(0,6);
                    switch (stat)
                    {
                        case 0:
                            unit.maxHP += Random.Range(1, 4);
                            unit.currentHP = unit.maxHP;
                            break;

                        case 1:
                            unit.str++;
                            break;

                        case 2:
                            unit.pot++;
                            break;

                        case 3:
                            unit.fin++;
                            break;

                        case 4:
                            unit.acu++;
                            break;

                        case 5:
                            unit.rea++;
                            break;

                    }
                }
            }
            unitData.Add(unit);
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

[System.Serializable]
public class OperationsUnitData
{
    [SerializeField]
    string unitBase;

    [SerializeField]
    string mainWeaponId;

    [SerializeField]
    string secondaryWeaponId;

    [SerializeField]
    string armorId;

    [SerializeField]
    int bonusLevels;

    public string GetUnitBase()
    {
        return unitBase;
    }

    public string GetMainWeaponId()
    {
        return mainWeaponId;
    }

    public string GetSecondaryWeaponId()
    {
        return secondaryWeaponId;
    }

    public string GetArmorId()
    {
        return armorId;
    }

    public int GetBonusLevels()
    {
        return bonusLevels;
    }

}