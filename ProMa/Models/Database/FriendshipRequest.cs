using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
	public class FriendshipRequest
	{
		[Key, Column(Order = 0)]
		public int SenderId { get; set; }
		[ForeignKey("SenderId")]
		public ProMaUser Sender { get; set; }

		[Key, Column(Order = 1)]
		public int RecipientId { get; set; }
		[ForeignKey("RecipientId")]
		public ProMaUser Recipient { get; set; }
	}
}
