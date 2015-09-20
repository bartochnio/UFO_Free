using UnityEngine;
using System.Collections;

public class ControlPointManip : MonoBehaviour {
	public int ControlPointIndex = 0;

	bool Manipulated = false;
	Vector3 PrevPos;

	const int SkipFrames = 0;
	int Frame = SkipFrames;


	void OnMouseDown () {
		if (LevelObject.GlobalInstance.GetGUIState () != LevelObject.GUIState.EditorControls)
			return;

		Manipulated = PositionManip.GlobalInstance.BeginManipulation (gameObject);
		PrevPos = LevelObject.GlobalInstance.GetControlPoint (ControlPointIndex);
	}

	void Update () {
		if (Manipulated) {
			LevelObject.GlobalInstance.MoveControlPoint (ControlPointIndex, transform.position);
		}
		else {
			--Frame;
			if (Frame < 0) {
				Frame = SkipFrames;
				transform.position = LevelObject.GlobalInstance.GetControlPoint (ControlPointIndex);
			}
		}
	}

	void OnMouseUp () {
		PositionManip.GlobalInstance.EndManipulation ();
		Manipulated = false;

		UndoRedoSys.PushCommand (new PositionManipCommand (ControlPointIndex, PrevPos, transform.position));
	}


// undo/redo
//
	class PositionManipCommand : UndoRedoSys.ICommand {
		int ControlPointIndex;
		Vector3 PrevPos;
		Vector3 NewPos;

		public PositionManipCommand (int ControlPointIndex,
		                             Vector3 PrevPos,
		                             Vector3 NewPos)
		{
			this.ControlPointIndex = ControlPointIndex;
			this.PrevPos = PrevPos;
			this.NewPos = NewPos;
		}

		public void Undo () {
			LevelObject.GlobalInstance.MoveControlPoint (ControlPointIndex, PrevPos);
		}

		public void Redo () {
			LevelObject.GlobalInstance.MoveControlPoint (ControlPointIndex, NewPos);
		}
		
		public string UndoDescString () {
			return "Undo move";
		}

		public string RedoDescString () {
			return "Redo move";
		}
	}
}
