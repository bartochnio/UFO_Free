﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

public class LevelObject : MonoBehaviour {
#region vars
// global instance
	static private LevelObject globalInstance = null;
	static public  LevelObject GlobalInstance { get { return globalInstance; } }

// public constants
//
	public const string LevelFileExtStr = ".ufo";


// public vars
//
	public Material		RoadMaterial;
	public GameObject	ControlPointManipPrefab;

	public LineRenderer	Hull;
	public LineRenderer	SelectedCurveHull;

	public GameObject				canvas;
	public UnityEngine.UI.Toggle	ToggleUI;
	public UnityEngine.UI.Text		CurrentCurveText;
	public UnityEngine.UI.Text		CurrentLevelText;

	public Material		GridMaterial;
	public MeshRenderer	GridMeshRenderer;


// private vars
//
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

	static string		currentLevelFilename = null;
	bool				modified = false;
	bool				isLoading = false;

	FileInfo[]			files;
	Vector2				scrollPosition;

	string				enterFilenameStr = "";


	public enum GUIState
	{
		EditorControls,
		LoadFileDialog,
		SaveFileDialog,
		NewFile,
		Warning
	}
	GUIState guiState = GUIState.EditorControls;


	enum WarningResult {
		Undecided,
		Yes,
		No
	}
	GUIState			nextStateAfterWarning;
	string				warningText = "";
	WarningResult		warningResult;
#endregion


#region monobehaviour
// MonoBehaviour
//
	void Awake () {
		globalInstance = this;
	}

	void OnDestroy() {
		//globalInstance = null;
	}

	void Start () {
		GridBounds = GridMeshRenderer.bounds;
		GridMaterial.mainTextureScale = GridBounds.max;

		Load (currentLevelFilename);
	}

	void Update () {
		switch (guiState) {
		case GUIState.EditorControls:
			{
				if (Input.GetKeyUp (KeyCode.Z)) {
					if (Input.GetKey (KeyCode.LeftCommand) && Input.GetKey (KeyCode.LeftShift)) {
					
						OnRedo ();
					} else if (Input.GetKey (KeyCode.LeftCommand)) {
					
						OnUndo ();
					}
				}

				CurrentCurveText.text = "Curve " + (SelectedCurve + 1) + " of " + Sections.Count;

				string filename = currentLevelFilename ?? "";
				CurrentLevelText.text = "Level: " + filename.ToUpper ();
			}
			break;

		case GUIState.LoadFileDialog:
			if (Input.GetKeyDown (KeyCode.Escape)) {
				guiState = GUIState.EditorControls;
				canvas.SetActive (true);
			}
			break;

		case GUIState.SaveFileDialog:
			if (Input.GetKeyDown (KeyCode.Escape)) {
				guiState = GUIState.EditorControls;
				canvas.SetActive (true);
			}
			break;

		case GUIState.NewFile:
			break;

		case GUIState.Warning: 
			if (Input.GetKeyDown (KeyCode.Escape)) {
				warningResult = WarningResult.No;
			}
			if (warningResult != WarningResult.Undecided) {
				if (warningResult == WarningResult.No) {
					guiState = GUIState.EditorControls;
					canvas.SetActive (true);
				} else if (nextStateAfterWarning == GUIState.LoadFileDialog) {
					scrollPosition = Vector2.zero;
					guiState = GUIState.LoadFileDialog;
				} else if (nextStateAfterWarning == GUIState.NewFile) {
					New ();
					guiState = GUIState.EditorControls;
				}
			}
			break;
		} // end switch
	}
		
	void OnGUI () {
		GUI.contentColor = Color.black;

		switch (guiState) {
			case GUIState.EditorControls:
			{
			}
			break;

			case GUIState.LoadFileDialog:
			{
				DoLoadFileDialog ();
			}
			break;

			case GUIState.SaveFileDialog:
			{
				DoSaveFileDialog ();
			}
			break;

			case GUIState.NewFile:
			{
			}
			break;

			case GUIState.Warning:
			{
				DoWarningDialog ();
			}
			break;
		} // end switch
	}
#endregion 


#region publicfuncs
// public functions
//
	public GUIState GetGUIState() {
		return guiState;
	}

