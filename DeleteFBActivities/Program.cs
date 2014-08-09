using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Net;
using System.Globalization;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;


namespace DeleteFBActivities
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{

				CookieAwareWebClient wc = new CookieAwareWebClient();
				wc.AddDefaultHeaders();
                byte[] buff = wc.DownloadData("https://www.facebook.com/the.habib.qureshi/allactivity"); //https://www.facebook.com/all_activity.php - 2012

				DateTime dt = DateTime.Now; //DateTime dt = DateTime.ParseExact("6/10/2012", "M/d/yyyy", null);
				String sDate = dt.ToString("M/d/yyyy");
				sDate = Uri.EscapeDataString(sDate);

				int main_count = 0;
				while(true)
				{
					main_count++;
					int i = 0;
					System.Console.WriteLine(dt.ToShortDateString());
					while (DeleteActivityOnThisDate(wc, sDate, dt)) i++;
					System.Threading.Thread.Sleep(500);
					dt = dt.AddDays(-1);
					sDate = dt.ToString("M/d/yyyy");
					sDate = Uri.EscapeDataString(sDate);
					if (main_count >= 5000) break; //10 years?
				}
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}

		public static bool DeleteActivityOnThisDate(WebClient wc, string this_date, DateTime dt)
		{
			int nWebExceptionCount = 0;
		RESTART_WORK:
			nWebExceptionCount++;
			try
			{
				byte[] buff = wc.DownloadData("https://www.facebook.com/ajax/pagelet/generic.php/FBXLogStoriesPagelet?__a=1&data=%7B%22date_str%22%3A%22" + this_date + "%22%2C%22log_filter%22%3Anull%2C%22profile_id%22%3A659625177%7D&__user=659625177");

				string sHtmlResult = System.Text.Encoding.UTF8.GetString(buff);
				int nIndex = sHtmlResult.IndexOf("No activity could be found");
				if (nIndex != -1) return false;

				string story_div_id = "story_div_id";
				string story_dom_id = "story_dom_id";
				string story_fbid = "story_fbid";
				string story_row_time = "story_row_time";
				story_div_id = sHtmlResult.Substring(sHtmlResult.IndexOf(story_div_id) + story_div_id.Length + 1, 8);
				story_dom_id = sHtmlResult.Substring(sHtmlResult.IndexOf(story_dom_id) + story_dom_id.Length + 1, 8);
				int start = sHtmlResult.IndexOf(story_fbid) + story_fbid.Length + 1;
				int end = sHtmlResult.IndexOf("&amp;", start);
				story_fbid = sHtmlResult.Substring(start, end-start);
				story_row_time = sHtmlResult.Substring(sHtmlResult.IndexOf(story_row_time) + story_row_time.Length + 1, 10);

				//Now prepare to delete all activities, 1 by 1
				wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
				string sURI = "https://www.facebook.com/ajax/timeline/take_action_on_story.php?__a=1";
				string myParamters = "permalink=1&profile_id=659625177&story_div_id={0}&story_dom_id={1}&story_fbid={2}&story_row_time={3}&__a=1&__user=659625177&action=remove_content&also_remove_app=0&fb_dtsg=AQCgOY5i&phstamp=1658167103798953105211";
				myParamters = String.Format(myParamters, story_div_id, story_dom_id, story_fbid, story_row_time);
				ServicePointManager.Expect100Continue = false;
				sHtmlResult = wc.UploadString(sURI, "POST", myParamters); //requesting to delete the particular activity
				myParamters += "&confirmed=true&ban_user=0";
				sHtmlResult = wc.UploadString(sURI, "POST", myParamters); //confirmation to delete the activity.

				nIndex = sHtmlResult.IndexOf("No Permission to Post");
				if (nIndex != -1)
				{
					using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\FB.txt", true))
					{
						file.WriteLine(dt.ToShortDateString()); //From here you remove these activities manually.
					}
					return false;
				}

				return true;
			}
			catch (WebException wex)
			{
				wex.ToString();
				if(nWebExceptionCount < 3)
					goto RESTART_WORK;
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\FB.txt", true))
				{
					file.WriteLine(dt.ToShortDateString()); //From here you remove these activities manually.
				}
				return false;
			}
			catch (Exception)
			{
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\FB.txt", true))
				{
					file.WriteLine(dt.ToShortDateString()); //From here you remove these activities manually.
				}
				return false;
			}
		}
	}
}
