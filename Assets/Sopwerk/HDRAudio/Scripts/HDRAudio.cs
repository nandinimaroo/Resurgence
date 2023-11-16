using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sopwerk.Utils;


namespace Sopwerk.HDRAudio
{
	/// <summary>
	/// Implements the HDR audio algorithm similar to the one described here:
	/// 	http://dice.se/publications/how-high-dynamic-range-audio-makes-battlefield-bad-company-go-boom/
	/// </summary>
	public partial class HDRAudio : MonoBehaviour
	{
		private IList<PoolItem> _pool = new List<PoolItem>();
		private Stack<PoolItem> _inactive = new Stack<PoolItem>();
		private EvictionCandidatesList _evictionCandidates = new EvictionCandidatesList();
		private int _playingSoundCount;
		private IMainCharacterExtensions _mainCharacterExtensions;

		private float _currentLoudness;
		private TweenValue _windowTop;

		
		// Define audio source priority ranges and useful constants
		private const int DefaultPriority = 128;
		private const int LowestPriority = 255;
		private const int NonePriority = 250;
		private const int PermanentPriority = 9;
		private const int PriorityRangeTop = 10;	// inclusive
		private const int PriorityRangeStep = 80;	// 3 ranges for Low, Medium and High priorities


		/// <summary>
		/// Maximum size of the pool.
		/// </summary>
		[SerializeField]
		private int _poolMaxSize = 80;

		/// <summary>
		/// How frequently HDR properties are re-computed.
		/// </summary>
		[SerializeField]
		private float _updateRate = 0.2f;

		/// <summary>
		/// Minimum top value of the HDR "window"
		/// </summary>
		[Header("HDR")]
		[SerializeField]
		private float _minWindowTop = 100;

		/// <summary>
		/// The size of the HDR "window", e.g. 60dB - because typical range of the digital audio is 96dB and 30dB is practically silence.
		/// </summary>
		[SerializeField]
		private float _windowSize = 60;

		/// <summary>
		/// The time during which the HDR window eases out back to MinWindowsTop
		/// </summary>
		[SerializeField]
		private float _releaseTime = 1;

		public AudioListener AudioListener
		{
			get; private set;
		}

		public float CurrentLoudness
		{
			get { return _currentLoudness; }
		}

		public float WindowTop
		{
			get { return _windowTop.Value; }
		}

		public float MinWindowTop
		{
			get { return _minWindowTop; }
			set { _minWindowTop = value; }
		}
		
		public float WindowSize
		{
			get { return _windowSize; }
			set { _windowSize = value; }
		}

		public float ReleaseTime
		{
			get { return _releaseTime; }
			set { _releaseTime = value; }
		}

		private int ActiveItemCount
		{
			get { return _pool.Count - _inactive.Count; }
		}


		/// <summary>
		/// The HDRAudio service is a singleton. 
		/// A single instance of GameObject with attached HDRAudio script should be present in the scene.
		/// This property is a single access point to the HDRAudio service.
		/// </summary>
		public static HDRAudio Instance
		{
			get; private set;
		}

		protected void Awake()
		{
			// initialize HDRAudio service singleton access property.
			Instance = this;

			// pluggable extensions (optional!)
			_mainCharacterExtensions = GetComponent<IMainCharacterExtensions>();

			// should be only one (or none) in the scene
			AudioListener = GameObject.FindObjectOfType<AudioListener>();
			if (AudioListener == null)
				throw new Exception("Missing audio listener.");
			
			// Easing-out function for the window top release (e.g. change from high to lower value).
			_windowTop = new TweenValue(t => 1-(1-t)*(1-t)*(1-t));
		}

		protected void Start()
		{
			// The pool update does not happen on each frame. 
			// This is a tradeoff between HDR window responsiveness and resource consumption.
			//
			// NOTE that there is (still) this “curSource->m_Channel != NULL” bug in Unity, 
			// which manifests itself sometimes when the audio is updated from the inside of InvokeRepeated.
			// Using coroutines seems to be fine.
			StartCoroutine(UpdateRoutine());
		}

		private IEnumerator UpdateRoutine()
		{
			while (true) {
				yield return new WaitForSeconds(_updateRate);

				try {
					UpdateLoudnessAndHdrWindow();
					UpdateSoundVolumes();
				} 
				catch (Exception e) {
					// Do not exit the loop in case of errors! Otherwise the pool will start growing unconrollably.
					Debug.LogError(e);
				}
			}
		}

