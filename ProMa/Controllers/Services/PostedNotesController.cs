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
	public class PostedNotesController : Controller
    {
		// Returns posted note
		[HttpGet]
		public PostedNote.PostedNotePayload PostNote(string noteText, int noteTypeId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (noteTypeId != -1 && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, noteTypeId))
				throw new Exception();

			PostedNote newNote = new PostedNote();

			newNote.NoteText = PostedNoteHandler.CleanNoteText(noteText);
			newNote.PostedTime = ProMaUser.NowTime();
			newNote.UserId = user.UserId;
			newNote.Active = true;

			newNote.Completed = false;
			newNote.CompletedTime = null;

			newNote.Highlighted = false;

			if (noteTypeId != -1)
				newNote.NoteTypeId = noteTypeId;
			else
				newNote.NoteTypeId = null;

			PostedNoteHandler.AddPostedNote(newNote);

			return PostedNoteHandler.GetPayloadNote(newNote, user.UserId);
		}

		[HttpGet]
		public PostedNote.PostedNotePayload SetNoteActive(int noteId, bool active)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			PostedNote relevantNote = PostedNoteHandler.GetNote(noteId);

			if (relevantNote == null)
				throw new Exception("No note with id " + noteId.ToString());

			if (relevantNote.NoteTypeId.HasValue && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, relevantNote.NoteTypeId.Value))
				throw new Exception("Cannot modify or create notes of this type");

			relevantNote.Active = active;

			relevantNote.EditedTime = ProMaUser.NowTime();
			relevantNote.EditedUserId = user.UserId;

			PostedNoteHandler.UpdatePostedNote(relevantNote);

			return PostedNoteHandler.GetPayloadNote(relevantNote, user.UserId);
		}

		[HttpGet]
		public PostedNote.PostedNotePayload EditNote(int noteId, string noteText, int noteTypeId, string noteTitle)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			PostedNote relevantNote = PostedNoteHandler.GetNote(noteId);

			if (relevantNote == null)
				throw new Exception("No note with id " + noteId.ToString());

			if (relevantNote.NoteTypeId.HasValue && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, relevantNote.NoteTypeId.Value))
				throw new Exception("Cannot modify or create notes of this type");

			relevantNote.NoteText = PostedNoteHandler.CleanNoteText(noteText);

			if (noteTypeId != -1)
			{
				relevantNote.NoteTypeId = noteTypeId;
			}
			else
			{
				relevantNote.NoteTypeId = null;
			}

			relevantNote.EditedTime = ProMaUser.NowTime();
			relevantNote.EditedUserId = user.UserId;
			relevantNote.NoteTitle = noteTitle;

			PostedNoteHandler.UpdatePostedNote(relevantNote);

			return PostedNoteHandler.GetPayloadNote(relevantNote, (int?)(user.UserId));
		}

		[HttpGet]
		public List<PostedNote.PostedNotePayload> GetAllNotes(string sortOption)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			// includeUnusuals looks like "[inactive][complete]", which we need to parse out to the below options
			bool includeInactive = sortOption.Contains("[showinactive]");
			bool onlyInactive = sortOption.Contains("[onlyinactive]");
			bool includeComplete = sortOption.Contains("[showcomplete]");
			bool includeMany = sortOption.Contains("[showmany]");
			int onlyShow = -1;

			if (sortOption.Contains("[notetype"))
			{
				onlyShow = int.Parse(sortOption.Substring(sortOption.IndexOf("[notetype") + "[notetype".Length, sortOption.IndexOf("endnotetype]") - sortOption.IndexOf("[notetype") - "[notetype".Length));
			}

			List<PostedNote> originalNotes = PostedNoteHandler.GetNotes(shownToUser: user.UserId, includeInactive: includeInactive, onlyInactive: onlyInactive, includeComplete: includeComplete, onlyThisNoteId: onlyShow, includeHibernatedNotes: false);

			var orderedQuery = originalNotes.OrderByDescending(x => x.PostedTime);
			bool firstSortDone = false;

			// sortOption looks like complete, which means complete descending, or posted, which means posted descending
			// this has limited options, based on what is implemented so far. it was a quick resolution for a minor problem, needs maintaining
			if (!string.IsNullOrEmpty(sortOption))
			{
				if (sortOption.Contains("[sortpinhighlighted]"))
				{
					if (!firstSortDone)
					{
						orderedQuery = orderedQuery.OrderByDescending(x => x.Highlighted);
						firstSortDone = true;
					}
					else
					{
						orderedQuery = orderedQuery.ThenByDescending(x => x.Highlighted);
					}
				}
				if (sortOption.Contains("[sortcomplete]"))
				{
					if (!firstSortDone)
					{
						orderedQuery = orderedQuery.OrderByDescending(x => x.CompletedTime);
						firstSortDone = true;
					}
					else
					{
						orderedQuery = orderedQuery.ThenByDescending(x => x.CompletedTime);
					}
				}
			}

			orderedQuery = orderedQuery.ThenByDescending(x => x.PostedTime);

			List<PostedNote> postedNoteFinishedList = null;

			if (!includeMany)
			{
				// TODO KAG: Fix this mobile recognition
				// if (HttpContext.Request.Browser.IsMobileDevice)
				{
				// 	postedNoteFinishedList = orderedQuery.Take(50).ToList();
				}
				// else
				{
					postedNoteFinishedList = orderedQuery.Take(100).ToList();
				}
			}
			else
			{
				postedNoteFinishedList = orderedQuery.ToList();
			}

			return PostedNoteHandler.GetPayloadNotes(postedNoteFinishedList, user.UserId);
		}

		[HttpGet]
		public PostedNote.PostedNotePayload SetNoteProgress(int noteId, bool progressLevel)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			PostedNote relevantNote = PostedNoteHandler.GetNote(noteId);

			if (relevantNote == null)
				throw new Exception("No note with id " + noteId.ToString());

			if (relevantNote.NoteTypeId.HasValue && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, relevantNote.NoteTypeId.Value))
				throw new Exception("Cannot modify or create notes of this type");

			relevantNote.Completed = progressLevel;

			if (progressLevel)
			{
				relevantNote.CompletedTime = ProMaUser.NowTime();
				relevantNote.CompletedUserId = user.UserId;
			}
			else
			{
				relevantNote.CompletedTime = null;
				relevantNote.CompletedUserId = null;
				relevantNote.CompletedUser = null;
			}

			PostedNoteHandler.UpdatePostedNote(relevantNote);

			return PostedNoteHandler.GetPayloadNote(relevantNote, user.UserId);
		}

		[HttpGet]
		public PostedNote.PostedNotePayload ToggleHighlightNote(int noteId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			PostedNote relevantNote = PostedNoteHandler.GetNote(noteId);

			if (relevantNote == null)
				throw new Exception("No note with id " + noteId.ToString());

			if (relevantNote.NoteTypeId.HasValue && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, relevantNote.NoteTypeId.Value))
				throw new Exception("Cannot modify or create notes of this type");

			relevantNote.Highlighted = !relevantNote.Highlighted;

			relevantNote.EditedTime = ProMaUser.NowTime();
			relevantNote.EditedUserId = user.UserId;

			PostedNoteHandler.UpdatePostedNote(relevantNote);

			return PostedNoteHandler.GetPayloadNote(relevantNote, user.UserId);
		}
	}
}
