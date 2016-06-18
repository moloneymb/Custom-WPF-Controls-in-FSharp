using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Chapters.Chapter16
{
	/// <summary>
	/// Interaction logic for CrossThreadDataBindingExample.xaml
	/// </summary>
	public partial class CrossThreadDataBindingExample : UserControl
	{
		private bool _cancelled;
		private Thread _bgThread;

		public CrossThreadDataBindingExample()
		{
			InitializeComponent();

			DataContext = 0.0;
			CancelButton.IsEnabled = false;
		}

		private void StartWork(object sender, RoutedEventArgs e)
		{
			_cancelled = false;

			_bgThread = new Thread(new ParameterizedThreadStart(PerformLongRunningOperation));
			_bgThread.IsBackground = true;

			StartButton.IsEnabled = false;
			CancelButton.IsEnabled = true;
			_bgThread.Start(Dispatcher);
		}

		private void PerformLongRunningOperation(object argument)
		{
			Dispatcher dispatcher = argument as Dispatcher;
			for (int i = 0; i <= 100; i++)
			{
				if (_cancelled) break;

				Thread.Sleep(250);
				dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<int>(ReportOperationProgress), i);
			}

			dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(OnOperationComplete));
		}

		private void ReportOperationProgress(int progress)
		{
			DataContext = progress;
		}

		private void CancelWork(object sender, RoutedEventArgs e)
		{
			_cancelled = true;
		}

		private void OnOperationComplete()
		{
			CancelButton.IsEnabled = false;
			StartButton.IsEnabled = true;
		}

	}
}
