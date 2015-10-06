using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    public float speed = 10.0f;
    public float maxSpeed = 10.0f;
    public BezierSpline track;
    public float timer = 2.0f;
	public float maxSwayOffRoad = 2.0f;
    public Transform playerCollision;

    //(Kamil): I know it's ugly as hell - will refactor ;)
    private float dirCounter = 0.0f;
    private bool paused = false;
    
    private bool isOutside = false;
    private GameObject arrow;
    private SpriteRenderer arrowSprite;
    private Vector2 velocity = Vector2.zero;
    private GameObject curItem = null;
    private List<GameObject> items = new List<GameObject>();

    private HashSet<int> visitedCurves = new HashSet<int>();
    private HashSet<int> collidingCurves = new HashSet<int>();
    private Vector3 closestPoint;
    private float curT = 0.0f;

    int curveIndex = 0;
    public int GetIndex() { return curveIndex; }

    public float GetT() { return ((float)curveIndex + curT) / (float)track.CurveCount; }

    public void Respawn()
    {
        Vector3 pos = track.GetCurvePoint(curveIndex, 0.5f);
        pos.z = -5.0f;
        transform.position = pos;
        curveIndex = 0;

        visitedCurves.Clear();
        collidingCurves.Clear();

        float t = 0.0f;
        closestPoint = track.GetClosestPointToACurve(curveIndex, transform.position, ref t);
        collidingCurves.Add(curveIndex);

        paused = false;
    }

    void OnEnable()
    {
        Messenger.AddListener(UFOEvents.GameOver, OnGameOver);
    }

    void OnDisable()
    {
        Messenger.RemoveListener(UFOEvents.GameOver, OnGameOver);
    }

	void Start () 
    {
        Respawn();
        arrow = transform.FindChild("Arrow").gameObject; //UNSAFE
        arrowSprite = arrow.GetComponent<SpriteRenderer>();
	}

    Vector3 computeClosestPoint(ref int index)
    {
        float t = 0.0f;
        Vector3 p = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 cp;
        foreach (int i in collidingCurves)
        {
            cp = track.GetClosestPointToACurve(i, transform.position, ref t);
            if (Vector3.Distance(cp, transform.position) <= Vector3.Distance(p, transform.position))
            {
                p = cp;
                index = i;
                curT = t;
            }
        }

        return p;
    }

	void Update () 
    {
        closestPoint = computeClosestPoint(ref curveIndex);
        playerCollision.position = closestPoint;
        
        Move();

        if (paused)
            return;

        UpdateArrow();

		if (Input.GetButtonDown("Jump")) 
        {
        	CollectItem();
		}

        if (!isOutside)
        {
            CheckDirection();
        }
	}

    void UpdateArrow()
    {
        float s = 10.0f;
        Vector2 dir = ((Vector2)closestPoint - (Vector2)transform.position).normalized;
        arrow.transform.up = Vector3.Slerp(arrow.transform.up, dir, Time.deltaTime * s);
        arrow.transform.position = Vector3.Lerp(arrow.transform.position, dir + (Vector2)transform.position, Time.deltaTime * s);
        arrow.SetActive(isOutside);

        if (!isOutside)
        {
            return;
        }

        arrowSprite.color = (Mathf.Sin(Time.time * 10.0f) * 0.5f + 0.5f) * Color.red;
    }

    public void CollectItem()
    {
        if (curItem != null)
        {
            Collectable.CollectType colT = curItem.GetComponent<Collectable>().collectType;

            curItem.SendMessage("SetBeam", transform);

            if (colT == Collectable.CollectType.Bad)
                Messenger.Invoke(UFOEvents.PlayerFail);
            else
                Messenger.Invoke(UFOEvents.PlayerScore);

            items.Remove(curItem);
            SetNextItem();
        }
    }

    void CheckDirection()
    {
        Vector2 curveTangent = track.GetCurveVelocity(curveIndex, curT).normalized;
        if (velocity.sqrMagnitude > 0.5f)
        {
            float p = Vector2.Dot(velocity.normalized, curveTangent);
            if (p < -0.2f)
                dirCounter += Time.deltaTime;
            else
                dirCounter = 0.0f;
        }
        else
            dirCounter = 0.0f;

        //display wrong direction sign
        if (dirCounter > 1.0f && !isOutside)
        {
            Messenger<bool>.Invoke(UFOEvents.PlayerWrongWay, true);
        }
        else
        {
            Messenger<bool>.Invoke(UFOEvents.PlayerWrongWay, false);
        }
    }

    void Move()
    {
        Vector2 axis = Vector3.zero;
		axis.x = VirtualPad.globalInstance.Horizontal; //Input.GetAxis("Horizontal");
		axis.y = VirtualPad.globalInstance.Vertical; //Input.GetAxis("Vertical");

        if (axis.sqrMagnitude > 0.00001f)
        {
           axis = Vector3.ClampMagnitude(axis,1.0f) * maxSpeed;
        }
		//axis *= maxSpeed;

        velocity.x = Mathf.Lerp(velocity.x, axis.x, speed * Time.deltaTime);
        velocity.y = Mathf.Lerp(velocity.y, axis.y, speed * Time.deltaTime);
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        transform.position += (Vector3)velocity * Time.deltaTime;

		// limit swaying off the road
        if (isOutside)
        {
            Vector2 D = (Vector2)closestPoint - (Vector2)transform.position;

            float magSqr = D.sqrMagnitude;
            if (magSqr > maxSwayOffRoad * maxSwayOffRoad)
            {
                float mag = Mathf.Sqrt(magSqr);
                float magInv = 1.0f / mag;
                Vector2 dir = D * magInv;

                transform.position += (Vector3)dir * (mag - maxSwayOffRoad);
            }
        }
    }

    void SetNextItem()
    {
        if (items.Count > 0)
        {
            curItem = items[items.Count - 1];
            if (curItem != null)
                curItem.SendMessage("SetFlash");
        }

        else
            curItem = null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Collectable")
        {
            if (curItem != null)
                curItem.SendMessage("SetIdle");

            curItem = other.gameObject;
            curItem.SendMessage("SetFlash");
            items.Add(curItem);
        }
        else if (other.tag == "Track")
        {
            int index = int.Parse(other.gameObject.name);
            if (index == curveIndex && isOutside)
            {
                Messenger<bool>.Invoke(UFOEvents.PlayerOutside, false);
                isOutside = false;
            }

            if (indexIsValid(index))
            {
                visitedCurves.Add(index);
            }
            //if (TrackIndexChanged != null) TrackIndexChanged(curveIndex, nextIndex, PrevIndex);
        }
        else if (other.tag == "Finish" && visitedCurves.Count == track.CurveCount)
        {
            //The player traversed the whole track - we can finish now
            Messenger.Invoke(UFOEvents.PlayerFinished);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Track")
        {
            int index = int.Parse(other.gameObject.name);

            if (index == curveIndex)
            {
                isOutside = true;
                Messenger<bool>.Invoke(UFOEvents.PlayerOutside, true);
            }
        }
        else if(other.tag == "Collectable")
        {
            if (curItem == other.gameObject)
            {
                curItem.SendMessage("SetIdle");
                items.Remove(curItem);
                SetNextItem();
            }
            else
                items.Remove(other.gameObject);
        }
    }

    void OnPlayerCollisionEnter(Collider2D other)
    {
        if (other.tag == "Track")
        {
            int index = int.Parse(other.gameObject.name);

            if (indexIsValid(index))
            {
                collidingCurves.Add(index);
                //track.SendMessage("EnableCurve", index);
            }
            //else
                //track.SendMessage("DisableCurve", index);
        }
    }

    void OnPlayerCollisionExit(Collider2D other)
    {
        if (other.tag == "Track")
        {
            int index = int.Parse(other.gameObject.name);

            if (indexIsValid(index))
            {
                collidingCurves.Remove(index);
            }
            else
                track.SendMessage("EnableCurve", index);
        }
    }

    bool indexIsValid(int index)
    {
        if (curveIndex == 0 && (index == track.CurveCount - 1))
            return true;
        else if ((curveIndex == track.CurveCount - 1) && index == 0)
            return true;
        else if (Mathf.Abs(index - curveIndex) <= 1)
            return true;

        return false;
    }

    //Event handlers
    private void OnGameOver()
    {
        paused = true;
        arrow.SetActive(false);
    }
}
