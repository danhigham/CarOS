using System;

namespace Applications.TestApplication
{
	public class DefinitionBase
	{
		protected Cairo.ImageSurface LoadIcon(string name) {
			string resourcePath = String.Format("Applications.{0}.Icon.png", name);
			System.IO.Stream imageStream 
				= System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

			byte[] buffer = new byte[imageStream.Length];
			imageStream.Read (buffer, 0, System.Convert.ToInt32(buffer.Length));

			string tempfile = System.IO.Path.GetTempFileName ();

			System.IO.File.WriteAllBytes (tempfile, buffer);
			var img = new Cairo.ImageSurface(tempfile);
			return img;
		}
	}
}

