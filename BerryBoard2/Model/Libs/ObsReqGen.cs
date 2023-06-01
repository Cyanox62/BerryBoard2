using Newtonsoft.Json;
using System.Collections.Generic;

namespace BerryBoard2.Model.Libs
{
	internal static class ObsReqGen
	{
		private const string requestId = "f819dcf0-89cc-11eb-8f0e-382c4ac93b9c"; // idk what this means

		internal static string Identify(string auth)
		{
			var req = new Dictionary<string, object>
			{
				["op"] = 1,
				["d"] = new Dictionary<string, object>
				{
					["rpcVersion"] = 1,
					["eventSubscriptions"] = 33
				}
			};

			if (auth != null)
			{
				((Dictionary<string, object>)req["d"]).Add("authentication", auth);
			}

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

		internal static string StartStreaming()
		{
			var req = new
			{
				op = 6,
				d = new
				{
					requestType = "StartStream",
					requestId
				}
			};

			return JsonConvert.SerializeObject(req);
		}

		internal static string StopStreaming()
		{
			var req = new
			{
				op = 6,
				d = new
				{
					requestType = "StopStream",
					requestId
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
