using UnityEngine;
using System.Collections;

public class StageController : MonoBehaviour {

    public HUD hud;
    public const int GoodPoints = 10;
    public const int BadPoints = -10;

    private int score = 0;

    //Events
    void OnEnable()
    {
        Messenger.AddListener(UFOEvents.PlayerFail, OnScoreBad);
        Messenger.AddListener(UFOEvents.PlayerScore, OnScoreGood);
        Messenger<bool>.AddListener(UFOEvents.PlayerOutside, OnPlayerOutside);
        Messenger<bool>.AddListener(UFOEvents.PlayerWrongWay, OnPlayerWrongWay);
    }

    void OnDisable()
    {
        Messenger.RemoveListener(UFOEvents.PlayerFail, OnScoreBad);
        Messenger.RemoveListener(UFOEvents.PlayerScore, OnScoreGood);
        Messenger<bool>.RemoveListener(UFOEvents.PlayerOutside, OnPlayerOutside);
        Messenger<bool>.RemoveListener(UFOEvents.PlayerWrongWay, OnPlayerWrongWay);
    }

	// Use this for initialization
	void Start () 
    {
        ResetScore();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    private void ResetScore()
    {
        score = 0;
        hud.ResetScore();
    }

    //Event handlers
    private void OnScoreBad()
    {
        score += BadPoints;
        hud.ChangeScore(score, false);
    }

    private void OnScoreGood()
    {
        score += GoodPoints;
        hud.ChangeScore(score, true);
    }

    private void OnPlayerOutside(bool val)
    {
        if (val)
            hud.ShowWarningMessage("GET BACK TO THE TRACK!", true, 3);
        else
            hud.HideWarning();
    }

    private void OnPlayerWrongWay(bool val)
    {
        if (val)
            hud.ShowWarningMessage("WRONG WAY!", true, 0);
        else
            hud.HideWarning();
    }
}
