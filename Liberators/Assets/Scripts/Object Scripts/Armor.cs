using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Armor Object", order = 49)]
public class Armor : ScriptableObject
{
    [SerializeField]
    string armorName;

    [SerializeField]
    int armorDefense;

    [SerializeField]
    int armorMagDefense;

    public string getName ()
    {
        return armorName;
    }

    public int[] getDefenses ()
    {
        return new int[] { armorDefense, armorMagDefense };
    }
}
