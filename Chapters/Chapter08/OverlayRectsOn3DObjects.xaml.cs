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

namespace Chapters.Chapter08
{
	/// <summary>
	/// Interaction logic for OverlayRectsOn3DObjects.xaml
	/// </summary>
	public partial class OverlayRectsOn3DObjects : UserControl
	{
		private ModelUIElement3D _dummyModel;

		public OverlayRectsOn3DObjects()
		{
			InitializeComponent();
			Loaded += OverlayRectsOn3DObjects_Loaded;
		}

		void OverlayRectsOn3DObjects_Loaded(object sender, RoutedEventArgs e)
		{
			_dummyModel = FindResource("Plane") as ModelUIElement3D;
			_dummyModel.Transform = new MatrixTransform3D();
			_dummyModel.IsHitTestVisible = false;
			_container.Children.Add(_dummyModel);

			for (int i = -2; i <= 2; i++)
			{
				ModelUIElement3D model = FindResource("Plane") as ModelUIElement3D;
				((model.Transform as Transform3DGroup).Children[1] as TranslateTransform3D).OffsetX = i * 1.5;
				_container.Children.Add(model);
			}

			GetRectBounds();
		}

		private void GetRectBounds()
		{
			Matrix3D mat = Matrix3D.Identity;
			mat.Rotate(new Quaternion(new Vector3D(0, 1, 0), 45));
			mat.Translate(new Vector3D(-2 * 1.5, 0, 0));
			(_dummyModel.Transform as MatrixTransform3D).Matrix = mat;
		}

		private void ShowOverlay(object sender, RoutedEventArgs e)
		{
			bool show = (sender as CheckBox).IsChecked.Value;
			if (show)
			{
				foreach (ModelUIElement3D child in _container.Children)
				{
					Rect rect = child.TransformToAncestor(_viewport).TransformBounds(VisualTreeHelper.GetContentBounds(child));
					Rectangle overlay = new Rectangle();
					Canvas.SetLeft(overlay, rect.Left);
					Canvas.SetTop(overlay, rect.Top);
					overlay.Width = rect.Width;
					overlay.Height = rect.Height;

					_rectOverlay.Children.Add(overlay);
				}
			}
			else
			{
				_rectOverlay.Children.Clear();
			}
		}
	}
}
