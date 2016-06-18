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
using Chapters.Resources;

namespace Chapters.Chapter08
{
	/// <summary>
	/// Interaction logic for StaggeredPanelTester.xaml
	/// </summary>
	public partial class StaggeredPanelTester : UserControl
	{
		private StringCollection _dataSource;

		public StaggeredPanelTester()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler(StaggeredPanelTester_Loaded);
		}

		void StaggeredPanelTester_Loaded(object sender, RoutedEventArgs e)
		{
			_dataSource = FindResource("DataSource") as StringCollection;
			for (int i = 0; i < 100; i++)
			{
				_dataSource.Add("" + i);
			}
		}
	}
}