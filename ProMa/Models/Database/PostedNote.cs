﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ProMa.Models
{
    public partial class PostedNote
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset PostedTime { get; set; }
        public bool Active { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
        public bool Highlighted { get; set; }
        public int? NoteTypeId { get; set; }
        public string NoteTitle { get; set; }
        public bool Completed { get; set; }
        public int? CompletedUserId { get; set; }
        public DateTimeOffset? EditedTime { get; set; }
        public int? EditedUserId { get; set; }
        public string NoteText { get; set; }

        public ProMaUser CompletedUser { get; set; }
        public ProMaUser EditedUser { get; set; }
        public NoteType NoteType { get; set; }
        public ProMaUser User { get; set; }

		public class PostedNotePayload
		{
			// These should match PostedNote; the reason it is not an inherited class is that implies it belongs in the database, and would create a Discriminator
			// Don't include foreign keys, just the class in payload form

			public PostedNote Data { get; set; }

			public PostedNoteProMaUserPayload PostedUser { get; set; }
			public PostedNoteProMaUserPayload CompletedUser { get; set; }
			public PostedNoteProMaUserPayload EditedUser { get; set; }
			public NoteType TypeOfNote { get; set; }

			private PostedNotePayload()
			{
				// Serializer only constructor, do not use
			}

			public PostedNotePayload(PostedNote toMake, List<NoteType> hydratedNoteTypes)
			{
				Data = toMake;

				PostedUser = new PostedNoteProMaUserPayload(ProMaUserHandler.GetUser(toMake.UserId));

				if (toMake.CompletedUserId.HasValue)
				{
					CompletedUser = new PostedNoteProMaUserPayload(ProMaUserHandler.GetUser(toMake.CompletedUserId.Value));
				}

				if (toMake.EditedUserId.HasValue)
				{
					EditedUser = new PostedNoteProMaUserPayload(ProMaUserHandler.GetUser(toMake.EditedUserId.Value));
				}

				TypeOfNote = hydratedNoteTypes.FirstOrDefault(x => x.NoteTypeId == toMake.NoteTypeId);
			}

			public class PostedNoteProMaUserPayload
			{
				public string UserName { get; set; }

				private PostedNoteProMaUserPayload()
				{
					// Serializer only constructor, do not use
				}

				public PostedNoteProMaUserPayload(ProMaUser toMake)
				{
					UserName = toMake.UserName;
				}
			}
		}
	}
}
