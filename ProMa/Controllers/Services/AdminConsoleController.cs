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
	public class AdminConsoleController : Controller
    {
		[HttpGet]
		public List<ProMaUser> GetMostRecentUsers()
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (!user.IsAdmin)
				throw new AdminAccessRequiredException();

			return ProMaUserHandler.ThisCache.OrderByDescending(x => x.JoinTime).Take(5).ToList();
		}

		[HttpGet]
		public void ResetCaches()
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (!user.IsAdmin)
				throw new AdminAccessRequiredException();

			lock (ProMaUserHandler.ThisCache)
			{
				ProMaUserHandler.ThisCache = null;
			}
			lock (NoteTypeHandler.ThisCache)
			{
				NoteTypeHandler.ThisCache = null;
			}

			CompletedChoreHandler.AddToEveryUserChoreCacheIterator();
			FriendshipRequestHandler.AddToEveryUserFriendshipCacheIterator();
		}
	}
}
