using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    public WarningMsg warning;
    public Score scoreText;
    public TimeLeft timeLeft;

	// Use this for initialization
	void Start () 
    {
        warning.HideMessage();
        warning.timerEvent += WarningTimeFinished;
        timeLeft.timerEvent += WarningTimeFinished;
	}
	
	// Update is called once per frame
	void Update () 
    {

	}

    public void ChangeScore(int amount, bool good)
    {
        if (good)
            scoreText.SetGoodScore(amount);
        else
            scoreText.SetBadScore(amount);
    }

    public void ResetScore()
    {
        scoreText.ResetScore();
    }

    public void Pause(bool val)
    {
        timeLeft.Pause(val);
    }

    public void ShowWarningMessage(string msg, bool icon, int time)
    {
        warning.ShowMessage(msg, icon, time);
    }

    public void HideWarning()
    {
        warning.HideMessage();
    }

    private void WarningTimeFinished()
    {
        Messenger.Invoke(UFOEvents.GameOver);
       //Scene.GlobalInstance.FinishStage(0);
    }
}
