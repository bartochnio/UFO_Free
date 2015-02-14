using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public float speed = 10.0f;
    public float maxSpeed = 10.0f;
    public BezierSpline track;
    public float timer = 2.0f;

    private float counter = 0.0f;
    private bool isOutside = false;
    private GameObject arrow;
    private SpriteRenderer arrowSprite;
    private Vector2 velocity = Vector2.zero;
    private GameObject item = null; //TODO: Add a list of all collided pickups

    public void Respawn()
    {
        transform.position = track.GetPoint(0.0f);
        counter = timer;
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
        CollectItem();

        counter -= Time.deltaTime;
        if (counter <= 0.0f)
            Scene.GlobalInstance.FinishStage(0.0f);

	}

    void ShowArrow()
    {
        arrow.SetActive(isOutside);
        if (!isOutside)
        {
            arrow.transform.rotation = Quaternion.identity;
            return;
        }

        Vector3 closestPoint = track.GetClosestPoint(transform.position);
        Vector2 dir = (closestPoint - transform.position).normalized;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);

        arrow.transform.localPosition = Vector3.zero;
        Quaternion q = Quaternion.Euler(Vector3.forward * (-90.0f)) * Quaternion.Euler(Vector3.forward * angle);
        arrow.transform.rotation = Quaternion.Slerp(arrow.transform.rotation, q, Time.deltaTime * 10.0f);
        arrow.transform.localPosition = arrow.transform.TransformDirection(Vector3.up * 0.12f);

        arrowSprite.color = (Mathf.Sin(Time.time * 10.0f) * 0.5f + 0.5f) * Color.red;
    }

    void CollectItem()
    {
        if (item != null && Input.GetButtonDown("Jump"))
        {
            Collectable.CollectType colT = item.GetComponent<Collectable>().collectType;

            Destroy(item);
            Scene.GlobalInstance.ScoreCollectible(colT);
            item = null;
        }
    }

    void Move()
    {
        Vector2 axis = Vector3.zero;
        axis.x = Input.GetAxis("Horizontal");
        axis.y = Input.GetAxis("Vertical");

        if (axis.sqrMagnitude > 0.00001f)
        {
            axis = axis.normalized * maxSpeed;
        }

        velocity.x = Mathf.Lerp(velocity.x, axis.x, speed * Time.deltaTime);
        velocity.y = Mathf.Lerp(velocity.y, axis.y, speed * Time.deltaTime);
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    void OnTriggerStay2D(Collider2D other) 
    {
        if (other.tag == "Collectable")
        {
            item = other.gameObject;
        }
        else
        {
            counter = timer;
            isOutside = false;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Track")
        {
            isOutside = true;
        }
        else if(other.tag == "Collectable")
        {
            item = null;
        }
    }
}
