using System.Collections;
using System.Collections.Generic;
using ImpossibleOdds.MouseEvents;
using UnityEngine;
using UnityEngine.UI;

namespace ImpossibleOdds.Examples.MouseEvents
{
	public class MouseEventsDemo : MonoBehaviour
	{
		[SerializeField]
		private Text txtLeft;
		[SerializeField]
		private Text txtMiddle;
		[SerializeField]
		private Text txtRight;
		[SerializeField]
		private Text txtIsCursorOverUI;
		[SerializeField]
		private Toggle toggleSuspendOverUI;

		[SerializeField]
		private MouseEventMonitor monitor;

		[SerializeField, Range(0f, 5f)]
		private float statusClearTime = 3f;

		private Dictionary<Text, Coroutine> pendingClearRoutines = new Dictionary<Text, Coroutine>();

		private void Start()
		{
			Application.targetFrameRate = 60;
			monitor.onEvent += OnMouseEvent;

			toggleSuspendOverUI.isOn = monitor.SuspendWhenOverUI;
			toggleSuspendOverUI.onValueChanged.AddListener((isOn) => monitor.SuspendWhenOverUI = isOn);
		}

		private void Update()
		{
			txtIsCursorOverUI.text = $"Is cursor over UI? - {(monitor.IsCursorOverUI ? "Yes" : "No")}";
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
				case MouseButtonEventType.DragStart:
					eventType = string.Format($"drag (start)");
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
				if (pendingClearRoutines.TryGetValue(display, out Coroutine routine))
				{
					StopCoroutine(routine);
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
