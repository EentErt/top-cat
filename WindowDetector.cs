using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public partial class WindowDetector : Node
{
	// Enumerates all top-level windows on the desktop
	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	// Gets the bounding rectangle of a window (screen coordinates)
	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	// Checks if a window is visible
	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	// Gets the window title text
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	// Gets the length of the window title
	[DllImport("user32.dll")]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	// Checks if window is minimized
	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr hWnd);

	// Gets extended window styles (to filter out tool windows etc.)
	[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
	private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern uint GetWindowLong32(IntPtr hWnd, int nIndex);

	// Delegate type for EnumWindows callback
	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	// Win32 RECT structure (screen coordinates)
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	// Godot-friendly window info that we pass back to GDScript
	public class WindowInfo
	{
		public string Title;
		public Vector2 Position;   // Top-left corner (screen coords)
		public Vector2 Size;       // Width x Height
	}

	private const int GwlExStyle = -20;
	private const uint WsExToolWindow = 0x80;

	// Store our own window handles so we can exclude them
	private HashSet<IntPtr> _ownHandles = new HashSet<IntPtr>();

	public override void _Ready()
	{
		// Collect all Godot window handles so we skip them
		CacheOwnWindows();
	}

	private void CacheOwnWindows()
	{
		_ownHandles.Clear();

		// Main window
		IntPtr mainHandle = (IntPtr)DisplayServer.WindowGetNativeHandle(
			DisplayServer.HandleType.WindowHandle,
			GetTree().Root.GetWindowId()
		);
		_ownHandles.Add(mainHandle);

		// Find all sub-windows
		CacheSubWindows(GetTree().Root);
	}

	private void CacheSubWindows(Node node)
	{
		foreach (var child in node.GetChildren())
		{
			if (child is Window window && window.Visible)
			{
				IntPtr handle = (IntPtr)DisplayServer.WindowGetNativeHandle(
					DisplayServer.HandleType.WindowHandle,
					window.GetWindowId()
				);
				_ownHandles.Add(handle);
			}
			CacheSubWindows(child);
		}
	}

	private static uint GetWindowStyle(IntPtr hWnd, int nIndex)
	{
		if (IntPtr.Size == 8)
			return (uint)GetWindowLong64(hWnd, nIndex).ToInt64();
		else
			return GetWindowLong32(hWnd, nIndex);
	}

	// Call this from GDScript to get all visible window rects
	public Godot.Collections.Array<Godot.Collections.Dictionary> GetVisibleWindows()
	{
		// Refresh our own window handles in case new ones were added
		CacheOwnWindows();

		var results = new List<WindowInfo>();

		EnumWindows((hWnd, lParam) =>
		{
			// Skip our own windows
			if (_ownHandles.Contains(hWnd))
				return true;

			// Skip invisible windows
			if (!IsWindowVisible(hWnd))
				return true;

			// Skip minimized windows
			if (IsIconic(hWnd))
				return true;

			// Skip windows with no title (usually system stuff)
			int titleLength = GetWindowTextLength(hWnd);
			if (titleLength == 0)
				return true;

			// Skip tool windows (tooltips, popups, etc.)
			uint exStyle = GetWindowStyle(hWnd, GwlExStyle);
			if ((exStyle & WsExToolWindow) != 0)
				return true;

			// Get the window rectangle
			if (GetWindowRect(hWnd, out RECT rect))
			{
				// Skip zero-size windows
				int width = rect.Right - rect.Left;
				int height = rect.Bottom - rect.Top;
				if (width <= 0 || height <= 0)
					return true;

				// Get the title
				StringBuilder titleBuilder = new StringBuilder(titleLength + 1);
				GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);

				results.Add(new WindowInfo
				{
					Title = titleBuilder.ToString(),
					Position = new Vector2(rect.Left, rect.Top),
					Size = new Vector2(width, height)
				});
			}

			return true; // continue enumerating
		}, IntPtr.Zero);

		// Convert to Godot Array of Dictionaries so GDScript can use it
		var godotArray = new Godot.Collections.Array<Godot.Collections.Dictionary>();
		foreach (var info in results)
		{
			var dict = new Godot.Collections.Dictionary
			{
				{ "title", info.Title },
				{ "position", info.Position },
				{ "size", info.Size },
				// Top edge of the window — useful for "standing on" a window
				{ "top_edge", info.Position.Y },
				// Left and right edges — useful for climbing sides
				{ "left_edge", info.Position.X },
				{ "right_edge", info.Position.X + info.Size.X }
			};
			godotArray.Add(dict);
		}

		return godotArray;
	}

	// Convenience: get just the top edges as platforms the cat can stand on
	public Godot.Collections.Array<Godot.Collections.Dictionary> GetWindowPlatforms()
	{
		var windows = GetVisibleWindows();
		var platforms = new Godot.Collections.Array<Godot.Collections.Dictionary>();

		foreach (var window in windows)
		{
			platforms.Add(new Godot.Collections.Dictionary
			{
				{ "title", window["title"] },
				// A platform is the top edge of the window, spanning its full width
				{ "left", (float)window["left_edge"] },
				{ "right", (float)window["right_edge"] },
				{ "y", (float)window["top_edge"] }
			});
		}

		return platforms;
	}
}
