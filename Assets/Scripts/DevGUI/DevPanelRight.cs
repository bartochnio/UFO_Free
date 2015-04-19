using UnityEngine;
using System.Collections.Generic;

public class DevPanelRight : MonoBehaviour {

	// implementor provides the prefabs and their meta data
	//
	public interface PrefabProvider {
		GameObject	GetPrefab (DevPanelControl.Type type);
		string		GetIdentifierChildName (DevPanelControl.Type type);
		string		GetScriptChildName (DevPanelControl.Type type);
	}


	// container for the dev controls in this panel
	List<DevPanelControl> controls_ = new List<DevPanelControl> ();

	private UnityEngine.UI.Scrollbar scrollbar_ = null;
	Transform scrolledPanelTx_;


	void Awake () {
		scrollbar_ = transform.FindChild ("Scrollbar").GetComponent <UnityEngine.UI.Scrollbar> ();
		scrollbar_.onValueChanged.AddListener (OnScrollbarChanged);

		scrolledPanelTx_ = transform.FindChild ("Panel");
	}


	// event handler for scrollbar
	void OnScrollbarChanged (float value) {
		float panelViewHeight	 = GetPanelViewHeight ();
		float panelContentHeight = CalcPanelContentHeight ();

		float offset = 0.0f;

		if (panelContentHeight > panelViewHeight) {
			float diff = panelContentHeight - panelViewHeight;
			offset = diff * value;
		}

		(scrolledPanelTx_ as RectTransform).anchoredPosition = new Vector2 (0, offset);
		//scrollbar_.interactable = offset > 0.0f;
	}

	float GetPanelViewHeight () {
		return (transform as RectTransform).rect.height;
	}

	float CalcPanelContentHeight () {
		float h = 0.0f;
		foreach (var c in controls_) {
			h += (c.go_.transform as RectTransform).rect.height;
		}
		return h;
	}


	// private function used to do common init stuff on a control
	//
	private DevPanelControl NewControl (GameObject prefab, int height) {
		DevPanelControl control = new DevPanelControl ();
		control.go_ = GameObject.Instantiate (prefab) as GameObject;

		RectTransform tx = control.go_.transform as RectTransform;
		tx.SetParent (scrolledPanelTx_, false);

		tx.anchorMin = new Vector2 (0, 1);
		tx.anchorMax = new Vector2 (1, 1);
		tx.anchoredPosition = new Vector2 (0, -CalcPanelContentHeight () - height * 0.5f);
		tx.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, height);

		UnityEngine.UI.Image image = control.go_.GetComponent <UnityEngine.UI.Image> ();

		Color darker = new Color (0.7f, 0.7f, 0.7f, image.color.a);
		Color lighter = new Color (1.0f, 1.0f, 1.0f, image.color.a);
		image.color = (controls_.Count % 2 == 0)? darker : lighter;

		controls_.Add (control);
		//Debug.Log ("New control");
		return control;
	}

	// to create a empty control
	//
	public DevPanelControl.Value AddEmptyControl (PrefabProvider prefabs, int height) {
		var control = NewControl (prefabs.GetPrefab (DevPanelControl.Type.Empty), height);
		if (control != null) {

			control.value_ = new DevPanelControl.Value ();

			return control.value_;
		}
		return null;
	}

	// to create a label control
	//
	public DevPanelControl.Value AddLabelControl (string name, PrefabProvider prefabs, int height) {
		var control = NewControl (prefabs.GetPrefab (DevPanelControl.Type.Label), height);
		if (control != null) {
			Transform aChild = control.go_.transform.FindChild (prefabs.GetIdentifierChildName (DevPanelControl.Type.Label));
			UnityEngine.UI.Text uiNameText = aChild.GetComponent <UnityEngine.UI.Text> ();
			uiNameText.text = name;
			
			control.value_ = new DevPanelControl.Value ();
			
			return control.value_;
		}
		return null;
	}

	// to create a readonly control
	//
	public DevPanelControl.Value AddReadonlyControl (string name, PrefabProvider prefabs, int height) {
		var control = NewControl (prefabs.GetPrefab (DevPanelControl.Type.Readonly), height);
		if (control != null) {
			Transform aChild = control.go_.transform.FindChild (prefabs.GetIdentifierChildName (DevPanelControl.Type.Readonly));
			UnityEngine.UI.Text uiNameText = aChild.GetComponent <UnityEngine.UI.Text> ();
			uiNameText.text = name;

			aChild = control.go_.transform.FindChild (prefabs.GetScriptChildName (DevPanelControl.Type.Readonly));
			UnityEngine.UI.Text uiValueText = aChild.GetComponent <UnityEngine.UI.Text> ();

			control.value_ = new DevPanelControl.ReadonlyValue (uiValueText);

			return control.value_;
		}
		return null;
	}

	// to create a slider control
	//
	public DevPanelControl.Value AddSliderControl (string name, PrefabProvider prefabs, int height, bool wholeNumbers) {
		var control = NewControl (prefabs.GetPrefab (DevPanelControl.Type.Slider), height);
		if (control != null) {
			Transform aChild = control.go_.transform.FindChild (prefabs.GetIdentifierChildName (DevPanelControl.Type.Slider));
			UnityEngine.UI.Text uiNameText = aChild.GetComponent <UnityEngine.UI.Text> ();
			uiNameText.text = name;

			aChild = control.go_.transform.FindChild (prefabs.GetScriptChildName (DevPanelControl.Type.Slider));
			UnityEngine.UI.Slider uiSlider = aChild.GetComponent <UnityEngine.UI.Slider> ();

			uiSlider.wholeNumbers = wholeNumbers;

			control.value_ = new DevPanelControl.SliderValue (uiSlider, uiNameText, name);

			return control.value_;
		}
		return null;
	}

	// to create a toggle control
	//
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

	// to create a button control
	//
	public DevPanelControl.Value AddButtonControl (string name, PrefabProvider prefabs, int height, string desc) {
		var control = NewControl (prefabs.GetPrefab (DevPanelControl.Type.Button), height);
		if (control != null) {
			Transform aChild = control.go_.transform.FindChild (prefabs.GetIdentifierChildName (DevPanelControl.Type.Button));
			UnityEngine.UI.Text uiNameText = aChild.GetComponent <UnityEngine.UI.Text> ();
			uiNameText.text = desc; // HACK.. for now use the Id control text as Description text
			
			aChild = control.go_.transform.FindChild (prefabs.GetScriptChildName (DevPanelControl.Type.Button));
			UnityEngine.UI.Button uiButton = aChild.GetComponent <UnityEngine.UI.Button> ();

			uiButton.transform.FindChild ("Text").GetComponent <UnityEngine.UI.Text> ().text = name;
			
			control.value_ = new DevPanelControl.ButtonValue (uiButton);
			
			return control.value_;
		}
		return null;
	}

	// to destroy a control
	//
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
