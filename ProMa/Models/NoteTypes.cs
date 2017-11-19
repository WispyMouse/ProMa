using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class NoteTypes
    {
        public NoteTypes()
        {
            NoteTypeMemberships = new HashSet<NoteTypeMemberships>();
            PostedNotes = new HashSet<PostedNotes>();
        }

        public int NoteTypeId { get; set; }
        public string NoteTypeName { get; set; }
        public bool Hibernated { get; set; }

        public ICollection<NoteTypeMemberships> NoteTypeMemberships { get; set; }
        public ICollection<PostedNotes> PostedNotes { get; set; }
    }
}
