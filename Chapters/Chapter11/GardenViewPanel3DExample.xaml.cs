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
using System.Windows.Shapes;
using Chapters.Resources;

namespace Chapters.Chapter11
{
	/// <summary>
	/// Interaction logic for GardenViewPanel3DExample.xaml
	/// </summary>
	public partial class GardenViewPanel3DExample : UserControl
	{
		private StringCollection _dataSource;
		private Random _random = new Random();

		public GardenViewPanel3DExample()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler(Window_V2_Loaded);
		}

		private void Window_V2_Loaded(object sender, RoutedEventArgs e)
		{
			_dataSource = FindResource("DataSource") as StringCollection;
		}

		private void AddItem(object sender, RoutedEventArgs e)
		{
			string img = "/Resources;component/Chapter11/0" + _random.Next(1, 10) + ".png";
			_dataSource.Add(img);
		}

		private void RemoveItem(object sender, RoutedEventArgs e)
		{
			if (_dataSource.Count <= 0) return;
			int index = _random.Next(_dataSource.Count);
			_dataSource.RemoveAt(index);
		}
	}
}