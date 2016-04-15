using System;

namespace Applications.TestApplication
{
	public class Definition: DefinitionBase, IApplication 
	{
		Cairo.ImageSurface _icon;
		string _name;

		public string Name {
			get {
				return(_name);
			}
			set {
				_name = value;
			}
		}

		public Cairo.ImageSurface Icon {
			get {
				if (_icon != null)
					return _icon;
				_icon = base.LoadIcon ("TestApplication"); 
				return _icon;
			}
		}

		public String WidgetClass {
			get {
				return "Applications.TestApplication.AppWidget";
			}
		}

		public Mono.GameMath.BoundingBox Bounds { get; set; }

		public Definition() {
			_name = "Test App";
		}
	}
}

