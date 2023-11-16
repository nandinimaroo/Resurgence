using UnityEngine;
using System;

namespace Sopwerk.HDRAudio
{
	/// <summary>
	/// Concrete definition of a single sound.
	/// </summary>
	public class SingleSoundDef : SoundDef
	{
		[SerializeField]
		private AudioClip _clip = null;

		/// <summary>
		/// Represents RMS curve for the sound. Used to compute perceived loudness.
		/// The curve can be computed for the given sound clip in the inspector GUI 
		/// (see SingleSoundDefEditor) and/or edited manually.
		/// </summary>
		[HideInInspector]
		[SerializeField]
		private AnimationCurve _rmsCurve = null;


		public AudioClip Clip
		{ 
			get { return _clip; } 
		}

		public AnimationCurve RmsCurve 
		{ 
			get { return _rmsCurve; } 
			set { _rmsCurve = value; } 
		}

		public override void InitAudioSource(AudioSource audioSource, ref AnimationCurve rmsCurve)
		{
			base.InitAudioSource(audioSource, ref rmsCurve);
			
			audioSource.clip = _clip;
			rmsCurve = _rmsCurve;
		}

		public override string ToString ()
		{
			return string.Format("[SimpleSoundDef]: {0}", _clip != null? _clip.name : "?");
		}
	}
}
