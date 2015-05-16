﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    public float speed = 10.0f;
    public float maxSpeed = 10.0f;
    public BezierSpline track;
    public float timer = 2.0f;

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

    int curveIndex = 0;
    public int CurveIndex
    {
        get { return curveIndex; }
    }

    int nextIndex = 0;
    public int NextIndex
    {
        get { return nextIndex; }
    }

    int prevIndex = 0;
    public int PrevIndex
    {
        get { return prevIndex; }
    }

    public void Respawn()
    {
        transform.position = track.GetCurvePoint(curveIndex, 0.5f);
        curveIndex = 0;

        visitedCurves.Clear();

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

        UpdateTrackVisibility(curveIndex);
	}


	void Update () 
    {
        Move();

        if (paused)
            return;

        ShowArrow();

		if (Input.GetButtonDown("Jump")) {
        	CollectItem();
		}

        if (!isOutside)
        {
            CheckDirection();
        }
	}

    void CheckDirection()
    {
        float t = 0.0f;
        track.GetClosestPointToACurve(curveIndex, transform.position, ref t);
        Vector2 curveTangent = track.GetCurveVelocity(curveIndex, t).normalized;
        if (velocity.sqrMagnitude > 0.5f)
        {
            float p = Vector2.Dot(velocity.normalized, curveTangent);
            if (p < -0.2)
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

    void UpdateTrackVisibility(int index)
    {
        //track.SendMessage("DisableTrack");
        visitedCurves.Add(curveIndex);

        curveIndex = index;
        nextIndex = curveIndex;
        nextIndex = (curveIndex + 1) % track.CurveCount;

        prevIndex = curveIndex;
        if (curveIndex > 0)
            prevIndex = curveIndex - 1;
        else
            prevIndex = track.CurveCount - 1;

        track.SendMessage("EnableCurve", prevIndex);
        track.SendMessage("EnableCurve", curveIndex);
        track.SendMessage("EnableCurve", nextIndex);
    }

    void ShowArrow()
    {
        arrow.SetActive(isOutside);
        if (!isOutside)
        {
            arrow.transform.rotation = Quaternion.identity;
            return;
        }

        float t = 0.0f;
        Vector2 closestPoint = track.GetClosestPointToACurve(curveIndex, transform.position, ref t);
        Vector2 dir = (closestPoint - (Vector2)transform.position).normalized;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);

        arrow.transform.localPosition = Vector3.zero;
        Quaternion q = Quaternion.Euler(Vector3.forward * (-90.0f)) * Quaternion.Euler(Vector3.forward * angle);
        arrow.transform.rotation = Quaternion.Slerp(arrow.transform.rotation, q, Time.deltaTime * 10.0f);
        arrow.transform.localPosition = arrow.transform.TransformDirection(Vector3.up * 0.12f);

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
    }

    void SetNextItem()
    {
        if (items.Count > 0)
        {
            curItem = items[items.Count - 1];
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

            if ((Mathf.Abs(index - curveIndex) == 1))
                UpdateTrackVisibility(index);
            else if (curveIndex == 0 && (index == track.CurveCount - 1))
                UpdateTrackVisibility(index);
            else if ((curveIndex == track.CurveCount - 1) && index == 0)
                UpdateTrackVisibility(index);
            
            else if (index != curveIndex)
                track.SendMessage("DisableCurve", index);

            if (index == curveIndex)
            {
                Messenger<bool>.Invoke(UFOEvents.PlayerOutside, false);
                isOutside = false;
            }

            if (TrackIndexChanged != null) TrackIndexChanged(curveIndex, nextIndex, PrevIndex); 
        }
        else if (other.tag == "Finish" && visitedCurves.Count == track.CurveCount)
        {
           //The player traversed the whole track - we can finish now
           //Scene.GlobalInstance.FinishStage(0);
            Messenger.Invoke(UFOEvents.GameOver);
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

            track.SendMessage("EnableCurve", index);
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

    public delegate void TrackPartChangeDelegate(int currentIdx,int preIdx, int nextIdx);
    public event TrackPartChangeDelegate TrackIndexChanged;

    //Event handlers
    private void OnGameOver()
    {
        paused = true;
        arrow.SetActive(false);
    }
}