	public void New()
	{
		Load (null);
	}

	public bool Load (string levelFilename)
	{
		isLoading = true;
		ResetMe();
		
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return false;
		
		if (!LoadFromLevelFile (levelFilename)) {
			spline.Reset ();
		}
		
		SelectedCurve = 0;
		Initialize ();

		currentLevelFilename = levelFilename;
		modified = false;

		isLoading = false;
		return true;
	}

	public void Save ()
	{
		SaveToLevelFile (currentLevelFilename);
	}

	public void SaveAs(string levelFilename)
	{
		SaveToLevelFile (levelFilename);
	}
#endregion


#region privatefuncs
// private functions
//
	void DoLoadFileDialog() {
	// Display Load File Dialog GUI
	//
		GUI.Box (new Rect(0, 0, Screen.width, Screen.height), "");

		GUI.contentColor = Color.white;
		GUI.Box (new Rect(0, 0, Screen.width, 25), "");
		GUI.Box (new Rect(0, 0, Screen.width, 25), Application.persistentDataPath);
		//GUI.contentColor = Color.black;

		const float buttonWidth = 255;
		const float buttonHeight = 30;
		const float marginY = 10;

		Rect contentRect = new Rect(0, 0, buttonWidth, (files.Length + 1) * (buttonHeight + marginY));

		const float scrollViewWidth = buttonWidth + 30;
		float scrollViewX = (Screen.width - scrollViewWidth) / 2;
		float scrollViewY = 30;

		Rect scrollViewRect = new Rect (scrollViewX, scrollViewY, scrollViewWidth, Screen.height - scrollViewY);

		scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);
		{
			GUI.Box (contentRect, "");

			float btnX = 0;
			float btnY = 0;

			int filesDisplayed = 0;
			for (int k = 0; k < files.Length; ++k)
			{
				if (files[k].Extension.ToLower() == LevelFileExtStr) {
					++filesDisplayed;

					string filename = Path.GetFileNameWithoutExtension(files[k].Name);
					if (GUI.Button (new Rect(btnX, btnY, buttonWidth, buttonHeight), filename))
					{
						Load (filename);

						guiState = GUIState.EditorControls;
						canvas.SetActive (true);

						break;
					}

					btnY += buttonHeight + marginY;
				}
			}
			if (filesDisplayed == 0) {
				GUI.Label (new Rect(btnX, btnY, buttonWidth, buttonHeight), "No files to list");
			}
		}
		GUI.EndScrollView(true);

