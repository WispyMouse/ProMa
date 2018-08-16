using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace ProMa.Models
{
	public class ProMaUser
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public int UserId { get; set; }

		public string UserName { get; set; }

		[XmlIgnore]
		[JsonIgnore]
		public string HashedPassword { get; set; }
		public DateTimeOffset JoinTime { get; set; }

		public string EmailAddress { get; set; }

		public bool IsAdmin { get; set; }
		public bool IsDemo { get; set; }
		public bool EnterIsNewLinePref { get; set; }

		// Used for meta calls so the user can be relogged in
		[NotMapped]
		public string PassBackPassword { get; set; }

		public ICollection<CalendarEntry> CalendarEntries { get; set; }
		public ICollection<FriendshipRequest> FriendshipRequestsRecipient { get; set; }
		public ICollection<FriendshipRequest> FriendshipRequestsSender { get; set; }
		public ICollection<Friendship> FriendshipsMemberOne { get; set; }
		public ICollection<Friendship> FriendshipsMemberTwo { get; set; }
		public ICollection<NoteTypeMembership> NoteTypeMemberships { get; set; }
		public ICollection<PostedNote> PostedNotesCompletedUser { get; set; }
		public ICollection<PostedNote> PostedNotesEditedUser { get; set; }
		public ICollection<PostedNote> PostedNotesUser { get; set; }
		public ICollection<SharedChoreMembership> SharedChoreMemberships { get; set; }

		// The password hashing method is to take the md5 input
		// then do a SHA256 hash of the result
		// this is just to avoid having plain text sent over http requests
		public static string ComputeMD5Hash(string plaintextPassword)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(plaintextPassword);
			byte[] hash = MD5.Create().ComputeHash(bytes);
			string hashString = string.Empty;
			foreach (byte theByte in hash)
			{
				hashString += theByte.ToString("x2");
			}

			return hashString;
		}

		public static string ComputeSHA256(string md5EncodedPassword)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(md5EncodedPassword);
			SHA256Managed hashstring = new SHA256Managed();
			byte[] hash = hashstring.ComputeHash(bytes);
			string hashString = string.Empty;
			foreach (byte theByte in hash)
			{
				hashString += theByte.ToString("x2");
			}

			return hashString;
		}

		public static DateTime NowTime(int utcOffset = 0)
		{
			return DateTime.UtcNow.ToUniversalTime().AddHours(utcOffset);
		}

		public static bool VerifyName(string name)
		{
			if (string.IsNullOrWhiteSpace(name) || name.Contains("@") || name.Contains(" ") || name.Contains("'") || name.Contains("\"") || name.Length > 20)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}
