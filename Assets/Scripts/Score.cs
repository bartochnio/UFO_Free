using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Score : MonoBehaviour {

    private Text guiText;
    private Vector3 baseScale;
    private Color scoreColor;

    void Awake()
    {
        guiText = gameObject.GetComponent<Text>();
        baseScale = Vector3.one;
    }

	// Use this for initialization
	void Start () {
        ResetScore();
	}

    public void SetBadScore(int amount)
    {
        transform.localScale = baseScale;
        StopAllCoroutines();
        scoreColor = Color.red;

        StartCoroutine("ChangeEffect");

        guiText.text = amount.ToString();
    }

    public void SetGoodScore(int amount)
    {
        transform.localScale = baseScale;
        StopAllCoroutines();
        scoreColor = Color.green;

        StartCoroutine("ChangeEffect");

        guiText.text = amount.ToString();
    }

    public void ResetScore()
    {
        guiText.text = "0";
        transform.localScale = baseScale;
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
