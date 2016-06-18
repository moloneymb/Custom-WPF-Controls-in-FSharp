using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace Chapters.Chapter14
{
	/// <summary>
	/// Interaction logic for WeakEventTester.xaml
	/// </summary>
	public partial class WeakEventTester : UserControl, IWeakEventListener
	{
		private DispatcherTimer _timer;

		public WeakEventTester()
		{
			InitializeComponent();
			Loaded += WeakEventTester_Loaded;
		}

		void WeakEventTester_Loaded(object sender, RoutedEventArgs e)
		{
			//DataChangedEventManager.AddListener(_eventSource, this);
		}

		public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(DataChangedEventManager))
			{
				HandleSourceEvent(e);
				return true;
			}

			return false;
		}

		private void HandleSourceEvent(EventArgs e)
		{
			_message.Text = "Weak Event delivered @ " + DateTime.Now;
		}

		private void RaiseWeakEvent(object sender, RoutedEventArgs e)
		{
			_eventSource.DoSomethingtoRaiseEvent();
		}
	}
}