		//GUI.contentColor = Color.white;
		GUI.Box(new Rect(0, 0, 50, 25), "OPEN");
		GUI.Box(new Rect(0, 25, 130, 20), "");
		GUI.Box(new Rect(0, 25, 130, 20), "Press Esc to cancel");
	}

	void DoSaveFileDialog() {
	// Display Save File Dialog GUI
	//
		GUI.Box (new Rect(0, 0, Screen.width, Screen.height), "");

		GUI.contentColor = Color.white;
		GUI.Box (new Rect(0, 0, Screen.width, 25), "");
		GUI.Box (new Rect(0, 0, Screen.width, 25), Application.persistentDataPath);
		//GUI.contentColor = Color.black;

		Rect textFieldRect = new Rect((Screen.width - 300) / 2 + 50, 30, 300, 30);

		GUI.Box (new Rect(textFieldRect.x - 105, textFieldRect.y - 5, 510, 40), "");
		GUI.Box (new Rect(textFieldRect.x - 100, textFieldRect.y, 100, 30), "Enter filename: ");
		GUI.contentColor = Color.white;
		enterFilenameStr = GUI.TextField (textFieldRect, enterFilenameStr);
		//GUI.contentColor = Color.black;
		if (GUI.Button(new Rect(textFieldRect.x + 300, textFieldRect.y, 100, 30), "CONFIRM"))
		{
			if (SaveToLevelFile (enterFilenameStr)) {
				currentLevelFilename = enterFilenameStr;
			}

			guiState = GUIState.EditorControls;
			canvas.SetActive(true);
		}

		const float buttonWidth = 255;
		const float buttonHeight = 30;
		const float marginY = 10;

		Rect contentRect = new Rect(0, 0, buttonWidth, (files.Length + 1) * (buttonHeight + marginY));

		const float scrollViewWidth = buttonWidth + 30;
		float scrollViewX = (Screen.width - scrollViewWidth) / 2;
		float scrollViewY = 100;

		Rect scrollViewRect = new Rect (scrollViewX, scrollViewY, scrollViewWidth, Screen.height - scrollViewY);

		scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);
		{
			GUI.Box (contentRect, "");

			float btnX = 0;
			float btnY = 0;

			for (int k = 0; k < files.Length; ++k)
			{
				if (files[k].Extension.ToLower() == LevelFileExtStr) {
					string filename = Path.GetFileNameWithoutExtension(files[k].Name);
					if (GUI.Button (new Rect(btnX, btnY, buttonWidth, buttonHeight), filename))
					{
						enterFilenameStr = filename;
					}

					btnY += buttonHeight + marginY;
				}
			}
		}
		GUI.EndScrollView(true);

		//GUI.contentColor = Color.white;
		GUI.Box(new Rect(0, 0, 50, 25), "SAVE");
		GUI.Box(new Rect(0, 25, 130, 20), "");
		GUI.Box(new Rect(0, 25, 130, 20), "Press Esc to cancel");
	}

	void DoWarningDialog() {
	// Display Warning message Dialog GUI
	//
		GUI.Box (new Rect(0, 0, Screen.width, Screen.height), "");

		GUI.contentColor = Color.white;
		float W = 400;
		float H = 100;
		GUI.Box(new Rect((Screen.width - W) / 2, (Screen.height - H) / 2, W, 25), "");
		GUI.Box(new Rect((Screen.width - W) / 2, (Screen.height - H) / 2, W, H), "Warning");

		GUI.Label(new Rect((Screen.width - W + 10) / 2, (Screen.height - H + 35) / 2 + 10, W - 10, 30), warningText);

		if (GUI.Button(new Rect((Screen.width - W) / 2 + 80, (Screen.height - H) / 2 + 60, 100, 30), "Yes"))
		{
			warningResult = WarningResult.Yes;
		}

		if (GUI.Button(new Rect((Screen.width - W) / 2 + 220, (Screen.height - H) / 2 + 60, 100, 30), "No"))
		{
			warningResult = WarningResult.No;
		}
	}


	bool LoadFromLevelFile (string levelFilename) {
		UndoRedoSys.Clear ();

		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return false;

		if (levelFilename == null)
			return false;

		try {
			string sepa = "" + System.IO.Path.DirectorySeparatorChar;
			string fullPath = Application.persistentDataPath + sepa + levelFilename + LevelFileExtStr;
			string fileContents = System.IO.File.ReadAllText (fullPath);

			XDocument doc = XDocument.Parse (fileContents);
			if (doc == null) return false;
			if (doc.Root == null) return false;

			XElement splineElem = doc.Root.Element("spline");
			if (splineElem == null) return false;

			spline.Loop = false;
			if (!spline.LoadFromXmlElement(splineElem)) return false;

			//loopToggle.GetComponent<UnityEngine.UI.Toggle>().isOn = spline.Loop;
			ToggleUI.isOn = spline.Loop;

			return true;
		}
		catch (System.Exception e) {
			Debug.Log(e.Message);
		}

		return false;
	}
		
	bool SaveToLevelFile(string levelFilename)
	{
		if (levelFilename == null)
			return false;

		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return false;

		string sepa = "" + System.IO.Path.DirectorySeparatorChar;
		string fullPath = Application.persistentDataPath + sepa + levelFilename + LevelFileExtStr;

		try {
			StringBuilder output = new StringBuilder();

			XmlWriterSettings ws = new XmlWriterSettings();
			ws.Indent = true;
			ws.Encoding = Encoding.UTF8;
			using (XmlWriter writer = XmlWriter.Create(output, ws))
			{
				writer.WriteStartElement("level");
				{
					writer.WriteAttributeString("version", "1.0");

					spline.WriteXml(writer);
				}
				writer.WriteEndElement();
			}

			System.IO.File.WriteAllText( fullPath, output.ToString () );

			modified = false;
			return true;

		} catch (System.Exception e) {
			Debug.Log (e.Message);
		}

		return false;
	}
