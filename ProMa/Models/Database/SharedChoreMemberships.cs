using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class SharedChoreMemberships
    {
        public int SharedChoreId { get; set; }
        public int UserId { get; set; }
        public int PersonalSortingOrder { get; set; }
        public int? AlertHour { get; set; }
        public int? AlertMinute { get; set; }

        public SharedChores SharedChore { get; set; }
        public ProMaUsers User { get; set; }
    }
}
