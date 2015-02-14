using UnityEngine;
using System.Collections;

public class TimerThingy : MonoBehaviour {

    float _timePast = 0;

    public float TimePast
    {
        get { return _timePast; }
    }

    public float TimeLeft
    {
        get { return _alarmTime - TimePast; }
    }

    float _alarmTime = 0;
    
    public void StartTimer(float alarmTime = Mathf.Infinity)
    {
        _timePast = 0;
        enabled = true;
        _alarmTime = alarmTime;
    }

    public float StopTimer()
    {
        enabled = false;
        if (IntervalEvent != null)
        {
            IntervalEvent(this, _timePast);
        }
        enabled = false;
        return _timePast;
    }

    void Awake()
    {
        enabled = false;
    }

  	// Update is called once per frame
	void Update () {
        
            _timePast += Time.deltaTime;
            if (_timePast >= _alarmTime)
            {
                if (IntervalEvent != null)
                {
                    IntervalEvent(this, _timePast);
                }
                enabled = false;

            }
       
	}

    public event IntervalDel IntervalEvent;

    public delegate void IntervalDel(TimerThingy sender, float interval);
}
