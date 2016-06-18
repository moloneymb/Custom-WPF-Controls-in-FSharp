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

namespace Chapters.Chapter16
{
	/// <summary>
	/// Interaction logic for BackgroundWorkerExample.xaml
	/// </summary>
	public partial class BackgroundWorkerExample : UserControl
	{
		private BackgroundWorker _worker;

		public BackgroundWorkerExample()
		{
			InitializeComponent();
			DataContext = 0.0;
			CancelButton.IsEnabled = false;
		}

		private void OnOperationCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_worker = null;
			CancelButton.IsEnabled = false;
			StartButton.IsEnabled = true;
		}

		private void ReportOperationProgress(object sender, ProgressChangedEventArgs e)
		{
			DataContext = e.ProgressPercentage;
		}

		private void PerformLongRunningOperation(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = sender as BackgroundWorker;

			for (int i = 0; i < 100; i++)
			{
				if (worker.CancellationPending)
				{
					break;
				}

				worker.ReportProgress(i);
				Thread.Sleep(1000);
			}
		}

		private void StartWork(object sender, RoutedEventArgs e)
		{
			_worker = new BackgroundWorker();
			_worker.WorkerSupportsCancellation = true;
			_worker.WorkerReportsProgress = true;

			_worker.DoWork += PerformLongRunningOperation;
			_worker.ProgressChanged += ReportOperationProgress;
			_worker.RunWorkerCompleted += OnOperationCompleted;

			StartButton.IsEnabled = false;
			CancelButton.IsEnabled = true;

			_worker.RunWorkerAsync();
		}

		private void CancelWork(object sender, RoutedEventArgs e)
		{
			if (_worker != null)
			{
				_worker.CancelAsync();
			}
		}
	}
}
