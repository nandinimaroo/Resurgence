using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Sopwerk.HDRAudio
{
	/// <summary>
	/// The RMS curve recorder for a SingleSoundDef class.
	/// </summary>
	[CustomEditor(typeof(SingleSoundDef))]
	public class SingleSoundDefEditor : SoundDefEditor
	{
		private SingleSoundDef SoundDef
		{
			get { return (SingleSoundDef)target; }
		}

		protected override AudioClip[] Clips
		{
			get { 
				return SoundDef.Clip != null ? new AudioClip[] { SoundDef.Clip } : null; 
			}
		}

		protected override AnimationCurve[] ResultCurves
		{
			get { 
				return SoundDef.RmsCurve != null ? new AnimationCurve[] { SoundDef.RmsCurve } : null; 
			}
			set {
				SoundDef.RmsCurve = value[0];
			}
		}

		protected override void ClearRmsCurves()
		{
			SoundDef.RmsCurve = null;
		}
	}


	/// <summary>
	/// The RMS curve recorder for a GroupSoundDef class.
	/// </summary>
	[CustomEditor(typeof(GroupSoundDef))]
	public class GroupSoundDefEditor : SoundDefEditor
	{
		private GroupSoundDef SoundDef
		{
			get { return (GroupSoundDef)target; }
		}
		
		protected override AudioClip[] Clips
		{
			get { return SoundDef.Clips; }
		}
		
		protected override AnimationCurve[] ResultCurves
		{
			get { return SoundDef.RmsCurves; }
			set { SoundDef.RmsCurves = value;	}
		}

		protected override void ClearRmsCurves()
		{
			SoundDef.RmsCurves = null;
		}
	}

	/// <summary>
	/// Custom editor for the SoundDef class.
	/// Implements RMS recording feature.
	/// </summary>
	public abstract class SoundDefEditor : Editor 
	{
		private SoundRmsRecorder _rmsRecorder = new SoundRmsRecorder();


		protected abstract AudioClip[] Clips 
		{ 
			get;
		}

		protected abstract AnimationCurve[] ResultCurves	
		{ 
			get; set;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			EditorGUILayout.Separator();
			var soundDef = (SoundDef)target;
			soundDef.RmsCurveEnabled = EditorGUILayout.ToggleLeft(" RMS Curves", soundDef.RmsCurveEnabled, EditorStyles.boldLabel);
			
			// disable RMS recording GUI
			GUI.enabled = !EditorApplication.isPlaying && CanRecordRmsValues;

			HorizontalControl(() => {
				// Record buttons 
				if (GUILayout.Button("Record RMS Curves")) {
					ResultCurves = _rmsRecorder.Record(Clips);

					// just to make sure that GUI repainted once the recording is done (e.g. button are enabled back, etc)
					Repaint();
					EditorUtility.SetDirty(target);			// save changes to target
				}
				// Clear buttons 
				if (GUILayout.Button("Clear RMS Curves")) {
					ClearRmsCurves();
					EditorUtility.SetDirty(target);			// save changes
				}
			});

			// Normalize curve
			ControlWithLabel("Normalize Curve", "Normalize curve values to fall into the [0, 1] range.", () => {
				_rmsRecorder.ShouldNormalizeCurve = EditorGUILayout.Toggle(_rmsRecorder.ShouldNormalizeCurve);
			});

			// Flatten curve attack
			ControlWithLabel("Flatten Curve Attack", "Flatten beginning of the curve by setting it's values equal to the curve's max value.", () => {
				_rmsRecorder.ShouldFlatenCurveAttack = EditorGUILayout.Toggle(_rmsRecorder.ShouldFlatenCurveAttack);
			});

			// Sampling rate
			ControlWithLabel("Samples per sec.", "Number of times per second the RMS value is computed for the audio clip.", () => {
				_rmsRecorder.RecordingRate = EditorGUILayout.IntSlider(_rmsRecorder.RecordingRate, SoundRmsRecorder.MinRecordingRate, SoundRmsRecorder.MaxRecordingRate);
			});

			// RMS curves 
			if (ResultCurves != null) {
				for (var i = 0; i < ResultCurves.Length; i++)
					ResultCurves[i] = EditorGUILayout.CurveField(ResultCurves[i]);
			}

			GUI.enabled = true;

			// Make sure that curve changes will be persited. Without this, the CurveField will not save changes.
			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}

		private bool CanRecordRmsValues
		{
			get {
				var soundDef = (SoundDef)target;
				return soundDef.RmsCurveEnabled && !soundDef.BypassHDR && Clips != null && Clips.Length > 0;
			}
		}

		private void ControlWithLabel(string label, string tooltip, Action body)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
			body();
			EditorGUILayout.EndHorizontal();
		}

		private void HorizontalControl(Action body)
		{
			EditorGUILayout.BeginHorizontal();
			body();
			EditorGUILayout.EndHorizontal();
		}
		
		protected abstract void ClearRmsCurves();


		//
		// Helps to record RMS values for the sound clips.
		//
		private class SoundRmsRecorder
		{
			public const int MinRecordingRate = 1;
			public const int MaxRecordingRate = 100;

			public bool ShouldNormalizeCurve
			{
				get; set;
			}

			public bool ShouldFlatenCurveAttack
			{
				get; set;
			}
			
			/// <summary>
			/// Number of recording taken pro second. 
			/// </summary>
			public int RecordingRate
			{
				get; set;
			}
			
			public SoundRmsRecorder()
			{
				ShouldNormalizeCurve = true;
				ShouldFlatenCurveAttack = true;
				RecordingRate = 10;
			}

			public AnimationCurve[] Record(AudioClip[] clips)
			{
				var resultCurves = new AnimationCurve[clips.Length];

				for(var i = 0; i < clips.Length; i++)
					resultCurves[i] = Record(clips[i]);

				return resultCurves;
			}

			private AnimationCurve Record(AudioClip clip)
			{
				var resultCurve = new AnimationCurve();

				var rmsRecordRate = 1f/RecordingRate;
				var rmsSampleCount = Mathf.CeilToInt(clip.frequency * rmsRecordRate);

				// the buffer, even if it covers only part of the clip, should be of size to include saples from all chaneels.
				var rmsBuffer = new float[rmsSampleCount * clip.channels];
				var hasRmsValues = false;

				for (var offset = 0; offset < clip.samples; offset += rmsSampleCount) {
					var rmsValue = AddRmsValueToCurve(rmsBuffer, resultCurve, clip, offset, rmsSampleCount);
					hasRmsValues |= rmsValue != 0;			// used to check if the data was retrieved from the clip at all
				}

				PostProcessCurve(resultCurve);

				Debug.Log(string.Format("RecordRate={0}, SampleRate={1}, Buffer={2}, ClipLen={3} ({4}), CurveLen={5} ({6})", rmsRecordRate, clip.frequency, rmsSampleCount*clip.channels, clip.samples, clip.length, resultCurve.length, clip.samples/rmsSampleCount));

				if (!hasRmsValues)
					Debug.LogError("Can not read data from the audio clip. Make sure that the clip load type is set to \"Load into memory\" when recording the RMS curve.");

				return resultCurve;
			}

			private float AddRmsValueToCurve(float[] buffer, AnimationCurve resultCurve, AudioClip clip, int offset, int rmsSampleCount)
			{
				// buffer size should be adjusted when at the end of the audio clip
				var bufferSize = Mathf.Min(clip.samples - offset, rmsSampleCount) * clip.channels;
							
				// clip's GetData, for some reason, requires exact buffer size when reading last segment of the samples, or it will read some extra values (wrapping to start of the clip?)
				if (buffer.Length != bufferSize)
					buffer = new float[bufferSize];
				
				clip.GetData(buffer, offset);
				
				// we just include all channels into RMS computations (sounds about right?)
				var rmsValue = ComputeRmsValue(buffer, bufferSize);
				var time = (float)offset / clip.frequency;

				resultCurve.AddKey(new Keyframe(time, rmsValue, 0, 0));

				return rmsValue;
			}

			private float ComputeRmsValue(float[] buffer, int bufferSize)
			{
				var sum = 0f;
				for (var i = 0; i < bufferSize; i++)
					sum += buffer[i] * buffer[i];
				
				return Mathf.Sqrt(sum / bufferSize);
			}

			private void PostProcessCurve(AnimationCurve curve)
			{
				if (ShouldNormalizeCurve)
					NormalizeCurve(curve);

				if (ShouldFlatenCurveAttack)
					FlattenCurveAttack(curve);
			}
			
			private void NormalizeCurve(AnimationCurve curve)
			{
				var maxIndex = GetMaxKeyIndex(curve.keys);
				if (maxIndex < 0) 
					return;

				var maxValue = curve.keys[maxIndex].value;
				var keys = curve.keys;

				for (var i = 0; i < keys.Length; i++)
					keys[i].value = keys[i].value / maxValue;
				
				curve.keys = keys;
			}

			private void FlattenCurveAttack(AnimationCurve curve)
			{
				var maxIndex = GetMaxKeyIndex(curve.keys);
				if (maxIndex < 1) 
					return;
				
				var maxValue = curve.keys[maxIndex].value;

				var keys = new List<Keyframe>(curve.keys);
				keys.RemoveRange(0, maxIndex);
				keys.Insert(0, new Keyframe(0, maxValue, 0, 0));

				curve.keys = keys.ToArray();
			}

			private int GetMaxKeyIndex(Keyframe[] keys)
			{
				var maxIndex = -1;
				var maxValue = float.MinValue;

				for (var i = 0; i < keys.Length; i++) {
					if (maxValue < keys[i].value) {
						maxValue = keys[i].value;
						maxIndex = i;
					}
				}

				return maxIndex;
			}
		}
	}
}
