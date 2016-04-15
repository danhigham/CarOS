using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;

namespace CarOS
{
	public static class ThreadHelper
	{
		private static List<Thread> _threads = new List<Thread>();

		public static void Add(ThreadStart ts, bool start) {
			Thread t = new Thread (ts);
			_threads.Add (t);

			if (start)
				t.Start (); 
		}

		public static void AbortAll() {
			_threads.ForEach ((t) => t.Abort());
		}

		public static void RemoveAll() {
			_threads.Clear ();
		}

		public static void Redraw(Gtk.Widget w) {
			GLib.Idle.Add(new GLib.IdleHandler(() => {
				w.QueueDraw();
				return true;
			}));
		}
	}
}

