using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WarningMsg : MonoBehaviour {

    public Image iconImage;
    public Text emergencyTime;
    public Text message;

    private bool isShown;
    private float timer = 0.0f;
    
	// Use this for initialization
	void Start () 
    {
	}

    public void ShowMessage(string msg, bool showIcon, int startTime)
    {
        isShown = true;

        message.enabled = true;
        message.text = msg;

        iconImage.enabled = showIcon;
        emergencyTime.enabled = startTime > 0;

        timer = startTime;
    }

    public void HideMessage()
    {
        isShown = false;

        message.enabled = false;
        iconImage.enabled = false;
        emergencyTime.enabled = false;

        timer = 0.0f;
    }

	// Update is called once per frame
	void Update () 
    {
        if (!isShown)
            return;

       //blink icon
       //<Kamil> : We can probably switch that to a coroutine and support more effects
       if (iconImage.enabled)
       {
           Color color = iconImage.color;
           color.a = Mathf.Sin(Time.time * 15.0f) * 0.5f + 0.5f;
           iconImage.color = color;
       }

       //handle timer
       if (emergencyTime.enabled)
       {
           timer -= Time.deltaTime;
           emergencyTime.text = timer.ToString("0.0");

           if (timer <= 0.0f)
           {
               timer = 0.0f;
               emergencyTime.enabled = false;
               
               if (timerEvent != null) timerEvent();
           }
       }
	}

    public event WarningTimerFinished timerEvent;
    public delegate void WarningTimerFinished();
}
