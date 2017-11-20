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
	public class NoteTypesController : Controller
    {
		[HttpPost]
		public List<NoteType> GetNoteTypes()
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			List<NoteType> returnThis = NoteTypeHandler.GetNoteTypesForUser(user.UserId).OrderByDescending(x => x.Membership.IsCreator).ThenBy(x => x.NoteTypeName).ThenBy(x => x.NoteTypeId).ToList();

			return returnThis;
		}

		[HttpPost]
		public void DeleteNoteType([FromBody]int noteTypeId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			NoteType relevantNoteType = NoteTypeHandler.GetNoteType(noteTypeId, user.UserId);

			if (relevantNoteType == null)
				throw new Exception("No note with id " + noteTypeId.ToString());

			if (!relevantNoteType.Membership.IsCreator)
				throw new Exception("Created by someone else");

			NoteTypeHandler.DeleteNoteType(relevantNoteType);
		}

		[HttpPost]
		public void HibernateNoteType([FromBody]int noteTypeId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			NoteType relevantNoteType = NoteTypeHandler.GetNoteType(noteTypeId, user.UserId);

			if (relevantNoteType == null)
				throw new Exception("No note with id " + noteTypeId.ToString());

			if (!relevantNoteType.Membership.IsCreator)
				throw new Exception("Created by someone else");

			relevantNoteType.Hibernated = true;
			NoteTypeHandler.UpdateNoteType(relevantNoteType);
		}

		[HttpPost]
		public void RestoreNoteType([FromBody]int noteTypeId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			NoteType relevantNoteType = NoteTypeHandler.GetNoteType(noteTypeId, user.UserId);

			if (relevantNoteType == null)
				throw new Exception("No note with id " + noteTypeId.ToString());

			if (!relevantNoteType.Membership.IsCreator)
				throw new Exception("Created by someone else");

			relevantNoteType.Hibernated = false;
			NoteTypeHandler.UpdateNoteType(relevantNoteType);
		}

		[HttpPost]
		public void AddNoteType([FromBody]string noteTypeName)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (noteTypeName.Contains("'") || noteTypeName.Contains("\""))
				throw new Exception("Invalid Note Type name");

			NoteType newNoteType = new NoteType();
			newNoteType.NoteTypeName = noteTypeName;
			NoteTypeHandler.AddNoteType(newNoteType);

			NoteTypeMembership originalMembership = new NoteTypeMembership();
			originalMembership.UserId = user.UserId;
			originalMembership.NoteTypeId = newNoteType.NoteTypeId;
			originalMembership.IsCreator = true;
			originalMembership.CanUseNotes = true;
			NoteTypeMembershipHandler.AddNoteTypeMembership(originalMembership);
		}

		public class ShareNoteTypeRequestObject
		{
			public int noteTypeId { get; set; }
			public int userId { get; set; }
			public bool canEdit { get; set; }
		}

		[HttpPost]
		public void ShareNoteType([FromBody]ShareNoteTypeRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			NoteType relevantNoteType = NoteTypeHandler.GetNoteType(requestObject.noteTypeId, user.UserId);

			if (!relevantNoteType.Membership.IsCreator)
				throw new Exception("Created by someone else");

			NoteTypeMembership newMembership = new NoteTypeMembership();
			newMembership.NoteTypeId = requestObject.noteTypeId;
			newMembership.CanUseNotes = requestObject.canEdit;
			newMembership.UserId = requestObject.userId;
			NoteTypeMembershipHandler.AddNoteTypeMembership(newMembership);
		}

		[HttpPost]
		public void RemoveFromNoteType([FromBody]int noteTypeId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			NoteTypeMembership toRemove = NoteTypeMembershipHandler.GetMembership(user.UserId, noteTypeId);

			NoteTypeMembershipHandler.RemoveNoteTypeMembership(toRemove);
		}

		public class RenameNoteTypeRequestObject
		{
			public int noteTypeId { get; set; }
			public string newName { get; set; }
		}

		[HttpPost]
		public void RenameNoteType([FromBody]RenameNoteTypeRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (requestObject.newName.Contains("'") || requestObject.newName.Contains("\""))
				throw new Exception("Invalid Note Type name");

			NoteType relevantNoteType = NoteTypeHandler.GetNoteType(requestObject.noteTypeId, user.UserId);

			if (!relevantNoteType.Membership.IsCreator)
				throw new Exception("Created by someone else");

			relevantNoteType.NoteTypeName = requestObject.newName;
			NoteTypeHandler.UpdateNoteType(relevantNoteType);
		}
	}
}
