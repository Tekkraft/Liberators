using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainTile : Tile
{
    string tileName;

    //Can units move past this tile?
    bool passable;
    //Can units target over this tile?
    bool pathable;

    int hitBonus;
    int avoidBonus;
}
