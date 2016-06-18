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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FSharp.Chapter9;

namespace Chapters.Chapter09
{
	/// <summary>
	/// Interaction logic for MagnifyingGlassExample.xaml
	/// </summary>
	public partial class MagnifyingGlassExample : UserControl
	{
		private MagnifyingAdorner _magnifier;

		public MagnifyingGlassExample()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler(MagnifyingGlassExample_Loaded);
		}

		void MagnifyingGlassExample_Loaded(object sender, RoutedEventArgs e)
		{
			_magnifier = new MagnifyingAdorner(Container)
			                            	{
			                            		MagnifierTemplate = (ControlTemplate) FindResource("MagnifierTemplate"),
												ContainerSize = new Size(300, 300),
												ScalingFactor = 3
			                            	};
			_magnifier.Prepare();
			AdornerLayer layer = AdornerLayer.GetAdornerLayer(Container);
			layer.Add(_magnifier);
		}

	}
}
