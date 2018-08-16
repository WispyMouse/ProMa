using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
	public class NoteTypeMembership
	{
		[Key, Column(Order = 0)]
		public int NoteTypeId { get; set; }
		[ForeignKey("NoteTypeId")]
		public NoteType NoteType { get; set; }

		[Key, Column(Order = 1)]
		public int UserId { get; set; }
		[ForeignKey("UserId")]
		public ProMaUser MemberUser { get; set; }

		public bool CanUseNotes { get; set; }
		public bool IsCreator { get; set; }
	}
}
