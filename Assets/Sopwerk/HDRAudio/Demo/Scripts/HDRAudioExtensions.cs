using UnityEngine;
using System.Collections;
using Sopwerk.HDRAudio;

namespace Sopwerk.HDRAudio.Demo
{
	/// <summary>
	/// HDRAudio service extensions for the demo project.
	/// </summary>
	public class HDRAudioExtensions : MonoBehaviour, HDRAudio.IMainCharacterExtensions
	{
		[SerializeField]
		private GameObject _mainCharacter = null;

		[SerializeField]
		private float _closeToListenerRadius = 3;


		/// <summary>
		/// Determines whether specified position is close to the main character (listener).
		/// </summary>
		bool HDRAudio.IMainCharacterExtensions.IsCloseToMainCharacter(Vector3 position)
		{
			if (_mainCharacter == null)
				return false;

			var direction = _mainCharacter.transform.position - position;
			return direction.sqrMagnitude <= _closeToListenerRadius * _closeToListenerRadius;
		}
	}
}
