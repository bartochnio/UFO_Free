using UnityEngine;
using System.Collections;

public class CollectableSpawner : MonoBehaviour {

    public BezierSpline track;
    public int amount;
    public float diff;
    public float radiusBias = 0.1f;
    public GameObject collectable;
    public int points;
    private Collectable[] collectables;

    void Awake() { Spawn(); }

	void Start ()   {
        
	}

    public void Spawn()
    {
        collectables = new Collectable[amount];
        for (int i = 0; i < amount; ++i)
        {
            float t = i / (float)amount;
            Vector2 side = Vector3.Cross(track.GetVelocity(t).normalized, -Vector3.forward).normalized;
            side *= Random.Range(-diff, diff);

            Vector2 newPos = track.GetPoint(t) + side;

            bool collided = false;
            foreach (Collectable other in collectables)
            {
                if (other == null)
                    continue;

                float d = 2.0f * other.GetColliderRadius() + radiusBias;
                if (Vector2.Distance(newPos, other.GetPos()) < d)
                {
                    collided = true;
                    break;
                }
            }

            if (!collided)
            {
                GameObject go = Instantiate(collectable) as GameObject;
                Collectable c = go.GetComponent<Collectable>();

                collectables[i] = c;
                c.Spline = track;
                c.startT = t;
                c.collectType = (Collectable.CollectType)(Random.Range(0, 2));
                go.transform.position = newPos;
            }
        }
    }
	
	
}
