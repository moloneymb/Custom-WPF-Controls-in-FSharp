using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xceed.Wpf.DataGrid;

namespace Chapter_AP
{

	public class RowVisibilityService
	{

		private static readonly DependencyProperty RVSContextProperty = DependencyProperty.Register(
			"RVSContext", typeof(RVSContext), typeof(RowVisibilityService));

		private static void DetachDataGrid(DataGridControl grid)
		{
			RVSContext context = grid.GetValue(RVSContextProperty) as RVSContext;

			ICollectionView view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
			view.CollectionChanged -= context.CollectionChangedHandler;

			ScrollViewer scroller = grid.Template.FindName("PART_ScrollViewer", grid) as ScrollViewer;
			scroller.ScrollChanged -= context.ScrollChangedHandler;

			context.Callback = null;

			grid.ClearValue(RVSContextProperty);
		}

		private static void AttachDataGrid(DataGridControl grid, Action<VisibleItemsChangedEventArgs> changed)
		{
			RVSContext context = new RVSContext();
			context.Callback = changed;

			// To monitor collection changes
			ICollectionView view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
			context.CollectionChangedHandler = delegate(object s, NotifyCollectionChangedEventArgs args)
			{
				DataGridControl_CollectionChanged(grid, args);
			};
			view.CollectionChanged += context.CollectionChangedHandler;

			// To monitor scroll changes
			ScrollViewer scroller = grid.Template.FindName("PART_ScrollViewer", grid) as ScrollViewer;
			context.ScrollChangedHandler = delegate(object s, ScrollChangedEventArgs args)
			{
				DataGridControl_ScrollChanged(grid, args);
			};
			scroller.ScrollChanged += context.ScrollChangedHandler;

			grid.SetValue(RVSContextProperty, context);

			// First time notification
			int start, end;
			CalculateIndexRange(grid, out start, out end);
			context.StartIndex = start;
			context.EndIndex = end;

			UpdateAndNotifyVisibleItems(grid);
		}

		private static void CalculateIndexRange(DataGridControl grid, out int start, out int end)
		{
			ScrollViewer scroller = grid.Template.FindName("PART_ScrollViewer", grid) as ScrollViewer;

			start = (int) scroller.VerticalOffset;

			end = (int)(scroller.VerticalOffset + scroller.ViewportHeight);
			end = Math.Min(end, grid.Items.Count- 1);

		}

		private static void DataGridControl_ScrollChanged(DataGridControl grid, ScrollChangedEventArgs e)
		{
			RVSContext context = grid.GetValue(RVSContextProperty) as RVSContext;
			int firstIndex, lastIndex;
			CalculateIndexRange(grid, out firstIndex, out lastIndex);

			if (firstIndex != context.StartIndex || lastIndex != context.EndIndex)
			{
				context.StartIndex = firstIndex;
				context.EndIndex = lastIndex;

				UpdateAndNotifyVisibleItems(grid);
			}
		}

		private static void DataGridControl_CollectionChanged(DataGridControl grid,
															  NotifyCollectionChangedEventArgs e)
		{
			RVSContext context = grid.GetValue(RVSContextProperty) as RVSContext;

			int firstIndex, lastIndex;
			CalculateIndexRange(grid, out firstIndex, out lastIndex);

			context.StartIndex = firstIndex;
			context.EndIndex = lastIndex;

			UpdateAndNotifyVisibleItems(grid);
		}

		private static void UpdateAndNotifyVisibleItems(DataGridControl grid)
		{
			RVSContext context = grid.GetValue(RVSContextProperty) as RVSContext;

			List<object> newItems = new List<object>();
			List<object> oldItems = new List<object>();
			List<object> unchangedItems = new List<object>();

			for (int i = context.StartIndex; i <= context.EndIndex; i++)
			{
				object item = grid.Items[i];
				if (context.VisibleItems.Contains(item))
				{
					context.VisibleItems.Remove(item);
					unchangedItems.Add(item);
				}
				else
				{
					newItems.Add(item);
				}
			}
			oldItems = context.VisibleItems;
			context.VisibleItems = new List<object>(unchangedItems);
			context.VisibleItems.AddRange(newItems);

			if (oldItems.Count == 0 && newItems.Count == 0) return; // No change

			// Changes occurred, Notify
			VisibleItemsChangedEventArgs args = new VisibleItemsChangedEventArgs()
													{
														AddedItems = newItems,
														RemovedItems = oldItems
													};

			context.Callback(args);
		}

		#region Private class for internal housekeeping
		private class RVSContext
		{
			public NotifyCollectionChangedEventHandler CollectionChangedHandler { get; set; }
			public ScrollChangedEventHandler ScrollChangedHandler { get; set; }

			internal List<object> VisibleItems { get; set; }

			public int StartIndex { get; set; }
			public int EndIndex { get; set; }

			public Action<VisibleItemsChangedEventArgs> Callback { get; set; }

			public RVSContext()
			{
				VisibleItems = new List<object>();
			}
		}

		#endregion

		public static void Attach(DataGridControl grid, Action<VisibleItemsChangedEventArgs> changed)
		{
			AttachDataGrid(grid, changed);
		}

		public static void Detach(DataGridControl grid)
		{
			DetachDataGrid(grid);
		}
	}

	public class VisibleItemsChangedEventArgs : EventArgs
	{
		public List<object> AddedItems { get; set; }
		public List<object> RemovedItems { get; set; }
	}
}