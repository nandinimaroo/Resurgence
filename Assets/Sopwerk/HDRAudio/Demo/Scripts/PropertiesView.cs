using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace Sopwerk.HDRAudio.Demo
{
	public class PropertiesView : MonoBehaviour 
	{
		[Header("HDR Properties")]
		[SerializeField]
		private Text _currentLoudness = null;

	    [SerializeField]
	    private Text _hdrWindowTop = null;

		[SerializeField]
		private Text _hdrWindowBottom = null;

		[Header("HDR Configuration")]
		[SerializeField]
		private SliderControl _hdrMinWindowTop = null;

		[SerializeField]
		private SliderControl _hdrWindowSize = null;

		[SerializeField]
		private SliderControl _hdrReleaseTime = null;

		[Header("Selected Sound")]
		[SerializeField]
		private Text _clipName = null;

		[SerializeField]
		private Text _distance = null;

		[SerializeField]
		private Text _loudness = null;

		[SerializeField]
		private Text _percievedLoudness = null;

		[SerializeField]
		private Text _audioVolume = null;


		[Serializable]
		public class SliderControl
		{
			[SerializeField]
			public Slider _slider;

			[SerializeField]
			public Text _value;
		}


		protected void Start()
		{
			var hdrAudio = HDRAudio.Instance;

			InitSlider(_hdrMinWindowTop._slider, hdrAudio.MinWindowTop, (v) => { hdrAudio.MinWindowTop = v; });
			InitSlider(_hdrWindowSize._slider, hdrAudio.WindowSize, (v) => { hdrAudio.WindowSize = v; });
			InitSlider(_hdrReleaseTime._slider, hdrAudio.ReleaseTime, (v) => { hdrAudio.ReleaseTime = v; });
		}

		private void InitSlider(Slider slider, float initalValue, Action<float> valueChangedCallback)
		{
			if (slider == null)
				return;
			
			slider.value = initalValue;
			slider.onValueChanged.AddListener((v) => valueChangedCallback(v));
		}

		protected void Update() 
		{
			var hdrAudio = HDRAudio.Instance;

			UpdateText(_currentLoudness, hdrAudio, "{0:n2}", p => p.CurrentLoudness);
			UpdateText(_hdrWindowTop, hdrAudio, "{0:n2}", p => p.WindowTop);
			UpdateText(_hdrWindowBottom, hdrAudio, "{0:n2}", p => p.WindowTop-p.WindowSize);

			UpdateText(_hdrMinWindowTop._value, hdrAudio, "{0:n2}", p => p.MinWindowTop);
			UpdateText(_hdrWindowSize._value, hdrAudio, "{0:n2}", p => p.WindowSize);
			UpdateText(_hdrReleaseTime._value, hdrAudio, "{0:n2}", p => p.ReleaseTime);

			var audioPoolItem = GetSelectedAudioPoolItem();
			UpdateText(_clipName, audioPoolItem, "{0}", p => "\""+p.ClipName+"\"");
			UpdateText(_distance, audioPoolItem, "{0:n2}", p => p.DistanceToListener);
			UpdateText(_loudness, audioPoolItem, "{0:n2}", p => p.Loudness);
			UpdateText(_percievedLoudness, audioPoolItem, "{0:n2}", p => p.PerceivedLoudness);
			UpdateText(_audioVolume, audioPoolItem, "{0:n2}", p => p.AudioSource.volume);
		}

		private void UpdateText<T, U>(Text textControl, T obj, string format, Func<T, U> valueFunc)
		{
			if (textControl != null) {
				textControl.text = string.Format(format, obj != null? valueFunc(obj) : default(U));

				var color = textControl.color;
				color.a = obj != null? 1 : 0.5f;
				textControl.color = color;
			}
		}

		private HDRAudio.PoolItem GetSelectedAudioPoolItem()
		{
			var selected = EventSystem.current.currentSelectedGameObject;
			if (selected == null)
				return null;

			var widget = selected.GetComponent<SoundSourceWidget>();
			if (widget == null)
				return null;

			return widget.AudioPoolItem;
		}
	}
}
