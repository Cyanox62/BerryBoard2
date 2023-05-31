using Newtonsoft.Json;

namespace BerryBoard2.Model.Libs
{
	internal static class ObsReqGen
	{
		private const string requestId = "f819dcf0-89cc-11eb-8f0e-382c4ac93b9c"; // idk what this means

		internal static string Identify()
		{
			var req = new
			{
				op = 1,
				d = new
				{
					rpcVersion = 1,
					eventSubscriptions = 33
				}
			};

			return JsonConvert.SerializeObject(req);
		}

		internal static string ChangeScene(string sceneName)
		{
			var req = new
			{
				op = 6,
				d = new
				{
					requestType = "SetCurrentProgramScene",
					requestId,
					requestData = new
					{
						sceneName
					}
				}
			};

			return JsonConvert.SerializeObject(req);
		}

		internal static string StartRecording()
		{
			var req = new
			{
				op = 6,
				d = new
				{
					requestType = "StartRecord",
					requestId
				}
			};

			return JsonConvert.SerializeObject(req);
		}

		internal static string PauseRecording()
		{
			var req = new
			{
				op = 6,
				d = new
				{
					requestType = "ToggleRecordPause",
					requestId
				}
			};

			return JsonConvert.SerializeObject(req);
		}

		internal static string StopRecording()
		{
			var req = new
			{
				op = 6,
				d = new
				{
					requestType = "StopRecord",
					requestId
				}
			};

			return JsonConvert.SerializeObject(req);
		}
	}
}
