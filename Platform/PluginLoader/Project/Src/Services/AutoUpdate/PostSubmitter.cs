#region Copyright
/*
 * Software: TickZoom Trading Platform
 * Copyright 2009 M. Wayne Walter
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * Business use restricted to 30 days except as otherwise stated in
 * in your Service Level Agreement (SLA).
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, see <http://www.tickzoom.org/wiki/Licenses>
 * or write to Free Software Foundation, Inc., 51 Franklin Street,
 * Fifth Floor, Boston, MA  02110-1301, USA.
 * 
 */
#endregion

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

using TickZoom.Api;

namespace TickZoom.Update
{
	/// <summary>
	/// Submits post data to a url.
	/// </summary>
	[CLSCompliant(false)]
	public class PostSubmitter
	{
		string downloadDirectory = Factory.Settings["AppDataFolder"];
		string contentDisposition;
		string fullFileName;
		string baseName;
		string extension;
		string rootName;
		string fileVersion;
		string fileName;
		BackgroundWorker backgroundWorker;
		
		internal string ContentDisposition {
			get { return contentDisposition; }
			set { contentDisposition = value; }
		}
		
		internal string FullFileName {
			get { return fullFileName; }
			set { fullFileName = value; }
		}
		
		internal string BaseName {
			get { return baseName; }
			set { baseName = value; }
		}
		
		public string Extension {
			get { return extension; }
			set { extension = value; }
		}
		
		internal string RootName {
			get { return rootName; }
		}
		
		public string FileVersion {
			get { return fileVersion; }
			set { fileVersion = value; }
		}
		
		internal string FileName {
			get { return fileName; }
		}
		
