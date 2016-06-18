using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Chapters.Resources;

namespace Chapters.Chapter09
{
	/// <summary>
	/// Interaction logic for LassoSelectionExample.xaml
	/// </summary>
	public partial class LassoSelectionExample : UserControl
	{
		List<ListBoxItem> _selections = new List<ListBoxItem>();
		private StringCollection _dataSource;

		public LassoSelectionExample()
		{
			InitializeComponent();
			Loaded += LassoSelectionExample_Loaded;
		}

		void LassoSelectionExample_Loaded(object sender, RoutedEventArgs e)
		{
			_dataSource = FindResource("DataSource") as StringCollection;
			for (int i = 0; i < 20; i++)
			{
				_dataSource.Add("Item - " + i);
			}

			OverlayInkCanvas.DefaultDrawingAttributes.Width = 10;
			OverlayInkCanvas.DefaultDrawingAttributes.Height = 10;
			OverlayInkCanvas.DefaultDrawingAttributes.Color = Colors.Red;

			ItemsListBox.Focus();
		}

		private void OnStrokeCollection(object sender, InkCanvasStrokeCollectedEventArgs args)
		{
			_selections.Clear();

			RectangleGeometry geom = new RectangleGeometry(args.Stroke.GetBounds());
			VisualTreeHelper.HitTest(ItemsListBox, OnFilter, OnResult, new GeometryHitTestParameters(geom));

			foreach (ListBoxItem item in _selections)
			{
				item.IsSelected = !item.IsSelected;
			}

			OverlayInkCanvas.Strokes.Remove(args.Stroke);
			EndLassoSelection();
			LassoCheckBox.IsChecked = false;
		}

		private HitTestFilterBehavior OnFilter(DependencyObject potentialHitTestTarget)
		{
			ListBoxItem item = potentialHitTestTarget as ListBoxItem;
			if (item != null)
			{
				_selections.Add(item);
				return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
			}

			return HitTestFilterBehavior.ContinueSkipSelf;
		}

		private HitTestResultBehavior OnResult(HitTestResult result)
		{
			return HitTestResultBehavior.Continue;
		}

		private void StartLassoSelection()
		{
			OverlayInkCanvas.Visibility = Visibility.Visible;
		}

		private void EndLassoSelection()
		{
			OverlayInkCanvas.Visibility = Visibility.Hidden;
			ItemsListBox.Focus();
		}

		private void ToggleSelection(object sender, RoutedEventArgs e)
		{
			bool isChecked = (sender as CheckBox).IsChecked.Value;
			if (isChecked) StartLassoSelection();
			else EndLassoSelection();
		}
	}
}
