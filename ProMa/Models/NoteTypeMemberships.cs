using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class NoteTypeMemberships
    {
        public int NoteTypeId { get; set; }
        public int UserId { get; set; }
        public bool CanUseNotes { get; set; }
        public bool IsCreator { get; set; }

        public NoteTypes NoteType { get; set; }
        public ProMaUsers User { get; set; }
    }
}
