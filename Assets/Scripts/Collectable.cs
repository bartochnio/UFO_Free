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

    public float GetColliderRadius()
    {
        return collider.radius * transform.localScale.x;
    }

    public Vector2 GetPos()
    {
        return transform.position;
    }

    private int points;
    private SpriteRenderer sprite;
    private CircleCollider2D collider;

    void Awake()
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();
        collider = gameObject.GetComponent<CircleCollider2D>();
    }

	void Start () 
    {
        if (collectType == CollectType.Good)
            sprite.color = Color.green;
        else
            sprite.color = Color.red;
	}
	
	void Update () 
    {
	
	}
}
