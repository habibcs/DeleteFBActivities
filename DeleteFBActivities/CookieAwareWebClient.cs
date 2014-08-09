using System;
using System.Net;

namespace DeleteFBActivities
{
	class CookieAwareWebClient : WebClient
	{
		private CookieContainer cc = new CookieContainer();
		private string lastPage;

		protected override WebRequest GetWebRequest(System.Uri address)
		{
			WebRequest R = base.GetWebRequest(address);
			if (R is HttpWebRequest)
			{
				HttpWebRequest WR = (HttpWebRequest)R;
				AddCookies(WR);
				if (lastPage != null)
				{
					WR.Referer = lastPage;
				}
			}
			lastPage = address.ToString();
			return R;
		}

        /// <summary>
        /// From Fiddler:
        /// GET https://www.facebook.com/the.habib.qureshi/allactivity HTTP/1.1
        /// Accept: text/html, application/xhtml+xml, */*
        /// Accept-Language: de-CH,de;q=0.8,en-GB;q=0.6,en;q=0.4,da;q=0.2
        /// User-Agent: Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.3; WOW64; Trident/7.0; MAARJS)
        /// Accept-Encoding: gzip, deflate
        /// Host: www.facebook.com
        /// DNT: 1
        /// Connection: Keep-Alive
        /// Cookie: datr=mpuHUltlM5M4tVf2i2GPTKu9; lu=TgUdlrddFNTjJuQ2_Md2haUw; locale=en_GB; c_user=100006494321954; fr=0IYyemryDVkPeh8IL.AWVx5Bt7tcKk-t91kfFF3tzTqkc.BT5nRM.0s.FPm.AWVdrvdp; xs=107%3Ahlw-ZENUDzjacA%3A2%3A1407611980%3A8886; csm=2; s=Aa7ySACnrs6HVvqp
        /// -------------------Breaking down the cookie array---------------
        /// datr=mpuHUltlM5M4tVf2i2GPTKu9;
        /// lu=TgUdlrddFNTjJuQ2_Md2haUw;
        /// locale=en_GB;
        /// c_user=100006494321954;
        /// fr=0IYyemryDVkPeh8IL.AWVx5Bt7tcKk-t91kfFF3tzTqkc.BT5nRM.0s.FPm.AWVdrvdp;
        /// xs=107%3Ahlw-ZENUDzjacA%3A2%3A1407611980%3A8886;
        /// csm=2;
        /// s=Aa7ySACnrs6HVvqp
        /// </summary>
		public void AddDefaultHeaders()
		{
			Headers.Add("Accept", "text/html, application/xhtml+xml, */*");
			Headers.Add("Accept-Language", "en-gb");
            Headers.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.3; WOW64; Trident/7.0; MAARJS)");
			Headers.Add("Cache-Control", "no-cache");
			//Headers.Add("X-SVN-Rev", "570609");
			//Headers.Add("Content-Type", "application/x-www-form-urlencoded");
			//Headers.Add("Accept-Encoding", "gzip, deflate");
		}

		private void AddCookies(HttpWebRequest wr)
		{
			string sPath = "/";
			string sDomain = "www.facebook.com";
			wr.CookieContainer = new CookieContainer();
			CookieContainer aCC = wr.CookieContainer;

			try
			{
				Cookie c = new Cookie("presence", "EM338654546EuserFA2659625177A2EstateFDsb2F0Et2F_5b_5dElm2FnullEuct2F1338651218BEtrFA2loadA2EtwF240618245EatF1338654185986G338654546498EsndF0EnotF0CEchFDp_5f659625177F31CC", sPath, sDomain);
				aCC.Add(c);
				//p=3; presence=; act=; datr=; locale=en_US; lu=; c_user=; csm=2; s=; xs=;
				c = new Cookie("sub", "1", sPath, sDomain); aCC.Add(c); //ChangedNextTime - CNT
				c = new Cookie("p", "3", sPath, sDomain); aCC.Add(c);
				c = new Cookie("act", "1339186818432%2F7%3A2", sPath, sDomain); aCC.Add(c); //CNT
				c = new Cookie("datr", "tlPDTzRmC25sWkXn4UHYAaBY", sPath, sDomain); aCC.Add(c);
				c = new Cookie("locale", "en_US", sPath, sDomain); aCC.Add(c);
				c = new Cookie("lu", "gAXoLw-DCrnHS9MTZh5-F4Ag", sPath, sDomain); aCC.Add(c);
				c = new Cookie("c_user", "659625177", sPath, sDomain); aCC.Add(c);
				c = new Cookie("csm", "2", sPath, sDomain); aCC.Add(c);
				c = new Cookie("s", "Aa5Eeeg4qdtbj9mr", sPath, sDomain); aCC.Add(c);
				c = new Cookie("xs", "3%3ABM89Z2TAic8JZg%3A2%3A1338651769", sPath, sDomain); aCC.Add(c);
				c = new Cookie("wd", "1366x654", sPath, sDomain); aCC.Add(c); //CNT
			}
			catch (Exception ex)
			{
				ex.ToString();
			}
		}
	}
}
