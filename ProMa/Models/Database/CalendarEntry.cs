using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace ProMa.Models
{
	public class CalendarEntry
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int CalendarId { get; set; }

		public string CalendarName { get; set; }

		public bool Yearly { get; set; }

		public DateTimeOffset ForDate { get; set; }

		public int UserId { get; set; }
		[XmlIgnore]
		[JsonIgnore]
		[ForeignKey("UserId")]
		public ProMaUser PostedUser { get; set; }
	}
}
