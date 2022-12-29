using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Armor Object", order = 49)]
public class Armor : ScriptableObject, IItem
{
    [SerializeField]
    string armorName;

    [SerializeField]
    int armorDefense;

    [SerializeField]
    int armorMagDefense;

    [SerializeField]
    List<ArmorResistPair> armorResistances;

    public string getName ()
    {
        return armorName;
    }

    public Sprite getImage()
    {
        return null;
    }

    public int[] getDefenses ()
    {
        return new int[] { armorDefense, armorMagDefense };
    }

    public float getElementResist(element searchElement)
    {
        ArmorResistPair value = containsElement(searchElement);
        if (value != null)
        {
            return value.resistPercent;
        }
        return 1f;
    }

    ArmorResistPair containsElement(element searchElement)
    {
        if (armorResistances == null)
        {
            return null;
        }
        foreach (ArmorResistPair value in armorResistances)
        {
            if (value.armorElement == searchElement)
            {
                return value;
            }
        }
        return null;
    }
}

[System.Serializable]
class ArmorResistPair {
    [SerializeField]
    public element armorElement;

    [SerializeField]
    public float resistPercent;
}
