using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InGameMenu : MonoBehaviour {

    public GameObject menuPanel;
    public Button openButton;
    static StageLoader loader;

	// Use this for initialization
	void Start () {

        menuPanel.SetActive(false);
        loader = StageLoader.globalInstance;

	}

    public void ShowMenu(bool value)
    {
        openButton.gameObject.SetActive(!value);
        menuPanel.SetActive(value);
        Scene.GlobalInstance.SetPause(value);
    }

    public void Restart()
    {
        ShowMenu(false);
        loader.LoadPlayStage(Application.loadedLevel);
    }

    public void EndStage()
    {
        loader.LoadMainMenu();
    }

    void OnLevelWasLoaded()
    {
       
    }

	// Update is called once per frame
	void Update () {
	
	}
}
