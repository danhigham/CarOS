using System;
using System.Threading;
using Gtk;
using Gdk;
using System.Collections.Generic;
using Mono.GameMath;

namespace CarOS
{
	public class LaunchEventArgs : EventArgs 
	{
		private readonly Applications.IApplication _app;

		public LaunchEventArgs(Applications.IApplication application) 
		{
			this._app = application;	
		}

		public Applications.IApplication Application {
			get { return this._app; }
		}
	}

	public delegate void LaunchEventHandler(object sender, LaunchEventArgs e);

	public class Launcher
	{
		private const int ICON_SIZE = 128;
		private Vector2 ICON_MARGIN = new Vector2(20, 40);
		private Vector2 INT_MARGIN = new Vector2 (40, 30);

		private List<Applications.IApplication> _apps;
		private int _totalPages;
		private bool _buttonDown;
		private Vector2 _lastPointerDown;
		private Vector2 _oldTranslation;
		private Vector2 _translation;
		private float _currentPage;
		private DrawingArea _drawingArea;

		public event LaunchEventHandler Launch;

		protected virtual void OnLaunch(LaunchEventArgs e)
		{
			if (Launch != null) 
				Launch(this, e); 
		}

		public Launcher (List<Applications.IApplication> apps, DrawingArea drawArea)
		{
			_apps = apps;
			_buttonDown = false;
			_drawingArea = drawArea;
			_translation = new Vector2 (0, 0);
			drawArea.AddEvents ((int) EventMask.ButtonPressMask);

			drawArea.AddEvents ((int) 
				(EventMask.ButtonPressMask    
					|EventMask.ButtonReleaseMask    
					|EventMask.KeyPressMask    
					|EventMask.PointerMotionMask));

			drawArea.ButtonPressEvent += (o, args) => {
				int x, y;
				drawArea.GetPointer(out x, out y);
				_lastPointerDown = new Vector2(x, y);
				_buttonDown = true;
			};

			drawArea.ButtonReleaseEvent += (o, args) => {

				// on release of button, ease the launcher back in to place
				_buttonDown = false;
				int width = -drawArea.Allocation.Width;
				_currentPage = (float)Math.Round(_translation.X / (float)width);

				float posFrom = _translation.X;
				float posTo = _currentPage * width;
				float diff;

				if (posFrom < posTo) { //moving left
					diff = -(posFrom - posTo);
				} else { // moving right
					diff = (posTo - posFrom);
				}

				new Thread(() => {
					double msElapsed = 0;
					while(msElapsed < 500) {

						msElapsed += 5;						
						float f = (float)Easing.easeOutQuart(msElapsed, 0, 1, 500);
						float pos = posFrom + (f * diff);

						_oldTranslation = _translation = new Vector2(pos, _translation.Y);

						ThreadHelper.Redraw(drawArea);
						Thread.Sleep(5);
					};

				}).Start();

				// If the pointer has not moved too much, launch an app.
				int x, y;
				drawArea.GetPointer(out x, out y);
				int distance = (x - (int)_lastPointerDown.X);

				if ((distance < 5) && (distance > -5)) {

					Vector3 clickVector = new Vector3(x, y, 0) - new Vector3(_translation, 0);

					// check bounds of apps
					_apps.ForEach((Applications.IApplication obj) => {

						if (obj.Bounds.Contains(clickVector) == ContainmentType.Contains) {
							LaunchEventArgs e = new LaunchEventArgs(obj);
							OnLaunch(e);
						}

					});
				}
			};

			drawArea.MotionNotifyEvent += (o, args) => {
				int width = drawArea.Allocation.Width;
				int x, y;

				if (_buttonDown) {

					drawArea.GetPointer(out x, out y);
					int distance = (x - (int)_lastPointerDown.X);
					_translation = Vector2.Add(_oldTranslation, new Vector2(distance, 0));
					if (_translation.X > 0) _translation = new Vector2(0, _translation.Y);
					if (_translation.X < -(width * (_totalPages))) 
						_translation = new Vector2(-(width * (_totalPages)), _translation.Y);

					drawArea.QueueDraw();
				}
			};
				
			drawArea.Drawn += (o, args) => {

				Cairo.Context cr = args.Cr;

				cr.SelectFontFace("Droid Sans", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);

				int width = drawArea.Allocation.Width;
				int height = drawArea.Allocation.Height;

				int iconsPerRow = (int)(width / (ICON_SIZE + ICON_MARGIN.X));
				int rows = (int)(height / (ICON_SIZE + ICON_MARGIN.Y));

				int iconsPerPage = iconsPerRow * rows;
				_totalPages = _apps.Count / iconsPerPage;

				int page = 0;
				int row = 0;

				Vector2 pos = new Vector2(INT_MARGIN.X, INT_MARGIN.Y);
				Vector2 pagerPos = new Vector2(width / 2, height - 20);

				cr.Save();

				drawPager(cr, pagerPos);

				cr.Translate(_translation.X, _translation.Y);

				_apps.ForEach((Applications.IApplication obj) => {

					drawIcon(cr, pos, obj);

					pos = new Vector2(pos.X + ICON_SIZE + ICON_MARGIN.X, pos.Y);

					// Move to next line
					if ((pos.X + ICON_SIZE) > (width + (page * width))) {
						row ++;
						pos = new Vector2(INT_MARGIN.X + (page * width), INT_MARGIN.Y + ICON_SIZE + ICON_MARGIN.Y);
					}

					if (row == 2) {
						row = 0;
						page ++;
						pos = new Vector2(INT_MARGIN.X + (page * width), INT_MARGIN.Y);
					}

				});

				cr.Restore();

				((IDisposable) cr.GetTarget()).Dispose();                                      
				((IDisposable) cr).Dispose();
			};
		}

		public void Hide() {
			_drawingArea.Visible = false;
		}

		private void drawIcon(Cairo.Context cr, Vector2 pos, Applications.IApplication app) {

			// draw icon

			Cairo.Surface icon = app.Icon;
			icon.Show(cr, pos.X, pos.Y);
			app.Bounds = new BoundingBox(new Vector3(pos, 0), new Vector3(pos.X + ICON_SIZE,  pos.Y + ICON_SIZE, 0));

			// draw name
			cr.SetSourceRGBA (0.3, 0.3, 0.3, 1);
			cr.SetFontSize(18);

			Cairo.TextExtents te = cr.TextExtents(app.Name);

			cr.MoveTo (pos.X + ((ICON_SIZE - te.Width) / 2), pos.Y + ICON_SIZE + (te.Height * 1.25));
			cr.ShowText(app.Name);
		}

		private void drawPager(Cairo.Context cr, Vector2 pos) {

			cr.LineWidth = 1;
			var r = 5;
			var pagerWidth = _totalPages * ((r * 2) + 10);
			pos = new Vector2 (pos.X - (pagerWidth / 2), pos.Y);

			for (int x = 0; x <= _totalPages; x++) {
				cr.MoveTo ((pos.X + r) + (x * 20), pos.Y);

				cr.SetSourceRGB(0.8, 0.8, 0.8);
				cr.Arc (pos.X + (x * 20), pos.Y, r, 0, 2 * Math.PI);

				if (x == _currentPage) {
					cr.StrokePreserve ();
					cr.Fill ();
				} else {
					cr.Stroke ();
				}
			}
		}
	}
}

