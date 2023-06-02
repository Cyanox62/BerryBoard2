using System.Runtime.InteropServices;

namespace BerryBoard2.Model.Libs
{
	internal static class SendKeys
	{
		[DllImport("user32.dll")]
		static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

		#region App Codes
		const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
		const uint KEYEVENTF_KEYUP = 0x0002;
		const byte VK_LSHIFT = 0xA0;
		const byte VK_LCONTROL = 0xA2;
		const byte VK_LMENU = 0xA4;  // left alt
		#endregion

		public static void Send(string key, bool ctrl = false, bool shift = false, bool alt = false)
		{
			if (ctrl) keybd_event(VK_LCONTROL, 0, KEYEVENTF_EXTENDEDKEY, 0);
			if (alt) keybd_event(VK_LMENU, 0, KEYEVENTF_EXTENDEDKEY, 0);
			if (shift) keybd_event(VK_LSHIFT, 0, KEYEVENTF_EXTENDEDKEY, 0);

			foreach (char c in key)
			{
				bool uppercase = char.IsUpper(c);
				if (uppercase && !shift) keybd_event(VK_LSHIFT, 0, KEYEVENTF_EXTENDEDKEY, 0);  // Press shift only for uppercase letters, if shift is not already being pressed

				byte vk = (byte)char.ToUpper(c);
				keybd_event(vk, 0, KEYEVENTF_EXTENDEDKEY, 0);
				keybd_event(vk, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

				if (uppercase && !shift) keybd_event(VK_LSHIFT, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);  // Release shift only if we had manually pressed it for an uppercase letter
			}

			if (shift) keybd_event(VK_LSHIFT, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
			if (alt) keybd_event(VK_LMENU, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
			if (ctrl) keybd_event(VK_LCONTROL, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
		}
	}

}
