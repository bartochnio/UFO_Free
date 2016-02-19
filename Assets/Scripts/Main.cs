using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {

	void Start() {
		if (Application.platform != RuntimePlatform.IPhonePlayer) {
			SceneManager.LoadScene("LevelEditor");
		}
	}

	Vector2 scrollPosition = Vector2.zero;

	void OnGUI() {
		if (Application.platform != RuntimePlatform.IPhonePlayer)
			return;

		string filesLocation = Application.dataPath.Replace (Application.productName + ".app/Data", "/Documents/");
		System.IO.FileInfo[] files = new System.IO.DirectoryInfo(filesLocation).GetFiles();

		int levelFilesCount = 0;
		foreach (System.IO.FileInfo fi in files) {
			if (fi.Extension == LevelObject.LevelFileExtStr) ++levelFilesCount;
		}

		if (levelFilesCount == 0) {

			GUI.Box (new Rect ((Screen.width - 200) / 2, (Screen.height - 40) / 2, 200, 40), "No level files found in the Documents folder.");

			return;
		}

		int buttonSize = 64;
		int buttonMargin = 2;
		int colCount = (Screen.width - 30) / (buttonSize + 2 * buttonMargin);
		int rowCount = levelFilesCount / colCount + 1;

		Rect scrollViewRect = new Rect (0, 0, Screen.width, Screen.height);
		Rect contentRect = new Rect (0, 0, colCount * (buttonSize + 2 * buttonMargin), rowCount * (buttonSize + 2 * buttonMargin));

		scrollPosition = GUI.BeginScrollView (scrollViewRect, scrollPosition, contentRect);
		{
			int index = 0;
			for (int r = 0; r < rowCount && index < levelFilesCount; ++r) {
				for (int c = 0; c < colCount && index < levelFilesCount; ++c) {
					System.IO.FileInfo fi = null;
					do {
						fi = files [index++];
					}
					while (fi.Extension != LevelObject.LevelFileExtStr);
					
					int x = (buttonSize + 2 * buttonMargin) * c;
					int y = (buttonSize + 2 * buttonMargin) * r;

					string filename = System.IO.Path.GetFileNameWithoutExtension(fi.Name);

					if (GUI.Button (new Rect (x + buttonMargin, y + buttonMargin, buttonSize, buttonSize), filename)) {

						Track.levelToLoadHack = filename;
						Track.lauchFromEditorHack = true;
						SceneManager.LoadScene ("Prototype");
					}
				}
			}
		}
		GUI.EndScrollView (true);
	}
}
