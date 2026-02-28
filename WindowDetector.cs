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

	// Gets extended window styles — 64-bit
	[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
	private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

	// Gets extended window styles — 32-bit
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

	private const int GwlExStyle = -20;
	private const uint WsExToolWindow = 0x80;

	// Store our own window handles so we can exclude them
	private HashSet<IntPtr> _ownHandles = new HashSet<IntPtr>();

	private static uint GetWindowStyle(IntPtr hWnd, int nIndex)
	{
		if (IntPtr.Size == 8)
			return (uint)GetWindowLong64(hWnd, nIndex).ToInt64();
		else
			return GetWindowLong32(hWnd, nIndex);
	}

	public override void _Ready()
	{
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

	// Returns all visible windows as an array of dictionaries
	public Godot.Collections.Array GetVisibleWindows()
	{
		CacheOwnWindows();

		var results = new Godot.Collections.Array();

		EnumWindows((hWnd, lParam) =>
		{
			if (_ownHandles.Contains(hWnd)) return true;
			if (!IsWindowVisible(hWnd)) return true;
			if (IsIconic(hWnd)) return true;

			int titleLength = GetWindowTextLength(hWnd);
			if (titleLength == 0) return true;

			uint exStyle = GetWindowStyle(hWnd, GwlExStyle);
			if ((exStyle & WsExToolWindow) != 0) return true;

			if (GetWindowRect(hWnd, out RECT rect))
			{
				int width = rect.Right - rect.Left;
				int height = rect.Bottom - rect.Top;
				if (width <= 0 || height <= 0) return true;

				StringBuilder titleBuilder = new StringBuilder(titleLength + 1);
				GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);

				results.Add(new Godot.Collections.Dictionary
				{
					{ "title", titleBuilder.ToString() },
					{ "position", new Vector2(rect.Left, rect.Top) },
					{ "size", new Vector2(width, height) },
					{ "top_edge", (float)rect.Top },
					{ "left_edge", (float)rect.Left },
					{ "right_edge", (float)rect.Right },
					{ "bottom_edge", (float)rect.Bottom }
				});
			}

			return true;
		}, IntPtr.Zero);

		GD.Print($"GetVisibleWindows found {results.Count} windows");
		return results;
	}

	// Returns just the platform data for the cat to stand on
	public Godot.Collections.Array GetWindowPlatforms()
	{
		CacheOwnWindows();

		var platforms = new Godot.Collections.Array();

		EnumWindows((hWnd, lParam) =>
		{
			if (_ownHandles.Contains(hWnd)) return true;
			if (!IsWindowVisible(hWnd)) return true;
			if (IsIconic(hWnd)) return true;

			int titleLength = GetWindowTextLength(hWnd);
			if (titleLength == 0) return true;

			uint exStyle = GetWindowStyle(hWnd, GwlExStyle);
			if ((exStyle & WsExToolWindow) != 0) return true;

			if (GetWindowRect(hWnd, out RECT rect))
			{
				int width = rect.Right - rect.Left;
				int height = rect.Bottom - rect.Top;
				if (width <= 0 || height <= 0) return true;

				StringBuilder titleBuilder = new StringBuilder(titleLength + 1);
				GetWindowText(hWnd, titleBuilder, titleBuilder.Capacity);

				platforms.Add(new Godot.Collections.Dictionary
				{
					{ "title", titleBuilder.ToString() },
					{ "left", (float)rect.Left },
					{ "right", (float)rect.Right },
					{ "y", (float)rect.Top },
					{ "bottom", (float)rect.Bottom }
				});
			}

			return true;
		}, IntPtr.Zero);

		// GD.Print($"GetWindowPlatforms found {platforms.Count} platforms");
		return platforms;
	}
}
