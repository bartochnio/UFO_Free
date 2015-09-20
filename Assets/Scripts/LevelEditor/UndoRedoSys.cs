using UnityEngine;
using System.Collections.Generic;

public class UndoRedoSys {

	public interface ICommand {
		void Undo ();
		void Redo ();

		string UndoDescString ();
		string RedoDescString ();
	};

	static List <ICommand> UndoList = new List<ICommand> ();
	static List <ICommand> RedoList = new List<ICommand> ();


	public static bool Undo () {
		if (UndoList.Count > 0) {
			int lastIndex = UndoList.Count - 1;
			ICommand comm = UndoList [lastIndex];

			comm.Undo ();

			RedoList.Add (comm);
			UndoList.RemoveAt (lastIndex);
			return true;
		}
		return false;
	}

	public static bool Redo () {
		if (RedoList.Count > 0) {
			int lastIndex = RedoList.Count - 1;
			ICommand comm = RedoList [lastIndex];
			
			comm.Redo ();
			
			UndoList.Add (comm);
			RedoList.RemoveAt (lastIndex);
			return true;
		}
		return false;
	}


	public static void PushCommand (ICommand comm) {
		RedoList.Clear ();
		UndoList.Add (comm);
	}

	public static void Clear () {
		UndoList.Clear ();
		RedoList.Clear ();
	}


	public static int GetUndoCommandCount () {
		return UndoList.Count;
	}

	public static int GetRedoCommandCount () {
		return RedoList.Count;
	}

	public static string GetUndoCommandDesc (int index) {
		return UndoList [index].UndoDescString ();
	}

	public static string GetRedoCommandDesc (int index) {
		return RedoList [index].RedoDescString ();
	}
}
