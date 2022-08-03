namespace ImpossibleOdds.MouseEvents
{
	/// <summary>
	/// States a mouse button can reside in.
	/// </summary>
	public enum MouseButtonEventType : int
	{
		/// <summary>
		/// No current event of interest has happened.
		/// </summary>
		None = 0,
		/// <summary>
		/// When the mouse button performed a single click, but is waiting for a follow-up action.
		/// </summary>
		SingleClickPending = 1,
		/// <summary>
		/// When the mouse button performed a single click.
		/// </summary>
		SingleClick = 2,
		/// <summary>
		/// When the mouse button performed a double click.
		/// </summary>
		DoubleClick = 3,
		/// <summary>
		/// When the mouse button is being used to initiate a drag action.
		/// </summary>
		DragStart = 4,
		/// <summary>
		/// When the mouse button is being used for dragging.
		/// </summary>
		Dragging = 5,
		/// <summary>
		/// When the mouse button completed a drag action.
		/// </summary>
		DragComplete = 6
	}
}
