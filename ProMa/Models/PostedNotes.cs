using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class PostedNotes
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

        public ProMaUsers CompletedUser { get; set; }
        public ProMaUsers EditedUser { get; set; }
        public NoteTypes NoteType { get; set; }
        public ProMaUsers User { get; set; }
    }
}
