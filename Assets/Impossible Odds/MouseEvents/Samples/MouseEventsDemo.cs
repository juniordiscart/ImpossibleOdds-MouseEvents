namespace ImpossibleOdds.Examples.MouseEvents
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using ImpossibleOdds.MouseEvents;

	public class MouseEventsDemo : MonoBehaviour
	{
		[SerializeField]
		private Text txtLeft = null;
		[SerializeField]
		private Text txtMiddle = null;
		[SerializeField]
		private Text txtRight = null;

		[SerializeField]
		private MouseEventMonitor monitor = null;

		[SerializeField, Range(0f, 5f)]
		private float statusClearTime = 3f;

		private Dictionary<Text, Coroutine> pendingClearRoutines = new Dictionary<Text, Coroutine>();

		private void Start()
		{
			monitor.onEvent += OnMouseEvent;
		}

		private void OnMouseEvent(MouseButtonEvent mouseEvent)
		{
			string eventType = string.Empty;
			switch (mouseEvent.EventType)
			{
				case MouseButtonEventType.SingleClick:
					eventType = string.Format($"click (single click)");
					break;
				case MouseButtonEventType.DoubleClick:
					eventType = string.Format($"click (double click)");
					break;
				case MouseButtonEventType.Dragging:
					eventType = string.Format($"drag (ongoing) - Drag delta: {mouseEvent.DragDelta.ToString("0")}");
					break;
				case MouseButtonEventType.DragComplete:
					eventType = string.Format($"drag (completed) - Drag delta: {mouseEvent.DragDelta.ToString("0")}");
					break;
				default:
					eventType = string.Format($"Unknown");
					break;
			}

			Text display = null;
			string clearText = string.Empty;
			switch (mouseEvent.Button)
			{
				case MouseButton.Left:
					display = txtLeft;
					txtLeft.text = string.Format($"Left - {eventType} - Modifiers: {mouseEvent.Modifiers.ToString()}");
					clearText = "Left - Idle";
					break;
				case MouseButton.Middle:
					display = txtMiddle;
					txtMiddle.text = string.Format($"Middle - {eventType} - Modifiers: {mouseEvent.Modifiers.ToString()}");
					clearText = "Middle - Idle";
					break;
				case MouseButton.Right:
					display = txtRight;
					txtRight.text = string.Format($"Right - {eventType} - Modifiers: {mouseEvent.Modifiers.ToString()}");
					clearText = "Right - Idle";
					break;
			}

			// The dragging event is not send out each frame if there's not mouse movement.
			// So don't clear it when this state is detected.
			if (!mouseEvent.IsDragging && (display != null))
			{
				if (pendingClearRoutines.ContainsKey(display))
				{
					StopCoroutine(pendingClearRoutines[display]);
				}

				pendingClearRoutines[display] = StartCoroutine(RoutineClearText(display, clearText));
			}
		}

		private IEnumerator RoutineClearText(Text display, string clearText)
		{
			yield return new WaitForSeconds(statusClearTime);
			display.text = clearText;
			pendingClearRoutines.Remove(display);
		}
	}
}