		private void UpdateLoudnessAndHdrWindow()
		{
			_playingSoundCount = 0;
			var loudnessAccumulator = new LoudnessAccumulator();

			// compute current loudness and mark idling pool items for recycling
			for (var i = 0; i < _pool.Count; i++) {
				UpdateItemLoudness(ref loudnessAccumulator, _pool[i]);
			}

			_currentLoudness = loudnessAccumulator.GetValue();
			UpdateHdrWindow();
		}

		private void UpdateItemLoudness(ref LoudnessAccumulator loudnessAccumulator, PoolItem item)
		{
			// the itme may be marked as active but have destroyed audio source
			if (!item.IsMarkedAsActive)
				return;

			// the item may still be reported as active but it was parented to the GO which has been destroyed.
			if (item.AudioSource == null) {
				Deactivate(item);
				return;
			}

			if (!item.IsPlaying) {
				// suspended item can be paused - if they are too far away or outside the HDR window.
				if (item.IsSuspended) {
					item.Update();					// needed here so the sound will be computed correctly in the 2nd loop
					return;
				}

				// When the Unity audio is getting overloaded, the Unity begins stopping sounds with the lowest priority.
				// The loop sound what got stopped should be restarted eventually in order to stay active.
				if (item.ShouldBePlaying) {
					item.IsSuspended = true;		// force-suspend sound so it will be reactivated automatically on the later iteration.
				}
				else {
					Deactivate(item);				// this pool item is a stopped one-shot that should be deactivated at this point
					return;
				}
			}

			item.Update();
			if (!item.BypassHDR)
				AddItemLoudness(ref loudnessAccumulator, item);
			_playingSoundCount++;
		}

		private void AddItemLoudness(ref LoudnessAccumulator loudnessAccumulator, PoolItem item)
		{
			loudnessAccumulator.Add(item.PerceivedLoudness);
		}
		
		private void UpdateSoundVolumes()
		{
			// prepare to count candidates that should be "evicted" from the pool
			var excessCount = Mathf.Max(0, _playingSoundCount - _poolMaxSize);
			_evictionCandidates.Init(excessCount);

			// compute pool items which can be played inside the current HDR window
			for (var i = 0; i < _pool.Count; i++)
				UpdateItemSoundVolume(ref _evictionCandidates, _pool[i]);

			// deactivate choosen low-prio one-shot sounds to give a place for the new ones
			if (excessCount > 0)
				Deactivate(_evictionCandidates.Candidates);
		}

		private void UpdateItemSoundVolume(ref EvictionCandidatesList evictionCandidates, PoolItem item)
		{
			if (!item.IsActive)
				return;
			
			// Note that the sound in the pool may end up having a bigger loudness then the top of the HDR window.
			// One case is when the sound has a duration longer than the ActiveRangeDuration attribute (if defined). 
			// Such sound will get it's loudness "removed" from the total current loudness calculation, affecting the HDR window as result.
			// This is not a problem for the system as it will just cap the loudness to the current HDR windo top.
			if (PositionInsideHdrWindow(item))
				SetSoundVolumeAndPriority(item);

			// Only one-shot sounds can be evicted from the pool, loops have to stay as they may be controlled from the outside of pool.
			if (evictionCandidates.MaxCount > 0 && !item.IsLoop && item.IsPlaying)
				_evictionCandidates.TryAddCandidate(item);
		}

		/// <summary>
		/// Play the specified sound and position.
		/// 
		/// Returns reference to the aqiuired audio pool item.
		/// Returns null if the item can't be aquired or it was culled.
		/// Note that the resurned item may be in a suspended state.
		/// </summary>
		public PoolItem Play(SoundDef soundDef, Vector3 position, Transform parent)
		{
			return Play(soundDef, position, parent, true);
		}

		/// <summary>
		/// Play the specified sound and position.
		/// 
		/// Returns reference to the aqiuired audio pool item.
		/// Returns null if the item can't be aquired or it was culled.
		/// Note that the resurned item may be in a suspended state.
		/// </summary>
		public PoolItem Play(SoundDef soundDef, Vector3 position, Transform parent, bool hasRolloff)
		{
			var poolItem = Aquire(soundDef, position, parent, hasRolloff);
			if (poolItem == null)
				return null;

			poolItem.Play();
			return poolItem;
		}
		
