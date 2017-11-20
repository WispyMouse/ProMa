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
	public class ChoresController : Controller
    {
		[HttpPost]
		public void AddNewChore([FromForm]string newItemName)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			SharedChore newChore = new SharedChore();
			newChore.ChoreName = newItemName;
			SharedChoreHandler.AddSharedChore(newChore, user.UserId);
		}

		public class GetChoreItemsRequestObject
		{
			public int year { get; set; }
			public int month { get; set; }
			public int day { get; set; }
		}

		[HttpPost]
		public List<CompletedChore> GetChoreItems([FromBody]GetChoreItemsRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			DateTime dayForRequest = new DateTime(requestObject.year, requestObject.month, requestObject.day).Date;

			List<CompletedChore> returnThis = CompletedChoreHandler.GetChoreItemsForDateAndUser(user.UserId, dayForRequest);

			// we need to hydrate the data appropriately
			foreach (CompletedChore curChore in returnThis)
			{
				curChore.SharedChore = SharedChoreHandler.GetSharedChore(curChore.SharedChoreId);

				if (curChore.UserId.HasValue)
				{
					curChore.CompletedUser = ProMaUserHandler.GetUser(curChore.UserId.Value);
				}

				curChore.SharedChore.Membership = SharedChoreMembershipHandler.GetSharedChoreMembership(curChore.SharedChoreId, user.UserId);

				// find the last completed version of this chore
				// we only need to do this if this chore isn't complete, because it won't be displayed in the ui otherwise
				if (!curChore.Completed)
				{
					CompletedChore lastCompletion = CompletedChoreHandler.GetPreviousCompletedChore(curChore);

					if (lastCompletion != null)
					{
						curChore.LastDoneUser = ProMaUserHandler.GetUser(lastCompletion.UserId.Value);
						curChore.LastDoneTime = lastCompletion.ChoreDate;
					}
				}
			}

			return returnThis.ToList();
		}

		[HttpPost]
		public void RemoveChoreMembership([FromForm]int choreId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			SharedChoreMembershipHandler.RemoveSharedChoreMembership(choreId, user.UserId);
		}

		public class ChangeChoreItemCompletionRequestObject
		{
			public int choreId { get; set; }
			public bool completed { get; set; }
			public int year { get; set; }
			public int month { get; set; }
			public int day { get; set; }
		}

		[HttpPost]
		public void ChangeChoreItemCompletion([FromBody]ChangeChoreItemCompletionRequestObject requestObject)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				ProMaUser user = DataController.LoggedInUser;

				if (user == null)
					throw new NotLoggedInException();

				DateTime dayForRequest = new DateTime(requestObject.year, requestObject.month, requestObject.day).Date;

				if (requestObject.completed)
				{
					CompletedChoreHandler.CompleteChore(requestObject.choreId, dayForRequest, user.UserId);
				}
				else
				{
					CompletedChoreHandler.UnCompleteChore(requestObject.choreId, dayForRequest);
				}
			}
		}

		[HttpPost]
		public List<ProMaUser> GetUsersNotAssignedToChore([FromForm]int choreId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				ProMaUser user = DataController.LoggedInUser;

				if (user == null)
					throw new NotLoggedInException();

				// get each shared chore membership for this chore
				List<SharedChoreMembership> memberships = SharedChoreMembershipHandler.GetSharedChoreMembershipsForChore(choreId);

				return FriendshipHandler.GetUserFriends(user.UserId).Where(x => !memberships.Any(y => y.UserId == x.UserId)).ToList();
			}
		}

		public class AssignUserToChoreRequestObject
		{
			public int choreId { get; set; }
			public int userId { get; set; }
		}

		[HttpPost]
		public void AssignUserToChore([FromBody]AssignUserToChoreRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (FriendshipHandler.GetUserFriends(user.UserId).Any(x => x.UserId == requestObject.userId))
			{
				SharedChoreMembershipHandler.AddSharedChoreMembership(requestObject.choreId, requestObject.userId);
			}
			else
			{
				throw new Exception("User not in friends list");
			}
		}

		[HttpPost]
		public void RememberSorting([FromForm]SerializableIntIntPair[] pairings)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			SharedChoreMembershipHandler.SaveSortingOrders(pairings, user.UserId);
		}

		public class SaveChoreAlertRequestObject
		{
			public int choreId { get; set; }
			public int alertHour { get; set; }
			public int alertMinute { get; set; }
		}

		[HttpPost]
		public void SaveChoreAlert([FromBody]SaveChoreAlertRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			SharedChoreMembership toUpdate = SharedChoreMembershipHandler.GetSharedChoreMembership(requestObject.choreId, user.UserId);

			if (requestObject.alertHour == -1)
			{
				toUpdate.AlertHour = null;
				toUpdate.AlertMinute = null;
			}
			else
			{
				toUpdate.AlertHour = requestObject.alertHour;
				toUpdate.AlertMinute = requestObject.alertMinute;
			}

			SharedChoreMembershipHandler.UpdateSharedChoreMembership(toUpdate);
		}
	}
}
