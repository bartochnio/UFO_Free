using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public float speed = 10.0f;
    public Transform target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 cur = transform.position;
        Vector3 trg = target.position;
        cur.z = trg.z = -10.0f;
        transform.position = Vector3.MoveTowards(cur, trg, Time.deltaTime * Vector3.Distance(cur, trg) * speed);
	}
}
