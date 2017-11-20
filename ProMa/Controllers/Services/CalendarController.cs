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
		public class AddCalendarRequestObject
		{
			public string newItemName { get; set; }
			public int year { get; set; }
			public int month { get; set; }
			public int day { get; set; }
			public bool yearly { get; set; }
		}

		[HttpPost]
		public void AddCalendar([FromBody]AddCalendarRequestObject requestObject)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
			{
				throw new NotLoggedInException();
			}

			CalendarEntry newEntry = new CalendarEntry();
			newEntry.CalendarName = requestObject.newItemName;
			newEntry.Yearly = requestObject.yearly;
			newEntry.ForDate = new DateTimeOffset(requestObject.year, requestObject.month, requestObject.day, 0, 0, 0, new TimeSpan());
			newEntry.UserId = user.UserId;

			CalendarHandler.AddCalendar(newEntry);
		}

		[HttpPost]
		public List<CalendarEntry> GetCalendarEntries([FromBody]int utcOffset)
		{
			ProMaUser user = DataController.LoggedInUser;

			if (user == null)
			{
				throw new NotLoggedInException();
			}

			return CalendarHandler.GetCalendarEntriesForUser(user.UserId, utcOffset);
		}

		[HttpPost]
		public void DeleteCalendar([FromBody]int calendarId)
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
