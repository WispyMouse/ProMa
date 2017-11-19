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
		[HttpGet]
		public List<NoteType> GetNoteTypes()
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			List<NoteType> returnThis = NoteTypeHandler.GetNoteTypesForUser(user.UserId).OrderByDescending(x => x.Membership.IsCreator).ThenBy(x => x.NoteTypeName).ThenBy(x => x.NoteTypeId).ToList();

			return returnThis;
		}

		[HttpGet]
		public void DeleteNoteType(int noteTypeId)
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

		[HttpGet]
		public void HibernateNoteType(int noteTypeId)
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

		[HttpGet]
		public void RestoreNoteType(int noteTypeId)
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

		[HttpGet]
		public void AddNoteType(string noteTypeName)
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

		[HttpGet]
		public void ShareNoteType(int noteTypeId, int userId, bool canEdit)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			NoteType relevantNoteType = NoteTypeHandler.GetNoteType(noteTypeId, user.UserId);

			if (!relevantNoteType.Membership.IsCreator)
				throw new Exception("Created by someone else");

			NoteTypeMembership newMembership = new NoteTypeMembership();
			newMembership.NoteTypeId = noteTypeId;
			newMembership.CanUseNotes = canEdit;
			newMembership.UserId = userId;
			NoteTypeMembershipHandler.AddNoteTypeMembership(newMembership);
		}

		[HttpGet]
		public void RemoveFromNoteType(int noteTypeId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			NoteTypeMembership toRemove = NoteTypeMembershipHandler.GetMembership(user.UserId, noteTypeId);

			NoteTypeMembershipHandler.RemoveNoteTypeMembership(toRemove);
		}

		[HttpGet]
		public void RenameNoteType(int noteTypeId, string newName)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (newName.Contains("'") || newName.Contains("\""))
				throw new Exception("Invalid Note Type name");

			NoteType relevantNoteType = NoteTypeHandler.GetNoteType(noteTypeId, user.UserId);

			if (!relevantNoteType.Membership.IsCreator)
				throw new Exception("Created by someone else");

			relevantNoteType.NoteTypeName = newName;
			NoteTypeHandler.UpdateNoteType(relevantNoteType);
		}
	}
}
