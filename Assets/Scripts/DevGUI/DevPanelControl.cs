using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DevPanelControl {
	public enum Type {
		None,
		Label,
		Slider,
		Toggle
	}

	public class Value {
		virtual public float Float { set {} get { return 0f; } }

		virtual public float MinFloat { set {} get { return 0f; } }
		virtual public float MaxFloat { set {} get { return 0f; } }

		virtual public bool  Bool { set {} get { return false; } }
	}

	public class LabelValue : Value {
		override public float Float {
			set { valueText_.text = value.ToString (); }
			get { return float.Parse (valueText_.text);	}
		}

		public LabelValue (UnityEngine.UI.Text valueText) {
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
			text_.text = string.Format ("{0}: {1} [{2}, {3}]", name_, this.Float, this.MinFloat, this.MaxFloat);
		}

		public SliderValue (UnityEngine.UI.Slider slider, UnityEngine.UI.Text text, string name) {
			slider_ = slider;
			text_ = text;
			name_ = name;

			slider_.onValueChanged.AddListener (value => { UpdateText (); });
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
		}
		private UnityEngine.UI.Toggle toggle_;
	}

	public Value					value_;
	public GameObject				go_;
}
