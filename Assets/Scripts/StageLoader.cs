using UnityEngine;
using System.Collections;

public class StageLoader : MonoBehaviour {

    static StageLoader instance;
    public static StageLoader globalInstance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<StageLoader>();
            return instance;
        }
    }

    MenuMgr menuMgr;

    IEnumerator Start()
    {
        menuMgr = MenuMgr.GlobalInstance;
        Object.DontDestroyOnLoad(gameObject as Object);

        while (Application.isLoadingLevel)
        {
            yield return new WaitForEndOfFrame();
        }
        LoadMainMenu();
    }

    bool isLoading;

    public bool IsLoading
    {
        get { return isLoading; }
        private set { isLoading = value; }
    }

    public void LoadPlayStage(int stageID)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadPlayStageCorutine(stageID));
        }
    }

   

    public void LoadMainMenu()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadMainMenuCoroutine());
        }
    }

    IEnumerator LoadMainMenuCoroutine()
    {
        isLoading = true;
        menuMgr.SetMenuVisiable(MenuMgr.MenuTypes.LoadScreen);

        // It's a backgorund!
        Application.LoadLevel(1);
        while (Application.isLoadingLevel)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.4f);
        menuMgr.SetMenuVisiable(MenuMgr.MenuTypes.MainMenu);
        isLoading = false;
    }

    IEnumerator LoadPlayStageCorutine(int stageId)
    {
        isLoading = true;
        menuMgr.SetMenuVisiable(MenuMgr.MenuTypes.LoadScreen);
        Application.LoadLevel(stageId);
        while (Application.isLoadingLevel)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.33f);


        menuMgr.SetMenuVisiable(MenuMgr.MenuTypes.InGame);
        isLoading = false;
    }
	
}
