namespace ImpossibleOdds.MouseEvents
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// A monitoring class of mouse events to detect single & double clicks as well as drag behaviour.
	/// Events are processed in OnGUI, which guarantees that the state is available for all scripts
	/// querying for mouse events in Update, Late Update, Coroutines etc.
	/// </summary>
	public class MouseEventMonitor : MonoBehaviour
	{
		/// <summary>
		/// Called when a new is started that will process the mouse events that happened.
		/// This may be useful for clearing mouse event caches that can be filled up again this frame.
		/// </summary>
		public event Action onNewFrame;
		/// <summary>
		/// Called when a single or double click is registered, or a drag event is registered.
		/// </summary>
		public event Action<MouseButtonEvent> onEvent;
		/// <summary>
		/// Called when a single click is registered.
		/// </summary>
		public event Action<MouseButtonEvent> onSingleClick;
		/// <summary>
		/// Called when a double click is registered.
		/// </summary>
		public event Action<MouseButtonEvent> onDoubleClick;
		/// <summary>
		/// Called when a drag-start event is generated.
		/// </summary>
		public event Action<MouseButtonEvent> onDragStart;
		/// <summary>
		/// Called when a drag event is generated.
		/// Note: this is not continuous, a drag event is only generated when actual mouse movement in detected.
		/// </summary>
		public event Action<MouseButtonEvent> onDragOngoing;
		/// <summary>
		/// Called when a drag event is completed.
		/// </summary>
		public event Action<MouseButtonEvent> onDragCompleted;

		[SerializeField, Tooltip("Which mouse buttons should be monitored?")]
		private List<MouseButton> monitoredButtons = new List<MouseButton>();
		[SerializeField, Tooltip("How long (in seconds) should it wait for registering a multi-click event?"), Range(0.1f, 1f)]
		private float multiClickTimeThreshold = 0.2f;

		private Dictionary<MouseButton, MouseButtonStateTracker> stateTrackers = new Dictionary<MouseButton, MouseButtonStateTracker>();
		private int frameCount = 0;

		/// <summary>
		/// The mouse buttons currently being monitored for events.
		/// </summary>
		public IReadOnlyList<MouseButton> MonitoredButtons
		{
			get => monitoredButtons;
		}

		/// <summary>
		/// How long should it wait to register a multi-click?
		/// </summary>
		public float MultiClickTimeThreshold
		{
			get => multiClickTimeThreshold;
			set
			{
				if (value <= 0f)
				{
					throw new ArgumentOutOfRangeException("The delay for single clicks should be greater than 0.");
				}

				multiClickTimeThreshold = value;
			}
		}

		/// <summary>
		/// Get the current event associated with a certain mouse button.
		/// </summary>
		/// <param name="mouseButton">The mouse button for which to get the current event.</param>
		/// <returns>The current event associated with the mouse button.</returns>
		public MouseButtonEvent CurrentEvent(MouseButton mouseButton)
		{
			return stateTrackers.ContainsKey(mouseButton) ? MouseButtonEvent.Create(stateTrackers[mouseButton]) : MouseButtonEvent.None;
		}

		/// <summary>
		/// Is the mouse button being monitored for events?
		/// </summary>
		/// <param name="mouseButton">The mouse button to check for.</param>
		/// <returns>True if the mouse button is being monitored for events, false otherwise.</returns>
		public bool IsMonitored(MouseButton mouseButton)
		{
			return monitoredButtons.Contains(mouseButton);
		}

		/// <summary>
		/// Monitor the given mouse button for events.
		/// </summary>
		/// <param name="mouseButton">The mouse button to monitor for events.</param>
		public void StartMonitoring(MouseButton mouseButton)
		{
			if (!IsMonitored(mouseButton))
			{
				monitoredButtons.Add(mouseButton);

				if (enabled)
				{
					MouseButtonStateTracker stateTracker = new MouseButtonStateTracker(mouseButton, () => multiClickTimeThreshold);
					stateTrackers[mouseButton] = stateTracker;
					stateTracker.onStateUpdated += OnMouseKeyStateUpdate;
				}
			}
		}

		/// <summary>
		/// Stop monitoring a mouse button for events.
		/// </summary>
		/// <param name="mouseButton">The mouse button to stop monitoring.</param>
		public void StopMonitoring(MouseButton mouseButton)
		{
			if (IsMonitored(mouseButton))
			{
				stateTrackers.Remove(mouseButton);
				monitoredButtons.RemoveAll(mB => mB == mouseButton);
			}
		}

		private void OnEnable()
		{
			frameCount = Time.frameCount;
			foreach (MouseButton key in monitoredButtons)
			{
				MouseButtonStateTracker state = new MouseButtonStateTracker(key, () => multiClickTimeThreshold);
				stateTrackers[key] = state;
				state.onStateUpdated += OnMouseKeyStateUpdate;
			}
		}

		private void OnDisable()
		{
			frameCount = 0;
			stateTrackers.Clear();
		}

		private void OnGUI()
		{
			if (frameCount != Time.frameCount)
			{
				frameCount = Time.frameCount;
				ProcessNewFrame();
			}

			if (Event.current.isMouse)
			{
				MouseButton mouseButtonIndex = (MouseButton)Event.current.button;
				if (stateTrackers.ContainsKey(mouseButtonIndex))
				{
					stateTrackers[mouseButtonIndex].ProcessEvent(Event.current);
				}
			}
		}

		private void ProcessNewFrame()
		{
			if (onNewFrame != null)
			{
				onNewFrame();
			}

			foreach (MouseButton key in monitoredButtons)
			{
				stateTrackers[key].NewFrame();
			}
		}

		private void OnMouseKeyStateUpdate(MouseButtonStateTracker stateTracker)
		{
			MouseButtonEvent currentEventData = MouseButtonEvent.Create(stateTracker);

			switch (stateTracker.State)
			{
				case MouseButtonEventType.SingleClick:
					CallEvent(onSingleClick, currentEventData);
					CallEvent(onEvent, currentEventData);
					break;
				case MouseButtonEventType.DoubleClick:
					CallEvent(onDoubleClick, currentEventData);
					CallEvent(onEvent, currentEventData);
					break;
				case MouseButtonEventType.DragStart:
					CallEvent(onDragStart, currentEventData);
					CallEvent(onEvent, currentEventData);
					break;
				case MouseButtonEventType.Dragging:
					CallEvent(onDragOngoing, currentEventData);
					CallEvent(onEvent, currentEventData);
					break;
				case MouseButtonEventType.DragComplete:
					CallEvent(onDragCompleted, currentEventData);
					CallEvent(onEvent, currentEventData);
					break;
			}
		}

		private void CallEvent(Action<MouseButtonEvent> action, MouseButtonEvent mouseEventData)
		{
			if (action != null)
			{
				action(mouseEventData);
			}
		}
	}
}
