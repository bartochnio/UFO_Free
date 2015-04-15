using UnityEngine;
using System.Collections;

public class Collectable : MonoBehaviour {

    public float startT = 0.0f;
    public float direction = -1.0f;

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
        return circle.radius * transform.localScale.x;
    }

    public Vector2 GetPos()
    {
        return transform.position;
    }

    public float beamTime = 2.0f;

    private enum State
    {
        idle,
        flash,
        beam
    }

    State state;

    private int points;
    private SpriteRenderer sprite;
    private CircleCollider2D circle;
    private Color baseColor;
    private Transform target;
    private BezierSpline spline;
    private PlayerController player;
    private float curT = 0.0f;

    public BezierSpline Spline
    {
        set { spline = value; }
    }

    public PlayerController Player
    {
        set { player = value; }
    }

    void Awake()
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();
        circle = gameObject.GetComponent<CircleCollider2D>();
    }

	void Start () 
    {
        if (collectType == CollectType.Good)
            baseColor = Color.green * RenderSettings.ambientLight;
        else
            baseColor = Color.red * RenderSettings.ambientLight;

        sprite.color = baseColor;
        state = State.idle;
        curT = startT;
	}
	
	void Update () 
    {
        if (state == State.idle || state == State.flash)
        {
            curT += (Time.deltaTime / 200.0f)*direction;
            if (curT >= 1.0f)
                curT = 0.0f;
            else if (curT <= 0.0f)
                curT = 1.0f;

            transform.position = spline.GetPoint(curT);

            Vector2 vel = spline.GetVelocity(curT);
            float angle = Mathf.Rad2Deg * Mathf.Atan2(vel.y, vel.x);
            Quaternion rot = Quaternion.Euler(new Vector3(0.0f,0.0f,angle));
            transform.rotation = rot;
        }
	}

    IEnumerator Beam()
    {
        for(float f = 0.0f; f < beamTime; f+=0.1f)
        {
            float t = f / beamTime;
            Color c = sprite.color;
            c.a = 1.0f - t;
            sprite.color = c;

            transform.position = Vector3.Lerp(transform.position, target.position, t);
            transform.localScale *= 1.05f;
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator Flash()
    {
        float i = 0.0f;
        while (true)
        {
            i += 0.01f;
            float t = (Mathf.Sin(i*20.0f) * 0.5f + 0.5f);

            Color c = baseColor;
            c *= new Color(5.0f, 5.0f, 1.5f);

            sprite.color = Color.Lerp(baseColor, c, t);
            yield return null;
        }
    }

    //Messages
    void SetFlash()
    {
        if (state == State.beam)
            return;

        state = State.flash;
        StartCoroutine("Flash");
    }

    void SetIdle()
    {
        if (state == State.beam)
            return;

        state = State.idle;
        sprite.color = baseColor;
        StopAllCoroutines();
    }

    void SetBeam(Transform trg)
    {
        if (state == State.beam)
            return;

        state = State.beam;
        target = trg;
        StopAllCoroutines();
        StartCoroutine("Beam");
    }
}
