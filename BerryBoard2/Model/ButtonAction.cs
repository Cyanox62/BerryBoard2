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

		// Browser
		Home,
		Search,
		Refresh,
		Forward,
		Backward,

		// Keyboard
		Cut,
		Copy,
		Paste,
		CustomInput,

		// Devices
		ChangeSpeakers,
		ChangeMicrophone,
		MuteMicrophone,

		// System
		StartProcess,
		OpenWebsite,
		CustomScript,
		Lock,
		Sleep,
		PowerOff
	}
}
