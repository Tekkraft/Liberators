using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability Object", order = 50)]
public class Ability : ScriptableObject
{
    [SerializeField]
    string abilityName;

    [SerializeField]
    Sprite abilitySprite;

    [SerializeField]
    int abilityAP;

    [SerializeField]
    ActionType abilityType;

    [SerializeField]
    string abilityXMLFolder;

    [SerializeField]
    string abilityXMLFile;

    public string getName()
    {
        return abilityName;
    }

    public Sprite getSprite()
    {
        return abilitySprite;
    }

    public int getAPCost()
    {
        return abilityAP;
    }

    public ActionType getAbilityType()
    {
        return abilityType;
    }

    public string GetAbilityXMLFile()
    {
        if (abilityXMLFolder == null || abilityXMLFile == null || abilityXMLFolder == "" || abilityXMLFile == "")
        {
            Debug.LogError("Null path in either File or Folder for ability: " + abilityName);
            return null;
        }
        return Path.Combine(abilityXMLFolder, abilityXMLFile + ".xml");
    }
}