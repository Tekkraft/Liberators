using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Unit Object", order = 47)]
public class Unit : ScriptableObject
{
    [SerializeField]
    string unitName;

    //Unit Stats
    /**
     * Movement (MOV): How far units can move in one move action.
     * Strength (STR): How much damage units can do with all physical melee attacks.
     * Potential (POT): How much damage units can do with all magical attacks.
     * Acuity (ACU): How much hit, evasion and critical evasion a unit has for ranged attacks and how much critical a unit has for melee attacks.
     * Finesse (FIN): How much hit, evasion and critical evasion a unit has for melee attacks, how much critical a unit has for ranged attacks.
     * Reaction (REA): Modifies evasion and critical evasion for all attacks and influences counterattacks.
     **/

    [SerializeField]
    int maxHP;

    [SerializeField]
    int mov;

    [SerializeField]
    int str;

    [SerializeField]
    int pot;

    [SerializeField]
    int acu;

    [SerializeField]
    int fin;

    [SerializeField]
    int rea;

    [SerializeField]
    string className;

    [SerializeField]
    List<Ability> prfAbilities;

    [SerializeField]
    UnitAnims unitAnims;

    [SerializeField]
    SkillTree unitSkillTree;

    public string getUnitName()
    {
        return unitName;
    }

    public int[] getStats()
    {
        return new int[] { maxHP, mov, str, pot, acu, fin, rea };
    }

    public List<Ability> getAbilities()
    {
        return prfAbilities;
    }

    public Sprite getBattleSprite(string tag)
    {
        return unitAnims.getLinkedSprite(tag);
    }

    public SkillTree getSkillTree()
    {
        return unitSkillTree;
    }
}

[System.Serializable]
class UnitAnims
{
    //TODO: Replace with proper animations when those exist
    [SerializeField]
    Sprite idleSprite;

    [SerializeField]
    Sprite attackSprite;

    [SerializeField]
    Sprite evadeSprite;

    [SerializeField]
    Sprite deadStaggerSprite;

    [SerializeField]
    Sprite deadSprite;

    [SerializeField]
    Sprite blockSprite;

    [SerializeField]
    Sprite staggerSprite;

    [SerializeField]
    Sprite defendSprite;

    public Sprite getLinkedSprite(string tag)
    {
        switch (tag)
        {
            case "attack":
                return attackSprite;

            case "evade":
                return evadeSprite;

            case "dead stagger":
                return deadStaggerSprite;

            case "dead":
                return deadSprite;

            case "block":
                return blockSprite;

            case "stagger":
                return staggerSprite;

            case "defend":
                return defendSprite;

            default:
                return idleSprite;
        }
    }
}