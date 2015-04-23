using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class OutOfThePlayzone : MonoBehaviour
{

    public GUITimer timer;
    public Canvas myCanvas;
    public Image signImg;

    public float timeToEmergency = 3.0f;

    bool isRunning = true;


    void OnLevelWasLoaded()
    {
        SetEmergencyScreen(false);
        
    }

    public void SetEmergencyScreen(bool value)
    {
        if (value == isRunning) return;

        myCanvas.enabled = value;

        if (value)
        {
            timer.Timer.StartTimer(timeToEmergency);
        }
        else
        {
            timer.Timer.StopTimer();
        }

        isRunning = value;
    }

	// Use this for initialization
	void Start () {
        timer.countBack = true; 
	}

    void Update()
    {
        if (isRunning)
        {
            Color color = signImg.color;
            color.a = Mathf.Sin(Time.time * 15.0f) * 0.5f + 0.5f;
            signImg.color = color;
        }
    }

   



   
	
}
