using UnityEngine;
using System;

namespace Sopwerk.Utils
{
	/// <summary>
	/// Simple tweening value class.
	/// </summary>
	public class TweenValue
	{
		private Func<float, float> _easingFunc;
		private float _from;
		private float _to;
		private float _fromTime;
		private float _toTime;


		public float Value
		{
			get {
				if (_from == _to)
					return _to;

				var t = Mathf.InverseLerp(_fromTime, _toTime, Time.time);
				if (t == 1f)
					return _from = _to;

				return Mathf.Lerp(_from, _to, _easingFunc(t));
			}
		}

		public bool IsTweening
		{
			get { return _from != _to; }
		}

		public TweenValue(Func<float, float> easingFunc)
		{
			_easingFunc = easingFunc;
		}

		public void Set(float value)
		{
			_from = _to = value;
		}

		public void StartEase(float from, float to, float duration)
		{
			if (duration <= 0) {
				_from = _to = to;
				return;
			}
		
			// the same end-value indicates that tweening is called redudnatly for the same value
			if (_to == to)
				return;

			_from = from;
			_to = to;
			_fromTime = Time.time;
			_toTime = _fromTime + duration;
		}
	}
}