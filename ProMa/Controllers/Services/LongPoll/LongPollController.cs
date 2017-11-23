using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ProMa.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace ProMa.Controllers
{
	public class LongPollController : Controller
	{

		public class LongPollRequestObject
		{
			public int userId { get; set; }
			public int choreCacheVersion { get; set; }
			public int friendshipCacheVersion { get; set; }
		}

		[HttpPost]
		public List<KeyValuePair<string, int>> LongPoll([FromBody]LongPollRequestObject requestObject)
		{
			Task<List<KeyValuePair<string, int>>> thisTask = Task.Run(() => LongPollInternal(requestObject));
			thisTask.Wait();
			return thisTask.Result;
		}
		
		public static async Task<List<KeyValuePair<string, int>>> LongPollInternal(LongPollRequestObject requestObject)
		{
			List<KeyValuePair<string, int>> returnThis = new List<KeyValuePair<string, int>>();

			do
			{
				await Task.Delay(100);
				returnThis.Clear();
				returnThis.Add(new KeyValuePair<string, int>("friendshipCacheVersion", FriendshipRequestHandler.LongPollInternal(requestObject.userId)));
				returnThis.Add(new KeyValuePair<string, int>("choreCacheVersion", CompletedChoreHandler.LongPollInternal(requestObject.userId)));
			} while
				(
					requestObject.friendshipCacheVersion == returnThis.First(x => x.Key == "friendshipCacheVersion").Value &&
					requestObject.choreCacheVersion == returnThis.First(x => x.Key == "choreCacheVersion").Value
				);

			return returnThis;
		}

	}
}
