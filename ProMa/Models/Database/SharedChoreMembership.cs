using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace ProMa.Models
{
	public class SharedChoreMembership
	{
		[Key, Column(Order = 0)]
		public int SharedChoreId { get; set; }
		[XmlIgnore]
		[JsonIgnore]
		[ForeignKey("SharedChoreId")]
		public SharedChore SharedChore { get; set; }

		[Key, Column(Order = 1)]
		public int UserId { get; set; }
		[XmlIgnore]
		[JsonIgnore]
		[ForeignKey("UserId")]
		public ProMaUser MemberUser { get; set; }

		public int PersonalSortingOrder { get; set; }

		public int? AlertHour { get; set; }
		public int? AlertMinute { get; set; }
	}
}
