using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chapters.Chapter14
{
	/// <summary>
	/// Interaction logic for RenderTargetBitmapExample.xaml
	/// </summary>
	public partial class RenderTargetBitmapExample : UserControl
	{
		public RenderTargetBitmapExample()
		{
			InitializeComponent();
		}

		private void CaptureBitmap(object sender, RoutedEventArgs e)
		{
			RenderTargetBitmap bitmap = new RenderTargetBitmap((int)_video.NaturalVideoWidth, 
			                                                   (int)_video.NaturalVideoHeight, 96, 96, PixelFormats.Pbgra32);
			bitmap.Render(_video);
			bitmap.Freeze();

			// Set as the image source
			ImageBrush brush = new ImageBrush(BitmapFrame.Create(bitmap));
			brush.Freeze();

			double aspect = _video.ActualWidth / _video.ActualHeight;
			double rtWidth = _renderTargets.Height * aspect;
			Border b = (Border)FindResource("Polaroid");
			b.Background = brush;
			b.Width = rtWidth;

			_renderTargets.Children.Add(b);
		}
	}
}