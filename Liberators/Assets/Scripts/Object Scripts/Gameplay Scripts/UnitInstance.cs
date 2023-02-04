using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInstance
{
    Unit baseUnit;

    int maxHP;
    int mov;
    int str;
    int pot;
    int acu;
    int fin;
    int rea;
    List<Ability> prfAbilities;
    SkillTreeInstance skillTree;

    public UnitInstance(Unit baseUnit)
    {
        this.baseUnit = baseUnit;
        maxHP = baseUnit.getStats()[0];
        mov = baseUnit.getStats()[1];
        str = baseUnit.getStats()[2];
        pot = baseUnit.getStats()[3];
        acu = baseUnit.getStats()[4];
        fin = baseUnit.getStats()[5];
        rea = baseUnit.getStats()[6];
        prfAbilities = baseUnit.getAbilities();
        skillTree = new SkillTreeInstance(baseUnit.getSkillTree());
    }

    public string getUnitName()
    {
        return baseUnit.getUnitName();
    }

    public int[] getStats()
    {
        return new int[] { maxHP, mov, str, pot, acu, fin, rea };
    }

    public List<Ability> getAbilities()
    {
        List<Ability> abilities = new List<Ability>();
        abilities.AddRange(prfAbilities);
        abilities.AddRange(skillTree.getObtainedAbilities());
        return abilities;
    }

    public Sprite getBattleSprite(string tag)
    {
        return baseUnit.getBattleSprite(tag);
    }

    public SkillTreeInstance getSkillTree()
    {
        return skillTree;
    }

    public void updateSkillTree(SkillTreeInstance skillTree)
    {
        this.skillTree = skillTree;
    }

    public void increaseStats(List<int> stats)
    {
        if (stats.Count <= 7)
        {
            return;
        }
        maxHP += stats[0];
        mov += stats[1];
        str += stats[2];
        pot += stats[3];
        acu += stats[4];
        fin += stats[5];
        rea += stats[6];
    }
}