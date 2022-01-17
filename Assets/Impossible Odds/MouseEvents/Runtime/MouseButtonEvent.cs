namespace ImpossibleOdds.MouseEvents
{
	using UnityEngine;

	public struct MouseButtonEvent
	{
		/// <summary>
		/// Create an event based on the given even tracker.
		/// </summary>
		/// <param name="stateTracker">The tracker from which to create the current event.</param>
		/// <returns>A mouse button event representing the state in the tracker.</returns>
		public static MouseButtonEvent Create(MouseButtonStateTracker stateTracker)
		{
			return new MouseButtonEvent(
				stateTracker.Key,
				stateTracker.Modifiers,
				stateTracker.State,
				stateTracker.DragStartPosition,
				stateTracker.MousePosition);
		}

		/// <summary>
		/// Create a single click event.
		/// </summary>
		/// <param name="mouseButton">The mouse button that relates to the single click event.</param>
		/// <param name="modifiers">The modifers active during this event.</param>
		/// <returns>An event representing a single click.</returns>
		public static MouseButtonEvent CreateSingleClickEvent(MouseButton mouseButton, EventModifiers modifiers = EventModifiers.None)
		{
			return new MouseButtonEvent(mouseButton, modifiers, MouseButtonEventType.SingleClick);
		}

		/// <summary>
		/// Create a double click event.
		/// </summary>
		/// <param name="mouseButton">The mouse button that relates to the double click event.</param>
		/// <param name="modifiers">The modifers active during this event.</param>
		/// <returns>An event representing a double click.</returns>
		public static MouseButtonEvent CreateDoubleClickEvent(MouseButton mouseButton, EventModifiers modifiers = EventModifiers.None)
		{
			return new MouseButtonEvent(mouseButton, modifiers, MouseButtonEventType.DoubleClick);
		}

		/// <summary>
		/// Create a drag event. This event can represent the start of the drag manouvre, or an ongoing drag manouvre.
		/// </summary>
		/// <param name="mouseButton">The mouse button that relates to the drag event.</param>
		/// <param name="startPosition">The start position of the mouse of this drag manouvre.</param>
		/// <param name="currentPosition">The current mouse position of this drag manouvre.</param>
		/// <param name="modifiers">The modifers active during this event.</param>
		/// <returns>An event representing a mouse drag manouvre.</returns>
		public static MouseButtonEvent CreateDraggingEvent(MouseButton mouseButton, Vector2 startPosition, Vector2 currentPosition, EventModifiers modifiers = EventModifiers.None)
		{
			return new MouseButtonEvent(mouseButton, modifiers, MouseButtonEventType.Dragging, startPosition, currentPosition);
		}

		/// <summary>
		/// Create a drag completed event.
		/// </summary>
		/// <param name="mouseButton">The mouse button that relates to the drag event.</param>
		/// <param name="startPosition">The start position of the mouse of this drag manouvre.</param>
		/// <param name="currentPosition">The mouse position at the end of this drag manouvre.</param>
		/// <param name="modifiers">The modifers active during this event.</param>
		/// <returns>An event representing a mouse drag completed.</returns>
		public static MouseButtonEvent CreateDragCompletedEvent(MouseButton mouseButton, Vector2 startPosition, Vector2 currentPosition, EventModifiers modifiers = EventModifiers.None)
		{
			return new MouseButtonEvent(mouseButton, modifiers, MouseButtonEventType.DragComplete, startPosition, currentPosition);
		}

		/// <summary>
		/// An empty mouse event.
		/// </summary>
		public static MouseButtonEvent None
		{
			get { return new MouseButtonEvent(MouseButton.None, EventModifiers.None, MouseButtonEventType.None); }
		}

		private readonly MouseButton mouseButton;
		private readonly EventModifiers modifiers;
		private readonly MouseButtonEventType buttonState;
		private readonly Vector2 dragStart;
		private readonly Vector2 mousePosition;

		private MouseButtonEvent(MouseButton mouseButton, EventModifiers modifiers, MouseButtonEventType state)
		: this(mouseButton, modifiers, state, Vector2.zero, Vector2.zero)
		{ }

		private MouseButtonEvent(MouseButton mouseButton, EventModifiers modifiers, MouseButtonEventType state, Vector2 dragStartPos, Vector2 dragPos)
		{
			this.mouseButton = mouseButton;
			this.modifiers = modifiers;
			this.buttonState = state;
			this.dragStart = dragStartPos;
			this.mousePosition = dragPos;
		}

		/// <summary>
		/// The mouse button this event is associated with.
		/// </summary>
		public MouseButton Button
		{
			get { return mouseButton; }
		}

		/// <summary>
		/// The type of event.
		/// </summary>
		public MouseButtonEventType EventType
		{
			get { return buttonState; }
		}

		/// <summary>
		/// The active modifiers during this mouse event.
		/// </summary>
		public EventModifiers Modifiers
		{
			get { return modifiers; }
		}

		/// <summary>
		/// Does the event represent a mouse click?
		/// </summary>
		public bool IsClick
		{
			get { return (buttonState == MouseButtonEventType.SingleClick) || (buttonState == MouseButtonEventType.DoubleClick); }
		}

		/// <summary>
		/// Does the event represent a dragging manouvre?
		/// </summary>
		public bool IsDrag
		{
			get { return (buttonState == MouseButtonEventType.Dragging) || (buttonState == MouseButtonEventType.DragComplete); }
		}

		/// <summary>
		/// Is this event a single-click event?
		/// </summary>
		public bool IsSingleClick
		{
			get { return buttonState == MouseButtonEventType.SingleClick; }
		}

		/// <summary>
		/// Is this event a double-click event?
		/// </summary>
		public bool IsDoubleClick
		{
			get { return buttonState == MouseButtonEventType.DoubleClick; }
		}

		/// <summary>
		/// Is this event an ongoing-drag event?
		/// </summary>
		public bool IsDragging
		{
			get { return buttonState == MouseButtonEventType.Dragging; }
		}

		/// <summary>
		/// Is this event a drag completed event?
		/// </summary>
		public bool IsDragCompleted
		{
			get { return buttonState == MouseButtonEventType.DragComplete; }
		}

		/// <summary>
		/// The position of the mouse at the start of the drag event.
		/// </summary>
		public Vector2 DragStartPosition
		{
			get { return dragStart; }
		}

		/// <summary>
		/// The position of the mouse for this event.
		/// </summary>
		public Vector2 MousePosition
		{
			get { return mousePosition; }
		}

		/// <summary>
		/// The drag delta position of the mouse during a drag event.
		/// </summary>
		public Vector2 DragDelta
		{
			get { return mousePosition - dragStart; }
		}
	}
}
