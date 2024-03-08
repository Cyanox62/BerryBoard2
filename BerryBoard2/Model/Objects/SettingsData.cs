namespace BerryBoard2.Model.Objects
{
	public class SettingsData
	{
		public bool ObsEnable { get; set; } = false;
		public int ObsPort { get; set; } = -1;
		public string ObsAuth { get; set; } = string.Empty;

		public bool MinimizeToTray { get; set; } = false;
	}
}
