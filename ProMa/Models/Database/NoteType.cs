using System;
using System.Collections.Generic;

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
    }
}
