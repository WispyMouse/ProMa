using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class NoteTypeMembership
    {
        public int NoteTypeId { get; set; }
        public int UserId { get; set; }
        public bool CanUseNotes { get; set; }
        public bool IsCreator { get; set; }

        public NoteType NoteType { get; set; }
        public ProMaUser User { get; set; }
    }
}
