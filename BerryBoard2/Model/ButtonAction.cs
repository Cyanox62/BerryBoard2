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

		// Media
		VolumeUp,
		VolumeDown,
		MuteAudio,
		PlayPause,
		NextTrack,
		PreviousTrack,

		// System
		StartProcess,
		MuteMicrophone,

		// OBS
		ChangeScene,
		StartStreaming,
		StopStreaming,
		StartRecording,
		StopRecording,
		PauseRecording
	}
}
