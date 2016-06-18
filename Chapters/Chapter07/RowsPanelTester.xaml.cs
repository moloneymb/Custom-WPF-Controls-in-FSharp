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
using FSharp.Chapter7;

namespace Chapters.Chapter07
{
	/// <summary>
	/// Interaction logic for RowsPanelTester.xaml
	/// </summary>
	public partial class RowsPanelTester : UserControl
	{
		private TestCollection _dataSource;

		public RowsPanelTester()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler(RowsPanelTester_Loaded);
		}

		void RowsPanelTester_Loaded(object sender, RoutedEventArgs e)
		{
			//_lastBtn.BringIntoView();
		}

		private void OnAnimateScroll(object sender, RoutedEventArgs e)
		{
			bool isChecked = (sender as CheckBox).IsChecked.Value;
			_panel.AnimateScroll = isChecked;
		}
	}
}
