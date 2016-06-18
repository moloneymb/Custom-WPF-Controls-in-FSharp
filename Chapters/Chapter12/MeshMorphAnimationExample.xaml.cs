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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FSharp.Chapter12;

namespace Chapters.Chapter12
{
	/// <summary>
	/// Interaction logic for MeshMorphAnimationExample.xaml
	/// </summary>
	public partial class MeshMorphAnimationExample : UserControl
	{
		public MeshMorphAnimationExample()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler(Window1_Loaded);
		}

		private void Window1_Loaded(object sender, RoutedEventArgs e)
		{
			ImageBrush brush = FindResource("MeshBrush") as ImageBrush;
			GeometryModel3D model = new GeometryModel3D();
			model.Geometry = MeshCreator.CreateMesh(SurfaceType.Plane, 100, 100);
			model.Material = new DiffuseMaterial(brush);
			model.BackMaterial = new DiffuseMaterial(Brushes.Gray);

			NameScope scope = NameScope.GetNameScope(this) as NameScope;
			scope.RegisterName("MeshGeometry", model.Geometry);

			_modelGroup.Children.Add(model);

			Trackball b = new Trackball();
			b.EventSource = this;
			_viewport.Camera.Transform = b.Transform;
		}

		private void ChangeTargetSurface(object sender, RoutedEventArgs e)
		{
			RadioButton b = e.Source as RadioButton;
			string mesh = b.Content as string;
			SurfaceType surface = SurfaceType.Plane;

            if (mesh == "Cone")
            {
                surface = SurfaceType.Cone;
            }
            if (mesh == "Cylinder")
            {
                surface = SurfaceType.Cylinder;
            }
            if (mesh == "Sphere")
            {
                surface = SurfaceType.Sphere;
            }
            if (mesh == "Plane")
            {
                surface = SurfaceType.Plane;
            }

			Storyboard animation = FindResource("MeshAnimator") as Storyboard;
			(animation.Children[0] as MeshMorphAnimation).EndSurface = surface;
			animation.Begin(this);
		}
	}
}