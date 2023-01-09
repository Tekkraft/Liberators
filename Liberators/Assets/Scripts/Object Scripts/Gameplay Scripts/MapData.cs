using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map Data File", menuName = "MapData Object", order = 45)]
public class MapData : ScriptableObject
{
    [SerializeField]
    List<SublistInt> teamAlignments;

    [SerializeField]
    List<string> teamNames;

    [SerializeField]
    List<int> AITeams;

    [SerializeField]
    List<ReachCondition> reachWinConditions;

    [SerializeField]
    List<TargetCondition> targetWinConditions;

    [SerializeField]
    List<TimeCondition> timeWinConditions;

    [SerializeField]
    List<ReachCondition> reachLoseConditions;

    [SerializeField]
    List<TargetCondition> targetLoseConditions;

    [SerializeField]
    List<TimeCondition> timeLoseConditions;

    public List<List<int>> getTeamAlignments()
    {
        List<List<int>> alignments = new List<List<int>>();
        foreach (SublistInt temp in teamAlignments)
        {
            alignments.Add(temp.items);
        }
        return alignments;
    }

    public List<int> getAllTeams()
    {
        List<int> teamList = new List<int>();
        foreach (SublistInt temp in teamAlignments)
        {
            teamList.AddRange(temp.items);
        }
        return teamList;
    }

    public Dictionary<int,string> getTeamNames()
    {
        List<int> fullList = getAllTeams();
        Dictionary<int, string> nameList = new Dictionary<int, string>();
        for (int i = 0; i < teamNames.Count; i++)
        {
            if (fullList.Contains(i))
            {
                nameList.Add(i, teamNames[i]);
            }
        }
        return nameList;
    }

    public List<int> getAITeams()
    {
        return AITeams;
    }

    public bool evaluateVictoryConditions(Dictionary<Vector2Int, GameObject> unitList, Dictionary<int, List<GameObject>> teamLists, int currentTurn)
    {
        foreach (ReachCondition zone in reachWinConditions)
        {
            foreach (Vector2Int coord in unitList.Keys)
            {
                if (coord.x <= Mathf.Max(zone.corner1.x, zone.corner2.x) && coord.x >= Mathf.Min(zone.corner1.x, zone.corner2.x))
                {
                    if (coord.y <= Mathf.Max(zone.corner1.y, zone.corner2.y) && coord.y >= Mathf.Min(zone.corner1.y, zone.corner2.y))
                    {
                        if (unitList[coord].GetComponent<UnitController>().getTeam() == zone.targetTeam)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        foreach (TargetCondition target in targetWinConditions)
        {
            if (!unitList.ContainsValue(target.target))
            {
                return true;
            }
        }

        foreach (TimeCondition time in timeWinConditions)
        {
            if (currentTurn >= time.timeRequired)
            {
                return true;
            }
        }

        return false;
    }

    public bool evaluateDefeatConditions(Dictionary<Vector2Int, GameObject> unitList, Dictionary<int, List<GameObject>> teamLists, int currentTurn)
    {
        foreach (ReachCondition zone in reachLoseConditions)
        {
            foreach (Vector2Int coord in unitList.Keys)
            {
                if (coord.x <= Mathf.Max(zone.corner1.x, zone.corner2.x) && coord.x >= Mathf.Min(zone.corner1.x, zone.corner2.x))
                {
                    if (coord.y <= Mathf.Max(zone.corner1.y, zone.corner2.y) && coord.y >= Mathf.Min(zone.corner1.y, zone.corner2.y))
                    {
                        if (unitList[coord].GetComponent<UnitController>().getTeam() == zone.targetTeam)
                        {
                            return true;
                        }
                    }
                }
            }
        }

        foreach (TargetCondition target in targetLoseConditions)
        {
            if (!unitList.ContainsValue(target.target))
            {
                return true;
            }
        }

        foreach (TimeCondition time in timeLoseConditions)
        {
            if (currentTurn >= time.timeRequired)
            {
                return true;
            }
        }

        return false;
    }
}

[System.Serializable]
class SublistInt
{
    public List<int> items;
}

[System.Serializable]
class ReachCondition
{
    public Vector2 corner1;
    public Vector2 corner2;
    public int targetTeam = -1;
}

[System.Serializable]
class TargetCondition
{
    public GameObject target;
}

[System.Serializable]
class TimeCondition
{
    public int timeRequired;
}

