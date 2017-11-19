using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class SharedChores
    {
        public SharedChores()
        {
            CompletedChores = new HashSet<CompletedChores>();
            SharedChoreMemberships = new HashSet<SharedChoreMemberships>();
        }

        public int SharedChoreId { get; set; }
        public string ChoreName { get; set; }

        public ICollection<CompletedChores> CompletedChores { get; set; }
        public ICollection<SharedChoreMemberships> SharedChoreMemberships { get; set; }
    }
}
