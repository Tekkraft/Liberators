using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSystem
{
    public static void SaveTeamData(string data)
    {
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "teamFile.json"), data);
    }

    public static string LoadTeamData()
    {
        return File.ReadAllText(Path.Combine(Application.persistentDataPath, "teamFile.json"));
    }
}
