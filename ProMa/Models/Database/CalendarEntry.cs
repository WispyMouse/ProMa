﻿using System;
using System.Collections.Generic;

namespace ProMa.Models
{
    public partial class CalendarEntry
    {
        public int CalendarId { get; set; }
        public string CalendarName { get; set; }
        public bool Yearly { get; set; }
        public DateTimeOffset ForDate { get; set; }
        public int UserId { get; set; }

        public ProMaUser User { get; set; }
    }
}