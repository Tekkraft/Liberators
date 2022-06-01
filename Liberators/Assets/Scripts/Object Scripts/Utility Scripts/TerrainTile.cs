using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    //"Borrowed" from Unity Documentation
    #if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a TerrainTile Asset
    [MenuItem("Assets/Create/TerrainTile")]
    public static void CreateTerrainTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Terrain Tile", "New Terrain Tile", "Asset", "Save Terrain Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TerrainTile>(), path);
    }
    #endif
}
