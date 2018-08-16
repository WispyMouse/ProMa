using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
	public class SharedChore
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int SharedChoreId { get; set; }

		public string ChoreName { get; set; }

		[NotMapped]
		public SharedChoreMembership Membership { get; set; }

		public ICollection<CompletedChore> CompletedChores { get; set; }
		public ICollection<SharedChoreMembership> SharedChoreMemberships { get; set; }
	}
}
