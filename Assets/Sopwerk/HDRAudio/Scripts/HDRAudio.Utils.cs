using UnityEngine;
using System.Collections.Generic;

namespace Sopwerk.HDRAudio
{
	public partial class HDRAudio : MonoBehaviour 
	{
		/// <summary>
		/// Defines extension point for the HDRAudio service.
		/// A class implementing this interface should be attached to the same GO where the HDRAudio is attached.
		/// </summary>
		public interface IMainCharacterExtensions
		{
			/// <summary>
			/// Determines whether the given position is close to the main character.
			/// 
			/// Te HDRAudio supports few special things (e.g. reduction of the loudness or constant loudness) 
			/// for the sonds played close to the main game character.
			/// </summary>
			bool IsCloseToMainCharacter(Vector3 position);
		}


		// Helper class, used for loudness computations.
		private struct LoudnessAccumulator
		{
			private float _sum;
			
			public void Add(float loudness)
			{
				_sum += Mathf.Pow(10, loudness/10f);
			}
			
			public float GetValue()
			{
				return _sum > 0? 10*Mathf.Log10(_sum) : 0;
			}
		}


		// This specialized list is used to collect references to lowest-priority items from the audio pool. 
		// The size of the list is restricted to the number of items that should be "evicted" from the pool.
		// The primary usage is to actively remove playing low-prio audio sources from the pool in order to 
		// give a place for the new sounds.
		private class EvictionCandidatesList
		{
			// Ascending priority comparer for the PoolItem.
			private static PriorityComparer _priorityComparer = new PriorityComparer();
			
			private class PriorityComparer : IComparer<PoolItem>
			{
				public int Compare(PoolItem x, PoolItem y)
				{
					return x.AudioSource.priority.CompareTo(y.AudioSource.priority);
				}
			}
			
			
			public List<PoolItem> Candidates
			{
				get; private set;
			}
			
			public int MaxCount 
			{
				get; private set;
			}
			
			
			public EvictionCandidatesList()
			{
				Candidates = new List<PoolItem>();
			}
			
			public void Init(int maxCount)
			{
				MaxCount = maxCount;
				Candidates.Clear();
				
				if (Candidates.Capacity < maxCount)
					Candidates.Capacity = maxCount;
			}
			
			public void TryAddCandidate(PoolItem candidateItem)
			{
				// fill up capacities first
				if (Candidates.Count < MaxCount) {
					AddSorted(candidateItem);
					return;
				}

				// try to find an item with the lower priority then the candidate item
				// Note that AudioSource priority is backward, so the list starts with highest priorities (smaller numbers) and goes up (to bigger numbers).
				var index = Candidates.BinarySearch(candidateItem, _priorityComparer);

				// got a duplicate which should be added into the list if there are any higher priority items (e.g. item with lower priority  number)
				if (index > 0) {
					AddDuplicate(candidateItem, index);
				}
				// complement of -1 is 0 , it is returned when the given item's priority is higher (e.g. lower number) then these in the candidate list. 
				else if (index < -1) {
					Candidates[(~index)-1] = candidateItem;				// replace higher priority item with just found lower priority one
				}
			}

			private void AddDuplicate(PoolItem item, int index)
			{
				for (var i = index-1; i >= 0; i--) {
					if (_priorityComparer.Compare(Candidates[i], item) < 0) {
						Candidates[i] = item;
						break;
					}
				}
			}
			
			private void AddSorted(PoolItem item)
			{
				var index = Candidates.BinarySearch(item, _priorityComparer);
				if (index < 0)
					index = ~index;
				
				// add items in sorted order, either before the found item (positive index) 
				// or right after the item with the lower priority (negative index).
				Candidates.Insert(index, item);
			}
		}
	}
}
