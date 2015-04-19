using UnityEngine;
using System.Collections.Generic;

public class DevGuiSystem : MonoBehaviour, DevPanelRight.PrefabProvider {
	static private DevGuiSystem instance = null;
	static public DevGuiSystem globalInstance { get { return instance; } }

	public GameObject	prefab_Empty;
	public int			emptyHeight = 20;

	public GameObject	prefab_Label;
	public string		labelIdChild;
	public int			labelHeight = 30;

	public GameObject	prefab_Readonly;
	public string		readonlyIdChild; // child of the prefab which should be used as value name UI
	public string		readonlyScriptChild; // child of the prefab which should be used as the value UI 
	public int			readonlyHeight = 40;

	public GameObject	prefab_Slider;
	public string		sliderIdChild;
	public string		sliderScriptChild;
	public int			sliderHeight = 40;

	public GameObject 	prefab_Toggle;
	public string		toggleIdChild;
	public string		toggleScriptChild;
	public int			toggleHeight = 40;

	public GameObject 	prefab_Button;
	public string		buttonDescChild;
	public string		buttonScriptChild;
	public int			buttonHeight = 30;

	private DevPanelRight panelRight_; // right now there is only 1 panel which placed to the right of the screen


	void Awake () {
		instance = this;
		Object.DontDestroyOnLoad (this.gameObject);

		panelRight_ = transform.FindChild ("RightPanel").gameObject.GetComponent <DevPanelRight> ();
		panelRight_.gameObject.SetActive (false);
	}
	

	// event handler called by the DEV button on top-right of the screen
	public void OnDevToggle () {
		panelRight_.gameObject.SetActive (!panelRight_.gameObject.activeSelf);
	}


	// implementation of DevPanelRight.PrefabProvider
	//
	public GameObject GetPrefab (DevPanelControl.Type type) {
		switch (type) {
		case DevPanelControl.Type.Empty: return prefab_Empty;
		case DevPanelControl.Type.Label: return prefab_Label;
		case DevPanelControl.Type.Readonly: return prefab_Readonly;
		case DevPanelControl.Type.Slider: return prefab_Slider;
		case DevPanelControl.Type.Toggle: return prefab_Toggle;
		case DevPanelControl.Type.Button: return prefab_Button;
		}
		return null;
	}

	public string GetIdentifierChildName (DevPanelControl.Type type) {
		switch (type) {
		case DevPanelControl.Type.Empty: return "";
		case DevPanelControl.Type.Label: return labelIdChild;
		case DevPanelControl.Type.Readonly: return readonlyIdChild;
		case DevPanelControl.Type.Slider: return sliderIdChild;
		case DevPanelControl.Type.Toggle: return toggleIdChild;
		case DevPanelControl.Type.Button: return buttonDescChild;
		}
		return "Id";
	}

	public string GetScriptChildName (DevPanelControl.Type type) {
		switch (type) {
		case DevPanelControl.Type.Empty: return "";
		case DevPanelControl.Type.Label: return "";
		case DevPanelControl.Type.Readonly: return readonlyScriptChild;
		case DevPanelControl.Type.Slider: return sliderScriptChild;
		case DevPanelControl.Type.Toggle: return toggleScriptChild;
		case DevPanelControl.Type.Button: return buttonScriptChild;
		}
		return "Control";
	}
	//


	// public functions used to create/destroy dev control
	//
	public DevPanelControl.Value AddEmpty () {
		return panelRight_.AddEmptyControl (this, emptyHeight);
	}

	public DevPanelControl.Value AddLabel (string identifier) {
		return panelRight_.AddLabelControl (identifier, this, labelHeight);
	}

	public DevPanelControl.Value AddReadonly (string identifier) {
		return panelRight_.AddReadonlyControl (identifier, this, readonlyHeight);
	}

	public DevPanelControl.Value AddSlider (string identifier, bool wholeNumbers = false) {
		return panelRight_.AddSliderControl (identifier, this, sliderHeight, wholeNumbers);
	}

	public DevPanelControl.Value AddToggle (string identifier) {
		return panelRight_.AddToggleControl (identifier, this, toggleHeight);
	}

	public DevPanelControl.Value AddButton (string identifier, string desc = "") {
		return panelRight_.AddButtonControl (identifier, this, buttonHeight, desc);
	}

	public void RemoveControl (DevPanelControl.Value value) {
		panelRight_.RemoveControl (value);
	}
	//


	// quick HACK to manage controls creation/destruction
	public class Helper {
		public List<DevPanelControl.Value> controls = new List<DevPanelControl.Value> ();

		public DevPanelControl.Value HoldOnTo (DevPanelControl.Value ctrl) {
			controls.Add (ctrl);
			return ctrl;
		}

		public void ReleaseControls () {
			foreach (var c in controls) {
				DevGuiSystem.globalInstance.RemoveControl (c);
			}
			controls.Clear ();
		}
	};
}
