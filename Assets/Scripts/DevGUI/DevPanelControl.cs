using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DevPanelControl {
	public enum Type {
		Empty,
		Label,
		Readonly,
		Slider,
		Toggle,
		Button
	}

	public class Value {
		virtual public float Float { set {} get { return 0f; } }

		virtual public float MinFloat { set {} get { return 0f; } }
		virtual public float MaxFloat { set {} get { return 0f; } }

		virtual public bool  Bool { set {} get { return false; } }

		public delegate void OnChangedHandler ();
		public event OnChangedHandler OnChanged;

		protected void FireOnChangeEvent () {
			if (OnChanged != null) {
				OnChanged ();
			}
		}
	}

	public class ReadonlyValue : Value {
		override public float Float {
			set { valueText_.text = value.ToString (); }
			get { return float.Parse (valueText_.text);	}
		}

		public ReadonlyValue (UnityEngine.UI.Text valueText) {
			valueText_ = valueText;
		}
		private UnityEngine.UI.Text valueText_;
	}

	public class SliderValue : Value {
		override public float Float {
			set { slider_.value = value; UpdateText (); }
			get { return slider_.value; }
		}
		
		override public float MinFloat {
			set { slider_.minValue = value;	UpdateText (); }
			get { return slider_.minValue; }
		}

		override public float MaxFloat {
			set { slider_.maxValue = value; UpdateText (); }
			get { return slider_.maxValue; }
		}

		public void UpdateText () {
			text_.text = string.Format ("{0}: {1:0.000} [{2}, {3}]", name_, this.Float, this.MinFloat, this.MaxFloat);
		}

		public SliderValue (UnityEngine.UI.Slider slider, UnityEngine.UI.Text text, string name) {
			slider_ = slider;
			text_ = text;
			name_ = name;

			slider_.onValueChanged.AddListener (value => {
				UpdateText ();
				FireOnChangeEvent ();
			});
		}
		private UnityEngine.UI.Slider slider_;
		private UnityEngine.UI.Text text_;
		private string name_;
	}

	public class ToggleValue : Value {
		override public bool Bool {
			set { toggle_.isOn = value; }
			get { return toggle_.isOn; }
		}

		public ToggleValue (UnityEngine.UI.Toggle toggle) {
			toggle_ = toggle;
			toggle_.onValueChanged.AddListener (flag => {
				FireOnChangeEvent ();
			});
		}
		private UnityEngine.UI.Toggle toggle_;
	}

	public class ButtonValue : Value {
		public ButtonValue (UnityEngine.UI.Button button) {
			button.onClick.AddListener (() => {
				FireOnChangeEvent (); // HACK.. for now use the onchange event for button press!
			});
		}
	}

	public Value					value_; // object used to get/set the control's value
	public GameObject				go_; // the control prefab instance
}
