using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class MenuMgr : MonoBehaviour {

    static MenuMgr globalInstance = null;

    public static MenuMgr GlobalInstance
    {
        get {
                if (globalInstance == null) globalInstance = FindObjectOfType<MenuMgr>();    
                return MenuMgr.globalInstance;
            }
    }
    

    public enum MenuTypes
    {
        MainMenu,
        PackSelection,
        StageSelect,
        InGame,
        EndStage,
        LoadScreen,
        StageEndSucces
    }


    // Canvases has to be named after the MenuTypes items' names
    public Canvas mainC;
    public Canvas stageC;
    public Canvas inGameC;
    //public Canvas endStageC;
    public Canvas packSelectionC;
    public Canvas loadScreen;
    public Canvas endScreenSucces;
    //~~

    public GUITimer playTimer;
    public Score playScore;
    static InGameMenu _ingameMenu;

    public InGameMenu inGameMenu
    {
        get { return MenuMgr._ingameMenu; }
    }
    
    // ToDo: Advertising canvas as overlay

    private List<Canvas> allCanvases;

    public void SetMenuVisiable(MenuTypes type)
    {
        string sType = type.ToString();

        SetMenuVisiableFromStr(sType);

    }

    // Need to do this like that, because of button event hook up in new gui system. Cannot pass enum as arg
    public void SetMenuVisiableFromStr(string menuName)
    {
        foreach (Canvas c in allCanvases)
        {
            c.enabled = (c.name == menuName);
        }
    }

	// Use this for initialization
	void Start () {

        Object.DontDestroyOnLoad(this.gameObject);

        allCanvases = new List<Canvas>() { mainC, stageC, inGameC, packSelectionC, loadScreen, endScreenSucces};
        foreach (Canvas c in allCanvases)
        {
            Object o = c.gameObject as Object;
            Object.DontDestroyOnLoad(o);
        }

        //SetMenuVisiable(MenuTypes.MainMenu);
        SetStagePackButtons(StagePacksManager.globalInstance.packs);
       
        _ingameMenu = inGameC.transform.GetComponent<InGameMenu>();
    }
        // Further we can additive load rest of the logic in next scene of put everything to main scene
	

    public void SetStagePackButtons(List<StagePack> stages)
    {
       GameObject FirstButton = packSelectionC.GetComponentInChildren<Button>().gameObject;
       
        FirstButton.GetComponent<Button>().onClick.AddListener(() => {
               SetStagesButtons(stages[0]);
               SetMenuVisiable(MenuTypes.StageSelect);
           });

       for (int i = 1; i < stages.Count; i++)
       {
           GameObject o = Instantiate(FirstButton as Object) as GameObject;
           o.transform.SetParent(FirstButton.transform.parent);
           o.transform.localScale = Vector3.one;
           Button b = o.GetComponent<Button>();
           b.GetComponentInChildren<Text>().text = "Pack "+ (i + 1).ToString();
           b.interactable = stages[i].isUnlocked;

           b.onClick.AddListener(() =>
           {
               SetStagesButtons(stages[i]);
               SetMenuVisiable(MenuTypes.StageSelect);
           });
        }

    }


    
    public void SetStagesButtons(StagePack p)
    {
        
        GameObject firstButton = stageC.GetComponentInChildren<Button>().gameObject;

        for (int i = 1; i < firstButton.transform.parent.childCount; i++ )
        {
            Destroy(firstButton.transform.parent.GetChild(i).gameObject);
        }


        (firstButton as GameObject).GetComponent<Button>().onClick.AddListener(() =>
            {
                StageLoader.globalInstance.LoadPlayStage(p.GetStageID(0));
            });

        for (int i = 1; i < p.StageCount; i++)
        {
            GameObject o = Instantiate((Object)firstButton) as GameObject;
            
            o.transform.SetParent((firstButton as GameObject).transform.parent);
            o.transform.localScale = Vector3.one;
            Button b = o.GetComponent<Button>();
            b.GetComponentInChildren<Text>().text = (i + 1).ToString();
            b.interactable = p.IsStageUnlocked(i);
            b.onClick.AddListener(() =>
            {
                StageLoader.globalInstance.LoadPlayStage(p.GetStageID(i));
            });
        }
        
    }
    
}
