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

namespace Explorer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded += OnWindowLoaded;
		}

		private void OnWindowLoaded(object sender, RoutedEventArgs e)
		{
			(_treeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem).IsSelected = true;
			_treeView.Focus();
		}
	}
}
