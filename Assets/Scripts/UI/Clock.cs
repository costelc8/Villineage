using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Clock : NetworkBehaviour
{
    private TextMeshProUGUI timeTextMesh;
    public float totalTime = 0f;
    [SyncVar(hook = nameof(TimeHook))]
    public int time;

    // Start is called before the first frame update
    public void Initialize()
    {
        timeTextMesh = GetComponent<TextMeshProUGUI>();
        totalTime = 0f;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isServer) return;
        totalTime += Time.deltaTime;
        time = (int)totalTime;
    }

    public void TimeHook(int oldValue, int newValue)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        int t = time;
        int hours = t / 3600;
        t %= 3600;
        int minutes = t / 60;
        t %= 60;
        int seconds = t;
        timeTextMesh.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
}
