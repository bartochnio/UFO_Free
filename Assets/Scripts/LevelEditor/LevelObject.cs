using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelObject : MonoBehaviour {
	static private LevelObject globalInstance;
	static public  LevelObject GlobalInstance { get { return globalInstance; } }

	public Material		RoadMaterial;
	public GameObject	ControlPointManipPrefab;

	public LineRenderer	Hull;
	public LineRenderer	SelectedCurveHull;

	public UnityEngine.UI.Toggle ToggleUI;

	public Material		GridMaterial;
	public MeshRenderer	GridMeshRenderer;


	class Section {
		public GameObject			geometry;
		public ControlPointManip[]	manipulators;
	}
	List<Section>		Sections = new List<Section> ();

	ControlPointManip	LastControlPointManip;

	MeshBuilder			Builder = new MeshBuilder();
	
	int					SelectedCurve = 0;

	Bounds				GridBounds;
	bool				SnapToGrid;


	void Awake () {
		globalInstance = this;
	}

	void Start () {
		UndoRedoSys.Clear ();
		LastControlPointManip = CreateControlPointManip (transform, Vector3.zero, 0);
		Initialize ();

		GridBounds = GridMeshRenderer.bounds;
		GridMaterial.mainTextureScale = GridBounds.max;
	}

	void Update () {
		if (Input.GetKeyUp (KeyCode.Z)) {
			if (Input.GetKey (KeyCode.LeftCommand) && Input.GetKey (KeyCode.LeftShift)) {

				UndoRedoSys.Redo ();
			}
			else if (Input.GetKey (KeyCode.LeftCommand)) {

				UndoRedoSys.Undo ();
			}
		}
	}

	void OnGUI () {
		GUI.contentColor = Color.black;
		{
			Rect r = new Rect (Screen.width - 100, 0, 100, 30);

			if (GUI.Button (r, "Undo count: " + UndoRedoSys.GetUndoCommandCount ())) {
				UndoRedoSys.Undo ();
			}
			r.y += 35;

			if (GUI.Button (r, "Redo count: " + UndoRedoSys.GetRedoCommandCount ())) {
				UndoRedoSys.Redo ();
			}
			r.y += 35;
		}

		GUI.Box (new Rect (0, Screen.height - 25, 100, Screen.height), "Curve " + (SelectedCurve + 1) + " of " + Sections.Count);

		SnapToGrid = GUI.Toggle (new Rect (Screen.width - 50, Screen.height - 25, 50, 25), SnapToGrid, "Snap");
	}


	void Initialize () {
		AddSection (0);
		RebuildSection (0);
		
		SelectedCurve = 0;
		UpdateHull ();
	}

	public void AppendCurve () { // used by Append UI button
		AppendCurve2 (true);
	}

	public void AppendCurve2 (bool pushUndo = true) { // HACK: used by UndoRedo command AppendCurveCommand
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;
		
		int lastControlPoint = spline.ControlPointCount - 1;
		Vector3 lastPoint = spline.GetControlPoint (lastControlPoint);
		Vector3 secondLastPoint = spline.GetControlPoint (lastControlPoint - 1);
		
		spline.AddCurve ();
		
		AddSection (lastControlPoint);
		
		RebuildSection (Sections.Count - 1);
		RebuildSection (Sections.Count - 2);
		if (spline.Loop)
			RebuildSection (0);
		
		SelectedCurve = Sections.Count - 1;
		UpdateHull ();
		
		
		if (pushUndo)
			UndoRedoSys.PushCommand (new AppendCurveCommand (lastControlPoint, lastPoint, secondLastPoint));
	}

	public void DeleteLastCurve (bool pushUndo = true) {
		DeleteCurveAt (Sections.Count - 1, pushUndo);
	}

	public void InsertCurveAtSelectedCurve () {
		InsertCurveAt (SelectedCurve);
	}

	public void InsertCurveAt (int CurveIndex, bool pushUndo = true) {
		if (CurveIndex < 0 || CurveIndex >= Sections.Count)
			return;

		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;
		
		Section section = Sections [CurveIndex];
		
		int startCtrlPt = section.manipulators [0].ControlPointIndex;
		Vector3 point = spline.GetControlPoint (startCtrlPt);

		Vector3[] prevPoints = new Vector3 [4] {
			point,
			spline.GetControlPoint (startCtrlPt + 1),
			spline.GetControlPoint (startCtrlPt + 2),
			spline.GetControlPoint (startCtrlPt + 3)
		};
		
		
		Vector3[] points = new Vector3 [3] {
			point, point, point
		};
		points [1].x += 4.0f;
		points [2].x += 8.0f;
		
		point.x += 12.0f;
		spline.SetControlPoint (startCtrlPt, point);
		
		spline.InsertPointsAt (CurveIndex * 3, points);
		
		spline.SetControlPoint (startCtrlPt + 1, spline.GetControlPoint (startCtrlPt + 1));
		spline.SetControlPoint (startCtrlPt + 2, spline.GetControlPoint (startCtrlPt + 2));
		
		if (spline.Loop) {
			spline.Loop = false;
			spline.Loop = true;
		}
		
		
		AddSection (startCtrlPt, CurveIndex);
		
		for (int k = CurveIndex + 1; k < Sections.Count; ++k) {
			Section aSection = Sections [k];
			
			for (int j = 0; j < aSection.manipulators.Length; ++j) {
				aSection.manipulators [j].ControlPointIndex += 3;
			}
		}
		
		
		RebuildSection (CurveIndex);
		
		if (SelectedCurve == Sections.Count - 1)
			RebuildSection (0);
		else
			RebuildSection (CurveIndex + 1);
		
		if (SelectedCurve == 0)
			RebuildSection (Sections.Count - 1);
		else
			RebuildSection (CurveIndex - 1);
		
		UpdateHull ();


		if (pushUndo)
			UndoRedoSys.PushCommand (new InsertCurveCommand (CurveIndex, prevPoints));
	}

	public void DeleteSelectedCurve () {
		DeleteCurveAt (SelectedCurve);
	}

	public void DeleteCurveAt (int CurveIndex, bool pushUndo = true) {
		if (Sections.Count == 1)
			return;
		if (CurveIndex < 0 || CurveIndex >= Sections.Count)
			return;
		/*if (CurveIndex == 0) {
			DeleteFirstCurve ();
			return;
		}*/

		bool IsLastCurve = CurveIndex == Sections.Count - 1;
		
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;

		Section selectedSection = Sections [CurveIndex];

		int firstPointIndex = selectedSection.manipulators [0].ControlPointIndex;
		Vector3[] prevPoints = new Vector3 [4] {
			spline.GetControlPoint (firstPointIndex),
			spline.GetControlPoint (firstPointIndex + 1),
			spline.GetControlPoint (firstPointIndex + 2),
			spline.GetControlPoint (firstPointIndex + 3)
		};
		

		spline.DeletePoints (firstPointIndex, 3);
		
		for (int k = CurveIndex + 1; k < Sections.Count; ++k) {
			Section section = Sections [k];
			
			for (int j = 0; j < section.manipulators.Length; ++j) {
				section.manipulators [j].ControlPointIndex -= 3;
			}
		}
		
		LastControlPointManip.ControlPointIndex -= 3;
		
		DeleteSection (CurveIndex);
		SelectedCurve = CurveIndex - 1;
		if (SelectedCurve < 0)
			SelectedCurve = 0;
		
		Section prevSection = Sections [SelectedCurve];
		ControlPointManip lastManipulator = prevSection.manipulators [prevSection.manipulators.Length - 1];
		spline.SetControlPoint (lastManipulator.ControlPointIndex, spline.GetControlPoint (lastManipulator.ControlPointIndex));
		
		RebuildSection (SelectedCurve);
		
		if (SelectedCurve == Sections.Count - 1)
			RebuildSection (0);
		else
			RebuildSection (SelectedCurve + 1);
		
		if (SelectedCurve == 0)
			RebuildSection (Sections.Count - 1);
		else
			RebuildSection (SelectedCurve - 1);
		
		UpdateHull ();


		if (pushUndo)
			UndoRedoSys.PushCommand (new DeleteCurveCommand (CurveIndex, prevPoints, IsLastCurve));
	}

	void DeleteFirstCurve () {
		if (Sections.Count == 1)
			return;

		//
	}

	public void ToggleLoop () { // used by Loop checkbox UI
		ToggleLoop2 (true);
	}

	public void ToggleLoop2 (bool pushUndo = false) { // HACK: used by ToggleLoopCommand undo/redo command
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;

		spline.Loop = !spline.Loop;

		RebuildSection (0);
		RebuildSection (spline.CurveCount - 1);

		UpdateHull ();


		if (pushUndo)
			UndoRedoSys.PushCommand (new ToggleLoopCommand (ToggleUI));
	}


	public Vector3 GetControlPoint (int index) {
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return Vector3.zero;

		return spline.GetControlPoint (index);
	}

	public void MoveControlPoint (int pointIndex, Vector3 pos) {
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;

		if (SnapToGrid) {
			pos.x = Mathf.Floor (pos.x);
			pos.y = Mathf.Floor (pos.y);
			pos.z = Mathf.Floor (pos.z);
		}

		spline.SetControlPoint (pointIndex, pos);

		if (pointIndex == 0) {
			RebuildSection (0);
			if (spline.CurveCount > 1 && spline.Loop)
				RebuildSection (spline.CurveCount - 1);
			SelectedCurve = 0;
		}
		else if (pointIndex == (spline.ControlPointCount - 1)) {
			RebuildSection (spline.CurveCount - 1);
			if (spline.CurveCount > 1 && spline.Loop) {
				RebuildSection (0);
				SelectedCurve = 0;
			}
			else
				SelectedCurve = spline.CurveCount - 1;
		}
		else {
			int curveIndex = pointIndex / 3;

			RebuildSection (SelectedCurve);
			
			if (SelectedCurve == Sections.Count - 1)
				RebuildSection (0);
			else
				RebuildSection (SelectedCurve + 1);
			
			if (SelectedCurve == 0)
				RebuildSection (Sections.Count - 1);
			else
				RebuildSection (SelectedCurve - 1);

			SelectedCurve = curveIndex;
		}

		UpdateHull ();
	}

	void AddSection (int firstCtrlPt, int atIndex = -1) {
		if (firstCtrlPt < 0)
			firstCtrlPt = 0;
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;

		int sectionIndex = Sections.Count;
		Section section = new Section ();

		GameObject GO = new GameObject ("section " + sectionIndex);
		GO.transform.parent = transform;
		section.geometry = GO;

		section.manipulators = new ControlPointManip [3];
		for (int k = 0; k < 3; ++k) {
			int controlPointIndex = firstCtrlPt + k;
			Vector3 controlPointPos = spline.GetControlPoint (controlPointIndex);
			section.manipulators [k] = CreateControlPointManip (GO.transform, controlPointPos, controlPointIndex);
		}

		if (atIndex == -1)
			Sections.Add (section); // append
		else
			Sections.Insert (atIndex, section);

		int lastControlPointIndex = spline.ControlPointCount - 1;
		LastControlPointManip.ControlPointIndex = lastControlPointIndex;

		Vector3 lastControlPointPos = spline.GetControlPoint (lastControlPointIndex);
		LastControlPointManip.transform.position = lastControlPointPos;
	}

	void DeleteSection (int sectionIndex) {
		if (sectionIndex < 0 || sectionIndex >= Sections.Count)
			return;
		Section section = Sections [sectionIndex];
		
		GameObject.Destroy (section.geometry);
		
		for (int k = 0; k < section.manipulators.Length; ++k) {
			GameObject.Destroy (section.manipulators [k].gameObject);
		}
		
		Sections.RemoveAt (sectionIndex);
	}

	void RebuildSection (int sectionIndex) {
		if (sectionIndex < 0 || sectionIndex >= Sections.Count)
			return;
		Section section = Sections [sectionIndex];

		Track.GenerateCurveMesh (GetComponent <BezierSpline> (), sectionIndex, Builder);
		{
			Mesh mesh = Builder.CreateMesh ();
			MeshFilter filter = section.geometry.GetComponent<MeshFilter> ();
			if (filter == null) filter = section.geometry.AddComponent<MeshFilter> ();
			filter.sharedMesh = mesh;
			
			MeshRenderer meshRenderer = section.geometry.GetComponent<MeshRenderer> ();
			if (meshRenderer == null) meshRenderer = section.geometry.AddComponent<MeshRenderer> ();
			meshRenderer.material = RoadMaterial;
		}
		Builder.Clear ();
	}

	ControlPointManip CreateControlPointManip (Transform ParentTransform, Vector3 Pos, int ControlPointIndex) {
		GameObject GO = GameObject.Instantiate (ControlPointManipPrefab, Pos, Quaternion.identity) as GameObject;
		GO.transform.parent = ParentTransform;

		ControlPointManip cpm  = GO.GetComponent <ControlPointManip> ();
		cpm.ControlPointIndex = ControlPointIndex;

		return cpm;
	}

	void UpdateHull () {
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;

		// Hull of the whole spline
		{
			if (Hull == null)
				return;

			int count = spline.ControlPointCount;
			Hull.SetVertexCount (count);
			Hull.SetWidth (0.2f, 0.2f);
			Hull.SetColors (Color.red, Color.white);

			for (int k = 0; k < count; ++k) {
				Hull.SetPosition (k, spline.GetControlPoint (k));
			}
		}

		// Hull of the selected curve
		{
			if (SelectedCurveHull == null)
				return;

			SelectedCurveHull.SetVertexCount (4);
			SelectedCurveHull.SetWidth (1.0f, 1.0f);
			SelectedCurveHull.SetColors (Color.red, Color.white);
			
			for (int k = 0; k < 4; ++k) {
				SelectedCurveHull.SetPosition (k, spline.GetControlPoint (k + SelectedCurve * 3));
			}
		}
	}


// undo/redo
//
	class AppendCurveCommand : UndoRedoSys.ICommand {
		int LastPointIndex;
		Vector3 PrevLastPointPos;
		Vector3 PrevSecondLastPoint;

		public AppendCurveCommand (int LastPointIndex, Vector3 PrevLastPointPos, Vector3 PrevSecondLastPoint) {
			this.PrevLastPointPos = PrevLastPointPos;
			this.LastPointIndex = LastPointIndex;
			this.PrevSecondLastPoint = PrevSecondLastPoint;
		}

		public void Undo () {
			LevelObject lvl = LevelObject.GlobalInstance;
			lvl.DeleteLastCurve (false);
			lvl.MoveControlPoint (LastPointIndex, PrevLastPointPos);
			lvl.MoveControlPoint (LastPointIndex - 1, PrevSecondLastPoint);
		}
		
		public void Redo () {
			LevelObject.GlobalInstance.AppendCurve2 (false);
		}
		
		public string UndoDescString () {
			return "Undo append curve";
		}
		
		public string RedoDescString () {
			return "Redo append curve";
		}
	}

	class InsertCurveCommand : UndoRedoSys.ICommand {
		int CurveIndex;
		Vector3[] PrevPoints;
		
		public InsertCurveCommand (int CurveIndex, Vector3[] PrevPoints)
		{
			this.CurveIndex = CurveIndex;
			this.PrevPoints = PrevPoints;
		}
		
		public void Undo () {
			LevelObject lvl = LevelObject.GlobalInstance;
			lvl.DeleteCurveAt (CurveIndex, false);

			int startPtIndex = CurveIndex * 3;
			lvl.MoveControlPoint (startPtIndex, PrevPoints [0]);
			lvl.MoveControlPoint (startPtIndex + 3, PrevPoints [3]);
			lvl.MoveControlPoint (startPtIndex + 1, PrevPoints [1]);
			lvl.MoveControlPoint (startPtIndex + 2, PrevPoints [2]);
		}
		
		public void Redo () {
			LevelObject.GlobalInstance.InsertCurveAt (CurveIndex, false);
		}
		
		public string UndoDescString () {
			return "Undo insert curve";
		}
		
		public string RedoDescString () {
			return "Redo insert curve";
		}
	}

	class DeleteCurveCommand : UndoRedoSys.ICommand {
		int CurveIndex;
		Vector3[] PrevPoints;
		bool Append;

		public DeleteCurveCommand (int CurveIndex, Vector3[] PrevPoints, bool Append) {
			this.CurveIndex = CurveIndex;
			this.PrevPoints = PrevPoints;
			this.Append = Append;
		}

		public void Undo () {
			LevelObject lvl = LevelObject.GlobalInstance;

			if (Append) {
				lvl.AppendCurve2 (false);
			}
			else {
				lvl.InsertCurveAt (CurveIndex, false);
			}

			int startPtIndex = CurveIndex * 3;
			lvl.MoveControlPoint (startPtIndex, PrevPoints [0]);
			lvl.MoveControlPoint (startPtIndex + 3, PrevPoints [3]);
			lvl.MoveControlPoint (startPtIndex + 1, PrevPoints [1]);
			lvl.MoveControlPoint (startPtIndex + 2, PrevPoints [2]);
		}

		public void Redo () {
			LevelObject.GlobalInstance.DeleteCurveAt (CurveIndex, false);
		}
		
		public string UndoDescString () {
			return "Undo remove curve";
		}

		public string RedoDescString () {
			return "Redo remove curve";
		}
	}

	class ToggleLoopCommand : UndoRedoSys.ICommand {
		UnityEngine.UI.Toggle ToggleUI;

		public ToggleLoopCommand (UnityEngine.UI.Toggle ToggleUI) {
			this.ToggleUI = ToggleUI;
		}

		public void Undo () {
			ToggleUI.isOn = !ToggleUI.isOn;
		}
		
		public void Redo () {
			ToggleUI.isOn = !ToggleUI.isOn;
		}
		
		public string UndoDescString () {
			return "Undo toggle loop";
		}
		
		public string RedoDescString () {
			return "Redo toggle loop";
		}
	}
	
}
