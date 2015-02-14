using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Score : MonoBehaviour {

    private Text guiText;
    private int score = 0;

    void Awake()
    {
        guiText = gameObject.GetComponent<Text>();
    }

	// Use this for initialization
	void Start () {
        guiText.text = score.ToString();
	}

   

    public void ChangeScore(int amount)
    {
        score += amount;
        guiText.text = score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
        guiText.text = score.ToString();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
