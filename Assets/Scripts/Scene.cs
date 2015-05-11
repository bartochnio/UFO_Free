using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scene : MonoBehaviour {

    public float stageTime;
    public GUITimer timer;

    public HUD hud;

    public PlayerController player;
    public CameraController cam;
    public bool isPaused = false;


	DevGuiSystem.Helper devControls = new DevGuiSystem.Helper (); // DEV PANEL


	int maxGoodCollectibles;
    int currentGoodCollectibles;
    int currentBadCollectibles;

    public const int GoodPoints = 10;
    public const int BadPoints = -10;

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
		CreateDevControls ();

        // Logic is always first so it's safe to be called here 

        menuMgr = MenuMgr.GlobalInstance;

        timer = menuMgr.playTimer;
	}

	// DEV PANEL
	void CreateDevControls () {
		devControls.ReleaseControls ();
		devControls.HoldOnTo (DevGuiSystem.globalInstance.AddLabel ("Scene setup:"));
		{
			var lightAngleSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Light Angle"));
			lightAngleSlider.MinFloat = 10;
			lightAngleSlider.MaxFloat = 100;
			lightAngleSlider.OnChanged += () => {
				player.transform.Find ("Spotlight").GetComponent <Light> ().spotAngle = lightAngleSlider.Float;
				PlayerPrefs.SetFloat ("SceneSetup-PlayerLightAngle", lightAngleSlider.Float);
			};
			lightAngleSlider.Float = PlayerPrefs.GetFloat ("SceneSetup-PlayerLightAngle", 32.0f);
			player.transform.Find ("Spotlight").GetComponent <Light> ().spotAngle = lightAngleSlider.Float;

			/*var lightRangeSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Light Range"));
			lightRangeSlider.MinFloat = 10;
			lightRangeSlider.MaxFloat = 100;
			lightRangeSlider.OnChanged += () => {
				player.transform.Find ("Spotlight").GetComponent <Light> ().range = lightRangeSlider.Float;
				PlayerPrefs.SetFloat ("SceneSetup-PlayerLightRange", lightRangeSlider.Float);
			};
			lightRangeSlider.Float = PlayerPrefs.GetFloat ("SceneSetup-PlayerLightRange", 10.0f);
			player.transform.Find ("Spotlight").GetComponent <Light> ().range = lightRangeSlider.Float; */

			var cameraSizeSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Camera Size"));
			cameraSizeSlider.MinFloat = 0.5f;
			cameraSizeSlider.MaxFloat = 10;
			cameraSizeSlider.OnChanged += () => {
				Camera.main.orthographicSize = cameraSizeSlider.Float;
				PlayerPrefs.SetFloat ("SceneSetup-CameraSize", cameraSizeSlider.Float);
			};
			cameraSizeSlider.Float = PlayerPrefs.GetFloat ("SceneSetup-CameraSize", 2.186279f);
			Camera.main.orthographicSize = cameraSizeSlider.Float;

			var maxSpeedSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Speed"));
			maxSpeedSlider.MinFloat = 0.5f;
			maxSpeedSlider.MaxFloat = 10.0f;
			maxSpeedSlider.OnChanged += () => {
				player.maxSpeed = maxSpeedSlider.Float;
				PlayerPrefs.SetFloat ("SceneSetup-PlayerMoveSpeed", maxSpeedSlider.Float);
			};
			maxSpeedSlider.Float = PlayerPrefs.GetFloat ("SceneSetup-PlayerMoveSpeed", 4.0f);
			player.maxSpeed = maxSpeedSlider.Float;

			var stageTimeSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Stage time", wholeNumbers: true));
			stageTimeSlider.MinFloat = 10;
			stageTimeSlider.MaxFloat = 300;
			stageTimeSlider.OnChanged += () => {
				stageTime = stageTimeSlider.Float;
				PlayerPrefs.SetFloat ("SceneSetup-StageTime", stageTimeSlider.Float);
			};
			stageTimeSlider.Float = PlayerPrefs.GetFloat ("SceneSetup-StageTime", 30);
			stageTime = stageTimeSlider.Float;


			var resetButton = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddButton ("Reset", "Reset scene setup values"));
			resetButton.OnChanged += () => {
				lightAngleSlider.Float = 32.0f;
				//lightRangeSlider.Float = 10.0f;
				cameraSizeSlider.Float = 2.186279f;
				maxSpeedSlider.Float = 4.0f;
				stageTimeSlider.Float = 30;
			};
		}

		devControls.HoldOnTo (DevGuiSystem.globalInstance.AddEmpty ());
	}
	//

	void Start () 
    {
        timer.Timer.StartTimer(stageTime);
        //menuMgr.playScore.ResetScore();
        timer.Timer.IntervalEvent += this.TimerEventHandler;
        
        List<Collectable> all = new List<Collectable>(FindObjectsOfType<Collectable>());

        maxGoodCollectibles = all.FindAll(x => x.collectType == Collectable.CollectType.Good).Count;

        menuMgr.inGameMenu.ShowMenu(false);

	}

    void OnDestroy()
    {
		devControls.ReleaseControls ();

        timer.Timer.IntervalEvent -= this.TimerEventHandler;
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
            cam.Shake();
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
        hud.ChangeScore(amount);
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
        hud.ResetScore();
        menuMgr.SetMenuVisiable(MenuMgr.MenuTypes.StageEndSucces);
        // Need Refactor on this
        StartCoroutine( menuMgr.endScreenSucces.GetComponentInChildren<ScorringScreen>().ScoringRoutine(currentGoodCollectibles, currentBadCollectibles, finishTime, GoodPoints, BadPoints, 10));
    }

    public void SetOutsideTrackMsg(bool value)
    {
        if (value)
            hud.ShowWarningMessage("GET BACK TO THE TRACK!", true, 3);
        else
            hud.HideWarning();
    }

    public void SetWrongWayMsg(bool value)
    {
        if (value)
            hud.ShowWarningMessage("WRONG WAY!", true, 0);
        else
            hud.HideWarning();
    }

    public void SetPause(bool value)
    {
        isPaused = value;
        timer.Timer.enabled = !value;
        player.enabled = !value;
    }
    	
}
