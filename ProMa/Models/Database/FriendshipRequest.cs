using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class FriendshipRequest
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }

        public ProMaUser Recipient { get; set; }
        public ProMaUser Sender { get; set; }
    }
}
