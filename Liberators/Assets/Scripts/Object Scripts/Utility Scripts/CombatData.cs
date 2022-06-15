using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatData : MonoBehaviour
{
    int damageDealt;
    bool lethal;
    bool hit;

    CombatData(int damageDealt, bool lethal, bool hit)
    {
        this.damageDealt = damageDealt;
        this.lethal = lethal;
        this.hit = hit;
    }

    public int getDamageDealt()
    {
        return damageDealt;
    }

    public bool getLethal()
    {
        return lethal;
    }

    public bool getHit()
    {
        return hit;
    }
}
