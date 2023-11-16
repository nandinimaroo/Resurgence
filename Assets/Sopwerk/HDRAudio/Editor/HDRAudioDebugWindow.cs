using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Sopwerk.HDRAudio
{
	/// <summary>
	/// Custom debug window for HDR Audio facility
	/// </summary>
	public class HDRAudioDebugWindow : EditorWindow 
	{
		private Vector2 _scrollPosition;


		[MenuItem ("Window/HDR Audio Debug Window")]
		protected static void Init() 
		{
			EditorWindow.GetWindow<HDRAudioDebugWindow>("HDR Audio");
		}

		protected void OnEnable()
		{
			_scrollPosition = new Vector2();
		}

		protected void OnInspectorUpdate()
		{
			Repaint();
		}
		
		protected void OnGUI() 
		{
			if (!EditorApplication.isPlaying)
				return;

			var hdrAudio = HDRAudio.Instance;
			var dataRows = hdrAudio.GetDebugState();

			_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

			// HDR stats
			RenderNameAndValue("Current Loudness:", "{0:n3}", hdrAudio.CurrentLoudness);
			RenderNameAndValue("Window Top:", "{0:n3}", hdrAudio.WindowTop);
			RenderNameAndValue("Window Bottom:", "{0:n3}", hdrAudio.WindowTop - hdrAudio.WindowSize);

			GUILayout.Space(20);

			// first row is a header
			RenderGridRow(dataRows[0], EditorStyles.boldLabel);

			for (var i = 1; i < dataRows.Length; i++) {
				GUI.color = dataRows[i].IsSuspended? Color.blue : Color.black;
				RenderGridRow(dataRows[i], EditorStyles.whiteMiniLabel);
			}

			GUILayout.EndScrollView();
		}

		private void RenderNameAndValue(string name, string format, object value)
		{
			GUILayout.BeginHorizontal();

			GUILayout.Label(name, EditorStyles.label, GUILayout.MaxWidth(120));
			GUILayout.Label(string.Format(format, value), EditorStyles.label);

			GUILayout.EndHorizontal();
		}

		private void RenderGridRow(HDRAudio.DebugStateRow row, GUIStyle style)
		{
			GUILayout.BeginHorizontal();

			for (var i = 0; i < row.Columns.Length; i++) {
				var opts = (i == 0)? GUILayout.MaxWidth(200) : GUILayout.MaxWidth(100);
				GUILayout.Label(row.Columns[i], style, opts);
			}

			GUILayout.EndHorizontal();
		}
	}
}
