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
		[HttpGet]
		public void AddNewChore(string newItemName)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			SharedChore newChore = new SharedChore();
			newChore.ChoreName = newItemName;
			SharedChoreHandler.AddSharedChore(newChore, user.UserId);
		}

		[HttpGet]
		public List<CompletedChore> GetChoreItems(int year, int month, int day)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			DateTime dayForRequest = new DateTime(year, month, day).Date;

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

		[HttpGet]
		public void RemoveChoreMembership(int choreId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			SharedChoreMembershipHandler.RemoveSharedChoreMembership(choreId, user.UserId);
		}

		[HttpGet]
		public void ChangeChoreItemCompletion(int choreId, bool completed, int year, int month, int day)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				ProMaUser user = DataController.LoggedInUser;

				if (user == null)
					throw new NotLoggedInException();

				DateTime dayForRequest = new DateTime(year, month, day).Date;

				if (completed)
				{
					CompletedChoreHandler.CompleteChore(choreId, dayForRequest, user.UserId);
				}
				else
				{
					CompletedChoreHandler.UnCompleteChore(choreId, dayForRequest);
				}
			}
		}

		[HttpGet]
		public List<ProMaUser> GetUsersNotAssignedToChore(int choreId)
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

		[HttpGet]
		public void AssignUserToChore(int choreId, int userId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (FriendshipHandler.GetUserFriends(user.UserId).Any(x => x.UserId == userId))
			{
				SharedChoreMembershipHandler.AddSharedChoreMembership(choreId, userId);
			}
			else
			{
				throw new Exception("User not in friends list");
			}
		}

		[HttpGet]
		public void RememberSorting(SerializableIntIntPair[] pairings)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			SharedChoreMembershipHandler.SaveSortingOrders(pairings, user.UserId);
		}

		[HttpGet]
		public void SaveChoreAlert(int choreId, int alertHour, int alertMinute)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			SharedChoreMembership toUpdate = SharedChoreMembershipHandler.GetSharedChoreMembership(choreId, user.UserId);

			if (alertHour == -1)
			{
				toUpdate.AlertHour = null;
				toUpdate.AlertMinute = null;
			}
			else
			{
				toUpdate.AlertHour = alertHour;
				toUpdate.AlertMinute = alertMinute;
			}

			SharedChoreMembershipHandler.UpdateSharedChoreMembership(toUpdate);
		}
	}
}
