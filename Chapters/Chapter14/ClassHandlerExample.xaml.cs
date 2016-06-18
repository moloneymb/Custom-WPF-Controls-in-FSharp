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

namespace Chapters.Chapter14
{
	/// <summary>
	/// Interaction logic for ClassHandlerExample.xaml
	/// </summary>
	public partial class ClassHandlerExample : UserControl
	{
		static ClassHandlerExample()
		{
			EventManager.RegisterClassHandler(typeof(ClassHandlerExample), Mouse.MouseDownEvent,
				new RoutedEventHandler(OnMouseDownClassHandler), true);
		}
		public ClassHandlerExample()
		{
			InitializeComponent();

		}

		private static void OnMouseDownClassHandler(object sender, RoutedEventArgs e1)
		{
			Debug.WriteLine("MouseDownEvent Occurred");
		}

		private static void HandleAllRoutedEvents()
		{
			var events = EventManager.GetRoutedEvents();
			foreach (RoutedEvent routedEvent in events)
			{
				EventManager.RegisterClassHandler(typeof(ClassHandlerExample), routedEvent, 
					new RoutedEventHandler(AllEventsClassHandler), true);
			}
		}

		private static void AllEventsClassHandler(object sender, RoutedEventArgs e)
		{
			// Handle the routed event
		}
	}
}
