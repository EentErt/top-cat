using Godot;
using System;
using System.Runtime.InteropServices;

public partial class TransparentWindow : Node
{
	// 32-bit versions
	[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
	private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern uint GetWindowLong32(IntPtr hWnd, int nIndex);

	// 64-bit versions
	[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
	private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
	private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

	// Helpers that pick the right version automatically
	private static void SetWindowStyle(IntPtr hWnd, int nIndex, uint value)
	{
		if (IntPtr.Size == 8)
			SetWindowLong64(hWnd, nIndex, new IntPtr(value));
		else
			SetWindowLong32(hWnd, nIndex, value);
	}

	private static uint GetWindowStyle(IntPtr hWnd, int nIndex)
	{
		if (IntPtr.Size == 8)
			return (uint)GetWindowLong64(hWnd, nIndex).ToInt64();
		else
			return GetWindowLong32(hWnd, nIndex);
	}

	private const int GwlExStyle = -20;
	private const uint WsExLayered = 0x80000;
	private const uint WsExTransparent = 0x20;

	[Export] public NodePath TargetWindowPath;

	private IntPtr _hWnd;
	private bool _initialized = false;

	public override void _Ready()
	{
		CallDeferred(nameof(InitWindow));
	}

	private void InitWindow()
	{
		// Print main window for comparison
		var mainWindow = GetTree().Root;
		int mainId = mainWindow.GetWindowId();
		IntPtr mainHandle = (IntPtr)DisplayServer.WindowGetNativeHandle(
			DisplayServer.HandleType.WindowHandle, mainId);
		GD.Print($"Main window - ID: {mainId}, Handle: {mainHandle}");

		// Find target window
		Window targetWindow = null;

		if (TargetWindowPath != null && !TargetWindowPath.IsEmpty)
		{
			targetWindow = GetNode<Window>(TargetWindowPath);
		}
		else if (GetParent() is Window parentWindow)
		{
			targetWindow = parentWindow;
		}

		if (targetWindow == null)
		{
			GD.PrintErr("TransparentWindow: No target window found!");
			return;
		}

		if (!targetWindow.Visible)
			targetWindow.Visible = true;

		int windowId = targetWindow.GetWindowId();
		_hWnd = (IntPtr)DisplayServer.WindowGetNativeHandle(
			DisplayServer.HandleType.WindowHandle, windowId);

		GD.Print($"Target window - Name: {targetWindow.Name}, ID: {windowId}, Handle: {_hWnd}");
		GD.Print($"Same window? {mainHandle == _hWnd}");

		// Debug styles
		uint styleBefore = GetWindowStyle(_hWnd, GwlExStyle);
		GD.Print($"Style before: 0x{styleBefore:X}");

		SetWindowStyle(_hWnd, GwlExStyle, styleBefore | WsExLayered);
		uint styleAfterLayered = GetWindowStyle(_hWnd, GwlExStyle);
		GD.Print($"Style after layered: 0x{styleAfterLayered:X}");

		SetWindowStyle(_hWnd, GwlExStyle, styleAfterLayered | WsExTransparent);
		uint styleAfterTransparent = GetWindowStyle(_hWnd, GwlExStyle);
		GD.Print($"Style after transparent: 0x{styleAfterTransparent:X}");

		bool hasLayered = (styleAfterTransparent & WsExLayered) != 0;
		bool hasTransparent = (styleAfterTransparent & WsExTransparent) != 0;
		GD.Print($"Has WS_EX_LAYERED: {hasLayered}");
		GD.Print($"Has WS_EX_TRANSPARENT: {hasTransparent}");

		_initialized = true;
	}

	public void SetClickThrough(bool clickthrough)
	{
		if (!_initialized) return;

		uint currentStyle = GetWindowStyle(_hWnd, GwlExStyle);
		if (clickthrough)
		{
			SetWindowStyle(_hWnd, GwlExStyle, currentStyle | WsExTransparent);
		}
		else
		{
			currentStyle &= ~WsExTransparent;
			SetWindowStyle(_hWnd, GwlExStyle, currentStyle);
		}
	}
}
