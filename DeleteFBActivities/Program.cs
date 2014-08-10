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

        //https://www.facebook.com/ajax/timeline/all_activity/remove_content.php
        /// <summary>
        /// These are two new cookies used in deletion (in URL encoded form)
        /// These two came in comment deletion
        /// _e_0orb_4=["0orb",1407621773184,"act",1407621773183,9,"https://www.facebook.com/the.habib.qureshi/allactivity#","click","click","all_activity_2014_8","r","/the.habib.qureshi/allactivity",{"ft":{},"gt":{"profile_owner":"100006494321954","ref":"timeline:allactivity"}},823.1699829101562,343.3299865722656,0,981,"4hyrs5","/profile_book.php:allactivity",18]
        /// _e_0orb_5=["0orb",1407621779327,"act",1407621779326,10,"/ajax/timeline/all_activity/remove_content.php?action=remove_comment&ent_identifier=S%3A_I100006494321954%3A1602359573323838%3A5&story_dom_id=u_0_26&timeline_token=100006494321954%3A1602359573323838%3A5%3A1407603580%3A1407574089","click","click","-","r","/the.habib.qureshi/allactivity",{"ft":{},"gt":{"profile_owner":"100006494321954","ref":"timeline:allactivity"}},811.5,368.3299865722656,0,981,"4hyrs5","/profile_book.php:allactivity",18]
        /// This came when unlike - (also another time 4 of similar type of cookies came)
        /// _e_0orb_13=["0orb",1407622231700,"act",1407622231699,20,"/ajax/timeline/all_activity/remove_content.php?action=unlike&ent_identifier=S%3A_I100006494321954%3A1602157226677406%3A4&story_dom_id=u_0_31&timeline_token=100006494321954%3A1602157226677406%3A4%3A1407574069%3A1407532006","click","click","-","r","/the.habib.qureshi/allactivity",{"ft":{},"gt":{"profile_owner":"100006494321954","ref":"timeline:allactivity"}},808.6699829101562,876.8299865722656,0,981,"4hyrs5","/profile_book.php:allactivity",18]
        /// </summary>
        /// <param name="wc"></param>
        /// <param name="this_date"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool DeleteActivityOnThisDate(WebClient wc, string this_date, DateTime dt)
        {
            int nWebExceptionCount = 0;
        RESTART_WORK:
            nWebExceptionCount++;
            try
            {
                byte[] buff = wc.DownloadData("https://www.facebook.com/ajax/timeline/all_activity/remove_content.php" + this_date + "%22%2C%22log_filter%22%3Anull%2C%22profile_id%22%3A659625177%7D&__user=659625177");

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
                story_fbid = sHtmlResult.Substring(start, end - start);
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
                if (nWebExceptionCount < 3)
                    goto RESTART_WORK;
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + @"\FB.txt", true))
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

		public static bool DeleteActivityOnThisDate2012(WebClient wc, string this_date, DateTime dt)
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
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + @"\FB.txt", true))
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
