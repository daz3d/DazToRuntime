using System;
using System.Text.RegularExpressions;

namespace Daz3D
{
	/// <summary>
	/// Misc helpers methods, such as file handling
	/// </summary>
	public class Utilities
	{
		protected static Regex nameScrub = new Regex("[^a-zA-Z0-9_-]+");
		public static string ScrubKey(string src)
		{
			src = src.Replace(" ", "_");
			src = nameScrub.Replace(src, "");
			return src;
		}

		protected static Regex directoryScrub = new Regex(@"[^a-zA-Z0-9-._/]+");
		public static string ScrubPath(string src)
		{
			src = src.Replace("\\", "/");
			src = src.Replace(" ", "_");
			src = directoryScrub.Replace(src, "");
			return src;
		}

		public static byte[] MD5AsBytes(string path)
		{
			var md5 = System.Security.Cryptography.MD5.Create();
			var stream = System.IO.File.OpenRead(path);
			var bytes = md5.ComputeHash(stream);
			return bytes;
		}

		public static string MD5(string path)
		{
			var bytes = MD5AsBytes(path);
			return BitConverter.ToString(bytes).Replace("-","").ToLowerInvariant();
		}
	}
}