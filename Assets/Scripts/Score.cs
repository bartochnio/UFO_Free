using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Score : MonoBehaviour {

    private Text guiText;
    private int score = 0;
    private Vector3 baseScale;
    private Color scoreColor;

    void Awake()
    {
        guiText = gameObject.GetComponent<Text>();
    }

	// Use this for initialization
	void Start () {
        guiText.text = score.ToString();
        baseScale = transform.localScale;
	}

    public void ChangeScore(int amount)
    {
        transform.localScale = baseScale;
        StopAllCoroutines();
        scoreColor = amount > 0 ? Color.green : Color.red;    
        
        StartCoroutine("ChangeEffect");

        score += amount;
        guiText.text = score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
        guiText.text = score.ToString();
        transform.localScale = baseScale;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator ChangeEffect()
    {
        for(float f = 0.0f; f < 1.0f; f+=0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x += 0.04f;
            scale.y += 0.04f;
            transform.localScale = scale;

            guiText.color = Color.Lerp(Color.white, scoreColor, f);
            yield return null;
        }

        for (float f = 1.0f; f >= 0.0f; f -= 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x -= 0.04f;
            scale.y -= 0.04f;
            transform.localScale = scale;

            guiText.color = Color.Lerp(Color.white, scoreColor, f);
            yield return null;
        }
    }
}
