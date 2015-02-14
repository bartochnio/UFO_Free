using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUITimer : MonoBehaviour {
    public Text guiText;
    public float alarm;
    private TimerThingy timer;

    public TimerThingy Timer
    {
        get { return timer; }
        private set { timer = value; }
    }

    void OnEnable()
    {
        timer.IntervalEvent += OnTimerAlarm;
    }

    void OnDisable()
    {
        timer.IntervalEvent -= OnTimerAlarm;
    }

    void Awake()
    {
        timer = gameObject.AddComponent<TimerThingy>();
        guiText = gameObject.GetComponent<Text>();
    }

	// Use this for initialization
	void Start () 
    {
        //timer.StartTimer(alarm);
	}
	
    void OnTimerAlarm(TimerThingy sender,  float t)
    {
        
    }

	// Update is called once per frame
	void Update () {
        guiText.text = timer.TimePast.ToString("0.0");
	}

    
}
