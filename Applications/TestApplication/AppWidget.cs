using System;
using System.Threading;
using System.Reflection;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace Applications.TestApplication
{
	public partial class AppWidget: Gtk.Widget
	{
		Builder _builder;

		public static AppWidget Create ()
		{
			Builder builder = new Builder (null, "Applications.TestApplication.AppWidget.glade", null);
			return new AppWidget (builder, builder.GetObject("box1").Handle);
		}

		protected AppWidget (Builder builder, IntPtr handle) : base (handle)
		{
			_builder = builder;
			builder.Autoconnect (this);

			DeleteEvent += (o, args) => {
				Application.Quit ();
				args.RetVal = true;
			};
		}
	}
}
