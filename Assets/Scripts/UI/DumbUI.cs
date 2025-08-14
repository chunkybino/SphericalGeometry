using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DumbUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] TextMeshProUGUI avgFpsText;

    [SerializeField] int frameCount;
    [SerializeField] float avgFPSInterval = 1;
    
    [SerializeField] float timeCount;

    // Update is called once per frame
    void Update()
    {
        int thisFps = Mathf.FloorToInt(1.0f/Time.deltaTime);
        fpsText.text = "FPS: " + thisFps;

        frameCount++;

        timeCount += Time.deltaTime;

        if (timeCount > avgFPSInterval) 
        {
            timeCount -= avgFPSInterval;

            avgFpsText.text = "AvgFPS: " + (frameCount/avgFPSInterval);

            frameCount = 0;
        }
    }
}
