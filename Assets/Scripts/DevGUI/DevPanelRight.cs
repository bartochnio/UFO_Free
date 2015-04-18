using UnityEngine;
using System.Collections.Generic;

public class DevPanelRight : MonoBehaviour {

	public interface PrefabProvider {
		GameObject	GetPrefab (DevPanelControl.Type type);
		string		GetIdentifierChildName (DevPanelControl.Type type);
		string		GetScriptChildName (DevPanelControl.Type type);
	}


	List<DevPanelControl> controls_ = new List<DevPanelControl> ();


	private DevPanelControl NewControl (GameObject prefab, int height) {
		DevPanelControl control = new DevPanelControl ();

		control.go_ = GameObject.Instantiate (prefab) as GameObject;

		RectTransform tx = control.go_.transform as RectTransform;
		tx.SetParent (transform, false);

		tx.offsetMax = new Vector2 (0, -height * controls_.Count);
		tx.offsetMin = new Vector2 (0, Screen.height + (transform as RectTransform).offsetMax.y - height * (controls_.Count + 1));

		UnityEngine.UI.Image image = control.go_.GetComponent <UnityEngine.UI.Image> ();

		Color darker = new Color (0.7f, 0.7f, 0.7f, image.color.a);
		Color lighter = new Color (1.0f, 1.0f, 1.0f, image.color.a);
		image.color = (controls_.Count % 2 == 0)? darker : lighter;

		controls_.Add (control);
		//Debug.Log ("New control");
		return control;
	}

	public DevPanelControl.Value AddLabelControl (string name, PrefabProvider prefabs, int height) {
		var control = NewControl (prefabs.GetPrefab (DevPanelControl.Type.Label), height);
		if (control != null) {
			Transform aChild = control.go_.transform.FindChild (prefabs.GetIdentifierChildName (DevPanelControl.Type.Label));
			UnityEngine.UI.Text uiNameText = aChild.GetComponent <UnityEngine.UI.Text> ();
			uiNameText.text = name;

			aChild = control.go_.transform.FindChild (prefabs.GetScriptChildName (DevPanelControl.Type.Label));
			UnityEngine.UI.Text uiValueText = aChild.GetComponent <UnityEngine.UI.Text> ();

			control.value_ = new DevPanelControl.LabelValue (uiValueText);

			return control.value_;
		}
		return null;
	}

	public DevPanelControl.Value AddSliderControl (string name, PrefabProvider prefabs, int height) {
		var control = NewControl (prefabs.GetPrefab (DevPanelControl.Type.Slider), height);
		if (control != null) {
			Transform aChild = control.go_.transform.FindChild (prefabs.GetIdentifierChildName (DevPanelControl.Type.Slider));
			UnityEngine.UI.Text uiNameText = aChild.GetComponent <UnityEngine.UI.Text> ();
			uiNameText.text = name;

			aChild = control.go_.transform.FindChild (prefabs.GetScriptChildName (DevPanelControl.Type.Slider));
			UnityEngine.UI.Slider uiSlider = aChild.GetComponent <UnityEngine.UI.Slider> ();

			control.value_ = new DevPanelControl.SliderValue (uiSlider, uiNameText, name);

			return control.value_;
		}
		return null;
	}

	public DevPanelControl.Value AddToggleControl (string name, PrefabProvider prefabs, int height) {
		var control = NewControl (prefabs.GetPrefab (DevPanelControl.Type.Toggle), height);
		if (control != null) {
			Transform aChild = control.go_.transform.FindChild (prefabs.GetIdentifierChildName (DevPanelControl.Type.Toggle));
			UnityEngine.UI.Text uiNameText = aChild.GetComponent <UnityEngine.UI.Text> ();
			uiNameText.text = name;

			aChild = control.go_.transform.FindChild (prefabs.GetScriptChildName (DevPanelControl.Type.Toggle));
			UnityEngine.UI.Toggle uiToggle = aChild.GetComponent <UnityEngine.UI.Toggle> ();

			control.value_ = new DevPanelControl.ToggleValue (uiToggle);

			return control.value_;
		}
		return null;
	}

	public void RemoveControl (DevPanelControl.Value value) {
		var control = controls_.Find ( ctrl => ctrl.value_ == value );
		if (control != null) {

			GameObject.Destroy (control.go_);
			control.go_ = null;

			//Debug.Log ("Delete control");
			controls_.Remove (control);
		}
	}
}
