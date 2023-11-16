using UnityEngine;


/// <summary>
/// Limiter for the repeating sounds.
/// Usage: add as a member attribute to the class that produces repeated sound and use CanRepeatPlay method 
/// to check whether the sound can be played or not.
/// </summary>
public class RepeatingSoundLimiter
{
	// Used when the RepeatRate is defined to "throtlle" how often the same clip can be played.
	private float _repeatPlayStartTime;


	/// <summary>
	/// Check if the clip can be played again. Used together with RepeatRate to throttle frequency of clips playing.
	/// </summary>
	public bool CanRepeatPlay(float repeatRate, bool resetTimer)
	{
		if (repeatRate <= 0)
			return false;
		
		if (_repeatPlayStartTime + repeatRate > Time.time)
			return false;

		if (resetTimer)
			ResetTimer();

		return true;
	}

	public void ResetTimer()
	{
		_repeatPlayStartTime = Time.time;
	}
}
