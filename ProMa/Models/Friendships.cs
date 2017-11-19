using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class Friendships
    {
        public int MemberOneId { get; set; }
        public int MemberTwoId { get; set; }

        public ProMaUsers MemberOne { get; set; }
        public ProMaUsers MemberTwo { get; set; }
    }
}
