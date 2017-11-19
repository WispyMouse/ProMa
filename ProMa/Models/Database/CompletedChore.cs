using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
    public partial class CompletedChore
    {
        public DateTime ChoreDate { get; set; }
		[Key, Column(Order = 1)]
		public int SharedChoreId { get; set; }
        public bool Completed { get; set; }
        public DateTimeOffset? PostedTime { get; set; }
        public int? UserId { get; set; }

		[ForeignKey("SharedChoreId")]
		public SharedChore SharedChore { get; set; } // May be included via hydration
		
		[ForeignKey("UserId")]
		public ProMaUser CompletedUser { get; set; } // May be included via hydration

		// Meta data that may be included via hydration
		[NotMapped]
		public ProMaUser LastDoneUser { get; set; }
		[NotMapped]
		public DateTimeOffset? LastDoneTime { get; set; }
	}
}
