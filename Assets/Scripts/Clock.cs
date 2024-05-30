using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Clock : MonoBehaviour
{
    private TextMeshProUGUI timeTextMesh;

    // Start is called before the first frame update
    void Start()
    {
        timeTextMesh = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        int time = (int)Time.timeSinceLevelLoad;
        int hours = time / 3600;
        time %= 3600;
        int minutes = time / 60;
        time %= 60;
        int seconds = time;
        timeTextMesh.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
}
