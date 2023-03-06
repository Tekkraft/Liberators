using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArmorInstance : ItemInstance
{
    [SerializeField]
    Armor armorBase;

    [SerializeField]
    string instanceName;

    [SerializeField]
    Sprite instanceSprite;

    public bool NullCheckBase()
    {
        return armorBase == null;
    }

    public string GetInstanceName()
    {
        if (instanceName != null && instanceName != "")
        {
            return instanceName;
        } else
        {
            return armorBase.GetName();
        }
    }

    public Sprite GetInstanceSprite()
    {
        if (instanceSprite != null)
        {
            return instanceSprite;
        }
        else
        {
            return armorBase.GetSprite();
        }
    }

    public ArmorGrade GetInstanceArmorGrade()
    {
        return armorBase.GetArmorGrade();
    }

    public int[] GetInstanceDefenses()
    {
        return armorBase.GetDefenses();
    }

    public float GetInstanceElementResist(DamageElement searchElement)
    {
        return armorBase.GetElementResist(searchElement);
    }
}
