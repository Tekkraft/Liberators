using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Armor Object", order = 49)]
public class Armor : ScriptableObject
{
    [SerializeField]
    string armorName;

    [SerializeField]
    Sprite armorIcon;

    [SerializeField]
    int armorDefense;

    [SerializeField]
    int armorMagDefense;

    [SerializeField]
    ArmorGrade armorGrade;

    [SerializeField]
    List<ArmorResistPair> armorResistances;

    public string GetName ()
    {
        return armorName;
    }

    public Sprite GetSprite()
    {
        return armorIcon;
    }

    public ArmorGrade GetArmorGrade()
    {
        return armorGrade;
    }

    public int[] GetDefenses ()
    {
        return new int[] { armorDefense, armorMagDefense };
    }

    public float GetElementResist(DamageElement searchElement)
    {
        ArmorResistPair value = ContainsElement(searchElement);
        if (value != null)
        {
            return value.GetResistPercent();
        }
        return 1f;
    }

    ArmorResistPair ContainsElement(DamageElement searchElement)
    {
        if (armorResistances == null)
        {
            return null;
        }
        foreach (ArmorResistPair value in armorResistances)
        {
            if (value.GetElement() == searchElement)
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
    DamageElement armorElement;

    [SerializeField]
    float resistPercent;

    public DamageElement GetElement()
    {
        return armorElement;
    }

    public float GetResistPercent()
    {
        return resistPercent;
    }
}
