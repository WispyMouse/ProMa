using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class CompletedChores
    {
        public DateTime ChoreDate { get; set; }
        public int SharedChoreId { get; set; }
        public bool Completed { get; set; }
        public DateTimeOffset? PostedTime { get; set; }
        public int? UserId { get; set; }

        public SharedChores SharedChore { get; set; }
    }
}
