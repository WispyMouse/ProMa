using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class PostedNote
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset PostedTime { get; set; }
        public bool? Active { get; set; }
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
    }
}
