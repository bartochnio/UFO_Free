using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VirtualPad : MonoBehaviour {
	static private VirtualPad instance = null;
	static public VirtualPad globalInstance { get { return instance; } }


	public Button btnDPad;
	public float  DPadRadiusFactor = 2.0f; // controls how far DPad sprite can be moved.
	public float  DPadSenstivity = 1.0f; // senstivity of the DPad;


	bool	bManipulateDPad = false; // flag which tells whether the virtual DPad is being used or not
	float   DPadBtnRadius; // Radius of the DPad sprite
	Vector2 DPadOriginalAnchorMin; // anchorMin of the DPad UI button
	Vector2 DPadOriginalAnchorMax; // anchorMax of the DPad UI button
	Vector2 DPadBtnOriginalPosition; // original position of the DPad button in pixel coords
	Vector3 DPadManipStartPosition; // pixel coords of the touch/mouse (depending on bTouched flag) before virtual DPad is moved (just touched)
	Vector3 DPadManipCurrentPosition; // pixel coords of the touch/mouse (depending on bTouched flag) current position
	bool	bTouched; // true = virtual DPad is bein manipulated with touchscreen. false = being manipulated with mouse.
	int     DPadManipTouchID; // ID of the finger which touched the virtual DPad
	Vector2 velocity; // final output of this class


	public float Horizontal { get { return velocity.x; } }
	public float Vertical   { get { return velocity.y; } }


	void Awake () {
		VirtualPad.instance = this;

		bManipulateDPad = false;

		RectTransform tx = btnDPad.transform as RectTransform;

		DPadOriginalAnchorMin = tx.anchorMin;
		DPadOriginalAnchorMax = tx.anchorMax;

		Vector2 btnDiagonal = DPadOriginalAnchorMax - DPadOriginalAnchorMin;
		Vector2 scrSize = new Vector2 (Screen.width, Screen.height);

		DPadBtnOriginalPosition = 0.5f * (DPadOriginalAnchorMax + DPadOriginalAnchorMin);
		DPadBtnOriginalPosition.Scale (scrSize);

		btnDiagonal.Scale (scrSize);
		DPadBtnRadius = 0.5f * btnDiagonal.magnitude;
	}

    void OnDisable()
    {
        velocity = Vector3.zero;
    }

	void Update () {
		velocity.x = Input.GetAxis("Horizontal");
		velocity.y = Input.GetAxis("Vertical");
		//if (velocity.magnitude > 0.00001f) velocity.Normalize ();

		if (bManipulateDPad) { // if virtual DPad is being manipulated
			if (false == bTouched) { // with mouse ?
				DPadManipCurrentPosition = Input.mousePosition;
			}

			else { // or with touchscreen
				DPadManipCurrentPosition = GetTouchPosition (DPadManipTouchID);
			}


			float maxDisplacement = DPadRadiusFactor * DPadBtnRadius;

			Vector3 delta = DPadManipCurrentPosition - DPadManipStartPosition;
			delta = Vector3.ClampMagnitude (delta, maxDisplacement);

			velocity = delta;

			Vector3 invScrSize = new Vector3 (1.0f / Screen.width, 1.0f / Screen.height, 1.0f);
			delta.Scale (invScrSize);

			RectTransform tx = btnDPad.transform as RectTransform;
			tx.anchorMin = DPadOriginalAnchorMin + (Vector2)delta;
			tx.anchorMax = DPadOriginalAnchorMax + (Vector2)delta;

			velocity.Scale (new Vector2 (1.0f / maxDisplacement, 1.0f / maxDisplacement));
			velocity *= DPadSenstivity;
		}
	}


	public void OnDPadManipBegin () {
		if (false == bManipulateDPad) {
			bManipulateDPad = true; // virtual DPad is now being manipulated

			if (Input.touchCount == 0) {
				bTouched = false; // being manipulated with mouse

				DPadManipStartPosition = Input.mousePosition;
			}

			else {
				bTouched = true; // being manipulated with touch screen

				DPadManipTouchID = FindClosestTouchID ();
				DPadManipStartPosition = GetTouchPosition (DPadManipTouchID);
			}

			DPadManipCurrentPosition = DPadManipStartPosition;
		}
	}

	public void OnDPadManipEnd () {
		bManipulateDPad = false; // virtual DPad is now released

		RectTransform tx = btnDPad.transform as RectTransform;
		tx.anchorMin = DPadOriginalAnchorMin;
		tx.anchorMax = DPadOriginalAnchorMax;
	}


	// Try to guess the finger which touched the virtual DPad (I didnt find any direct method where the button reports this information...)
	// This finger will be tracked in the update.
	//
	int FindClosestTouchID () {
		int closestTouchID = Input.GetTouch (0).fingerId;
		float closestDistance = (Input.GetTouch (0).position - DPadBtnOriginalPosition).magnitude;

		if (Input.touchCount > 1) {
			for (int k = 1; k < Input.touchCount; ++k) {
				float distance = (Input.GetTouch (k).position - DPadBtnOriginalPosition).magnitude;
				if (distance < closestDistance) {
					closestTouchID = Input.GetTouch (k).fingerId;
					closestDistance = distance;
				}
			}
		}

		return closestTouchID;
	}

	Vector3 GetTouchPosition (int touchID) {
		Vector3 pos = Vector3.zero;

		for (int k = 0; k < Input.touchCount; ++k) {
			if (Input.GetTouch (k).fingerId == touchID) {
				Vector2 p = Input.GetTouch (k).position;
				pos = new Vector3 (p.x, p.y, 1.0f);
				break;
			}
		}

		return pos;
	}
}
