using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class Friendship
    {
        public int MemberOneId { get; set; }
        public int MemberTwoId { get; set; }

        public ProMaUser MemberOne { get; set; }
        public ProMaUser MemberTwo { get; set; }
    }
}
