namespace ImpossibleOdds.MouseEvents
{
	/// <summary>
	/// States a mouse button can reside in.
	/// </summary>
	public enum MouseButtonEventType
	{
		/// <summary>
		/// No current event of interest has happened.
		/// </summary>
		None,
		/// <summary>
		/// When the mouse button performed a single click, but is waiting for a follow-up action.
		/// </summary>
		SingleClickPending,
		/// <summary>
		/// When the mouse button performed a single click.
		/// </summary>
		SingleClick,
		/// <summary>
		/// When the mouse button performed a double click.
		/// </summary>
		DoubleClick,
		/// <summary>
		/// When the mouse button is being dragged, but the threshold for a drag action has not been met yet.
		/// </summary>
		DragPending,
		/// <summary>
		/// When the mouse button is being used to initiate a drag action.
		/// </summary>
		DragStart,
		/// <summary>
		/// When the mouse button is being used for dragging.
		/// </summary>
		Dragging,
		/// <summary>
		/// When the mouse button completed a drag action.
		/// </summary>
		DragComplete,

		Idle = None
	}

	public static partial class MouseButtonEventTypeExtensions
	{
		public static bool IsTerminalEvent(this MouseButtonEventType e)
		{
			switch (e)
			{
				case MouseButtonEventType.SingleClick:
				case MouseButtonEventType.DoubleClick:
				case MouseButtonEventType.DragComplete:
					return true;
				default:
					return false;
			}
		}
	}
}
