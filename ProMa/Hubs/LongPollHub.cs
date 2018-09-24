using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ProMa.Models;

namespace ProMa.Hubs
{
    public class LongPollHub : Hub
    {
		public async Task LongPoll(int userId, int friendshipCacheVersion, int choreCacheVersion)
		{
			int newFriendshipCacheVersion = friendshipCacheVersion;
			int newChoreCacheVersion = choreCacheVersion;

			do
			{
				await Task.Delay(100);
				newFriendshipCacheVersion = FriendshipRequestHandler.LongPollInternal(userId);
				newChoreCacheVersion = CompletedChoreHandler.LongPollInternal(userId);
			} while
				(
					friendshipCacheVersion == newFriendshipCacheVersion &&
					choreCacheVersion == newChoreCacheVersion
				);

			await Clients.Caller.SendAsync("LongPollPop", newChoreCacheVersion, newFriendshipCacheVersion);
		}
    }
}
