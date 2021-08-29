namespace ImpossibleOdds.MouseEvents
{
	using System;
	using UnityEngine;

	public class MouseButtonStateTracker
	{
		public event Action<MouseButtonStateTracker> onStateUpdated;

		private readonly MouseButton button;
		private readonly Func<float> multiClickTimeTreshold = null;

		private MouseButtonEventType state = MouseButtonEventType.None;
		private EventModifiers lastModifiers = EventModifiers.None;
		private float lastClickAction = 0f;
		private Vector2 dragStartPosition = Vector2.zero;
		private Vector2 mousePosition = Vector2.zero;

		public MouseButton Key
		{
			get { return button; }
		}

		public MouseButtonEventType State
		{
			get { return state; }
		}

		public EventModifiers Modifiers
		{
			get { return lastModifiers; }
		}

		public Vector2 DragStartPosition
		{
			get { return dragStartPosition; }
		}

		public Vector2 MousePosition
		{
			get { return mousePosition; }
		}

		public MouseButtonStateTracker(MouseButton button, Func<float> multiClickTimeTreshold)
		{
			this.button = button;
			this.multiClickTimeTreshold = multiClickTimeTreshold;
			ClearState();
		}

		public void NewFrame()
		{
			if (state != MouseButtonEventType.None)
			{
				lastClickAction += Time.smoothDeltaTime;
			}

			switch (state)
			{
				case MouseButtonEventType.SingleClickPending:
					if (lastClickAction >= multiClickTimeTreshold())
					{
						state = MouseButtonEventType.SingleClick;
						lastClickAction = 0f;
						OnStateUpdated();
					}
					break;
				case MouseButtonEventType.SingleClick:
				case MouseButtonEventType.DoubleClick:
				case MouseButtonEventType.DragComplete:
					ClearState();
					OnStateUpdated();
					break;
			}
		}

		public void ProcessEvent(Event mouseEvent)
		{
			// If the button doesn't match, don't bother.
			if ((MouseButton)mouseEvent.button != (MouseButton)button)
			{
				return;
			}

			switch (mouseEvent.type)
			{
				case EventType.MouseUp:

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
							state = MouseButtonEventType.DragComplete;
							lastModifiers = mouseEvent.modifiers;
							mousePosition = mouseEvent.mousePosition;
							OnStateUpdated();
							break;
						default:
							ClearState();
							OnStateUpdated();
							break;
					}

					break;
				case EventType.MouseDrag:
					if (state != MouseButtonEventType.Dragging)
					{
						dragStartPosition = mouseEvent.mousePosition;
					}

					state = MouseButtonEventType.Dragging;
					lastModifiers = mouseEvent.modifiers;
					mousePosition = mouseEvent.mousePosition;
					OnStateUpdated();
					break;
			}
		}

		private void ClearState()
		{
			state = MouseButtonEventType.None;
			lastModifiers = EventModifiers.None;
			lastClickAction = 0f;

			dragStartPosition = Vector2.zero;
			mousePosition = Vector2.zero;
		}

		private void OnStateUpdated()
		{
			if (onStateUpdated != null)
			{
				onStateUpdated(this);
			}
		}
	}
}
