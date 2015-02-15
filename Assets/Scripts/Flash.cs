using UnityEngine;
using System.Collections;

public class Flash : MonoBehaviour {


    public float interval;
    public GameObject target;

    IEnumerator FlashRoutine()
    {
        while (true)
        {
            target.SetActive(!target.activeSelf);
            yield return new WaitForSeconds(interval);
        }
    }


    void OnEnable()
    {
        StartCoroutine("FlashRoutine");
    }

    void OnDisable()
    {
        StopCoroutine("FlashRoutine");
        target.SetActive(true);
    }

	// Use this for initialization
	void Start () {
        if (target == null || interval <= 0)
        {
            Debug.LogError(name + ": Improper script settings, check for nulls");
        }


        Object.DontDestroyOnLoad(gameObject);   
	}
	
	
}