		internal string DownloadDirectory {
			get { return downloadDirectory; }
			set { downloadDirectory = value + @"\"; }
		}
		
		/// <summary>
		/// determines what type of post to perform.
		/// </summary>
		internal enum PostTypeEnum
		{
			/// <summary>
			/// Does a get against the source.
			/// </summary>
			Get,
			/// <summary>
			/// Does a post against the source.
			/// </summary>
			Post
		}
		
		private string m_url=string.Empty;
		private NameValueCollection m_values=new NameValueCollection();
		private PostTypeEnum m_type=PostTypeEnum.Get;
		private Progress progress;
		/// <summary>
		/// Default constructor.
		/// </summary>
		public PostSubmitter(Progress progress)
		{
			this.progress = progress;
		}
		
		/// <summary>
		/// Gets or sets the url to submit the post to.
		/// </summary>
		public string Url
		{
			get
				{
					return m_url;
				}
		set
				{
					m_url=value;
				}
		}
		/// <summary>
		/// Gets or sets the name value collection of items to post.
		/// </summary>
		public NameValueCollection PostItems
		{
			get
			{
				return m_values;
			}
			set
			{
				m_values=value;
			}
		}
		/// <summary>
		/// Gets or sets the type of action to perform against the url.
		/// </summary>
		internal PostTypeEnum Type
		{
		get
		{
		return m_type;
		}
		set
		{
		m_type=value;
		}
		}
		/// <summary>
		/// Posts the supplied data to specified url.
		/// </summary>
		/// <returns>a string containing the result of the post.</returns>
		public string Post()
		{
			Log log = Factory.SysLog.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			this.m_type=PostSubmitter.PostTypeEnum.Post;
			StringBuilder parameters=new StringBuilder();
			for (int i=0;i < m_values.Count;i++)
			{
				EncodeAndAddItem(ref parameters,m_values.GetKey(i),m_values[i]);
			}
			log.Debug("Post to " + m_url + " with parameters = " + System.Web.HttpUtility.UrlDecode(parameters.ToString()));
			string result=PostData(m_url,parameters.ToString());
			return result;
		}
		
		private bool CancelPending {
			get { return backgroundWorker !=null && backgroundWorker.CancellationPending; }
		}
			
		/// <summary>
		/// Posts data to a specified url. Note that this assumes that you have already url encoded the post data.
		/// </summary>
		/// <param name="postData">The data to post.</param>
		/// <param name="url">the url to post to.</param>
		/// <returns>Returns the result of the post.</returns>
		private string PostData(string url, string postData)
		{
			HttpWebRequest request=null;
			if (m_type==PostTypeEnum.Post)
			{
				Uri uri = new Uri(url);
				request = (HttpWebRequest) WebRequest.Create(uri);
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = postData.Length;
				using(Stream writeStream = request.GetRequestStream())
				{
					UTF8Encoding encoding = new UTF8Encoding();
					byte[] bytes = encoding.GetBytes(postData);
					writeStream.Write(bytes, 0, bytes.Length);
				}
			}
			else
			{
				Uri uri = new Uri(url + "?" + postData);
				request = (HttpWebRequest) WebRequest.Create(uri);
				request.Method = "GET";
			}
			
			string result=string.Empty;
			using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
			{
				contentDisposition = response.Headers.Get("Content-Disposition");
							
				using (Stream responseStream = response.GetResponseStream())
				{
					if( "application/octet-stream".Equals(response.ContentType)) {
				
						string[] contentDisp = contentDisposition.Split( new char[] { ';' } );
						string[] fileNameValue = contentDisp[1].Split( new char[] { '=' } );
						fullFileName = fileNameValue[1].Replace("\"","");
						fullFileName = fullFileName.Trim();
						int lastDot = fullFileName.LastIndexOf('.');
						baseName = fullFileName.Substring(0,lastDot);
						extension = fullFileName.Substring(lastDot+1);
						string[] baseNameParts = baseName.Split( new char[] { '-' } );
						rootName = baseNameParts[0];
						fileVersion = baseNameParts[1];
						fileName = rootName + "." + extension;
						long final = response.ContentLength;

						byte[] buffer = new byte[0x10000];
						int bytes;
						Directory.CreateDirectory(downloadDirectory);
						string tempFileName = fullFileName+"_temp";
						long current = 0;
						string text = "Downloading " + fullFileName;
						using( FileStream fileStream = new FileStream(downloadDirectory+tempFileName, FileMode.Create)) {
							responseStream.ReadTimeout = 10000;
							while ((bytes = responseStream.Read(buffer, 0, buffer.Length)) > 0) {
								if( CancelPending ) {
									result = "Download was interrupted.";
									break;
								}
								fileStream.Write(buffer, 0, bytes);
								current += bytes;
					    		if( backgroundWorker!=null && !backgroundWorker.CancellationPending) {
									progress.UpdateProgress(text,current,final);
									backgroundWorker.ReportProgress(0, progress);
								}
							}
						}
						if( result == null || result.Length == 0) {
			    			File.Delete(downloadDirectory+fullFileName);
			    			File.Move(downloadDirectory+tempFileName,downloadDirectory+fullFileName);
						}
					} else {
						using (StreamReader readStream = new StreamReader (responseStream, Encoding.UTF8))
						{
							result = readStream.ReadToEnd();
						}
						return result;
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Encodes an item and ads it to the string.
		/// </summary>
		/// <param name="baseRequest">The previously encoded data.</param>
		/// <param name="dataItem">The data to encode.</param>
		/// <param name="key">The key to register to the value.</param>
		/// <returns>A string containing the old data and the previously encoded data.</returns>
		private void EncodeAndAddItem(ref StringBuilder baseRequest, string key, string dataItem)
		{
			if (baseRequest==null)
			{
				baseRequest=new StringBuilder();
			}
			if (baseRequest.Length!=0)
			{
				baseRequest.Append("&");
			}
			baseRequest.Append(key);
			baseRequest.Append("=");
			baseRequest.Append(System.Web.HttpUtility.UrlEncode(dataItem));
		}
		
		public BackgroundWorker BackgroundWorker {
			get { return backgroundWorker; }
			set { backgroundWorker = value; }
		}
	}
	
}			
