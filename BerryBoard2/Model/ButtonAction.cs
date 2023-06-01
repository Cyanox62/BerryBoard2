using System.Windows.Controls;

namespace BerryBoard2.Model
{
	internal class ButtonAction
	{
		public KeyAction action = KeyAction.None;
		public string param = string.Empty;
	}

	internal enum KeyAction
	{
		None,

		// OBS
		ChangeScene,
		StartStreaming,
		StopStreaming,
		StartRecording,
		StopRecording,
		PauseRecording,

		// Media
		VolumeUp,
		VolumeDown,
		MuteAudio,
		PlayPause,
		NextTrack,
		PreviousTrack,

		// Keyboard
		Cut,
		Copy,
		Paste,
		CustomText,

		// System
		StartProcess,
		PlayAudio,
		OpenWebsite,
		MuteMicrophone,
		PowerOff
	}
}
