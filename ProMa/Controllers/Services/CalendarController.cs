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
	public class CalendarController : Controller
    {
		[HttpGet]
		public void AddCalendar(string newItemName, int year, int month, int day, bool yearly)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
			{
				throw new NotLoggedInException();
			}

			CalendarEntry newEntry = new CalendarEntry();
			newEntry.CalendarName = newItemName;
			newEntry.Yearly = yearly;
			newEntry.ForDate = new DateTimeOffset(year, month, day, 0, 0, 0, new TimeSpan());
			newEntry.UserId = user.UserId;

			CalendarHandler.AddCalendar(newEntry);
		}

		[HttpGet]
		public List<CalendarEntry> GetCalendarEntries(int utcOffset)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
			{
				throw new NotLoggedInException();
			}

			return CalendarHandler.GetCalendarEntriesForUser(user.UserId, utcOffset);
		}

		[HttpGet]
		public void DeleteCalendar(int calendarId)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
			{
				throw new NotLoggedInException();
			}

			CalendarEntry toDelete = CalendarHandler.GetEntry(calendarId);

			if (toDelete.UserId != user.UserId)
			{
				throw new Exception("Created by someone else");
			}

			CalendarHandler.DeleteCalendar(calendarId);
		}
	}
}
