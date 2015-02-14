using UnityEngine;
using System.Collections;

public class Collectable : MonoBehaviour {

    public enum CollectType
    {
        Good,
        Bad
    }

    public CollectType collectType;

    public int Points
    {
        get { return points; }
        set { points = value; }
    }

    private int points;
    private SpriteRenderer sprite;

	void Start () 
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();

        if (collectType == CollectType.Good)
            sprite.color = Color.green;
        else
            sprite.color = Color.red;
	}
	
	void Update () 
    {
	
	}
}
