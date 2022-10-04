namespace ImpossibleOdds.MouseEvents
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.EventSystems;

	/// <summary>
	/// A monitoring class of mouse events to detect single & double clicks as well as drag behaviour.
	/// Events are processed in OnGUI, which guarantees that the state is available for all scripts
	/// querying for mouse events in Update, Late Update, Coroutines etc.
	/// </summary>
	public class MouseEventMonitor : MonoBehaviour
	{
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
		private float multiClickTimeThreshold = 0.3f;
		[SerializeField, Tooltip("When the cursor is over UI, should the mouse event monitor suspend operations?")]
		private bool suspendWhenOverUI = false;
		[SerializeField, Tooltip("Should the cursor coordinates be reported in pixel coordinates (as reported by Input.mousePosition), or in GUI coordinates (as reported by OnGUI events)?")]
		private bool cursorPositionInPixelCoordinates = false;

		private Dictionary<MouseButton, MouseButtonStateTracker> stateTrackers = new Dictionary<MouseButton, MouseButtonStateTracker>();
		private int frameCount = -1;
		private Vector3 previousMousePosition = Vector3.zero;

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
		/// When the cursor is over UI, should the mouse event monitor suspend operations?
		/// </summary>
		public bool SuspendWhenOverUI
		{
			get => suspendWhenOverUI;
			set => suspendWhenOverUI = value;
		}

		/// <summary>
		/// Should the cursor coordinates be reported in pixel coordinates (as reported by Input.mousePosition), or in GUI coordinates (as reported by OnGUI events)?
		/// </summary>
		public bool CursorPositionInPixelCoordinates
		{
			get => cursorPositionInPixelCoordinates;
			set => cursorPositionInPixelCoordinates = value;
		}

		/// <summary>
		/// Is the cursor currently registered to be over an interactable UI element?
		/// </summary>
		public bool IsCursorOverUI
		{
			get => (EventSystem.current != null) ? EventSystem.current.IsPointerOverGameObject() : false;
		}

		private Vector3 PreviousMousePosition
		{
			get
			{
				if (cursorPositionInPixelCoordinates)
				{
					return previousMousePosition;
				}
				else
				{
					Vector3 t = previousMousePosition;
					t.y = Screen.height - t.y;
					return t;
				}
			}
		}

		private Vector3 CurrentMousePosition
		{
			get
			{
				if (cursorPositionInPixelCoordinates)
				{
					return Input.mousePosition;
				}
				else
				{
					Vector3 t = Input.mousePosition;
					t.y = Screen.height - t.y;
					return t;
				}
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
			foreach (MouseButton key in monitoredButtons)
			{
				MouseButtonStateTracker state = new MouseButtonStateTracker(key, () => multiClickTimeThreshold);
				stateTrackers[key] = state;
				state.onStateUpdated += OnMouseKeyStateUpdate;
			}

			ProcessNewFrame();
		}

		private void OnDisable()
		{
			frameCount = -1;
			stateTrackers.Clear();
		}

		private void Update()
		{
			ProcessNewFrame();
		}

		private void FixedUpdate()
		{
			ProcessNewFrame();
		}

		private void ProcessNewFrame()
		{
			if (!Input.mousePresent || (frameCount == Time.frameCount))
			{
				return;
			}

			frameCount = Time.frameCount;

			// Let each state tracker know a new is being processed.
			foreach (MouseButton key in monitoredButtons)
			{
				stateTrackers[key].NewFrame();
			}

			foreach (MouseButton mouseButton in stateTrackers.Keys)
			{
				int mouseButtonIndex = (int)mouseButton;
				Event mouseEvent = null;

				if (Input.GetMouseButtonDown(mouseButtonIndex))
				{
					mouseEvent = new Event();
					mouseEvent.type = EventType.MouseDown;
				}
				else if (Input.GetMouseButton(mouseButtonIndex))
				{
					if (PreviousMousePosition != CurrentMousePosition)
					{
						mouseEvent = new Event();
						mouseEvent.type = EventType.MouseDrag;
						mouseEvent.delta = CurrentMousePosition - PreviousMousePosition;
					}
				}
				else if (Input.GetMouseButtonUp(mouseButtonIndex))
				{
					mouseEvent = new Event();
					mouseEvent.type = EventType.MouseUp;
				}

				// If no event was created, then just skip this mouse button.
				if (mouseEvent == null)
				{
					continue;
				}

				mouseEvent.button = mouseButtonIndex;
				mouseEvent.mousePosition = CurrentMousePosition;
				mouseEvent.keyCode = MapMouseButtonToKeyCode(mouseButton);
				ApplyModifierKeys(mouseEvent);

				if (suspendWhenOverUI && IsCursorOverUI)
				{
					stateTrackers[mouseButton].Suspend(mouseEvent);
				}
				else
				{
					stateTrackers[mouseButton].ProcessEvent(mouseEvent);
				}
			}

			// Keep this one unfiltered by the option to return pixel coordinates.
			previousMousePosition = Input.mousePosition;

			/// <summary>
			/// Apply any modifier keys being held down to the mouse event.
			/// </summary>
			void ApplyModifierKeys(Event mouseEvent)
			{
				if (Input.GetKey(KeyCode.AltGr) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
				{
					mouseEvent.alt = true;
				}

				if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				{
					mouseEvent.shift = true;
				}

				if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
				{
					mouseEvent.control = true;
				}

				if (Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows) ||
					Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand))
				{
					mouseEvent.command = true;
				}
			}

			/// <summary>
			/// Map a mouse button value to a mouse button key code.
			/// </summary>
			KeyCode MapMouseButtonToKeyCode(MouseButton mouseButton)
			{
				switch (mouseButton)
				{
					case MouseButton.Left:
						return KeyCode.Mouse0;
					case MouseButton.Right:
						return KeyCode.Mouse1;
					case MouseButton.Middle:
						return KeyCode.Mouse2;
					default:
						return KeyCode.None;
				}
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
