// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JiraRestAPIMethods.cs" company="Copyright © 2017 Trimble Solutions Corporation">
//     Copyright (C) 2017 [Trimble Solutions Corporation, tekla.buildmaster@trimble.com]
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 3 of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details. 
// You should have received a copy of the GNU Lesser General Public License along with this program; if not, write to the Free
// Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// --------------------------------------------------------------------------------------------------------------------
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JiraConnector
{
    /// <summary>
    /// The JSessionIdData
    /// </summary>
    public class JSessionIdData
    {
        public string Cookie { get; set; }
        public string Resultcode { get; set; }
    }

    /// <summary>
    /// The CreateIssue resultData
    /// </summary>
    public class ResultData
    {
        public string Result { get; set; }
        public string Newissueid { get; set; }
        public bool Resultcode { get; set; }
    }
    public class JiraRestAPIMethods
    {
        public async Task<string> GetAuthenticationCookie(string username, string password, string url)
        {
            string result = string.Empty;
            string cookie = string.Empty;
            string cname = string.Empty;
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                string jsondata = "{\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
                var content = new StringContent(jsondata, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("session", content);
                if (response.IsSuccessStatusCode)
                {
                    result = response.StatusCode.ToString();
                    string resultmessage = response.Content.ReadAsStringAsync().Result;
                    dynamic dynObj = JsonConvert.DeserializeObject(resultmessage);
                    cname = dynObj.session.name;
                    cookie = dynObj.session.value;
                }
                else
                {
                    result = response.StatusCode.ToString();
                    Debug.WriteLine("Unable to get Jsessionid.Check username, password and project" + result);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message + Environment.NewLine + "Cannot Authenticate to JIRA.Check the username, password entered.");
            }
            return cookie;
        }

        /// <summary>
        /// The CreateIssue
        /// </summary>
        /// <param name="baseAddress">
        /// The url to upload attachments
        /// </param>
        /// <param name="file">
        /// The vfile to upload
        /// </param>
        /// <param name="_cookie">
        /// The username
        /// </param>
        /// <param name="jsondata">
        /// The password
        /// </param>
        /// <returns>
        /// The <see cref="ObservableCollection<ResultData>"/>.
        /// </returns>
        public ObservableCollection<ResultData> CreateIssue(string baseAddress, string _cookie, string jsondata)
        {
            ObservableCollection<ResultData> _resultdata = new ObservableCollection<ResultData>();
            string result = string.Empty;
            string newissuecreated = string.Empty;
            bool IsSuccessStatusCode;
            try
            {
                Uri uri = new Uri(baseAddress);
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = new CookieContainer();
                handler.CookieContainer.Add(uri, new Cookie("JSESSIONID", _cookie)); // Adding a Cookie
                HttpClient client = new HttpClient(handler);
                client.BaseAddress = uri;
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(jsondata, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync("issue", content).Result;
                IsSuccessStatusCode = response.IsSuccessStatusCode;
                if (response.IsSuccessStatusCode)
                {
                    result = response.StatusCode.ToString();
                    string resultmessage = response.Content.ReadAsStringAsync().Result;
                    dynamic dynObj = JsonConvert.DeserializeObject(resultmessage);
                    newissuecreated = dynObj.key;
                }
                else
                {
                    result = response.StatusCode.ToString();
                }
            }
            catch (Exception exception)
            {
                IsSuccessStatusCode = false;
                result = Convert.ToString(exception);
            }
            _resultdata.Add(new ResultData { Result = result, Newissueid = newissuecreated, Resultcode = IsSuccessStatusCode });
            return _resultdata;
        }

        /// <summary>
        /// The AddComment
        /// </summary>
        /// <param name="baseAddress">
        /// The url to addcomment
        /// </param>
        /// <param name="_cookie">
        /// The cookie
        /// </param>
        /// <param name="jsondata">
        /// The jsondata
        /// </param>
        /// <param name="jiraid">
        /// The jiraid
        /// </param>
        /// <returns>
        /// The <see cref="ObservableCollection<ResultData>"/>.
        /// </returns>
        public string AddComment(string baseurl, string _cookie, string jsondata, string jiraid)
        {
            string result = string.Empty;
            try
            {
                Uri uri = new Uri(baseurl);
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = new CookieContainer();
                handler.CookieContainer.Add(uri, new Cookie("JSESSIONID", _cookie)); // Adding a Cookie
                HttpClient client = new HttpClient(handler);
                client.BaseAddress = uri;
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(jsondata, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync("comment", content).Result;
                result = response.StatusCode.ToString();
            }
            catch (Exception exception)
            {
                result = Convert.ToString(exception);
            }
            return result;
        }

        /// <summary>
        /// The GetIssue
        /// </summary>
        /// <param name="_cookie">
        /// The cookie
        /// </param>
        /// <param name="url">
        /// The url
        /// </param>
        /// <returns>
        /// The <see cref="ObservableCollection<ResultData>"/>.
        /// </returns>
        public async Task<string> GetIssue(string _cookie, string url)
        {
            Uri uri = new Uri(url);
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            string resultmessage = string.Empty;
            handler.CookieContainer.Add(uri, new Cookie("JSESSIONID", _cookie)); // Adding a Cookie
            HttpClient cclient = new HttpClient(handler);
            try
            {
                HttpResponseMessage response = await cclient.GetAsync(uri);
                resultmessage = await response.Content.ReadAsStringAsync();
                if (response.StatusCode.ToString() == "Unauthorized")
                {
                    resultmessage = response.StatusCode.ToString();
                }
                else if (response.StatusCode.ToString() == "Ok" && resultmessage.Contains("\"projects\":[]"))
                {
                    resultmessage = string.Empty;
                }
            }
            catch (Exception exception)
            {
                resultmessage = "Exception: " + Convert.ToString(exception);
            }
            return resultmessage;
        }

        /// <summary>
        /// The UpdateIssue
        /// </summary>
        /// <param name="url">
        /// The url to updateissue
        /// </param>
        /// <param name="_cookie">
        /// The cookie
        /// </param>
        /// <param name="jsondata">
        /// The jsondata
        /// </param>
        /// <param name="jiraid">
        /// The jiraid
        /// </param>
        /// <returns>
        /// The <see cref="ObservableCollection<ResultData>"/>.
        /// </returns>
        public string UpdateIssue(string url, string _cookie, string jsondata, string jiraid)
        {
            string result = string.Empty;
            try
            {
                Uri uri = new Uri(url);
                HttpClientHandler handler = new HttpClientHandler();
                handler.CookieContainer = new CookieContainer();
                handler.CookieContainer.Add(uri, new Cookie("JSESSIONID", _cookie)); // Adding a Cookie
                HttpClient client = new HttpClient(handler);
                client.BaseAddress = uri;
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(jsondata, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PutAsync(jiraid, content).Result;
                result = response.StatusCode.ToString();
            }
            catch (Exception exception)
            {
                result = Convert.ToString(exception);
            }
            return result;
        }
    }
}
