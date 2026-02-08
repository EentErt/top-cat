using Godot;
using System;
using System.Runtime.InteropServices;

public partial class SystemTray : Node
{
	// --- 32-bit window style functions ---
	[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
	private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern uint GetWindowLong32(IntPtr hWnd, int nIndex);

	// --- 64-bit window style functions ---
	[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
	private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
	private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

	// --- Helpers that pick the right version ---
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

	// --- System tray functions ---
	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern IntPtr LoadImage(
		IntPtr hInst, string name, uint type,
		int cx, int cy, uint fuLoad);

	[DllImport("user32.dll")]
	private static extern bool DestroyIcon(IntPtr hIcon);

	// Window style constants
	private const int GwlExStyle = -20;
	private const uint WsExToolWindow = 0x80;
	private const uint WsExAppWindow = 0x40000;

	// Tray icon constants
	private const uint NimAdd = 0x00000000;
	private const uint NimDelete = 0x00000002;
	private const uint NifIcon = 0x00000002;
	private const uint NifTip = 0x00000004;
	private const uint ImageIcon = 1;
	private const uint LrLoadFromFile = 0x00000010;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct NOTIFYICONDATA
	{
		public int cbSize;
		public IntPtr hWnd;
		public int uID;
		public uint uFlags;
		public uint uCallbackMessage;
		public IntPtr hIcon;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string szTip;
	}

	private IntPtr _hWnd;
	private IntPtr _iconHandle;
	private NOTIFYICONDATA _notifyData;

	[Export] public string TooltipText = "My Godot App";
	[Export] public string IconFileName = "icon.ico";

	public override void _Ready()
	{
		// Get the main window handle
		_hWnd = (IntPtr)DisplayServer.WindowGetNativeHandle(
			DisplayServer.HandleType.WindowHandle,
			GetTree().Root.GetWindowId()
		);

		// Hide main window from taskbar
		HideFromTaskbar(_hWnd);

		// Hide all sub-windows from taskbar
		HideAllSubWindows(GetTree().Root);

		// Add a single tray icon for the whole app
		AddTrayIcon();
	}

	private void HideFromTaskbar(IntPtr handle)
	{
		uint currentStyle = GetWindowStyle(handle, GwlExStyle);
		currentStyle &= ~WsExAppWindow;
		currentStyle |= WsExToolWindow;
		SetWindowStyle(handle, GwlExStyle, currentStyle);
	}

	private void HideAllSubWindows(Node node)
	{
		foreach (var child in node.GetChildren())
		{
			if (child is Window window && window != GetTree().Root)
			{
				CallDeferred(nameof(DeferredHideWindow), window.GetPath());
			}
			HideAllSubWindows(child);
		}
	}

	private void DeferredHideWindow(NodePath path)
	{
		var window = GetNode<Window>(path);

		if (!window.Visible)
			window.Visible = true;

		IntPtr handle = (IntPtr)DisplayServer.WindowGetNativeHandle(
			DisplayServer.HandleType.WindowHandle,
			window.GetWindowId()
		);

		HideFromTaskbar(handle);
		GD.Print($"Hidden from taskbar: {window.Name}");
	}

	// Call this from GDScript if you add new windows at runtime
	public void HideWindowFromTaskbar(Window window)
	{
		if (!window.Visible)
			window.Visible = true;

		IntPtr handle = (IntPtr)DisplayServer.WindowGetNativeHandle(
			DisplayServer.HandleType.WindowHandle,
			window.GetWindowId()
		);

		HideFromTaskbar(handle);
	}

	private void AddTrayIcon()
	{
		string iconPath = System.IO.Path.Combine(
			System.IO.Path.GetDirectoryName(OS.GetExecutablePath()),
			IconFileName
		);

		_iconHandle = LoadImage(IntPtr.Zero, iconPath, ImageIcon, 16, 16, LrLoadFromFile);

		_notifyData = new NOTIFYICONDATA();
		_notifyData.cbSize = Marshal.SizeOf(_notifyData);
		_notifyData.hWnd = _hWnd;
		_notifyData.uID = 1;
		_notifyData.uFlags = NifIcon | NifTip;
		_notifyData.hIcon = _iconHandle;
		_notifyData.szTip = TooltipText;

		Shell_NotifyIcon(NimAdd, ref _notifyData);
	}

	private void RemoveTrayIcon()
	{
		Shell_NotifyIcon(NimDelete, ref _notifyData);
		if (_iconHandle != IntPtr.Zero)
			DestroyIcon(_iconHandle);
	}

	public override void _ExitTree()
	{
		RemoveTrayIcon();
	}
}
