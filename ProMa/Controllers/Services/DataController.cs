﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ProMa.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

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
				return null;
				// TODO KAG: How do I get this in a static...
				//if (ContextTools.Current.Session == null)
				//	return null;

				//int userId = ContextTools.Current.Items[USERIDSESSIONKEY] != null ? (int)ContextTools.Current.Items[USERIDSESSIONKEY] : -1;
				//string userPassword = ContextTools.Current.Items[USERPASSWORDSESSIONKEY] != null ? ContextTools.Current.Items[USERPASSWORDSESSIONKEY].ToString() : null;

				//if (userId == -1 || string.IsNullOrEmpty(userPassword))
				//	return null;

				//ProMaUser cacheUser = ProMaUserHandler.GetUser(userId);

				//if (cacheUser.HashedPassword == userPassword)
				//{
				//	cacheUser.PassBackPassword = userPassword;
				//	return cacheUser;
				//}
				//else
				//{
				//	return null;
				//}
			}
		}

		public class LogInProMaUserRequestObject
		{
			public string userName { get; set; }
			public string password { get; set; }
			public bool skipHash { get; set; }
		}

		[HttpPost]
		[AllowAnonymous]
		public ProMaUser LogInProMaUser([FromBody]LogInProMaUserRequestObject request)
		{
			string shaPassword = request.skipHash ? request.password : ProMaUser.ComputeSHA256(request.password);

			// For the convenience of users, we want to return a message in the case where a user name exists, but the password is wrong
			// the slight security concerns relating to this is noted
			ProMaUser relevantUser = ProMaUserHandler.ThisCache.FirstOrDefault(x => x.UserName.ToLower() == request.userName.ToLower());

			if (relevantUser != null)
			{
				if (relevantUser.HashedPassword == shaPassword)
				{
					HttpContext.Items.Add(USERIDSESSIONKEY, relevantUser.UserId);
					HttpContext.Items.Add(USERPASSWORDSESSIONKEY, shaPassword);

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

		[HttpGet]
		public void LogOutUser()
		{
			HttpContext.Items.Clear();
		}

		[HttpGet]
		public ProMaUser RegisterProMaUser(string userName, string md5Password)
		{
			using (ProMaDB scope = new ProMaDB())
			{
				if (string.IsNullOrWhiteSpace(md5Password))
					throw new Exception("Invalid password");

				if (!ProMaUser.VerifyName(userName))
					throw new Exception("Invalid user name");

				// make sure no user with the same name
				ProMaUser existingUser = ProMaUserHandler.GetUserByUserName(userName);

				if (existingUser == null)
				{
					ProMaUser newUser = new ProMaUser();

					newUser.HashedPassword = ProMaUser.ComputeSHA256(md5Password); ;
					newUser.JoinTime = ProMaUser.NowTime();
					newUser.UserName = userName;

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
				else
				{
					throw new Exception("User already exists by that name");
				}
			}
		}

		[HttpGet]
		public ProMaUser GetLoggedInUser()
		{
			if (LoggedInUser == null)
				throw new NotLoggedInException();

			ProMaUser returnThis = LoggedInUser;

			return LoggedInUser;
		}

		[HttpGet]
		public List<ProMaUser> GetFriends()
		{
			ProMaUser user = LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			return FriendshipHandler.GetUserFriends(user.UserId);
		}

		// Expects an image with the file name image and strings userId and md5Password
		[HttpGet]
		public void UploadImage()
		{
			// KAG TODO: This had the most amount of changes; Need to find several changes
			/*
			ProMaUser user = LoggedInUser;

			if (user == null)
				throw new NotLoggedInException();

			HttpPostedFile file = HttpContext.Request.Current.Request.Files["image"];

			if (file == null)
				throw new ArgumentException("No image in transport; this system only allows for uploading of a single image");

			string randomFileName = Guid.NewGuid().ToString() + "." + Regex.Match(file.FileName, @"\.(.*)").Groups[1].Value;

			if (file.ContentLength > 10485760) // 10 MB; the web.config won't allow anything this large to get through in the first place, though
				throw new Exception("File too large");

			file.SaveAs(HttpRuntime.AppDomainAppPath + "/Images/UploadedImages/" + randomFileName);
			HttpContext.Response.BufferOutput = true;
			HttpContext.Response.Write("{\"fileName\": \"" + randomFileName + "\"}");
			HttpContext.Response.Body.Flush();
			*/
		}

		[HttpGet]
		public bool HeartBeat()
		{
			if (LoggedInUser == null)
				return false;
			else
			{
				HttpContext.Items["tick"] = ProMaUser.NowTime().Second; // add something to the session so that the heartbeat stays alive
				return true;
			}
		}

		[HttpGet]
		public List<KeyValuePair<string, int>> LongPoll(int userId, int choreCacheVersion, int friendshipCacheVersion)
		{
			Task<List<KeyValuePair<string, int>>> thisTask = Task.Run(() => LongPollInternal(userId, choreCacheVersion, friendshipCacheVersion));
			thisTask.Wait();
			return thisTask.Result;
		}

		[HttpGet]
		public static async Task<List<KeyValuePair<string, int>>> LongPollInternal(int userId, int choreCacheVersion, int friendshipCacheVersion)
		{
			List<KeyValuePair<string, int>> returnThis = new List<KeyValuePair<string, int>>();

			do
			{
				await Task.Delay(100);
				returnThis.Clear();
				returnThis.Add(new KeyValuePair<string, int>("friendshipCacheVersion", FriendshipRequestHandler.LongPollInternal(userId)));
				returnThis.Add(new KeyValuePair<string, int>("choreCacheVersion", CompletedChoreHandler.LongPollInternal(userId)));
			} while
				(
					friendshipCacheVersion == returnThis.First(x => x.Key == "friendshipCacheVersion").Value &&
					choreCacheVersion == returnThis.First(x => x.Key == "choreCacheVersion").Value
				);

			return returnThis;
		}
	}
}
