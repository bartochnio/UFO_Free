using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scene : MonoBehaviour {

    public float stageTime;
    public GUITimer timer;
    public PlayerController player;
    public CameraController camera;
    public bool isPaused = false;


    int maxGoodCollectibles;
    int currentGoodCollectibles;
    int currentBadCollectibles;

    public const int GoodPoints = 10;
    public const int BadPoints = -10;

    public OutOfThePlayzone outOfPlayzoneScript;

    public int CurrentCollectScore
    {
        get
        {
            return currentBadCollectibles * BadPoints + currentGoodCollectibles * GoodPoints;
        }
    }

    public float timeLeft
    {
        get { return stageTime - timer.Timer.TimePast; }
    }

    static MenuMgr menuMgr;

    static Scene globalInstance = null;

    public static Scene GlobalInstance
    {
        get
        {
            if (globalInstance == null) globalInstance = FindObjectOfType<Scene>();
            return Scene.globalInstance;
        }
    }
    void Awake()
    {
        // Logic is always first so it's safe to be called here 

        menuMgr = MenuMgr.GlobalInstance;

        timer = menuMgr.playTimer;
    }

	void Start () 
    {
        timer.Timer.StartTimer(stageTime);
        menuMgr.playScore.ResetScore();
        timer.Timer.IntervalEvent += this.TimerEventHandler;
        
        List<Collectable> all = new List<Collectable>(FindObjectsOfType<Collectable>());

        maxGoodCollectibles = all.FindAll(x => x.collectType == Collectable.CollectType.Good).Count;

        outOfPlayzoneScript = FindObjectOfType<OutOfThePlayzone>();
        outOfPlayzoneScript.timer.Timer.IntervalEvent += TimeOuthandler;
	}

    void TimeOuthandler(TimerThingy t, float time)
    {
        if (t.TimeLeft <= 0)
        Scene.GlobalInstance.FinishStage(0);
    }

    void OnDestroy()
    {
        timer.Timer.IntervalEvent -= this.TimerEventHandler;
        outOfPlayzoneScript.timer.Timer.IntervalEvent -= TimeOuthandler;
    }


    public void ScoreCollectible(Collectable.CollectType type)
    {
        int s = 0;

        if (type == Collectable.CollectType.Good)
        {
            s = GoodPoints;
            currentGoodCollectibles++;
        }
        else
        {
            camera.Shake();
            s = BadPoints;
            currentBadCollectibles++;
        }

        if (currentGoodCollectibles == maxGoodCollectibles)
        {
            FinishStage(timeLeft);
        }

        ChangeDisplayedScore(s);
    }


    public void ChangeDisplayedScore(int amount)
    {
        
        menuMgr.playScore.ChangeScore(amount);
    }

    public void TimerEventHandler(TimerThingy sender, float intervalTime)
    {
        if (intervalTime != 0)
        FinishStage(timeLeft);
    }

    public void FinishStage(float finishTime)
    {
        // ToDo: Show all necessary stuff. Show the score set score etc.
        SetPause(true);
        menuMgr.playScore.ResetScore();
        menuMgr.SetMenuVisiable(MenuMgr.MenuTypes.StageEndSucces);
        // Need Refactor on this
        StartCoroutine( menuMgr.endScreenSucces.GetComponentInChildren<ScorringScreen>().ScoringRoutine(currentGoodCollectibles, currentBadCollectibles, finishTime, GoodPoints, BadPoints, 10));
    }

    public void SetOutsideTheTrack(bool value)
    {
        if (outOfPlayzoneScript != null)
            outOfPlayzoneScript.SetEmergencyScreen(value);
        else Debug.LogError(name + " out of the playzone script is missing");
    }

    public void SetPause(bool value)
    {
        isPaused = value;
        timer.Timer.enabled = !value;
        player.enabled = !value;
    }
    	
}
