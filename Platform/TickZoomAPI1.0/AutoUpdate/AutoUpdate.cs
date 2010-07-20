using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using ICSharpCode.SharpZipLib.Zip;

namespace TickZoom.Api
{
	public class AutoUpdate
	{
		private static readonly Log log = Factory.Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private string remoteCgi = "http://tickzoom.wnmh.net/cgi-bin/";
		private string remotePage = "versioncheck.py";
		private string userKey;
		private string userKeyFile = @"AutoUpdate\tickZoomUser.key";
		private string userKeyPath;
		private string currentVersion;
		private string message;
		private string autoUpdateDirectory;
		private BackgroundWorker backgroundWorker;
		private PostSubmitter post;
		
		private string DownloadDirectory {
			get { return autoUpdateDirectory + Path.DirectorySeparatorChar + currentVersion; }
		}
		
		public AutoUpdate() {
			currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			autoUpdateDirectory = Factory.Settings["AppDataFolder"] +
				Path.DirectorySeparatorChar + "AutoUpdate";
			LoadUserKey();
		}
		
		private void LoadUserKey() {
			string appDataFolder = Factory.Settings["AppDataFolder"];
			userKeyPath = appDataFolder + @"\" + userKeyFile;
			try {
				StreamReader streamReader = new StreamReader(userKeyPath);
				userKey = streamReader.ReadToEnd();
				userKey = userKey.Replace("-----BEGIN RSA PRIVATE KEY-----","");
				userKey = userKey.Replace("-----END RSA PRIVATE KEY-----","");
		        Regex r = new Regex(@"\s+");
		        userKey = r.Replace(userKey, @"");
			} catch( Exception ex) {
				userKey = "";
				message = "User key problem: " + ex.Message + " Please consider upgrading.";
				log.Notice( message);
			}
		}
		
		public bool UpdateAll() {
			bool retVal = false;
			string[] lines = GetFileList();
			if( lines == null) return false;
			foreach( string line in lines) {
				string[] parts = line.Split(' ');
				string file = parts[0];
				string compareFile = file;
				string checksum = parts[1];
				if( file.ToLower().EndsWith(".zip")) {
				   	compareFile = file.Substring(0,file.Length-4);
				}
				string compareChecksum = GetMD5Hash(compareFile);
				if( checksum != compareChecksum && 
				    DownloadFile(file)) {
					if( file.ToLower().EndsWith(".zip")) {
						UnzipFile(file);
					}
					compareChecksum = GetMD5Hash(compareFile);
					if( checksum != compareChecksum) {
						log.Warn("Checksum match failed for " + compareFile + ". Possibly corrupted or infected.");
					}
					retVal = true;
				}
			}
			if( !retVal && backgroundWorker != null) {
				string text = "AutoUpdate Complete."; 
				backgroundWorker.ReportProgress(0, new ProgressImpl(text,0,0));
				log.Notice("AutoUpdate Complete. Zero files updated.");
			}

			return retVal;
		}
		
		private string GetMD5Hash(string fileName)
		{
			string path = DownloadDirectory + Path.DirectorySeparatorChar + fileName;
			return GetMD5HashFromFile(path);
		}
			
		public string GetMD5HashFromFile(string path) {
			try { 
				FileStream file = new FileStream(path, FileMode.Open);
				MD5 md5 = new MD5CryptoServiceProvider();
				byte[] retVal = md5.ComputeHash(file);
				file.Close();
				string hex = BitConverter.ToString(retVal);
				hex = hex.Replace("-","").ToLower();
				return hex;
			} catch( IOException) {
				return "";
			}
		}	
		
		public void UnzipFile(string zipFile) {
			FastZip zip = new FastZip();
			string path = DownloadDirectory + Path.DirectorySeparatorChar + zipFile;
			zip.ExtractZip(path,DownloadDirectory,"");
			File.Delete(zipFile);
		}
		
		public bool DownloadFile(string fileName)
		{   
			bool Ret = false;
			if( userKey == null) {
				return false;
			}
			
			string url = remoteCgi + currentVersion + "/" + remotePage;
			try {
				post=new PostSubmitter();
				post.BackgroundWorker = backgroundWorker;
				post.Url=url;
				post.PostItems.Add("key",userKey);
				post.PostItems.Add("version",currentVersion);
				post.PostItems.Add("file",fileName);
//				post.PostItems.Add("model","Free");
				post.DownloadDirectory = DownloadDirectory;
				log.Debug("Downloading " + fileName + " to " + post.DownloadDirectory);
				message=post.Post();
				if( "".Equals(message)) {
					// Now copy it to the generic filename for the latest version.
					// Ensure that the target does not exist.
	    			Ret = true;
				} else {
					log.Notice( "Skipping Auto Update of " + fileName + ": " + message);
				}
			}
			catch (Exception ex) {
				message = "AutoUpdate ERROR: " + ex.Message + " Check log file for details.";
				log.Notice(message);
				log.Info("AutoUpdate ERROR",ex);
			}
			return Ret;
		}
	
		public string[] GetFileList()
		{   
			string fileName = "fileList-"+currentVersion+".txt";
			if( !DownloadFile(fileName)) {
				log.Error("Auto Update: '"+fileName+"' was not returned for version " + CurrentVersion + ".");
				return null;
			}
			
			List<string> files = new List<string>();
			using ( StreamReader reader = new StreamReader(DownloadDirectory + @"\" + post.FullFileName)) {
				while( !reader.EndOfStream) {
					files.Add(reader.ReadLine());
				}
			}
			return files.ToArray();
		}
		
		private string GetVersion(string Version)
		{
			string[] x = Version.Split(".".ToCharArray());
			return string.Format("{0:00000}{1:00000}{2:00000}{3:00000}", Convert.ToInt32(x[0]), Convert.ToInt32(x[1]), Convert.ToInt32(x[2]), Convert.ToInt32(x[3]));
		}
	
		public string UserKey {
			get { return userKey; }
			set { userKey = value; }
		}
		
		public string Message {
			get { return message; }
		}
		
		public string UserKeyFile {
			get { return userKeyFile; }
			set { userKeyFile = value;
				LoadUserKey(); }
		}
		
		public string CurrentVersion {
			get { return currentVersion; }
			set { currentVersion = value; }
		}
		
		public BackgroundWorker BackgroundWorker {
			get { return backgroundWorker; }
			set { backgroundWorker = value; }
		}
		
		public string UserKeyPath {
			get { return userKeyPath; }
		}
	}
}