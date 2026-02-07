using Godot;
using System;
using System.Runtime.InteropServices;

public partial class SystemTray : Node
{
	// --- Window style functions ---
	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

	[DllImport("user32.dll")]
	private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

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
	private const int WsExToolWindow = 0x80;      // Hidden from taskbar + Alt+Tab
	private const int WsExAppWindow = 0x40000;     // Shown in taskbar (we remove this)

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

	// Set these in the Inspector or override in _Ready
	[Export] public string TooltipText = "My Godot App";
	[Export] public string IconFileName = "icon.ico";

	public override void _Ready()
	{
		_hWnd = (IntPtr)DisplayServer.WindowGetNativeHandle(
			DisplayServer.HandleType.WindowHandle,
			GetWindow().GetWindowId()
		);

		HideFromTaskbar();
		AddTrayIcon();
	}

	private void HideFromTaskbar()
	{
		uint currentStyle = GetWindowLong(_hWnd, GwlExStyle);
		currentStyle &= ~(uint)WsExAppWindow;   // Remove "show in taskbar"
		currentStyle |= (uint)WsExToolWindow;    // Add "tool window" style
		SetWindowLong(_hWnd, GwlExStyle, currentStyle);
	}

	private void AddTrayIcon()
	{
		// Load .ico file from next to the executable
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