#endregion


#region leveleditor
// Level editor functions
//

// GUI control hooks
//
	public void OnUndo () {
		UndoRedoSys.Undo ();

		if (UndoRedoSys.GetUndoCommandCount () == 0) {
			modified = false;
		}
	}

	public void OnRedo () {
		UndoRedoSys.Redo ();
	}

	public void OnNew () {
		if (modified) {
			ShowWarning ("Do you want to discard changes to the current level?", GUIState.NewFile);
		}
		else {
			New ();
		}
	}

	public void OnSave () {
		if (currentLevelFilename == null)
		{
			OnSaveAs ();
		}
		else {
			Save ();
			modified = false;
		}
	}

	public void OnSaveAs () {
		enterFilenameStr = "";
		canvas.SetActive (false);
		files = new System.IO.DirectoryInfo(Application.persistentDataPath).GetFiles();
		guiState = GUIState.SaveFileDialog;
	}

	public void OnOpen () {
		canvas.SetActive (false);
		files = new System.IO.DirectoryInfo(Application.persistentDataPath).GetFiles();

		if (modified) {
			ShowWarning ("Do you want to discard changes to the current level?", GUIState.LoadFileDialog);
		}
		else {
			guiState = GUIState.LoadFileDialog;
		}
	}

	public void OnPlay () {
		Track.levelToLoadHack = currentLevelFilename;
		Track.lauchFromEditorHack = true;
		SceneManager.LoadScene ("Prototype");
	}

	public void OnSnapToggle(bool flag) {
		SnapToGrid = !SnapToGrid;
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

		modified = true;
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

		modified = true;
	}

	public void DeleteSelectedCurve () {
		DeleteCurveAt (SelectedCurve);
	}

	public void DeleteCurveAt (int CurveIndex, bool pushUndo = true) {
		if (Sections.Count == 1)
			return;
		if (CurveIndex < 0 || CurveIndex >= Sections.Count)
			return;

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

		modified = true;
	}

	public void ToggleLoop () { // used by Loop checkbox UI
		ToggleLoop2 (true);
	}

	public void ToggleLoop2 (bool pushUndo = false) { // HACK: used by ToggleLoopCommand undo/redo command
		if (isLoading)
			return;

		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;

		spline.Loop = !spline.Loop;

		RebuildSection (0);
		RebuildSection (spline.CurveCount - 1);

		UpdateHull ();


		if (pushUndo)
			UndoRedoSys.PushCommand (new ToggleLoopCommand (ToggleUI));

		modified = true;
	}


// Level editor public funcs
	public Vector3 GetControlPoint (int index) {
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return Vector3.zero;

		return spline.GetControlPoint (index);
	}

	public void MoveControlPoint (int pointIndex, Vector3 pos) {
		if (!canvas.activeSelf)
			return;

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

		modified = true;
	}
		

// Level editor private helpers
//
	void ResetMe()
	{
		if (LastControlPointManip != null)
			GameObject.Destroy (LastControlPointManip.gameObject);

		foreach(Section sec in Sections)
		{
			GameObject.Destroy(sec.geometry);
			foreach (ControlPointManip manip in sec.manipulators)
			{
				GameObject.Destroy(manip.gameObject);
			}
		}
		Sections.Clear ();
	}

	void Initialize () {
		BezierSpline spline = GetComponent <BezierSpline> ();
		if (spline == null)
			return;

		LastControlPointManip = CreateControlPointManip (transform, Vector3.zero, 0);

		for (int k = 0; k < spline.CurveCount; ++k) {
			AddSection (3 * k);
			RebuildSection (k);
		}

		UpdateHull ();
	}

	void DeleteFirstCurve () {
		if (Sections.Count == 1)
			return;

		//
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

	void ShowWarning(string mssg, GUIState nextState) {
		warningResult = WarningResult.Undecided;
		guiState = GUIState.Warning;
		warningText = mssg;
		nextStateAfterWarning = nextState;
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
#endregion
}
