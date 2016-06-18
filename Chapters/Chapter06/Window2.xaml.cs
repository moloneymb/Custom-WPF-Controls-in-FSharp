using System;
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
using System.Windows.Shapes;

namespace Chapters.Chapter06
{
	/// <summary>
	/// Interaction logic for Window2.xaml
	/// </summary>
	public partial class Window2 : Window
	{
		private ICollectionView _view;
		private StringCollection _dataSource;
		Random _random = new Random();

		public Window2()
		{
			InitializeComponent();

			Loaded += Window2_Loaded;
		}

		void Window2_Loaded(object sender, RoutedEventArgs e)
		{
			_view = CollectionViewSource.GetDefaultView(_grid.ItemsSource);
			_dataSource = FindResource("DataSource") as StringCollection;
			RowVisibilityService.Attach(_grid, OnVisibleItemsChanged);
		}

		private void SortClicked(object sender, RoutedEventArgs e)
		{
			_view.SortDescriptions.Add(new SortDescription());
		}

		private void ResetClicked(object sender, RoutedEventArgs e)
		{
			_view.SortDescriptions.Clear();
		}

		private void AddClicked(object sender, RoutedEventArgs e)
		{
			_dataSource.Add("Item - " + Math.Round(_random.NextDouble(), 3));
		}

		private void RemoveClicked(object sender, RoutedEventArgs e)
		{
			if (_dataSource.Count > 0)
				_dataSource.RemoveAt(0);
		}

		private void OnVisibleItemsChanged(VisibleItemsChangedEventArgs e)
		{
			_addedItems.ItemsSource = e.AddedItems;
			_removedItems.ItemsSource = e.RemovedItems;
		}
	}
}