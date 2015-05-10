using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {

    public WarningMsg warning;
    public Score scoreText;

	// Use this for initialization
	void Start () 
    {
        warning.HideMessage();
        warning.timerEvent += WarningTimeFinished;
	}
	
	// Update is called once per frame
	void Update () 
    {

	}

    public void ChangeScore(int amount)
    {
        scoreText.ChangeScore(amount);
    }

    public void ResetScore()
    {
        scoreText.ResetScore();
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
        Scene.GlobalInstance.FinishStage(0);
    }
}
