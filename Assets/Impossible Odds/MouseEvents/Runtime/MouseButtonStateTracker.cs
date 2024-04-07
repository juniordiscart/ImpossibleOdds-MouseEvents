using System;
using UnityEngine;

namespace ImpossibleOdds.MouseEvents
{
	public class MouseButtonStateTracker
	{
		public event Action<MouseButtonStateTracker> onStateUpdated;

		private readonly MouseButton button;
		private readonly Func<float> multiClickTimeThreshold = null;

		private MouseButtonEventType state = MouseButtonEventType.None;
		private EventModifiers lastModifiers = EventModifiers.None;
		private float lastClickAction = 0f;
		private bool wasSuspended = false;
		private Vector2 dragStartPosition = Vector2.zero;
		private Vector2 mousePosition = Vector2.zero;

		/// <summary>
		/// The mouse button it is tracking.
		/// </summary>
		public MouseButton Key => button;

		/// <summary>
		/// The current state the button resides in.
		/// </summary>
		public MouseButtonEventType State => state;

		/// <summary>
		/// The modifier keys.
		/// </summary>
		public EventModifiers Modifiers => lastModifiers;

		/// <summary>
		/// The mouse cursor position when a drag operation started.
		/// </summary>
		public Vector2 DragStartPosition => dragStartPosition;

		/// <summary>
		/// The current mouse cursor position.
		/// </summary>
		public Vector2 MousePosition => mousePosition;

		public MouseButtonStateTracker(MouseButton button, Func<float> multiClickTimeThreshold)
		{
			this.button = button;
			this.multiClickTimeThreshold = multiClickTimeThreshold;
			ClearState();
		}

		public void NewFrame()
		{
			if (state == MouseButtonEventType.None)
			{
				return;
			}

			lastClickAction += Time.smoothDeltaTime;
			if (state == MouseButtonEventType.SingleClickPending)
			{
				if (lastClickAction >= multiClickTimeThreshold())
				{
					state = MouseButtonEventType.SingleClick;
					lastClickAction = 0f;
					OnStateUpdated();
				}
			}
			else if (state.IsTerminalEvent())
			{
				ClearState();
				OnStateUpdated();
			}
		}

		internal void ProcessEvent(Event mouseEvent)
		{
			// If the button doesn't match, don't bother.
			if (mouseEvent.button != button)
			{
				return;
			}

			switch (mouseEvent.type)
			{
				case EventType.MouseUp:
					ProcessMouseUpEvent(mouseEvent);
					break;
				case EventType.MouseDrag:
					ProcessMouseDragEvent(mouseEvent);
					break;
			}
		}

		internal void Suspend(Event mouseEvent)
		{
			// If the button doesn't match, don't bother.
			if ((MouseButton)mouseEvent.button != (MouseButton)button)
			{
				return;
			}

			switch (mouseEvent.type)
			{
				case EventType.MouseUp:
					SuspendMouseUpEvent(mouseEvent);
					break;
				case EventType.MouseDown:
					wasSuspended = true;
					break;
				case EventType.MouseDrag:
					// Do nothing here.
					// If the mouse was being dragged, then we may want it to complete
					// once it isn't suspended anymore if no other event has taken precedence.
					break;
				default:
					if (state != MouseButtonEventType.None)
					{
						ClearState();
						OnStateUpdated();
					}
					break;
			}
		}

		private void ProcessMouseUpEvent(Event mouseEvent)
		{
			switch (state)
			{
				case MouseButtonEventType.None:
					state = MouseButtonEventType.SingleClickPending;
					lastModifiers = mouseEvent.modifiers;
					mousePosition = mouseEvent.mousePosition;
					OnStateUpdated();
					break;
				case MouseButtonEventType.SingleClickPending:
					state = MouseButtonEventType.DoubleClick;
					lastModifiers = mouseEvent.modifiers;
					mousePosition = mouseEvent.mousePosition;
					OnStateUpdated();
					break;
				case MouseButtonEventType.Dragging:
					if (!wasSuspended)
					{
						state = MouseButtonEventType.DragComplete;
						lastModifiers = mouseEvent.modifiers;
						mousePosition = mouseEvent.mousePosition;
						OnStateUpdated();
					}
					break;
				default:
					ClearState();
					OnStateUpdated();
					break;
			}
		}

		private void ProcessMouseDragEvent(Event mouseEvent)
		{
			// If the mouse was suspended, then don't continue the drag operation.
			if (wasSuspended)
			{
				return;
			}

			// If a single click was still pending,
			// then perform the click before getting to the drag operation.
			if (state == MouseButtonEventType.SingleClickPending)
			{
				state = MouseButtonEventType.SingleClick;
				lastClickAction = 0f;
			}
			else if ((state != MouseButtonEventType.DragStart) && (state != MouseButtonEventType.Dragging))
			{
				state = MouseButtonEventType.DragStart; // The first of such events initiates the drag start.
				dragStartPosition = mouseEvent.mousePosition;
				mousePosition = mouseEvent.mousePosition;
			}
			else
			{
				state = MouseButtonEventType.Dragging;
				mousePosition = mouseEvent.mousePosition;
			}

			lastModifiers = mouseEvent.modifiers;
			OnStateUpdated();
		}

		private void SuspendMouseUpEvent(Event mouseEvent)
		{
			switch (state)
			{
				case MouseButtonEventType.SingleClickPending:
					state = MouseButtonEventType.SingleClick;
					lastClickAction = 0f;
					OnStateUpdated();
					break;
				default:
					if (state != MouseButtonEventType.None)
					{
						ClearState();
						OnStateUpdated();
					}
					else
					{
						wasSuspended = false;   // Reset this value here, since it may not register the next action properly otherwise.
					}
					break;
			}
		}

		private void ClearState()
		{
			state = MouseButtonEventType.None;
			lastModifiers = EventModifiers.None;
			lastClickAction = 0f;
			wasSuspended = false;

			dragStartPosition = Vector2.zero;
			mousePosition = Vector2.zero;
		}

		private void OnStateUpdated()
		{
			onStateUpdated?.Invoke(this);
		}
	}
}
