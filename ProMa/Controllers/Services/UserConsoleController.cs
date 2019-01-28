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
	public class UserConsoleController : Controller
    {
		[HttpPost]
		public List<ProMaUser> UsersForFriendRequest([FromForm]string name)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if(name == null)
			{
				name = String.Empty;
			}

			List<ProMaUser> fittingUsers = ProMaUserHandler.GetAllUsers().Where(x => x.UserName.ToLower().Contains(name.ToLower()) && x.UserId != user.UserId).ToList();

			fittingUsers = fittingUsers.Where(x => x.UserName != "DemoAccount").ToList();

			List<ProMaUser> returnThis = new List<ProMaUser>();

			List<FriendshipRequest> excludeBecauseRequested = FriendshipRequestHandler.GetRequestsForUser(user.UserId);
			List<ProMaUser> excludeBecauseAlreadyFriends = FriendshipHandler.GetUserFriends(user.UserId);

			fittingUsers.ForEach(x =>
			{
				if (!excludeBecauseRequested.Any(y => y.SenderId == x.UserId) && !excludeBecauseRequested.Any(y => y.RecipientId == x.UserId))
				{
					if (!excludeBecauseAlreadyFriends.Any(y => y.UserId == x.UserId))
						returnThis.Add(x);
				}
			});

			return returnThis;
		}

		[HttpPost]
		public void SendFriendRequest([FromForm]int toUser)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (user.IsDemo)
				throw new Exception("Can't send friend requests as the Demo account");

			ProMaUser target = ProMaUserHandler.GetUser(toUser);

			if (target == null)
				throw new Exception("No user with that ID exists");

			if (target.IsDemo)
				throw new Exception("Can't send friend requests to the Demo account");

			FriendshipRequest newRequest = new FriendshipRequest();
			newRequest.SenderId = user.UserId;
			newRequest.RecipientId = toUser;

			FriendshipRequestHandler.AddFriendshipRequest(newRequest);
		}

		[HttpPost]
		public List<FriendshipRequest> GetFriendshipRequests()
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			List<FriendshipRequest> requests = FriendshipRequestHandler.GetRequestsForUser(user.UserId).Where(x => x.RecipientId == user.UserId || x.SenderId == user.UserId).ToList();

			requests.ForEach(x => { x.Sender = ProMaUserHandler.GetUser(x.SenderId); x.Recipient = ProMaUserHandler.GetUser(x.RecipientId); });

			return requests;
		}

		[HttpPost]
		public void AcceptFriendRequest([FromForm]int fromUser)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			FriendshipRequestHandler.AcceptRequestBetweenUsers(user.UserId, fromUser);
		}

		[HttpPost]
		public void RejectFriendRequest([FromForm]int fromUser)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			FriendshipRequestHandler.RejectRequestBetweenUsers(user.UserId, fromUser);
		}

		[HttpPost]
		public void CancelFriendRequest([FromForm]int recipient)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			FriendshipRequestHandler.RejectRequestBetweenUsers(recipient, user.UserId);
		}

		[HttpPost]
		public void RemoveFriend([FromForm]int fromUser)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			FriendshipHandler.RemoveFriendship(user.UserId, fromUser);
		}

		[HttpPost]
		public void ChangeEnterPref([FromForm]bool value)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			user.EnterIsNewLinePref = value;

			ProMaUserHandler.UpdateUser(user);
		}

		[HttpPost]
		public void UpdateEmailAddress([FromForm]string emailAddress)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (user.IsDemo)
				throw new Exception("Can't change Demo Account email address");

			user.EmailAddress = emailAddress;

			ProMaUserHandler.UpdateUser(user);
		}

		[HttpPost]
		public void ChangeUsername([FromForm]string userName)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (!ProMaUser.VerifyName(userName))
				throw new Exception("Invalid user name");

			// make sure no user with the same name
			ProMaUser existingUser = ProMaUserHandler.GetUserByUserName(userName);

			if (existingUser.IsDemo)
				throw new Exception("Can't change Demo Account user name");

			if (existingUser == null)
			{
				user.UserName = userName;

				ProMaUserHandler.UpdateUser(user);
			}
			else
			{
				throw new Exception("User already exists by that name");
			}
		}

		[HttpPost]
		public void ChangePassword([FromForm]string md5Password)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (string.IsNullOrEmpty(md5Password))
				throw new Exception("Invalid password");

			if (user.IsDemo)
				throw new Exception("Can't change Demo Account password");

			user.HashedPassword = ProMaUser.ComputeSHA256(md5Password);

			ProMaUserHandler.UpdateUser(user);
		}
	}
}
