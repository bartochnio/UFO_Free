using UnityEngine;
using System.Collections;

public class CollectableSpawner : MonoBehaviour {

    public BezierSpline track;
    public PlayerController player;
    public int amount;
    public float diff;
    public float radiusBias = 0.1f;
    public GameObject collectable;
    public int points;
    private Collectable[] collectables;

	private DevGuiSystem.Helper devControls = new DevGuiSystem.Helper (); // DEV PANEL

    
	void Awake() {
		CreateDevControls ();
		Spawn();
	}

	// DEV PANEL
	void CreateDevControls () {
		devControls.ReleaseControls ();
		devControls.HoldOnTo (DevGuiSystem.globalInstance.AddLabel ("Collectables setup:"));
		{
			var amountSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Amount", wholeNumbers:true));
			amountSlider.MinFloat = 1;
			amountSlider.MaxFloat = 200;
			amountSlider.OnChanged += () => {
				amount = (int)amountSlider.Float;
				PlayerPrefs.SetFloat ("CollectablesSetup-Amount", amountSlider.Float);
			};
			amountSlider.Float = PlayerPrefs.GetFloat ("CollectablesSetup-Amount", 40);

			var spreadSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Spread"));
			spreadSlider.MinFloat = 0.1f;
			spreadSlider.MaxFloat = 1.0f;
			spreadSlider.OnChanged += () => {
				diff = amountSlider.Float;
				PlayerPrefs.SetFloat ("CollectablesSetup-Spread", spreadSlider.Float);
			};
			spreadSlider.Float = PlayerPrefs.GetFloat ("CollectablesSetup-Spread", 0.5f);

			var radiusBiasSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Radius bias"));
			radiusBiasSlider.MinFloat = 0.01f;
			radiusBiasSlider.MaxFloat = 1.0f;
			radiusBiasSlider.OnChanged += () => {
				radiusBias = radiusBiasSlider.Float;
				PlayerPrefs.SetFloat ("CollectablesSetup-RadiusBias", radiusBiasSlider.Float);
			};
			radiusBiasSlider.Float = PlayerPrefs.GetFloat ("CollectablesSetup-RadiusBias", 0.1f);

			var pointsSlider = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddSlider ("Points", wholeNumbers:true));
			pointsSlider.MinFloat = 1;
			pointsSlider.MaxFloat = 50;
			pointsSlider.OnChanged += () => {
				points = (int)pointsSlider.Float;
				PlayerPrefs.SetFloat ("CollectablesSetup-Points", pointsSlider.Float);
			};
			pointsSlider.Float = PlayerPrefs.GetFloat ("CollectablesSetup-Points", 10);


			var resetButton = devControls.HoldOnTo (DevGuiSystem.globalInstance.AddButton ("Reset", "Reset collectibles setup values"));
			resetButton.OnChanged += () => {
				amountSlider.Float = 40;
				spreadSlider.Float = 0.5f;
				radiusBiasSlider.Float = 0.1f;
				pointsSlider.Float = 10;
			};
		}

		devControls.HoldOnTo (DevGuiSystem.globalInstance.AddEmpty ());
	}
	//

	void OnDestroy () {
		devControls.ReleaseControls ();
	}


	void Start ()   {
        
	}


    public void Spawn()
    {
        collectables = new Collectable[amount];
        for (int i = 0; i < amount; ++i)
        {
            float t = i / (float)amount;
            Vector3 side = Vector3.Cross(track.GetVelocity(t).normalized, -Vector3.forward).normalized;
            side *= Random.Range(-diff, diff);

            Vector3 newPos = track.GetPoint(t) + side;

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
                c.Player = player;
                c.startT = t;
                c.collectType = (Collectable.CollectType)(Random.Range(0, 2));
                go.transform.position = newPos;
            }
        }
    }
	
	
}