		/// <summary>
		/// Returns reference to the aqiuired audio pool item.
		/// Returns null if the item can't be aquired or it was culled.
		/// Note that the resurned item may be in a suspended state.
		/// </summary>
		public PoolItem Aquire(SoundDef soundDef, Vector3 position, Transform parent, bool hasRolloff)
		{
			// may return null if the pool capacity was exeeded
			var poolItem = GetOrCreatePoolItem(soundDef, position, parent, hasRolloff);
			if (poolItem == null)
				return null;

			// check the new sound against the HDR window (state from the previous pool udate)
			if (PositionInsideHdrWindow(poolItem)) {
				
				// update current loudness, set HDR window and compute corresponding volume and priority of the sound 
				if (!poolItem.BypassHDR) {
					AddToCurrentLoudness(poolItem.PerceivedLoudness);
					UpdateHdrWindow();
				}

				SetSoundVolumeAndPriority(poolItem);
				return poolItem;
			}
			
			// may return suspended (e.g. paused) audio source
			if (poolItem.IsSuspended)
				return poolItem;

			return null;
		}

		/// <summary>
		/// Returns the given audio source to the pool.
		/// </summary>
		public void Release(PoolItem poolItem)
		{
			Deactivate(poolItem);
		}

		private PoolItem GetOrCreatePoolItem(SoundDef soundDef, Vector3 position, Transform parent, bool hasRolloff)
		{
			var item = GetOrCreatePoolItem();

			item.Init(soundDef, position, parent, hasRolloff);
			item.Update();

			return item;
		}

		private PoolItem GetOrCreatePoolItem()
		{
			var item = PopInactive();			// try to recycle inactive item
			if (item != null)
				return item;

			item = new PoolItem(this);
			_pool.Add(item);

			// The game should be tuned so the the pool size is not grows uncontrollably.
			if (_pool.Count == _poolMaxSize)
				Debug.LogWarning(string.Format("Maximum pool size ({0}) exeeded!", _poolMaxSize));

			return item;
		}

		private bool PositionInsideHdrWindow(PoolItem poolItem)
		{
			// check sound loudness against the bottom of the HDR window
			if (IsAudible(poolItem)) {
				// unsuspend item that may have been suspended before
				if (poolItem.IsSuspended)
					poolItem.IsSuspended = false;

				return true;
			}

			// Suspend inaudible loops, but keep them pooled.
			if (poolItem.IsLoop) {
				poolItem.IsSuspended = true;
				return false;
			}

			// cull (deactivate) inaudible one-shot sounds
			Deactivate(poolItem);
			return false;
		}

		/// <summary>
		/// The sound with a loudness below bottom of the HDR "window" or too distant are considered inaudible.
		/// </summary>
		private bool IsAudible(PoolItem poolItem)
		{
			return 	poolItem.DistanceToListener <= poolItem.AudioSource.maxDistance && 
					poolItem.PerceivedLoudness >= _windowTop.Value - _windowSize;
		}

		private void SetSoundVolumeAndPriority(PoolItem poolItem)
		{
			poolItem.AudioSource.volume = ComputeSoundVolume(poolItem.PerceivedLoudness, _windowTop.Value);
			poolItem.AudioSource.priority = ComputeSoundPriority(poolItem, _windowTop.Value);
		}

		// this method is used only for sounds which are inside of the HDR window.
		private int ComputeSoundPriority(PoolItem poolItem, float maxLoudness)
		{
			switch (poolItem.SoundDef.Priority) {
			case SoundPriority.None:		return NonePriority;
			case SoundPriority.Low:			return ComputeSoundPriority(poolItem.PerceivedLoudness, maxLoudness, 2);
			case SoundPriority.Medium:		return ComputeSoundPriority(poolItem.PerceivedLoudness, maxLoudness, 1);
			case SoundPriority.High:		return ComputeSoundPriority(poolItem.PerceivedLoudness, maxLoudness, 0);
			case SoundPriority.Permanent:	return PermanentPriority;
			}

			throw new Exception("Undefined: " + poolItem.SoundDef.Priority);
		}

