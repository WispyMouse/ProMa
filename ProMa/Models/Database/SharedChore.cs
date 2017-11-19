using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
    public partial class SharedChore
    {
        public SharedChore()
        {
            CompletedChores = new HashSet<CompletedChore>();
            SharedChoreMemberships = new HashSet<SharedChoreMembership>();
        }

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int SharedChoreId { get; set; }
        public string ChoreName { get; set; }

        public ICollection<CompletedChore> CompletedChores { get; set; }
        public ICollection<SharedChoreMembership> SharedChoreMemberships { get; set; }

		[NotMapped]
		public SharedChoreMembership Membership { get; set; }
	}
}
