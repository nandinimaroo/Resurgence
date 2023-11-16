using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

namespace Sopwerk.HDRAudio.Demo
{
	/// <summary>
	/// Widget that represents AudioListener on the GUI
	/// </summary>
	public class AudioListenerWidget : WidgetBase
	{
		protected override void Awake ()
		{
			base.Awake();

			// attach AudioListener to the audioObject so it will be scaled properly when widget dragged on the canvas.
			_audioObject.AddComponent<AudioListener>();
		}

		protected override bool IsActive 
		{
			get { return true; }
		}
		
		protected override void OnClick()
		{}
	}
}
