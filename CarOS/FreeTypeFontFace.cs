using System;
using System.Runtime.InteropServices;
using Cairo;

public class FreeTypeInitException : Exception
{
	public FreeTypeInitException() : base("Can't initialize freetype environment.")
	{
	}
}

public class CreateFaceException : Exception
{
	public CreateFaceException(string filename) : base("Can't create the face for file: " + filename + ".")
	{
	}
}

public class LoadFaceException : Exception
{
	public LoadFaceException(string filename) : base("Can't load the face for file: " + filename + ".")
	{
	}
}

public class FreeTypeFontFace : FontFace
{
	private static bool initialized = false;
	private static IntPtr ft_lib;
	private IntPtr ft_face;

	private FreeTypeFontFace(IntPtr handler, IntPtr ft_face):base(handler)
	{
		this.ft_face = ft_face;
	}

	public new void Dispose()
	{
		cairo_font_face_destroy (Handle);
		FT_Done_Face (ft_face);
		((IDisposable) this).Dispose ();
	}

	public static FreeTypeFontFace Create(string filename, int faceindex, int loadoptions)
	{
		if(!initialized)
			initialize();

		IntPtr ft_face;
		if(FT_New_Face (ft_lib, filename, faceindex, out ft_face) != 0)
			throw new LoadFaceException(filename);

		IntPtr handler = cairo_ft_font_face_create_for_ft_face (ft_face, loadoptions);
		if(cairo_font_face_status(handler) != 0)
			throw new CreateFaceException(filename);

		return new FreeTypeFontFace(handler, ft_face);
	}

	private static void initialize() {
		if(FT_Init_FreeType (out ft_lib) != 0)
			throw new FreeTypeInitException();
		initialized = true;
	}

	[DllImport ("libfreetype.so.6")]
	private static extern int FT_Init_FreeType (out IntPtr ft_lib);

	[DllImport ("libfreetype.so.6")]
	private static extern int FT_New_Face (IntPtr ft_lib, string filename, int faceindex, out IntPtr ft_face);

	[DllImport ("libfreetype.so.6")]
	private static extern int FT_Done_Face (IntPtr ft_face);

	[DllImport ("libcairo.so.2")]
	private static extern IntPtr cairo_ft_font_face_create_for_ft_face (IntPtr ft_face, int loadoptions);

	[DllImport ("libcairo.so.2")]
	private static extern int cairo_font_face_status (IntPtr cr_face);

	[DllImport ("libcairo.so.2")]
	private static extern int cairo_font_face_destroy (IntPtr cr_face);

}