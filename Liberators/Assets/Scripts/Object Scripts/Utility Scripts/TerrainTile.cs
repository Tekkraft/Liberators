using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainTile : Tile
{
    public string tileName;

    //Can units move past this tile?
    public bool passable;
    //Can units target over this tile?
    public bool pathable;

    public int hitBonus;
    public int avoidBonus;

    public string getTileName()
    {
        return tileName;
    }

    public bool isPassable()
    {
        return passable;
    }

    public bool isPathable()
    {
        return pathable;
    }

    public int[] getBonuses()
    {
        return new int[] { hitBonus, avoidBonus };
    }
}
