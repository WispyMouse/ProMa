using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
	public class Friendship
	{
		[Key, Column(Order = 0)]
		public int MemberOneId { get; set; }
		[ForeignKey("MemberOneId")]
		public ProMaUser MemberOne { get; set; }

		[Key, Column(Order = 1)]
		public int MemberTwoId { get; set; }
		[ForeignKey("MemberTwoId")]
		public ProMaUser MemberTwo { get; set; }
	}
}
