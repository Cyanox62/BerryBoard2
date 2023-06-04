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
		StartStreaming,
		StopStreaming,
		StartRecording,
		StopRecording,
		PauseRecording,
		ChangeScene,

		// Media
		PlayPause,
		NextTrack,
		PreviousTrack,
		VolumeUp,
		VolumeDown,
		MuteAudio,
		PlayAudio,

		// Keyboard
		Cut,
		Copy,
		Paste,
		CustomText,

		// Devices
		ChangeSpeakers,
		ChangeMicrophone,
		MuteMicrophone,

		// System
		StartProcess,
		OpenWebsite,
		PowerOff
	}
}
