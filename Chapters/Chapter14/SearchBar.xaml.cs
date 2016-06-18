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
	/// Interaction logic for SearchBar.xaml
	/// </summary>
	public partial class SearchBar : UserControl
	{
		public static readonly RoutedEvent SearchInvoked = EventManager.RegisterRoutedEvent("SearchInvoked",
		                                                                                    RoutingStrategy.Bubble,
		                                                                                    typeof (RoutedEventHandler),
		                                                                                    typeof (SearchBar));

		private void RaiseSearchInvoked(string searchText)
		{
			var args = new SearchEventArgs()
			           	{
			           		SearchText = searchText, 
							RoutedEvent = SearchInvoked
			           	};

			RaiseEvent(args);
		}

		public SearchBar()
		{
			InitializeComponent();
		}
	}

	public class SearchEventArgs : RoutedEventArgs
	{
		public string SearchText { get; set; }
	}
}
