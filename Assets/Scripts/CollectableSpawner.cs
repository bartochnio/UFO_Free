using UnityEngine;
using System.Collections;

public class CollectableSpawner : MonoBehaviour {

    public BezierSpline track;
    public int amount;
    public float diff;
    public GameObject collectable;
    public int points;

	void Awake () 
    {
        Spawn();
	}

    public void Spawn()
    {
        for (int i = 0; i < amount; ++i)
        {
            float t = i / (float)amount;

            Vector2 side = Vector3.Cross(track.GetVelocity(t).normalized, -Vector3.forward).normalized;
            side *= Random.Range(-diff, diff);

            GameObject go = Instantiate(collectable) as GameObject;
            Collectable c = go.GetComponent<Collectable>();
            //c.Points = (Random.Range(0, 2) * 2 - 1) * points;
            c.collectType = (Collectable.CollectType)(Random.Range(0, 2));
            go.transform.position = track.GetPoint(t) + side;
        }
    }
	
	
}
