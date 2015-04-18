using UnityEngine;
using System.Collections;

public class DevGuiSystem : MonoBehaviour, DevPanelRight.PrefabProvider {
	static private DevGuiSystem instance = null;
	static public DevGuiSystem globalInstance { get { return instance; } }
	
	public GameObject	prefab_Label;
	public string		labelIdChild;
	public string		labelScriptChild;
	public int			labelHeight = 40;

	public GameObject	prefab_Slider;
	public string		sliderIdChild;
	public string		sliderScriptChild;
	public int			sliderHeight = 40;

	public GameObject 	prefab_Toggle;
	public string		toggleIdChild;
	public string		toggleScriptChild;
	public int			toggleHeight = 40;


	private DevPanelRight panelRight_;


	void Awake () {
		Object.DontDestroyOnLoad (this.gameObject);

		panelRight_ = transform.FindChild ("RightPanel").gameObject.GetComponent <DevPanelRight> ();
		panelRight_.gameObject.SetActive (false);
	}

	private DevPanelControl.Value testLabel;
	private DevPanelControl.Value testSlider;
	private DevPanelControl.Value testToggle;
	void Start () {
		// Test
		testLabel = AddLabel ("Test Label");
		testLabel.Float = 30f;

		testSlider = AddSlider ("Test Slider");
		testSlider.Float = 0.5f;
		testSlider.MinFloat = 0.1f;
		testSlider.MaxFloat = 2.0f;

		testToggle = AddToggle ("Test Toggle");
		testToggle.Bool = false;
	}

	void OnDestroy () {
		RemoveControl (testLabel);
	}

	public void OnDevToggle () {
		panelRight_.gameObject.SetActive (!panelRight_.gameObject.activeSelf);
	}


	public GameObject GetPrefab (DevPanelControl.Type type) {
		switch (type) {
			case DevPanelControl.Type.None: return null;
			case DevPanelControl.Type.Label: return prefab_Label;
			case DevPanelControl.Type.Slider: return prefab_Slider;
			case DevPanelControl.Type.Toggle: return prefab_Toggle;
		}
		return null;
	}

	public string GetIdentifierChildName (DevPanelControl.Type type) {
		switch (type) {
		case DevPanelControl.Type.None: return "";
		case DevPanelControl.Type.Label: return labelIdChild;
		case DevPanelControl.Type.Slider: return sliderIdChild;
		case DevPanelControl.Type.Toggle: return toggleIdChild;
		}
		return "Id";
	}

	public string GetScriptChildName (DevPanelControl.Type type) {
		switch (type) {
		case DevPanelControl.Type.None: return "";
		case DevPanelControl.Type.Label: return labelScriptChild;
		case DevPanelControl.Type.Slider: return sliderScriptChild;
		case DevPanelControl.Type.Toggle: return toggleScriptChild;
		}
		return "Control";
	}

	
	public DevPanelControl.Value AddLabel (string identifier) {
		return panelRight_.AddLabelControl (identifier, this, labelHeight);
	}

	public DevPanelControl.Value AddSlider (string identifier) {
		return panelRight_.AddSliderControl (identifier, this, sliderHeight);
	}

	public DevPanelControl.Value AddToggle (string identifier) {
		return panelRight_.AddToggleControl (identifier, this, toggleHeight);
	}

	public void RemoveControl (DevPanelControl.Value value) {
		panelRight_.RemoveControl (value);
	}
}