		// The priority level is an integer in range [0..2], where High = 0, Medium = 1 and Low = 2
		// The reversed order is because the highest priority is 0 for the Unity's audio source.
		private int ComputeSoundPriority(float perceivedLoudness, float maxLoudness, int priorityLevel)
		{
			var t = Mathf.InverseLerp(maxLoudness-_windowSize, maxLoudness, perceivedLoudness);

			var top = PriorityRangeTop + PriorityRangeStep * priorityLevel;
			var bottom = top+PriorityRangeStep-1;
			return Mathf.RoundToInt(Mathf.Lerp(bottom, top, t));		// AudioSource priority is reversed
		}

		private PoolItem PopInactive()
		{
			if (_inactive.Count == 0)
				return null;

			var item = _inactive.Pop();
			item.IsActive = true;
			return item;
		}

		private void Deactivate(PoolItem item)
		{
			// disconnect item from the parrent (if any) so it will not be deleted together with parent.
			if (item.AudioSource != null)
				item.AudioSource.transform.SetParent(transform);

			item.IsActive = false;
			_inactive.Push(item);
		}

		private void Deactivate(IList<PoolItem> items)
		{
			for (var i = 0; i < items.Count; i++)
				Deactivate(items[i]);
		}

		private void UpdateHdrWindow()
		{
			var newWindowTop = Mathf.Max(_currentLoudness, _minWindowTop);
			
			if (newWindowTop > _windowTop.Value) {
				// window front change (attack time) is immediate
				_windowTop.Set(newWindowTop);				
			}
			else {
				// release window top - move it (with easing) to the lower position 
				_windowTop.StartEase(_windowTop.Value, newWindowTop, _releaseTime);
			}
		}

		private void AddToCurrentLoudness(float loudness)
		{
			// The loadness is computed as Lcur = 10*log10(sum(pow(10, Li/10)))
			// Therefore we can simply "add" the given loudness to the current totlal loudness.
			var accumulator = new LoudnessAccumulator();
			accumulator.Add(_currentLoudness);
			accumulator.Add(loudness);
			_currentLoudness = accumulator.GetValue();
		}

		/// <summary>
		/// Returns normalized (0..1) sound volume
		/// </summary>
		private float ComputeSoundVolume(float loudness, float maxLoudness)
		{
			// In order to get normalized amplitude (volume) the loudness should not be higher then maxLoudness.
			loudness = Mathf.Min(loudness, maxLoudness);

			// normalized amplitude of the sound: A = pow10((L - Lmax)/20)
			return Mathf.Pow(10, (loudness - maxLoudness) / 20f);
		}

		/// <summary>
		/// Determines whether given position is close to main character.
		/// </summary>
		private bool IsCloseToMainCharacter(Vector3 position)
		{
			return _mainCharacterExtensions != null? _mainCharacterExtensions.IsCloseToMainCharacter(position) : false;
		}

	#if UNITY_EDITOR

		public class DebugStateRow
		{
			public bool IsSuspended;
			public string[] Columns;
		}

		/// <summary>
		/// Called by HDRAudioDebugWindow to render current state of each playing sound
		/// </summary>
		public DebugStateRow[] GetDebugState()
		{
			var orderedItems = _pool.Where(x => x.IsActive).OrderBy(x => x.AudioSource.priority).ToList();
			var result = new DebugStateRow[orderedItems.Count()+1];

			// first row is a header 
			result[0] = new DebugStateRow {
				IsSuspended = false,
				Columns = new string[] {
					"Name", "Priority", "IsPlaying", "IsSuspended", "WinTop", "Loudness", "LoudnessMod", "LoudnessPerc", "Volume", "Distance", "MinDistance", "MaxDistance", "AudioPriority"
				}
			};	

			for (var i = 0; i < orderedItems.Count(); i++) {
				var item = orderedItems[i];

				result[i+1] = new DebugStateRow {
					IsSuspended = item.IsSuspended,
					Columns = new string[] {
						item.ClipName,
						item.SoundDef.Priority.ToString(),
						item.IsPlaying.ToString(),
						item.IsSuspended.ToString(),
						string.Format("{0:n3}", WindowTop),
						string.Format("{0:n3}", item.Loudness),
						item.LoudnessMod.ToString(),
						string.Format("{0:n3}", item.PerceivedLoudness),
						string.Format("{0:n4}", item.AudioSource.volume),
						string.Format("{0:n3}", item.DistanceToListener),
						item.MinDistance.ToString(),
						item.SoundDef.MaxDistance.ToString(),
						item.AudioSource.priority.ToString()
					}
				};
			}

			return result;
		}

	#endif
	}
}
