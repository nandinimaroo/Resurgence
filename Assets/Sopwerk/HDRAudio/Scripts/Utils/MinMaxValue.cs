using System;
using UnityRandom = UnityEngine.Random;

namespace Sopwerk.Utils
{
	[Serializable]
	public class MinMaxValue
	{
		public float Min;
		public float Max;

		public float Random()
		{
			// note that Max value is _inclusive_
			return UnityRandom.Range(Min, Max);
		}
	}
}