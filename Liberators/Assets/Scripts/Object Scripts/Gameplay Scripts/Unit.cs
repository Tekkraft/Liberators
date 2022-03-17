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
}
