using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ProMa.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace ProMa.Controllers
{
	public class DataController : Controller
	{
		public const string USERIDSESSIONKEY = "userId";
		public const string USERPASSWORDSESSIONKEY = "userPassword";

		public static ProMaUser LoggedInUser
		{
			get
			{
				// The first time someone makes a request to the server, they don't have a session yet
				// This checks if any features in their HttpContext are Session related; otherwise, this will crash when checking for Session values
				if (ContextTools.Current.Features.Get<ISessionFeature>()?.Session == null)
					return null;

				int? userId = ContextTools.Current.Session.GetInt32(USERIDSESSIONKEY);
				string userPassword = ContextTools.Current.Session.GetString(USERPASSWORDSESSIONKEY);

				if (!userId.HasValue || string.IsNullOrEmpty(userPassword))
					return null;

				ProMaUser cacheUser = ProMaUserHandler.GetUser(userId.Value);

				if (cacheUser.HashedPassword == userPassword)
				{
					cacheUser.PassBackPassword = userPassword;
					return cacheUser;
				}
				else
				{
					return null;
				}
			}
		}

		public class LogInProMaUserRequestObject
		{
			public string userName { get; set; }
			public string password { get; set; }
			public bool skipHash { get; set; }
		}

		[HttpPost]
		public ProMaUser LogInProMaUser([FromBody]LogInProMaUserRequestObject requestObject)
		{
			string shaPassword = requestObject.skipHash ? requestObject.password : ProMaUser.ComputeSHA256(requestObject.password);

			// For the convenience of users, we want to return a message in the case where a user name exists, but the password is wrong
			// the slight security concerns relating to this is noted
			ProMaUser relevantUser = ProMaUserHandler.ThisCache.FirstOrDefault(x => x.UserName.ToLower() == requestObject.userName.ToLower());

			if (relevantUser != null)
			{
				if (relevantUser.HashedPassword == shaPassword)
				{
					HttpContext.Session.SetInt32(USERIDSESSIONKEY, relevantUser.UserId);
					HttpContext.Session.SetString(USERPASSWORDSESSIONKEY, shaPassword);

					relevantUser.PassBackPassword = shaPassword;

					return relevantUser;
				}
				else
				{
					throw new InvalidLogInException();
				}
			}
			else
			{
				throw new InvalidLogInException();
			}
		}

		[HttpPost]
		public void LogOutUser()
		{
			HttpContext.Session.Clear();
		}

		public class RegisterProMaUserRequestObject
		{
			public string userName { get; set; }
			public string md5Password { get; set; }
		}

		[HttpPost]
		public ProMaUser RegisterProMaUser([FromBody]RegisterProMaUserRequestObject requestObject)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				if (string.IsNullOrWhiteSpace(requestObject.md5Password))
					throw new Exception("Invalid password");

				if (!ProMaUser.VerifyName(requestObject.userName))
					throw new Exception("Invalid user name");

				// make sure no user with the same name
				ProMaUser existingUser = ProMaUserHandler.GetUserByUserName(requestObject.userName);

				if (existingUser != null)
				{
					throw new Exception("User already exists by that name");
				}

				ProMaUser newUser = new ProMaUser();

				newUser.HashedPassword = ProMaUser.ComputeSHA256(requestObject.md5Password); ;
				newUser.JoinTime = ProMaUser.NowTime();
				newUser.UserName = requestObject.userName;

				ProMaUserHandler.AddProMaUser(newUser);

				PostedNote seedNote = new PostedNote();
				seedNote.UserId = newUser.UserId;
				seedNote.NoteText = @"You can create new notes by using the text area in the right.\r\n\r\nNotes can have note types (see the ""as type"" selector). You can create new note types using the utilties area to the bottom right, and selecting the ""Note Types"" tab.\r\n\r\nYou can sort by note types using the filters at the bottom of the screen, among other filter options.\r\n\r\nEach note has buttons to the top right of them, like the pencil icon for editing a note or the target icon for marking it as complete. Use these to alter the notes however you would like.\r\n\r\nTry out the other tabs for useful utilities, like keeping track of daily chores, or the Egg Timer tab to handle productivity cycles.\r\n\r\nHave fun using ProMa!";
				seedNote.PostedTime = ProMaUser.NowTime();
				seedNote.Active = true;
				seedNote.Completed = false;
				seedNote.CompletedTime = null;
				seedNote.Highlighted = false;
				seedNote.NoteTypeId = null;

				PostedNoteHandler.AddPostedNote(seedNote);

				return newUser;
			}
		}

		[HttpPost]
		public ProMaUser GetLoggedInUser()
		{
			if (LoggedInUser == null)
				throw new NotLoggedInException();

			ProMaUser returnThis = LoggedInUser;

			return LoggedInUser;
		}

		[HttpPost]
		public List<ProMaUser> GetFriends()
		{
			ProMaUser user = LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			return FriendshipHandler.GetUserFriends(user.UserId);
		}

		// Expects an image with the file name image and strings userId and md5Password
		[HttpPost]
		public void UploadImage()
		{
			// KAG TODO: This had the most amount of changes; Need to find several changes
			
			ProMaUser user = LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			//if (uploadedFile == null)
				throw new ArgumentException("No image in transport; this system only allows for uploading of a single image");

			//string randomFileName = Guid.NewGuid().ToString() + "." + Regex.Match(uploadedFile.FileName, @"\.(.*)").Groups[1].Value;

			//if (uploadedFile.Length > 10485760) // 10 MB; the web.config won't allow anything this large to get through in the first place, though
				throw new Exception("File too large");

			
			// file.SaveAs(HttpRuntime.AppDomainAppPath + "/Images/UploadedImages/" + randomFileName);
			// HttpContext.Response.BufferOutput = true;
			// HttpContext.Response.Write("{\"fileName\": \"" + randomFileName + "\"}");
			// HttpContext.Response.Body.Flush();
		}

		[HttpPost]
		public bool HeartBeat()
		{
			if (LoggedInUser == null)
				return false;
			else
			{
				HttpContext.Session.SetInt32("tick", ProMaUser.NowTime().Second); // add something to the session so that the heartbeat stays alive
				return true;
			}
		}
	}
}
