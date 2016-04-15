using System;
namespace Applications
{
	public interface IApplication
	{
		string Name { get; set; }
		Cairo.ImageSurface Icon { get; } 
		string WidgetClass { get; }
		Mono.GameMath.BoundingBox Bounds { get; set; }
	}
}

