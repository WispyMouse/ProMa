using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
	public class NoteType
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int NoteTypeId { get; set; }

		public string NoteTypeName { get; set; }

		// Hibernated means the note type doesn't show up in normal places; it's inactive
		// good for archiving projects
		public bool Hibernated { get; set; }

		// Used in a lot of meta calls, but not mapped in database as a sensible one to one
		[NotMapped]
		public NoteTypeMembership Membership { get; set; }

		// Meta information; it's memberships for a note when you're the owner
		[NotMapped]
		public List<NoteTypeMembership> SharedWithOthers { get; set; }

		public ICollection<NoteTypeMembership> NoteTypeMemberships { get; set; }
		public ICollection<PostedNote> PostedNotes { get; set; }
	}
}
