using System;
using System.Threading;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public partial class MainWindow: Gtk.Window
{
	Gtk.Widget _widget;
	Builder builder;

	[UI] Gtk.Label lblTime;
	[UI] Gtk.DrawingArea drwMainArea;
	[UI] Gtk.Box appBox;
	[UI] Gtk.Box mainLayout;
	[UI] Gtk.Fixed header;
	[UI] Gtk.Separator separator1;
	[UI] Gtk.Button btnBack;

	private CarOS.Launcher _launcher;

	public static MainWindow Create ()
	{
		Builder builder = new Builder (null, "CarOS.interfaces.MainWindow.glade", null);
		return new MainWindow (builder, builder.GetObject ("MainWindow").Handle);
	}

	protected MainWindow (Builder builder, IntPtr handle) : base (handle)
	{
		this.builder = builder;
		builder.Autoconnect (this);
		DeleteEvent += OnDeleteEvent;

		Gtk.CssProvider css = new CssProvider ();
		string cssContent = System.Text.Encoding.UTF8.GetString(
			GetResource (System.Reflection.Assembly.GetExecutingAssembly(), "CarOS.Assets.App.css"));

		css.LoadFromData(cssContent);

		Gtk.StyleContext.AddProviderForScreen (
			Gdk.Screen.Default, css, 600);

		CarOS.ThreadHelper.Add(() => {
			while (true) {
				DateTime date = DateTime.Now;
				string markup = String.Format (
					                "<span>{0}:{1}:{2}</span>",
					                date.Hour.ToString ("D2"),
					                date.Minute.ToString ("D2"),
					                date.Second.ToString ("D2")
				                );

				lblTime.Markup = markup;

				Thread.Sleep (1000);
			}
		}, true);

		var appList = getApps ();
		_launcher = new CarOS.Launcher(appList, drwMainArea);

		_launcher.Launch += (object sender, CarOS.LaunchEventArgs e) => {
			Console.WriteLine(String.Format("Launching {0}...", e.Application.Name));

			_launcher.Hide();
			appBox.Visible = true;

			var assembly = Assembly.GetAssembly(typeof(Applications.IApplication));

			var type = assembly.GetTypes()
				.First(t => String.Format("{0}.{1}", t.Namespace, t.Name) == e.Application.WidgetClass);

			_widget = (Widget)type.GetMethod("Create").Invoke(null, null);
			_widget.HasWindow = false;

			Gtk.CssProvider widgetCSS = new CssProvider ();
			string resourceId = (String.Format("{0}.css", e.Application.WidgetClass));
			css.LoadFromData(GetResourceAsString(assembly, resourceId));

			_widget.StyleContext.AddProvider (widgetCSS, 600);

			appBox.Add(_widget);

			btnBack.Show();
			_widget.Show();

			this.QueueDraw();
		};

	}
		
	void btnBack_clicked_cb (object sender, EventArgs a) {
		Console.WriteLine (a.ToString ());

		drwMainArea.Show ();
		btnBack.Hide ();

		foreach (Widget child in appBox.AllChildren) {
			appBox.Remove (child);
		}
		appBox.Hide ();

	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		CarOS.ThreadHelper.AbortAll ();
		CarOS.ThreadHelper.RemoveAll ();

		Application.Quit ();
		a.RetVal = true;
	}

	private List<Applications.IApplication> getApps() 
	{
		List<Applications.IApplication> apps = new List<Applications.IApplication> ();
		for (int x = 0; x < 26; x++) {
			var app = new Applications.TestApplication.Definition ();
			app.Name = app.Name + x;
			apps.Add (app);
		}

		return apps;
	}

	private Cairo.ImageSurface LoadImage(string resource) {

		System.IO.Stream imageStream 
			= System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);

		byte[] buffer = new byte[imageStream.Length];
		imageStream.Read (buffer, 0, System.Convert.ToInt32(buffer.Length));

		string tempfile = System.IO.Path.GetTempFileName ();

		System.IO.File.WriteAllBytes (tempfile, buffer);
		var img = new Cairo.ImageSurface(tempfile);
		return img;
	}

	private byte[] GetResource(Assembly assembly, string resource) {

		System.IO.Stream resourceStream 
			= assembly.GetManifestResourceStream(resource);

		byte[] buffer = new byte[resourceStream.Length];
		resourceStream.Read (buffer, 0, System.Convert.ToInt32(buffer.Length));

		string tempfile = System.IO.Path.GetTempFileName ();

		System.IO.File.WriteAllBytes (tempfile, buffer);
		return buffer;
	}

	public string GetResourceAsString(Assembly assembly, string resource) {
		return System.Text.Encoding.UTF8.GetString(GetResource(assembly, resource));
	}
}
