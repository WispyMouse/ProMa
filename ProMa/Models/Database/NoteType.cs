using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
    public partial class NoteType
    {
        public NoteType()
        {
            NoteTypeMemberships = new HashSet<NoteTypeMembership>();
            PostedNotes = new HashSet<PostedNote>();
        }

        public int NoteTypeId { get; set; }
        public string NoteTypeName { get; set; }
        public bool Hibernated { get; set; }

        public ICollection<NoteTypeMembership> NoteTypeMemberships { get; set; }
        public ICollection<PostedNote> PostedNotes { get; set; }

		// Used in a lot of meta calls, but not mapped in database as a sensible one to one
		[NotMapped]
		public NoteTypeMembership Membership { get; set; }

		// Meta information; it's memberships for a note when you're the owner
		[NotMapped]
		public List<NoteTypeMembership> SharedWithOthers { get; set; }
	}
}
