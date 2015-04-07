using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    public float speed = 10.0f;
    public float maxSpeed = 10.0f;
    public BezierSpline track;
    public float timer = 2.0f;

    //private float counter = 0.0f;
    private bool isOutside = false;
    private GameObject arrow;
    private SpriteRenderer arrowSprite;
    private Vector2 velocity = Vector2.zero;
    private GameObject curItem = null;
    private List<GameObject> items = new List<GameObject>();

    int curveIndex = 0;

    public void Respawn()
    {
        transform.position = track.GetPoint(0.0f);
    }

	void Start () 
    {
        Respawn();
        arrow = transform.FindChild("Arrow").gameObject; //UNSAFE
        arrowSprite = arrow.GetComponent<SpriteRenderer>();
	}
	
	void Update () 
    {
        Move();
        ShowArrow();

		if (Input.GetButtonDown("Jump")) {
        	CollectItem();
		}

        float t = 0.0f;
        Vector2 closestPoint = track.GetClosestPointToACurve(curveIndex, transform.position, ref t);
        Vector2 end = track.GetCurvePoint(curveIndex, 1.0f);
        Vector2 start = track.GetCurvePoint(curveIndex, 0.0f);

       // Vector2 curveTangent = track.GetCurveVelocity(curveIndex, t).normalized;

        if (Vector2.Distance(end, closestPoint) < 0.6f)
        {
            curveIndex = (curveIndex + 1) % track.CurveCount;
        }
        else if (Vector2.Distance(start, closestPoint) < 0.6f)
        {
            if (curveIndex == 0)
                curveIndex = track.CurveCount - 1;
            else
                curveIndex = curveIndex - 1;
        }


       //Direction check
       // if (velocity.sqrMagnitude > 0.0001f)
       // {
       //     float p = Vector2.Dot(velocity.normalized, curveTangent);

       //     if (p < -0.2)
       //     {
       //         Debug.DrawLine(transform.position, (Vector2)transform.position + velocity.normalized, Color.blue);
       //         Debug.DrawLine(transform.position, (Vector2)transform.position + curveTangent, Color.cyan);
       //     }
       // }

       //Debug.DrawLine(transform.position, closestPoint, Color.red);
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
            Scene.GlobalInstance.ScoreCollectible(colT);

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
            axis = axis.normalized * maxSpeed;
        }

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
    }
    
    void OnTriggerStay2D(Collider2D other) 
    {
        if (other.tag == "Track")
            
        {
            Scene.GlobalInstance.SetOutsideTheTrack(false);
            isOutside = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Track")
        {
            Scene.GlobalInstance.SetOutsideTheTrack(true);
            isOutside = true;
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
}
