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
using System.IO;
using System.Text;

namespace ProMa.Controllers
{
	public class PostedNotesController : Controller
	{
		public class PostNoteRequestObject
		{
			public string noteText { get; set; }
			public int noteTypeId { get; set; }
		}

		// Returns posted note
		[HttpPost]
		public PostedNote.PostedNotePayload PostNote([FromBody]PostNoteRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			if (requestObject.noteTypeId != -1 && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, requestObject.noteTypeId))
				throw new Exception();

			PostedNote newNote = new PostedNote();

			newNote.NoteText = PostedNoteHandler.CleanNoteText(requestObject.noteText);
			newNote.PostedTime = ProMaUser.NowTime();
			newNote.UserId = user.UserId;
			newNote.Active = true;

			newNote.Completed = false;
			newNote.CompletedTime = null;

			newNote.Highlighted = false;

			if (requestObject.noteTypeId != -1)
				newNote.NoteTypeId = requestObject.noteTypeId;
			else
				newNote.NoteTypeId = null;

			PostedNoteHandler.AddPostedNote(newNote);

			return PostedNoteHandler.GetPayloadNote(newNote, user.UserId);
		}

		public class SetNoteActiveRequestObject
		{
			public int noteId { get; set; }
			public bool active { get; set; }
		}

		[HttpPost]
		public PostedNote.PostedNotePayload SetNoteActive([FromBody]SetNoteActiveRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			PostedNote relevantNote = PostedNoteHandler.GetNote(requestObject.noteId);

			if (relevantNote == null)
				throw new Exception("No note with id " + requestObject.noteId.ToString());

			if (relevantNote.NoteTypeId.HasValue && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, relevantNote.NoteTypeId.Value))
				throw new Exception("Cannot modify or create notes of this type");

			relevantNote.Active = requestObject.active;

			relevantNote.EditedTime = ProMaUser.NowTime();
			relevantNote.EditedUserId = user.UserId;

			PostedNoteHandler.UpdatePostedNote(relevantNote);

			return PostedNoteHandler.GetPayloadNote(relevantNote, user.UserId);
		}

		public class EditNoteRequestObject
		{
			public int noteId { get; set; }
			public string noteText { get; set; }
			public int noteTypeId { get; set; }
			public string noteTitle { get; set; }
		}

		[HttpPost]
		public PostedNote.PostedNotePayload EditNote([FromBody]EditNoteRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			PostedNote relevantNote = PostedNoteHandler.GetNote(requestObject.noteId);

			if (relevantNote == null)
				throw new Exception("No note with id " + requestObject.noteId.ToString());

			if (relevantNote.NoteTypeId.HasValue && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, relevantNote.NoteTypeId.Value))
				throw new Exception("Cannot modify or create notes of this type");

			relevantNote.NoteText = PostedNoteHandler.CleanNoteText(requestObject.noteText);

			if (requestObject.noteTypeId != -1)
			{
				relevantNote.NoteTypeId = requestObject.noteTypeId;
			}
			else
			{
				relevantNote.NoteTypeId = null;
			}

			relevantNote.EditedTime = ProMaUser.NowTime();
			relevantNote.EditedUserId = user.UserId;
			relevantNote.NoteTitle = requestObject.noteTitle;

			PostedNoteHandler.UpdatePostedNote(relevantNote);

			return PostedNoteHandler.GetPayloadNote(relevantNote, (int?)(user.UserId));
		}

		[HttpPost]
		public List<PostedNote.PostedNotePayload> GetAllNotes([FromForm]string sortOption)
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

		public class SetNoteProgressRequestObject
		{
			public int noteId { get; set; }
			public bool progressLevel { get; set; }
		}

		[HttpPost]
		public PostedNote.PostedNotePayload SetNoteProgress([FromBody]SetNoteProgressRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			PostedNote relevantNote = PostedNoteHandler.GetNote(requestObject.noteId);

			if (relevantNote == null)
				throw new Exception("No note with id " + requestObject.noteId.ToString());

			if (relevantNote.NoteTypeId.HasValue && !NoteTypeHandler.UserCanPostNotesOfType(user.UserId, relevantNote.NoteTypeId.Value))
				throw new Exception("Cannot modify or create notes of this type");

			relevantNote.Completed = requestObject.progressLevel;

			if (requestObject.progressLevel)
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

		[HttpPost]
		public PostedNote.PostedNotePayload ToggleHighlightNote([FromForm]int noteId)
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
