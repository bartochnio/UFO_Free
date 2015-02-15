using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class OutOfThePlayzone : MonoBehaviour
{

    public GUITimer timer;
    public Flash flashScript;

    public Canvas myCanvas;
    public Image signImg;

    public float timeToEmergency = 3.0f;

    bool isRunning = false;

    public void SetEmergencyScreen(bool value)
    {
        if ((value && isRunning) || (!value && !isRunning)) return;

        myCanvas.enabled = value;

        if (value)
        {
           
            timer.Timer.StartTimer(timeToEmergency);
        }
        else
        {
            timer.Timer.StopTimer();
        }
        flashScript.enabled = value ;
        isRunning = value;
    }

	// Use this for initialization
	void Start () {
        timer.countBack = true;
        SetEmergencyScreen(false);
	}
	
}
