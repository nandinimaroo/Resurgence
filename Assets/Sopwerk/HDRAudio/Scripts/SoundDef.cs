using UnityEngine;
using System;


namespace Sopwerk.HDRAudio
{
	public enum SoundPriority
	{
		None,					// 250
		Low,					// [170-250[
		Medium,					// [90-170[
		High,					// [10-90[
		Permanent				// 9
	}

	/// <summary>
	/// Base definition of the HDR sound. 
	/// The main parameter is "loudness", which is used by HDR routines to compute perceived loudness of the sound.
	/// 
	/// The individual HDR sound can be defined as object on the Scene or as a prefab in the project. 
	/// </summary>
	public abstract class SoundDef: MonoBehaviour
	{
		/// <summary>
		/// If this flag is set, the loudness of this sound will not be affected by the HDR computations and remain constant.
		/// </summary>
		[Header("HDR")]
		[SerializeField]
		private bool _bypassHDR = false;

		[SerializeField]
		private SoundPriority _priority = SoundPriority.None;

		/// <summary>
		/// "Real-world" loudness of the sound in dB(SPL).
		/// Note that if the BypassHDR flag is true, the loudnes is relative to the top of HDR window.
		/// </summary>
		[Tooltip("Loudness of the sound in dB(SPL). Note that if the bypassHDR flag is true, this value is relative to the top of HDR window.")]
		[SerializeField]
		private float _loudness = 0;

		/// <summary>
		/// A modificator that can be defined to modify loudness when the audio source is near the main character.
		/// For example, a loudness of impact hitting the player can be reduced, 
		/// but the same impact hitting enemy will play with unaltered loudness.
		/// </summary>
		[Header("Near Player Mods")]
		[Tooltip("Loudness of the sound originating from the player's character will be modified by specified value.")]
		[SerializeField]
		private float _nearPlayerLoudnessMod = 0;

		/// <summary>
		/// If this flag is set and the sound is originating from the player's character, 
		/// the loudness will be adjusted relativelly to the HDR window top, 
		/// so the sound will remain constant and will not be affected by the HDR computations.
		/// This can be used to keep loudness of the own character sounds always the same. 
		/// For instance, without this flag, the sound of gun fire will be affected by the HDR window, which can be quite notisable.
		/// Setting the flag will make character's gun fire sound with a constant loudness.
		/// </summary>
		[Tooltip("Loudness of the sound originating from the player's character will not be affected by the HDR window position.")]
		[SerializeField]
		private bool _nearPlayerFixedLoudness = false;

		/// <summary>
		/// Defines how frequently the audio clips can be played.
		/// Note that this property is not used directly by the HDR audio manager.
		/// It is just stored together with sound definition for convenience and should be used by external utilities.
		/// </summary>
		[Header("Misc")]
		[Tooltip("Defines how frequently the audio clips can be played.")]
		[SerializeField]
		private float _repeatRate = -1f;

		/// <summary>
		/// With Unity 5, the 3D flag was moved from the clip import definition to the AudioSource. 
		/// </summary>
		[Header("Audio Source")]
		[SerializeField]
		private bool _is3D = true;

		[SerializeField]
		private bool _ignoreListenerPause = false;
		
		/// <summary>
		/// The sound is either an infinite loop or a short "one-shot" sound. 
		/// </summary>
		[SerializeField]
		private bool _isLoop = false;
		
		/// <summary>
		/// Influences resulting logarithmic roloff curve, similar to how MinDistance works in Unity:
		/// bigger value makes curve less steep
		/// </summary>
		[SerializeField]
		private float _minDistance = 1;
		
		[SerializeField]
		private float _maxDistance = 500;
		
		[Range(-3, 3)]
		[SerializeField]
		private float _pitch = 1;
		
		[Range(0, 5)]
		[SerializeField]
		private float _dopplerLevel = 0;

		[SerializeField]
		[HideInInspector]
		private bool _rmsCurveEnabled = true;
		

		public bool BypassHDR 
		{ 
			get { return _bypassHDR; } 
		}

		public SoundPriority Priority 
		{ 
			get { return _priority; } 
		}

		public float Loudness 
		{ 
			get { return _loudness; } 
		}

		public float NearPlayerLoudnessMod 
		{ 
			get { return _nearPlayerLoudnessMod; } 
		}

		public bool NearPlayerFixedLoudness 
		{ 
			get { return _nearPlayerFixedLoudness; } 
		}

		public float RepeatRate 
		{ 
			get { return _repeatRate; } 
		}

		public bool IsLoop
		{ 
			get { return _isLoop; } 
		}
		
		public float MinDistance 
		{ 
			get { return _minDistance; } 
		}

		public float MaxDistance 
		{ 
			get { return _maxDistance; } 
		}

		public bool RmsCurveEnabled 
		{ 
			get { return _rmsCurveEnabled; } 
			set { _rmsCurveEnabled = value; } 
		}


		/// <summary>
		/// Init the audio source with parameters from this sound definition.
		/// </summary>
		public virtual void InitAudioSource(AudioSource audioSource, ref AnimationCurve rmsCurve)
		{
			audioSource.spatialBlend = _is3D? 1: 0;
			audioSource.ignoreListenerPause = _ignoreListenerPause;
			audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
			audioSource.minDistance = _minDistance;
			audioSource.maxDistance = _maxDistance;
			audioSource.pitch = _pitch;
			audioSource.dopplerLevel = _dopplerLevel;
			audioSource.loop = _isLoop;
			audioSource.playOnAwake = false;
			audioSource.mute = false;
		}
	}
}
