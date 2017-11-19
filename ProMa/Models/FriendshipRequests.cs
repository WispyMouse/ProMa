using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class FriendshipRequests
    {
        public int SenderId { get; set; }
        public int RecipientId { get; set; }

        public ProMaUsers Recipient { get; set; }
        public ProMaUsers Sender { get; set; }
    }
}
