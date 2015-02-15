using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScorringScreen : MonoBehaviour
{

    public Text goodColText;
    public Text badColText;
    public Text timeLeftText;
    public Text generalScoreText;

    public float scoreCountSpeed = 0.25f;

    bool scorring = false;

    public IEnumerator ScoringRoutine(int good, int bad, float tLeft, int goodScore, int badScore, int timeScore)
    {
        scorring = true;

        goodColText.text = "0";
        badColText.text = "0";
        timeLeftText.text = "0";


        int goodS = good * goodScore;
        int badS = bad * badScore;
        int tScore = (int)(tLeft * timeScore);
        int gScore = goodS + badS + tScore;

        int aS = 0;
        int gG = 0;
        for (int i = 0; i < good; i++)
        {

            gG += 10;

            aS += 10;
            generalScoreText.text = aS.ToString();
            generalScoreText.color = Color.green;
            goodColText.text = gG.ToString();
            yield return new WaitForSeconds(scoreCountSpeed);
        }

        int bG = 0;
        for (int i = 0; i < bad; i++)
        {

            bG -= 10;
            aS -= 10;
            generalScoreText.text = aS.ToString();
            generalScoreText.color = Color.red;
            badColText.text = bG.ToString();
            yield return new WaitForSeconds(scoreCountSpeed);
        }

        int tG = 0;
        for (int i = 0; i < (int)tLeft; i++)
        {

            aS += 10;
            generalScoreText.text = aS.ToString();
            generalScoreText.color = Color.green;
            tG += 10;
            timeLeftText.text = tG.ToString();
            yield return new WaitForSeconds(scoreCountSpeed);
        }

        if (gScore > 0)
        {
            generalScoreText.color = Color.green;
        }
        else
        {
            generalScoreText.color = Color.red;
        }


        scorring = false;
    }

    

    // Use this for initialization
    void Start()
    {
        Object.DontDestroyOnLoad(this);
        scorring = false;
        
    }

  
}
