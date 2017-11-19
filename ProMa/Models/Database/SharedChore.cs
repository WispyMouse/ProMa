using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class SharedChore
    {
        public SharedChore()
        {
            CompletedChores = new HashSet<CompletedChore>();
            SharedChoreMemberships = new HashSet<SharedChoreMembership>();
        }

        public int SharedChoreId { get; set; }
        public string ChoreName { get; set; }

        public ICollection<CompletedChore> CompletedChores { get; set; }
        public ICollection<SharedChoreMembership> SharedChoreMemberships { get; set; }
    }
}
