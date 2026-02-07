using Godot;                          // Godot engine classes (Node, DisplayServer, etc.)
using System;                         // Basic C# types (IntPtr)
using System.Runtime.InteropServices; // Allows calling native Windows DLL functions

public partial class TransparentWindow : Node
{
	// Import the SetWindowLong function from Windows' user32.dll
	// This function lets you modify properties/styles of any window
	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

	// GWL_EXSTYLE (-20) tells SetWindowLong we want to modify the "extended style" property
	private const int GwlExStyle = -20;

	// Flag that makes the window "layered" — required for transparency to work
	private const int WsExLayered = 0x80000;

	// Flag that makes the window ignore all mouse input (clicks pass through)
	private const int WsExTransparent = 0x20;

	// Stores the native Windows handle (HWND) for our Godot window
	private IntPtr _hWnd;

	public override void _Ready()
	{
		// Ask Godot for the native Windows window handle so we can manipulate it
		_hWnd = (IntPtr)DisplayServer.WindowGetNativeHandle(
			DisplayServer.HandleType.WindowHandle,
			GetWindow().GetWindowId()
		);

		// First set the window as layered (prerequisite for click-through)
		SetWindowLong(_hWnd, GwlExStyle, WsExLayered);

		// Then enable click-through
		SetClickThrough(true);
	}

	// Toggle click-through on or off at runtime
	public void SetClickThrough(bool clickthrough)
	{
		if (clickthrough)
		{
			// Layered + Transparent = window is visible but all clicks go through it
			SetWindowLong(_hWnd, GwlExStyle, WsExLayered | WsExTransparent);
		}
		else
		{
			// Layered only = window is visible AND clickable again
			SetWindowLong(_hWnd, GwlExStyle, WsExLayered);
		}
	}
}
