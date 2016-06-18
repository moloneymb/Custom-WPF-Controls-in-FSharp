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

namespace Chapters.Chapter14
{
	/// <summary>
	/// Interaction logic for CommandSourceTester.xaml
	/// </summary>
	public partial class CommandSourceTester : UserControl
	{
		public CommandSourceTester()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler(CommandSourceTester_Loaded);
		}

		void CommandSourceTester_Loaded(object sender, RoutedEventArgs e)
		{
			//_spiker.Focus();
		}

		private void OnButtonClick(object sender, RoutedEventArgs e)
		{
			//_spiker.DoSomethingToInvokeCommand();
		}

		private void OnExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			MessageBox.Show("Help invoked with parameter: " + e.Parameter);
		}

		private void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}
	}
}
