using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

namespace Sopwerk.HDRAudio.Demo
{
	public abstract class WidgetBase : MonoBehaviour,
		IBeginDragHandler, IDragHandler, IEndDragHandler, 
		IPointerEnterHandler, IPointerExitHandler, 
		IPointerDownHandler, IPointerUpHandler,
		ISelectHandler, IDeselectHandler
	{
		protected GameObject _audioObject;

		private bool _isPointerDown;
		private Vector3 _initialMousePosOffset;

		// used to control difference in distances between on-screen widgets and desired distances between audio objects in world space.
		private const float WidgetToAudioObjectScale = 0.1f;
		

		[Header("Widget")]
		[SerializeField]
		private Image _icon = null;
		
		[SerializeField]
		private Color _normalColor = Color.white;
		
		[SerializeField]
		private Color _activeColor = Color.red;
		
		[SerializeField]
		private Image _selectionFrame = null;
		
		[SerializeField]
		private Texture2D _grabCursor = null;
		
		[SerializeField]
		private Texture2D _dragCursor = null;
		
		
		private RectTransform ParentPanel
		{
			get { return (RectTransform)transform.parent; }
		}

		public bool IsSelectable
		{
			get { return _selectionFrame != null; }
		}


		protected virtual void Awake()
		{
			// a separate game object is used as parent for the AudioSource - to support adjustable scaling of the sound sources
			_audioObject = new GameObject("AudioObject-" + name);
			_audioObject.hideFlags = HideFlags.HideInHierarchy;
		}
		
		protected virtual void Update()
		{
			UpdateSoundSourcePosition();
			UpdateIconColor(IsActive);
		}

		private void UpdateSoundSourcePosition()
		{
			// scale position of the sound sources in order to adjust disance to the listener
			var scaledLocalPos = transform.localPosition * WidgetToAudioObjectScale;
			_audioObject.transform.position = transform.parent.TransformPoint(scaledLocalPos);
		}
		
		/// <summary>
		/// Called when widget is clicked.
		/// </summary>
		protected abstract void OnClick();

		/// <summary>
		/// Indicates whether widget is active (e.g. playing sound) or not
		/// </summary>
		protected abstract bool IsActive { get; }


		#region Mouse pointer and drag & drop handling
		
		void IPointerEnterHandler.OnPointerEnter(PointerEventData data)
		{
			if (data.pointerDrag == null)
				SetCursor(_grabCursor);
		}
		
		void IPointerExitHandler.OnPointerExit(PointerEventData data)
		{
			if (data.pointerDrag == null)
				SetCursor(null);
		}
		
		void IPointerDownHandler.OnPointerDown(PointerEventData data)
		{
			_isPointerDown = true;
			
			if (_selectionFrame != null)
				EventSystem.current.SetSelectedGameObject(gameObject);
		}
		
		void IPointerUpHandler.OnPointerUp(PointerEventData data)
		{
			// only left mouse button is registered as click, the right will just change selection
			if (_isPointerDown && data.button == PointerEventData.InputButton.Left)
				OnClick();
			
			_isPointerDown = false;
		}
		
		void ISelectHandler.OnSelect(BaseEventData eventData) 
		{
			if (_selectionFrame != null)
				_selectionFrame.gameObject.SetActive(true);
		}
		
		void IDeselectHandler.OnDeselect(BaseEventData eventData) 
		{
			// the eventData is actually a pointerEventData if the deselection was a sesult of the mouse click
			var pointerEventData = eventData as PointerEventData;

			// Only selectable widget can be selected, anything else should bounce selection back to self.
			// Note that we can't change current selection from within the event handler, thus co-orutine is used.
			if (pointerEventData != null && !IsTargetWidgetSelectable(pointerEventData)) {
				StartCoroutine(DelayedSelfSelectRoutine());
				return;
			}

			if (_selectionFrame != null)
				_selectionFrame.gameObject.SetActive(false);
		}

		private bool IsTargetWidgetSelectable(PointerEventData pointerEventData)
		{
			if (pointerEventData == null || pointerEventData.pointerEnter == null) 
				return false;

			var widget = pointerEventData.pointerEnter.GetComponent<WidgetBase>();
			return widget != null && widget.IsSelectable;
		}

		private IEnumerator DelayedSelfSelectRoutine()
		{
			yield return new WaitForEndOfFrame();
			EventSystem.current.SetSelectedGameObject(gameObject);
		}

		void IBeginDragHandler.OnBeginDrag(PointerEventData data)
		{
			SetCursor(_dragCursor);
			
			// the offset is used during dragging to prevent center of the widget from "snapping" to the mouse position.
			Vector3? mousePos = GetWorldMousePosition(data);
			_initialMousePosOffset = mousePos != null? mousePos.Value - transform.position : Vector3.zero;
		}
		
		void IDragHandler.OnDrag(PointerEventData data)
		{
			// click will not affect state of the widget while drag is in progress.
			_isPointerDown = false;
			
			Vector3? mousePos = GetWorldMousePosition(data);
			if (mousePos == null)
				return;
			
			var offset = mousePos.Value - transform.position - _initialMousePosOffset;
			
			// make sure that the widget can't be dragged outside parent panel's boundaries
			if (Contains(ParentPanel, (RectTransform)transform, offset, data.pressEventCamera)) {
				transform.position += offset;
				transform.rotation = ParentPanel.rotation;
			}
		}
		
		void IEndDragHandler.OnEndDrag(PointerEventData data)
		{
			SetCursor(data.pointerEnter != null? _grabCursor : null);
		}
		
		#endregion
		
		private Vector3? GetWorldMousePosition(PointerEventData data)
		{
			Vector3 mousePos;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(ParentPanel, data.position, data.pressEventCamera, out mousePos))
				return mousePos;
			
			return null;
		}
		
		private bool Contains(RectTransform left, RectTransform right, Vector3 rightOffset, Camera cam)
		{
			// there should be easier way to check UI component boundaries (?)
			var corners = new Vector3[4];
			right.GetWorldCorners(corners);
			
			return corners.All(c => RectangleContainsWordPoint(left, c + rightOffset, cam));
		}
		
		private bool RectangleContainsWordPoint(RectTransform rect, Vector3 worldPoint, Camera cam)
		{
			var screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPoint);
			return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam);
		}
		
		private void SetCursor(Texture2D texture)
		{
			if (texture != null)
				Cursor.SetCursor(texture, new Vector2(texture.width/2, texture.height/2), CursorMode.Auto);
			else
				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
		
		protected void UpdateIconColor(bool isActive)
		{
			if (_icon != null)
				_icon.color = isActive? _activeColor : _normalColor;
		}
	}
}
