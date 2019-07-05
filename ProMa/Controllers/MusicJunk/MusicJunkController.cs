using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ProMa.Models;

namespace ProMa.Controllers
{
	[Route("MusicJunk")]
    public class MusicJunkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

		[HttpGet("GetArtist/{artistName}")]
		public MusicJunkArtist GetArtist(string artistName)
		{
			return RetrieveNewArtist(artistName);
		}

		MusicJunkArtist RetrieveNewArtist(string artistName)
		{
			// Seemingly, to search for an artist by name, we need to search for a song of theirs, then parse up to the artist

			IConfigurationBuilder builder = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("localsettings.json");

			IConfigurationRoot Configuration = builder.Build();

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://api.genius.com/search?q={artistName}");
			request.Headers.Add("Authorization", $"Bearer {Configuration.GetSection("GeniusAPI").GetValue<string>("ClientAccessToken")}");
			request.Method = "GET";

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			using (Stream responseStream = response.GetResponseStream())
			{
				StreamReader reader = new StreamReader(responseStream, true);
				string responseString = reader.ReadToEnd();
				JObject parsedObject = JObject.Parse(responseString);
				JToken primaryArtistToken = parsedObject.SelectToken("response.hits[0].result.primary_artist");

				if (!primaryArtistToken.HasValues)
				{
					return null;
				}
				else
				{
					// do something else, s'pose
				}

				MusicJunkArtist artist = new MusicJunkArtist()
				{
					GeniusId = primaryArtistToken.SelectToken("id").Value<int>(),
					ArtistName = primaryArtistToken.SelectToken("name").Value<string>()
				};

				return artist;
			}
		}
	}
}
