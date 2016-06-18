using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using FSharp.Chapter16;

namespace Chapters.Chapter16
{
	/// <summary>
	/// Interaction logic for NotifySourceTargetExample.xaml
	/// </summary>
	public partial class NotifySourceTargetExample : UserControl
	{
		private DispatcherTimer _timer;

		public NotifySourceTargetExample()
		{
			InitializeComponent();

			_timer = new DispatcherTimer();
			_timer.Interval = TimeSpan.FromSeconds(3);
			_timer.Tick += ChangeItemsSource;

			Loaded += delegate
			          	{
			          		_timer.Start();
			          	};
			Unloaded += delegate
			            	{
			            		_timer.Stop();
			            		_timer = null;
			            	};
		}

		private void ChangeItemsSource(object sender, EventArgs e)
		{
			// Take Action
			List<string> names = new List<string>();
			for (int i = 0; i < 10; i++)
			{
				names.Add("First Last [" + i + "] @ " + DateTime.Now.TimeOfDay);
			}
			(FindResource("DataSource") as AddressBook).ContactNames = names;
		}

		private void OnItemsSourceChanged(object sender, DataTransferEventArgs e)
		{
		}
		//private void GetNotifiedUsingAddValueChanged()
		//{
		//    ListBox lb = FindName("ContactsList") as ListBox;
		//    var descriptor = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof (IEnumerable));
		//    descriptor.AddValueChanged(lb, (sender, e) =>
		//                                    {
		//                                        // Take action
		//                                    });
		//}
	}
}
