using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProMa.Models
{
	public class MusicJunkSong
	{
		[Key, Column(Order = 0)]
		public int ArtistId { get; set; }
		[ForeignKey("ArtistId")]
		public MusicJunkArtist Artist { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int SongId { get; set; }

		public int GeniusId { get; set; }

		public string Lyrics { get; set; }
	}
}
