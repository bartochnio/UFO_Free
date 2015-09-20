using UnityEngine;
using System.Collections;

public class PositionManip : MonoBehaviour {
	private static PositionManip globalInstance;
	public static PositionManip GlobalInstance { get { return globalInstance; } }

	public GameObject ManipulatedObject;

	Vector3 Offset;
	Matrix4x4 CameraToWorldMatrix;
	bool MovingCamera;

	void Awake () {
		globalInstance = this;
	}

	void Update () {
		if (ManipulatedObject != null) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			RaycastHit hitInfo;
			if (GetComponent <MeshCollider> ().Raycast (ray, out hitInfo, 1000)) {
				Vector3 hitPt = Camera.main.worldToCameraMatrix.MultiplyPoint (hitInfo.point);
				Vector3 objPos = hitPt + Offset;

				if (MovingCamera) {
					objPos *= -1;
				}

				ManipulatedObject.transform.position = CameraToWorldMatrix.MultiplyPoint (objPos);

				if (MovingCamera) {
					Vector3 pos = ManipulatedObject.transform.position;

					float H = Camera.main.orthographicSize;
					float W = H * (float)Screen.width / (float)Screen.height;

					Bounds bounds = GetComponent <MeshRenderer> ().bounds;

					if (pos.x < (bounds.min.x + W)) {
						pos.x = bounds.min.x + W;
					}
					if (pos.x > (bounds.max.x - W)) {
						pos.x = bounds.max.x - W;
					}

					if (pos.y < (bounds.min.y + H)) {
						pos.y = bounds.min.y + H;
					}
					if (pos.y > (bounds.max.y - H)) {
						pos.y = bounds.max.y - H;
					}

					ManipulatedObject.transform.position = pos;
				}
			}
		}
	}

	public bool BeginManipulation (GameObject ObjectToManipulate, bool movingCamera=false) {
		if (ManipulatedObject != null)
			return false;

		if (ObjectToManipulate != null) {
			MovingCamera = movingCamera;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			
			RaycastHit hitInfo;
			if (GetComponent <MeshCollider> ().Raycast (ray, out hitInfo, 1000)) {
				ManipulatedObject = ObjectToManipulate;
				CameraToWorldMatrix = Camera.main.cameraToWorldMatrix;

				Vector3 hitPt = Camera.main.worldToCameraMatrix.MultiplyPoint (hitInfo.point);
				Vector3 objPos = Camera.main.worldToCameraMatrix.MultiplyPoint (ManipulatedObject.transform.position);

				Offset = objPos - hitPt;
				return true;
			}
		}
		
		return false;
	}

	public void EndManipulation () {
		ManipulatedObject = null;
	}
}
