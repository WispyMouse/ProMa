using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
	public class MusicJunkArtist
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int ArtistId { get; set; }

		public string ArtistName { get; set; }
		public int GeniusId { get; set; }

		public ICollection<MusicJunkSong> Songs { get; set; }
	}
}
