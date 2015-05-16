using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeLeft : MonoBehaviour {

    public Text guiText;
    public float startTime;
    public float warningTime = 15.0f;

    private float counter = 0.0f;
    private bool paused = false;

	// Use this for initialization
	void Start () 
    {
        counter = startTime;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (paused)
            return;

        counter -= Time.deltaTime;
        counter = Mathf.Max(counter, 0.0f);

        //Timer effect
        guiText.text = counter.ToString("0.0");

        float t = Mathf.Sin(Time.time * 10.0f) * 0.5f + 0.5f;
        if (counter < warningTime)
        {
           guiText.color = Color.Lerp(Color.white, Color.red, t);
        }
        else
           guiText.color = Color.white;
	}

    public void ResetTime()
    {
        counter = startTime;
        paused = false;
    }

    public void Pause(bool val)
    {
        paused = val;
    }
}
