using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Sopwerk.HDRAudio
{
	public partial class HDRAudio : MonoBehaviour 
	{
		/// <summary>
		/// The run-time structure for the audio source, the item of the audio pool.
		/// Implement methods used in the HDR calculations.
		/// </summary>
		public class PoolItem
		{
			private HDRAudio _hdrAudio;
			private float _lastFrameCount;
			private SoundDef _soundDef;
			private AnimationCurve _soundRmsCurve;
			private bool _isActive = true;
			private bool _isSuspended;
			private bool _shouldBePlaying;
			private bool _isFixedLoudness;
			private bool? _isCloseToMainCharacterCache;


			public AudioSource AudioSource 
			{ 
				get; private set; 
			}

			public float DistanceToListener 
			{ 
				get; private set; 
			}

			/// <summary>
			/// Returns the minimum distance where the sound is audible without attenuation.
			/// By setting min distance equal to the max distance one can disable rolloff altogether.
			/// </summary>
			public float MinDistance
			{
				get { return AudioSource.minDistance; }
			}

			/// <summary>
			/// Effective loudness of the audio source. 
			/// It is initialized from the SoundDef structure, but can be changed later.
			/// </summary>
			public float Loudness
			{
				get; set;
			}

			/// <summary>
			/// Optimal modificator of the base loudness.
			/// Can be used to tone down sounds which are too close from the player.
			/// </summary>
			public float LoudnessMod
			{
				get; private set;
			}

			/// <summary>
			/// Loudness at the listener's position.
			/// </summary>
			public float PerceivedLoudness 
			{ 
				get; private set; 
			}
				
			public SoundDef SoundDef 
			{ 
				get { return _soundDef; }
			}

			public string ClipName
			{
				get { return AudioSource.clip.name; }
			}
			
			public bool IsLoop
			{
				get { return AudioSource.loop; }
			}

			/// <summary>
			/// Makes this sound to be ignored by the HDR algorithm.
			/// As a result, the sound will have a constant loudness, and it would not affect current total loudness of the HDR manager.
			/// </summary>
			public bool BypassHDR
			{
				get { return _soundDef.BypassHDR || _isFixedLoudness; }
			}
			
			/// <summary>
			/// Indicates that the pool item is not active anymore and so it can be recycled.
			/// Note that it's possible that the pool item is still active but the associated AudioSource was destroyed
			/// (e.g. if the AudioSource was parented to the GO which got destroyed)
			/// </summary>
			public bool IsActive 
			{ 
				get { return _isActive && AudioSource != null; } 

				set { 
					_isActive = value;
					if (AudioSource != null)
						AudioSource.gameObject.SetActive(value); 
				}
			}

			public bool IsMarkedAsActive
			{
				get { return _isActive; }
			}

			/// <summary>
			/// Suspend sound by pausing it and setting its priority to the max value.
			/// </summary>
			public bool IsSuspended
			{
				get { return _isSuspended; }

				set {
					if (!AudioSource.loop)
						Debug.LogWarning("Only loop sound can be suspended or unsuspended: " + ClipName);

					if (value) {
						if (AudioSource.isPlaying)
							AudioSource.Stop();
					}
					else {
						// resume suspended sound
						if (!AudioSource.isPlaying && ShouldBePlaying) 
							AudioSource.Play();
					}

					_isSuspended = value;
				}
			}

			public bool IsPlaying
			{
				// check is the audioSource was not destroyed outside of the pool
				get { return AudioSource != null && AudioSource.isPlaying; }
			}

			public bool ShouldBePlaying
			{
				get { return _shouldBePlaying && IsLoop; }
			}


			public PoolItem(HDRAudio hdrAudio)
			{
				_hdrAudio = hdrAudio;
			}

			/// <summary>
			/// Initialize (re-initialize) pool item.
			/// </summary>
			public void Init(SoundDef soundDef, Vector3 position, Transform parent, bool hasRolloff)
			{
				_lastFrameCount = 0;		// should reset or we may get unwantd stale computation if the item gets reused in the same frame
				_soundDef = soundDef;
				_isSuspended = false;
				_shouldBePlaying = false;

				if (AudioSource == null)
					AudioSource = CreateAudioSource();
				_soundDef.InitAudioSource(AudioSource, ref _soundRmsCurve);

				// position (always in world coordinaes) should be set before the parent is assigned
				AudioSource.transform.position = position;
				AudioSource.transform.SetParent(parent);

				// by setting min distance equal to the max distance we can create an audio source without rolloff
				AudioSource.minDistance = hasRolloff? soundDef.MinDistance : soundDef.MaxDistance;
				Loudness = soundDef.Loudness;

				// used to reduce loudness if the sound source position is too close to the character currently followed by the main camera
				LoudnessMod = GetNearMainChanracterLoudnessMod();

				// the fixed loudness can be used for sounds playing too close to player to keep them constant
				_isFixedLoudness = _soundDef.NearPlayerFixedLoudness && IsCloseToMainCharacter;
			}

			private AudioSource CreateAudioSource()
			{
				var obj = new GameObject("AudioPoolItem");
				obj.hideFlags = HideFlags.HideInHierarchy;
				return obj.AddComponent<AudioSource>();
			}

			public void Play()
			{
				if (!IsSuspended)
					AudioSource.Play();

				_shouldBePlaying = true;
			}

			public void Pause()
			{
				AudioSource.Pause();
				_shouldBePlaying = false;
			}

			public void Stop()
			{
				AudioSource.Stop();
				_shouldBePlaying = false;
			}
			
			/// <summary>
			/// Recompute properties (e.g. distance, perceived loudness) of the item for the given frame.
			/// </summary>
			public void Update()
			{
				// calculate only once per frame
				if (_lastFrameCount == Time.frameCount)
					return;
				_lastFrameCount = Time.frameCount;

				InvalidateCachedValues();
				
				DistanceToListener = Vector3.Distance(AudioSource.transform.position, _hdrAudio.AudioListener.transform.position);

				// The initial value is computed during the PoolItem init. For the loops, it has to be be updated here as well, 
				// in order to cover the case when the camera switches to another vehicle.
				if (IsLoop) {
					// Reduce loudness if the sound source position is too close to the character currently followed by the main camera.
					LoudnessMod = GetNearMainChanracterLoudnessMod();

					// the fixed loudness can be used for sounds playing too close to player to keep them constant
					_isFixedLoudness = _soundDef.NearPlayerFixedLoudness && IsCloseToMainCharacter;
				}

				var loudness = ComputeRealLoudness() + LoudnessMod;

				PerceivedLoudness = ComputePerceivedLoudness(loudness, MinDistance, DistanceToListener);

				if (_soundDef.RmsCurveEnabled)
					PerceivedLoudness += ComputeLoudnessRms();
			}

			private float ComputeRealLoudness()
			{
				// The loudness for "bypassed" or "fixed" sounds is relative value with the base 0. 
				// Therefore it has to be summed up with the current HDR window in order to get correct loudness.
				if (_soundDef.BypassHDR)
					return Loudness + _hdrAudio.WindowTop;

				// The loudness is adjusted relativelly to the HDR window top, so the sound will remain constant and will not be affected by the HDR.
				// This can be used for sounds playing too close to player to keep them constant.
				if (_isFixedLoudness)
					return Loudness + (_hdrAudio.WindowTop - _hdrAudio._minWindowTop);

				return Loudness;
			}

			private float ComputePerceivedLoudness(float baseLoudness, float minDistance, float distance)
			{
				distance = Mathf.Max(minDistance, distance);						// clamp to min distance

				// Change of loudness depending on distance (from Wikipedia).
				// Note that minDistance influences curve steepness.
				// Note that the result can be negative, if base loudness is less then aprox. 54dB over max distance 500
				return baseLoudness + 20 * Mathf.Log10(minDistance / distance);
			}

			/// <summary>
			/// Reflects momentary change in loudness (based on output RMS) of the sound allowing for more precise calculation.
			/// </summary>
			private float ComputeLoudnessRms()
			{
				if (IsRmsCurveEmpty)
					return 0;
				
				var rmsValue = EvaluateRmsCurve(_soundRmsCurve, AudioSource.clip, AudioSource.timeSamples);
				return rmsValue > 0 ? 20 * Mathf.Log10(rmsValue) : -160;
			}

			private bool IsRmsCurveEmpty
			{
				get { return _soundRmsCurve == null || _soundRmsCurve.length == 0; }
			}

			/// <summary>
			/// Returns an RSM value for the given position in the clip.
			/// </summary>
			private float EvaluateRmsCurve(AnimationCurve rmsCurve, AudioClip clip, int timeSamples)
			{
				var curveTime = rmsCurve[rmsCurve.length-1].time;
				var curveRate = curveTime / (float)clip.samples;
				
				// use timeSamples as they are independent from the sound compression.
				return rmsCurve.Evaluate(curveRate * timeSamples);
			}

			private float GetNearMainChanracterLoudnessMod()
			{
				return _soundDef.NearPlayerLoudnessMod != 0 && IsCloseToMainCharacter? _soundDef.NearPlayerLoudnessMod : 0;
			}

			/// <summary>
			/// Returns true if the given position is close (e.g. close to bounds radius) 
			/// to the character currently observed by the main camera.
			/// </summary>
			private bool IsCloseToMainCharacter
			{
				get {
					if (!_isCloseToMainCharacterCache.HasValue)
						_isCloseToMainCharacterCache = _hdrAudio.IsCloseToMainCharacter(AudioSource.transform.position);

					return _isCloseToMainCharacterCache.Value;
				}
			}

			private void InvalidateCachedValues()
			{
				_isCloseToMainCharacterCache = null;
			}
		}
	}
}
