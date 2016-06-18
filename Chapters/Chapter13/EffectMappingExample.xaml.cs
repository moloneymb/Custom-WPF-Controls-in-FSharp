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

namespace Chapters.Chapter13
{
	/// <summary>
	/// Interaction logic for EffectMappingExample.xaml
	/// </summary>
	public partial class EffectMappingExample : UserControl
	{
		public EffectMappingExample()
		{
			InitializeComponent();
			Loaded += EffectMappingExample_Loaded;
		}

		void EffectMappingExample_Loaded(object sender, RoutedEventArgs e)
		{
			GeneralTransform transform = _button.TransformToAncestor(_buttonParent);
		}
	}
}
