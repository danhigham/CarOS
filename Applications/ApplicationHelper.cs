using System;

namespace Applications
{
	public static class ApplicationHelper
	{
		public static byte[] GetResource(string resource) {

			System.IO.Stream resStream 
			= System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);

			byte[] buffer = new byte[resStream.Length];
			resStream.Read (buffer, 0, System.Convert.ToInt32(buffer.Length));

			string tempfile = System.IO.Path.GetTempFileName ();

			System.IO.File.WriteAllBytes (tempfile, buffer);

			return buffer;
		}

		public static string GetResourceAsString(string resource) {
			return System.Text.Encoding.UTF8.GetString(GetResource(resource));
		}
	}
}

