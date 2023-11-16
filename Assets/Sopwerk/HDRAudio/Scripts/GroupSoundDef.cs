using UnityEngine;
using UnityRandom = UnityEngine.Random;
using System;
using System.Linq;
using Sopwerk.Utils;


namespace Sopwerk.HDRAudio
{
	/// <summary>
	/// Concrete definition of a multi-clip sound: 
	/// each time this sound is played, a new clip is randomly selected from the array of clips.
	/// </summary>
	public class GroupSoundDef : SoundDef
	{
		/// <summary>
		/// Base pitch +/- variations
		/// </summary>
		[Header("Audio Group")]
		[SerializeField]
		private MinMaxValue _pitchVariation = null;
		
		/// <summary>
		/// A clip will be choosen to play randomly from this list
		/// </summary>
		[SerializeField]
		private AudioClip[] _clips = null;

		/// <summary>
		/// Represents RMS curve for the sound. Used to compute perceived loudness.
		/// The curve can be generated for the given sound clip in the inspector GUI 
		/// (see SingleSoundDefEditor) and/or edited manually.
		/// </summary>
		[HideInInspector]
		[SerializeField]
		private AnimationCurve[] _rmsCurves = null;


		public AudioClip[] Clips 
		{ 
			get { return _clips; } 
		}

		public AnimationCurve[] RmsCurves
		{ 
			get { return _rmsCurves; } 
			set { _rmsCurves = value; } 
		}

		public override void InitAudioSource(AudioSource audioSource, ref AnimationCurve rmsCurve)
		{
			base.InitAudioSource(audioSource, ref rmsCurve);
			
			// pitch can be sligtly changed so the clip in the croup does not sound the same
			audioSource.pitch += _pitchVariation.Random();												// max is _inclusive_ for floats

			var clipIndex = UnityRandom.Range(0, _clips.Length);										// max is _exclusive_ for ints
			audioSource.clip = _clips[clipIndex];

			rmsCurve = _rmsCurves != null && _rmsCurves.Length > 0? _rmsCurves[clipIndex] : null;
		}

		public override string ToString ()
		{
			return string.Format ("[GroupSoundDef]: {0}", string.Join(", ", _clips.Select(x => x.name).ToArray()));
		}
	}
}
