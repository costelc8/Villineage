using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SimLogs : MonoBehaviour
{
    private const string filename = "simlog.txt";
    private bool firstLine = true;
    Storage tcStorage;
    StreamWriter sw;

    // Start is called before the first frame update
    public void StartLogging()
    {
        sw = File.CreateText(filename);
        sw.WriteLine('[');
        tcStorage = TownCenter.TC.GetComponent<Storage>();
        InvokeRepeating(nameof(LogVillageStats), 0, 60);
        Debug.Log("Logging...");
    }

    void LogVillageStats()
    {
        StringBuilder sb = new StringBuilder();
        if (!firstLine) sb.Append(",\n");
        sb.Append("{\"Resources\":{");
        for (int i = 1; i < (int)ResourceType.MAX_VALUE; i++)
        {
            if (i != 1) sb.Append(',');
            sb.Append('\"');
            sb.Append((ResourceType)i);
            sb.Append('\"');
            sb.Append(':');
            sb.Append(tcStorage.resources[i]);
        }
        sb.Append("},\"Villagers\":");
        sb.Append(TownCenter.TC.villagers.Count);
        sb.Append(",\"Houses\":");
        sb.Append(BuildingGenerator.GetHouses().Count);
        sb.Append('}');
        Debug.Log("Logging: " + sb.ToString());
        firstLine = false;
        sw.Write(sb.ToString());
        sw.Flush();
    }

    private void OnDestroy()
    {
        StopLogging();
    }

    public void StopLogging()
    {
        CancelInvoke(nameof(LogVillageStats));
        if (sw != null)
        {
            sw.WriteLine("\n]");
            sw.Close();
            firstLine = true;
            sw = null;
        }
    }
}
