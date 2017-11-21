using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace ProMa.Models
{
	public static class TransactionLocks
	{
		public static string StringLock = string.Empty;
	}

	public static class ProMaUserHandler
	{
		public static List<ProMaUser> ThisCache
		{
			get
			{
				if (_ThisCache == null)
				{
					using (ProMaDB scope = new ProMaDB())
					{
						_ThisCache = scope.ProMaUsers.ToList();
					}
				}

				return _ThisCache;
			}
			set
			{
				_ThisCache = value;
			}
		}
		private static List<ProMaUser> _ThisCache = null;

		public static void AddProMaUser(ProMaUser toAdd)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.ProMaUsers.Add(toAdd);
				scope.Entry(toAdd).State = EntityState.Added;
				scope.SaveChanges();
			}

			lock (ThisCache)
			{
				ThisCache.Add(toAdd);
			}
		}

		public static void UpdateUser(ProMaUser toUpdate)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.ProMaUsers.Attach(toUpdate);
				scope.Entry(toUpdate).State = EntityState.Modified;
				scope.SaveChanges();
			}

			lock (ThisCache)
			{
				ThisCache.RemoveAll(x => x.UserId == toUpdate.UserId);
				ThisCache.Add(toUpdate);
			}
		}

		public static ProMaUser GetUser(int userId)
		{
			return ThisCache.FirstOrDefault(x => x.UserId == userId);
		}

		public static ProMaUser GetUserByUserName(string userName)
		{
			return ThisCache.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower());
		}

		public static List<ProMaUser> GetAllUsers()
		{
			return ThisCache;
		}
	}

	public static class PostedNoteHandler
	{
		public static List<PostedNote> GetAllNotesOfType(int noteTypeId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.PostedNotes.Where(x => x.NoteTypeId == noteTypeId).ToList();
			}
		}

		public static void AddPostedNote(PostedNote toAdd)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.PostedNotes.Add(toAdd);
				scope.Entry(toAdd).State = EntityState.Added;
				scope.SaveChanges();
			}
		}

		public static void UpdatePostedNote(PostedNote toUpdate)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.PostedNotes.Attach(toUpdate);
				scope.Entry(toUpdate).State = EntityState.Modified;
				scope.SaveChanges();
			}
		}

		public static PostedNote GetNote(int noteId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.PostedNotes.FirstOrDefault(x => x.NoteId == noteId);
			}
		}

		public static PostedNote.PostedNotePayload GetPayloadNote(PostedNote raw, int? relevantUserId)
		{
			List<PostedNote.PostedNotePayload> returnThis = new List<PostedNote.PostedNotePayload>();

			List<NoteType> noteTypes = new List<NoteType>();

			if (relevantUserId.HasValue)
			{
				noteTypes = NoteTypeHandler.GetNoteTypesForUser(relevantUserId.Value);
			}

			return new PostedNote.PostedNotePayload(raw, noteTypes);
		}

		public static List<PostedNote.PostedNotePayload> GetPayloadNotes(List<PostedNote> raw, int? relevantUserId)
		{
			List<PostedNote.PostedNotePayload> returnThis = new List<PostedNote.PostedNotePayload>();

			List<NoteType> noteTypes = new List<NoteType>();

			if (relevantUserId.HasValue)
			{
				noteTypes = NoteTypeHandler.GetNoteTypesForUser(relevantUserId.Value);
			}

			foreach (PostedNote curNote in raw)
			{
				returnThis.Add(new PostedNote.PostedNotePayload(curNote, noteTypes));
			}

			return returnThis;
		}

		public static List<PostedNote> GetNotes(int shownToUser, bool includeInactive, bool onlyInactive, bool includeComplete, int onlyThisNoteId, bool includeHibernatedNotes)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				var returnThis =
					// User posted this note and it has no note type
					(from pn in scope.PostedNotes
					 where pn.UserId == shownToUser
					 && pn.NoteTypeId == null
					 select pn)
					  .Union(
					  // Note is of a type this user has membership in
					  from pn in scope.PostedNotes
					  join nt in scope.NoteTypes on pn.NoteTypeId equals nt.NoteTypeId
					  join ntm in scope.NoteTypeMemberships on nt.NoteTypeId equals ntm.NoteTypeId
					  where ntm.UserId == shownToUser
					  && (includeHibernatedNotes || nt.Hibernated == false)
					  select pn
					  );

				if (!includeInactive)
				{
					returnThis = returnThis.Where(x => x.Active);
				}
				else if (onlyInactive)
				{
					returnThis = returnThis.Where(x => !x.Active);
				}

				if (!includeComplete)
				{
					returnThis = returnThis.Where(x => !x.Completed);
				}

				if (onlyThisNoteId != -1)
				{
					returnThis = returnThis.Where(x => x.NoteTypeId == onlyThisNoteId);
				}

				return returnThis.ToList();
			}
		}

		public static string CleanNoteText(string incoming)
		{
			string returnThis = incoming;

			// double new lines in chrome result in <div><br></div>, which is a lot of whitespace; trim that down by one to reduce extra whitespace
			returnThis = Regex.Replace(returnThis, @"(<div>\s*?<br/?>\s*?</div>)", @"\r\n");

			// additionally chrome sometimes adds <br><div>; remove the first <br>
			returnThis = Regex.Replace(returnThis, @"(<br/?>\s*?<div>)", @"<div>");

			// chrome uses <div></div> after hitting enter in content editables
			// internet explorer uses <p> in content editables, including the first element
			// replace back-to-back </div><div> with a single new line; then replace each <div> (unless it starts the note) and </div> (unless it ends the note) with a new line, otherwise remove them (same with <p> elements)
			returnThis = Regex.Replace(returnThis, @"(</div>\s*?<div>)|(</p>\s*?<p>)", @"\r\n");
			returnThis = Regex.Replace(returnThis, @"</((div)|(p))>(?!\\r\\n)(?!$)", @"\r\n");
			returnThis = Regex.Replace(returnThis, @"(?<!^)<((div)|(p))>", @"\r\n");
			returnThis = Regex.Replace(returnThis, @"(<div>)|(</div>)|(<p>)|(</p>)", @"");

			// firefox uses <br> after hitting enter in content editables
			returnThis = Regex.Replace(returnThis, @"<br/?>", @"\r\n");

			return returnThis;
		}
	}

	public static class CompletedChoreHandler
	{
		public static Dictionary<int, int> UserIterators = new Dictionary<int, int>();

		public static void CompleteChore(int sharedChoreId, DateTime forDay, int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				CompletedChore newChore = new CompletedChore();
				newChore.SharedChoreId = sharedChoreId;
				newChore.ChoreDate = forDay;
				newChore.Completed = true;
				newChore.UserId = userId;
				newChore.PostedTime = ProMaUser.NowTime();

				scope.CompletedChores.Add(newChore);

				scope.SaveChanges();
			}

			// change the cache for each user in this membership
			List<SharedChoreMembership> memberships = SharedChoreMembershipHandler.GetSharedChoreMembershipsForChore(sharedChoreId);

			foreach (SharedChoreMembership curMembership in memberships)
			{
				CompletedChoreHandler.AddToUserChoreCacheIterator(curMembership.UserId);
			}
		}

		public static void UnCompleteChore(int sharedChoreId, DateTime forDay)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				CompletedChore toRemove = scope.CompletedChores.FirstOrDefault(x => x.SharedChoreId == sharedChoreId && x.ChoreDate == forDay);
				scope.CompletedChores.Remove(toRemove);
				scope.SaveChanges();
			}

			// change the cache for each user in this membership
			List<SharedChoreMembership> memberships = SharedChoreMembershipHandler.GetSharedChoreMembershipsForChore(sharedChoreId);

			foreach (SharedChoreMembership curMembership in memberships)
			{
				CompletedChoreHandler.AddToUserChoreCacheIterator(curMembership.UserId);
			}
		}

		private static List<CompletedChore> GetChoreItems(List<int> sharedChoreIds, DateTime day)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				List<CompletedChore> completedChores = scope.CompletedChores.Where(x => sharedChoreIds.Contains(x.SharedChoreId) && x.ChoreDate == day).ToList();

				foreach (int curId in sharedChoreIds)
				{
					if (!completedChores.Any(x => x.SharedChoreId == curId))
					{
						// if the item doesn't exist, we want to create it
						CompletedChore newItem = new CompletedChore();
						newItem.SharedChoreId = curId;
						newItem.ChoreDate = day;
						completedChores.Add(newItem);
					}
				}

				List<CompletedChore> returnThis = new List<CompletedChore>();

				foreach (int curId in sharedChoreIds)
				{
					returnThis.Add(completedChores.First(x => curId == x.SharedChoreId));
				}

				return returnThis;
			}
		}

		public static void AddToUserChoreCacheIterator(int userId)
		{
			if (UserIterators.ContainsKey(userId))
			{
				UserIterators[userId] = (UserIterators[userId] + 1) % 10000; // silly potential for one user to make millions of requests in one app cycle, so avoid potential overflow
			}
		}

		public static int LongPollInternal(int userId)
		{
			if (!UserIterators.ContainsKey(userId))
			{
				lock (UserIterators)
				{
					// seems a bit silly, but this double lock method means we don't need to lock twice to ensure our cache doesn't get two entries
					if (!UserIterators.ContainsKey(userId))
					{
						UserIterators.Add(userId, 0);
					}
				}
			}
			return UserIterators[userId];
		}

		public static List<CompletedChore> GetChoreItemsForDateAndUser(int userId, DateTime day)
		{
			// first, get a list of all tasks this user is a part of
			List<SharedChoreMembership> taskMemberships = SharedChoreMembershipHandler.GetSharedChoreMembershipsForUser(userId).OrderBy(x => x.PersonalSortingOrder).ThenBy(x => x.SharedChoreId).ToList();

			// now, get the items that are for this chore item, on this day
			List<CompletedChore> existingChoreItems = GetChoreItems(taskMemberships.Select(x => x.SharedChoreId).ToList(), day);

			return existingChoreItems;
		}

		public static void AddToEveryUserChoreCacheIterator()
		{
			List<int> toModify = new List<int>();
			foreach (KeyValuePair<int, int> kvp in UserIterators)
			{
				toModify.Add(kvp.Key);
			}
			foreach (int curKey in toModify)
			{
				UserIterators[curKey]++;
			}
		}

		public static CompletedChore GetPreviousCompletedChore(CompletedChore beforeThis)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.CompletedChores.Where(x => x.SharedChoreId == beforeThis.SharedChoreId && x.ChoreDate < beforeThis.ChoreDate && x.Completed).OrderByDescending(x => x.ChoreDate).FirstOrDefault();
			}
		}
	}

	public static class SharedChoreMembershipHandler
	{
		public static SharedChoreMembership GetSharedChoreMembership(int choreId, int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.SharedChoreMemberships.FirstOrDefault(x => x.SharedChoreId == choreId && x.UserId == userId);
			}
		}

		public static List<SharedChoreMembership> GetSharedChoreMembershipsForChore(int choreId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.SharedChoreMemberships.Where(x => x.SharedChoreId == choreId).ToList();
			}
		}

		public static List<SharedChoreMembership> GetSharedChoreMembershipsForUser(int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.SharedChoreMemberships.Where(x => x.UserId == userId).ToList();
			}
		}

		public static void AddSharedChoreMembership(int choreId, int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				List<SharedChoreMembership> maximumSharedChoreOrdering = GetSharedChoreMembershipsForUser(userId);

				int orderForChore = 0;

				if (maximumSharedChoreOrdering.Count > 0)
				{
					orderForChore = maximumSharedChoreOrdering.Max(x => x.PersonalSortingOrder) + 1;
				}

				SharedChoreMembership newMembership = new SharedChoreMembership();
				newMembership.UserId = userId;
				newMembership.SharedChoreId = choreId;
				newMembership.PersonalSortingOrder = orderForChore;
				scope.SharedChoreMemberships.Add(newMembership);
				scope.SaveChanges();
			}

			CompletedChoreHandler.AddToUserChoreCacheIterator(userId);
		}

		public static void RemoveSharedChoreMembership(int sharedChoreId, int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				SharedChoreMembership membership = scope.SharedChoreMemberships.FirstOrDefault(x => x.SharedChoreId == sharedChoreId && x.UserId == userId);
				scope.SharedChoreMemberships.Remove(membership);
				scope.SaveChanges();

				// while this may affect other users, we only care that it was removed from this user
				CompletedChoreHandler.AddToUserChoreCacheIterator(userId);
			}
		}

		public static void SaveSortingOrders(SerializableIntIntPair[] pairings, int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				foreach (SerializableIntIntPair curPairing in pairings)
				{
					SharedChoreMembership thisMembership = scope.SharedChoreMemberships.FirstOrDefault(x => x.SharedChoreId == curPairing.Key && x.UserId == userId);

					if (thisMembership == null)
						throw new Exception("Attempted to save ordering for non existant membership");

					scope.SharedChoreMemberships.Attach(thisMembership);
					thisMembership.PersonalSortingOrder = curPairing.Value;
				}

				scope.SaveChanges();
			}
		}

		public static void UpdateSharedChoreMembership(SharedChoreMembership toUpdate)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.SharedChoreMemberships.Attach(toUpdate);
				scope.Entry(toUpdate).State = EntityState.Modified;
				scope.SaveChanges();
			}
		}
	}

	public static class SharedChoreHandler
	{
		public static void AddSharedChore(SharedChore toAdd, int seedUserId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.SharedChores.Add(toAdd);
				scope.Entry(toAdd).State = EntityState.Added;
				scope.SaveChanges();
			}

			SharedChoreMembershipHandler.AddSharedChoreMembership(toAdd.SharedChoreId, seedUserId);

			// we just added the chore, so the only possible user on it is the logged in user
			CompletedChoreHandler.AddToUserChoreCacheIterator(seedUserId);
		}

		public static SharedChore GetSharedChore(int sharedChoreId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.SharedChores.FirstOrDefault(x => x.SharedChoreId == sharedChoreId);
			}
		}
	}

	public static class NoteTypeHandler
	{
		public static List<NoteType> ThisCache
		{
			get
			{
				if (_ThisCache == null)
				{
					using (ProMaDB scope = new ProMaDB())
					{
						_ThisCache = scope.NoteTypes.ToList();
					}
				}

				return _ThisCache;
			}
			set
			{
				_ThisCache = value;
			}
		}
		private static List<NoteType> _ThisCache = null;

		public static List<NoteType> GetNoteTypesForUser(int userId)
		{
			List<NoteTypeMembership> memberships = NoteTypeMembershipHandler.GetMembershipsForUser(userId);
			List<int> membershipIds = memberships.Select(x => x.NoteTypeId).ToList();
			List<NoteType> returnThis = ThisCache.Where(x => membershipIds.Contains(x.NoteTypeId)).ToList();

			// we need to saturate the note type membership, in case these note types are passed down
			returnThis.ForEach(x => x.Membership = memberships.First(y => y.NoteTypeId == x.NoteTypeId));
			returnThis.Where(x => x.Membership.IsCreator).ToList().ForEach(x => x.SharedWithOthers = NoteTypeMembershipHandler.GetMembershipsForType(x.NoteTypeId).Where(y => !y.IsCreator).ToList());

			return returnThis;
		}

		public static NoteType GetNoteType(int noteTypeId, int? relevantUserId)
		{
			NoteType returnThis = ThisCache.FirstOrDefault(x => x.NoteTypeId == noteTypeId);

			if (relevantUserId.HasValue)
			{
				returnThis.Membership = NoteTypeMembershipHandler.GetMembership(relevantUserId.Value, noteTypeId);
			}

			return returnThis;
		}

		public static void DeleteNoteType(NoteType toDelete)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				foreach (PostedNote curNote in PostedNoteHandler.GetAllNotesOfType(toDelete.NoteTypeId))
				{
					PostedNote updateThis = PostedNoteHandler.GetNote(curNote.NoteId);
					updateThis.NoteTypeId = null;
					updateThis.Active = false;
					PostedNoteHandler.UpdatePostedNote(updateThis);
				}

				foreach (NoteTypeMembership membership in NoteTypeMembershipHandler.GetMembershipsForType(toDelete.NoteTypeId))
				{
					NoteTypeMembershipHandler.RemoveNoteTypeMembership(membership);
				}

				scope.NoteTypes.Attach(toDelete);
				scope.NoteTypes.Remove(toDelete);

				scope.SaveChanges();

				ThisCache.Remove(toDelete);
			}
		}

		public static void AddNoteType(NoteType toAdd)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.NoteTypes.Add(toAdd);
				scope.Entry(toAdd).State = EntityState.Added;
				scope.SaveChanges();
			}

			ThisCache.Add(toAdd);
		}

		public static void UpdateNoteType(NoteType toUpdate)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.NoteTypes.Attach(toUpdate);
				scope.Entry(toUpdate).State = EntityState.Modified;
				scope.SaveChanges();
			}

			lock (ThisCache)
			{
				ThisCache.RemoveAll(x => x.NoteTypeId == toUpdate.NoteTypeId);
				ThisCache.Add(toUpdate);
			}
		}

		public static bool UserCanPostNotesOfType(int userId, int noteTypeId)
		{
			List<NoteType> userNoteTypes = GetNoteTypesForUser(userId);

			foreach (NoteType curNoteType in userNoteTypes)
			{
				if (curNoteType.NoteTypeId == noteTypeId)
				{
					if (curNoteType.Membership != null && curNoteType.Membership.CanUseNotes == true)
						return true;

					return false;
				}
			}

			return false;
		}
	}

	public static class NoteTypeMembershipHandler
	{
		public static void AddNoteTypeMembership(NoteTypeMembership toAdd)
		{
			lock (TransactionLocks.StringLock)
			{
				using (ProMaDB scope = new ProMaDB())
				{
					// if this membership already exists, recreate it with the new information
					NoteTypeMembership existingMembership = scope.NoteTypeMemberships.FirstOrDefault(x => x.UserId == toAdd.UserId && x.NoteTypeId == toAdd.NoteTypeId);
					if (existingMembership != null)
					{
						scope.NoteTypeMemberships.Remove(existingMembership);
						scope.SaveChanges();
					}

					scope.NoteTypeMemberships.Add(toAdd);
					scope.Entry(toAdd).State = EntityState.Added;
					scope.SaveChanges();
				}
			}
		}

		public static List<NoteTypeMembership> GetMembershipsForUser(int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.NoteTypeMemberships.Where(x => x.UserId == userId).ToList();
			}
		}

		public static List<NoteTypeMembership> GetMembershipsForType(int typeId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.NoteTypeMemberships.Where(x => x.NoteTypeId == typeId).ToList();
			}
		}

		public static void RemoveNoteTypeMembership(NoteTypeMembership toRemove)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.NoteTypeMemberships.Attach(toRemove);
				scope.NoteTypeMemberships.Remove(toRemove);
				scope.SaveChanges();
			}
		}

		public static NoteTypeMembership GetMembership(int userId, int typeId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.NoteTypeMemberships.FirstOrDefault(x => x.UserId == userId && x.NoteTypeId == typeId);
			}
		}
	}

	public static class FriendshipRequestHandler
	{
		public static Dictionary<int, int> UserIterators = new Dictionary<int, int>();

		public static void AddFriendshipRequest(FriendshipRequest toAdd)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.FriendshipRequests.Add(toAdd);
				scope.Entry(toAdd).State = EntityState.Added;
				scope.SaveChanges();
			}

			AddToUserFriendshipCacheIterator(toAdd.SenderId);
			AddToUserFriendshipCacheIterator(toAdd.RecipientId);
		}

		public static List<FriendshipRequest> GetRequestsForUser(int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.FriendshipRequests.Where(x => x.RecipientId == userId || x.SenderId == userId).ToList();
			}
		}

		public static void AcceptRequestBetweenUsers(int recipient, int sender)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				FriendshipRequest toRemove = scope.FriendshipRequests.FirstOrDefault(x => x.RecipientId == recipient && x.SenderId == sender);

				if (toRemove != null)
				{
					scope.FriendshipRequests.Attach(toRemove);
					scope.FriendshipRequests.Remove(toRemove);
				}

				Friendship newFriendship = new Friendship();
				newFriendship.MemberOneId = recipient;
				newFriendship.MemberTwoId = sender;
				FriendshipHandler.AddFriendship(newFriendship);

				scope.SaveChanges();
			}

			AddToUserFriendshipCacheIterator(recipient);
			AddToUserFriendshipCacheIterator(sender);
		}

		public static void RejectRequestBetweenUsers(int recipient, int sender)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				FriendshipRequest toRemove = scope.FriendshipRequests.FirstOrDefault(x => x.RecipientId == recipient && x.SenderId == sender);

				if (toRemove != null)
				{
					scope.FriendshipRequests.Attach(toRemove);
					scope.FriendshipRequests.Remove(toRemove);
				}

				scope.SaveChanges();
			}

			AddToUserFriendshipCacheIterator(recipient);
			AddToUserFriendshipCacheIterator(sender);
		}

		public static void AddToUserFriendshipCacheIterator(int userId)
		{
			if (UserIterators.ContainsKey(userId))
			{
				UserIterators[userId] = (UserIterators[userId] + 1) % 10000; // silly potential for one user to make millions of requests in one app cycle, so avoid potential overflow
			}
		}

		public static int LongPollInternal(int userId)
		{
			if (!UserIterators.ContainsKey(userId))
			{
				lock (UserIterators)
				{
					// seems a bit silly, but this double lock method means we don't need to lock twice to ensure our cache doesn't get two entries
					if (!UserIterators.ContainsKey(userId))
					{
						UserIterators.Add(userId, 0);
					}
				}
			}
			return UserIterators[userId];
		}

		public static void AddToEveryUserFriendshipCacheIterator()
		{
			List<int> toModify = new List<int>();
			foreach (KeyValuePair<int, int> kvp in UserIterators)
			{
				toModify.Add(kvp.Key);
			}
			foreach (int curKey in toModify)
			{
				UserIterators[curKey]++;
			}
		}
	}

	public static class FriendshipHandler
	{
		public static void AddFriendship(Friendship toAdd)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.Friendships.Add(toAdd);
				scope.Entry(toAdd).State = EntityState.Added;
				scope.SaveChanges();
			}
		}

		public static void RemoveFriendship(int userOne, int userTwo)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				Friendship toRemove = scope.Friendships.FirstOrDefault(x => (x.MemberOneId == userOne && x.MemberTwoId == userTwo) || (x.MemberOneId == userTwo && x.MemberTwoId == userOne));

				scope.Friendships.Attach(toRemove);
				scope.Friendships.Remove(toRemove);

				scope.SaveChanges();
			}

			FriendshipRequestHandler.AddToUserFriendshipCacheIterator(userOne);
			FriendshipRequestHandler.AddToUserFriendshipCacheIterator(userTwo);
		}

		public static List<ProMaUser> GetUserFriends(int userId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				List<ProMaUser> returnThis = new List<ProMaUser>();

				List<Friendship> friendships = scope.Friendships.ToList();

				foreach (Friendship curFriendship in friendships)
				{
					if (curFriendship.MemberOneId == userId)
						returnThis.Add(ProMaUserHandler.GetUser(curFriendship.MemberTwoId));
					else if (curFriendship.MemberTwoId == userId)
						returnThis.Add(ProMaUserHandler.GetUser(curFriendship.MemberOneId));
				}

				return returnThis;
			}
		}
	}

	public static class CalendarHandler
	{
		public static void AddCalendar(CalendarEntry toAdd)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				scope.CalendarEntries.Add(toAdd);
				scope.SaveChanges();
			}
		}

		public static void DeleteCalendar(int calendarId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				CalendarEntry toDelete = scope.CalendarEntries.First(x => x.CalendarId == calendarId);
				scope.CalendarEntries.Remove(toDelete);
				scope.SaveChanges();
			}
		}

		public static CalendarEntry GetEntry(int calendarId)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				return scope.CalendarEntries.First(x => x.CalendarId == calendarId);
			}
		}

		public static List<CalendarEntry> GetCalendarEntriesForUser(int userId, int utcOffset)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				List<CalendarEntry> toReturn = scope.CalendarEntries.Where(x => x.UserId == userId).ToList();

				// first, remove elements that are NOT yearly if they already happened
				// we don't care about the particular time, just the date
				DateTime todaysDate = ProMaUser.NowTime(utcOffset).Date;
				toReturn = toReturn.Where(x => x.Yearly || x.ForDate.Date >= todaysDate).ToList();

				// anything that's left that has a date before today needs to have years added to it until it's correct
				// the year for Yearly reoccuring is this year if we haven't passed that month and day yet, and it's next year if it has
				toReturn.ForEach(x => {
					if (x.Yearly)
					{
						int targetYearDifference = todaysDate.Year - x.ForDate.Year;

						// it already passed, add a year
						if (x.ForDate.Month < todaysDate.Month || (x.ForDate.Month == todaysDate.Month && x.ForDate.Day < todaysDate.Day))
						{
							targetYearDifference++;
						}

						x.ForDate = x.ForDate.AddYears(targetYearDifference);
					}
				});


				// then sort by year, month, day
				toReturn = toReturn.OrderBy(x => x.ForDate.Year).ThenBy(x => x.ForDate.Month).ThenBy(x => x.ForDate.Day).ToList();

				return toReturn;
			}
		}
	}
}