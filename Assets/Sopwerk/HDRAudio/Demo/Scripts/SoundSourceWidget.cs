using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

namespace Sopwerk.HDRAudio.Demo
{
	/// <summary>
	/// Widget that represents sound source on the scene.
	/// </summary>
	public class SoundSourceWidget : WidgetBase
	{
		private bool _isActive;
		private HDRAudio.PoolItem _audioPoolItem;
		private RepeatingSoundLimiter _repeatLimiter = new RepeatingSoundLimiter();

		[Header("HDR Audio")]
		[SerializeField]
		private SoundDef _soundDef = null;


		/// <summary>
		/// Returns currently playing HDR Audio pool item.
		/// </summary>
		public HDRAudio.PoolItem AudioPoolItem
		{
			get { return IsActive? _audioPoolItem : null;	}
		}

		protected override void Update()
		{
			base.Update();

			// will repeatedly play the sound for which the repeatRate is defined
			if (IsActive && IsRepeatingSound)
				RepeatPlaySound();
		}

		protected override bool IsActive 
		{
			get {
				// Deactivate widget once sound stops playing.
				// Note that the repeating sound or loop can only be switched off by clicking on the widget.
				if (!IsRepeatingSound && !_soundDef.IsLoop && !IsSoundPlaying)
					_isActive = false;

				return _isActive;
			}
		}

		protected override void OnClick()
		{
			if (!IsActive)
				PlaySound(true);
			else
				StopSound();

			_isActive = !_isActive;
		}

		
		private bool IsSoundPlaying 
		{
			get { 
				// Note that the audio pool item will be re-used to play another sound, once it stops playing.
				// The check of the SoundDef property used to find out if the item is still "ours"
				return _audioPoolItem != null && _audioPoolItem.SoundDef == _soundDef && _audioPoolItem.IsPlaying;
			}
		}

		private bool IsRepeatingSound
		{
			get { return _soundDef.RepeatRate > 0; }
		}

		private float AudioClipLength
		{
			get { return _audioPoolItem != null? _audioPoolItem.AudioSource.clip.length : 0; }
		}

		private void PlaySound(bool resetRepeatLiniter)
		{
			if (_soundDef == null)
				return;

			if (resetRepeatLiniter)
			    _repeatLimiter.ResetTimer();

			// play method returns new pool item on each invocation
			_audioPoolItem = HDRAudio.Instance.Play(_soundDef, _audioObject.transform.position, _audioObject.transform);
		}

		private void RepeatPlaySound()
		{
			// Reset limiter if the sound clip is shorter then the repeat rate limit,
			// so there will be no pauses between repetitions.
			if (!IsSoundPlaying && _soundDef.RepeatRate > AudioClipLength) {
				PlaySound(true);
				return;
			}

			// limit sound play repetitions, if repeat rate was defined for this sound
			if (_repeatLimiter.CanRepeatPlay(_soundDef.RepeatRate, true))
				PlaySound(false);
		}
		
		private void StopSound()
		{
			if (_audioPoolItem != null) {
				_audioPoolItem.Stop();
				_audioPoolItem = null;
			}
		}
	}
}
