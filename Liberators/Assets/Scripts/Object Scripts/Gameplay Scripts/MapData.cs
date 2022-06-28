using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map Data File", menuName = "MapData Object", order = 45)]
public class MapData: ScriptableObject
{
    //To Add: Win/Lose Conditions
    [SerializeField]
    List<SublistInt> teamAlignments;

    public List<List<int>> getTeamAlignments()
    {
        List<List<int>> alignments = new List<List<int>>();
        foreach (SublistInt temp in teamAlignments)
        {
            alignments.Add(temp.items);
        }
        return alignments;
    }
}

[System.Serializable]
public class SublistInt
{
    public List<int> items;
}