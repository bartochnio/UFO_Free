using UnityEngine;
using System.Collections;

public class Navigation : MonoBehaviour {

	float OriginalOrthoSize;
	const float MinOrthoSize = 2;
	const float MaxOrthoSize = 20;

	bool MMBDown = false;


	void Update () {
		ApplyZoom ();
		ApplyPan ();
	}

	void ApplyZoom () {
		if (LevelObject.GlobalInstance.GetGUIState () != LevelObject.GUIState.EditorControls)
			return;

		Camera cam = Camera.main;

		float mouseWheel = Input.GetAxis ("Mouse ScrollWheel");
		cam.orthographicSize -= mouseWheel;

		if (cam.orthographicSize < MinOrthoSize)
			cam.orthographicSize = MinOrthoSize;

		if (cam.orthographicSize > MaxOrthoSize)
			cam.orthographicSize = MaxOrthoSize;
	}

	void ApplyPan () {
		if (LevelObject.GlobalInstance.GetGUIState () != LevelObject.GUIState.EditorControls)
			return;

		if (MMBDown && !Input.GetMouseButton (1)) {
			MMBDown = false;
			PositionManip.GlobalInstance.EndManipulation ();
		}
		if (Input.GetMouseButtonDown (1)) {
			MMBDown = PositionManip.GlobalInstance.BeginManipulation (Camera.main.gameObject, movingCamera:true);
		}
	}
}
