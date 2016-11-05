using System.Windows;

namespace Microsoft.Research.DynamicDataDisplay
{
	/// <summary>
	/// Target of rendering
	/// </summary>
	public enum RenderTo
	{
		/// <summary>
		/// Rendering directly to screen
		/// </summary>
		Screen,
		/// <summary>
		/// Rendering to bitmap, which will be drawn to screen later.
		/// </summary>
		Image
	}

	public sealed class RenderState
	{
		private readonly Rect visible;

		private readonly Rect output;


		private readonly Rect renderVisible;

		private readonly RenderTo renderingType;

		public Rect RenderVisible
		{
			get { return renderVisible; }
		}

		public RenderTo RenderingType
		{
			get { return renderingType; }
		}

		public Rect Output
		{
			get { return output; }
		}

		public Rect Visible
		{
			get { return visible; }
		}

		public RenderState(Rect renderVisible, Rect visible, Rect output, RenderTo renderingType)
		{
			this.renderVisible = renderVisible;
			this.visible = visible;
			this.output = output;
			this.renderingType = renderingType;
		}
	}
}
