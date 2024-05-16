using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SimLogs : MonoBehaviour
{
    private const string filename = "simlog.txt";
    Storage tcStorage;
    StreamWriter sw;

    // Start is called before the first frame update
    public void StartLogging()
    {
        sw = File.CreateText(filename);
        tcStorage = TownCenter.TC.GetComponent<Storage>();
        InvokeRepeating(nameof(LogVillageStats), 0, 60);
        Debug.Log("Logging...");
    }

    void LogVillageStats()
    {
        Debug.Log("Logging Info");
        StringBuilder sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < (int)ResourceType.MAX_VALUE - 1; i++)
        {
            if (i != 0) sb.Append(',');
            sb.Append(tcStorage.resources[i]);
        }
        sb.Append("]:");
        sb.Append(TownCenter.TC.villagers.Count);
        Debug.Log(sb.ToString());
        sw.WriteLine(sb.ToString());
        sw.Flush();
    }

    private void OnDestroy()
    {
        sw.Close();
    }
}
