using System.Windows.Controls;

namespace BerryBoard2.Model
{
	internal class ButtonAction
	{
		public Action action = Action.None;
		public string param = string.Empty;
	}

	internal enum Action
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
		MuteMicrophone
	}
}
