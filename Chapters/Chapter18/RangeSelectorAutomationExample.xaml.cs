using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Condition=System.Windows.Condition;

namespace Chapters.Chapter18
{
	/// <summary>
	/// Interaction logic for RangeSelectorAutomationExample.xaml
	/// </summary>
	public partial class RangeSelectorAutomationExample : UserControl
	{
		private IntPtr _handle;

		public RangeSelectorAutomationExample()
		{
			InitializeComponent();
			Loaded += delegate
						{
							_handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
							ThreadPool.QueueUserWorkItem(FindAutomationElements);
						};
		}

		private void FindAutomationElements(object state)
		{
			var elt = AutomationElement.FromHandle(_handle);
			var rangeSelector = elt.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "RangeSelector"));
			Automation.AddAutomationPropertyChangedEventHandler(rangeSelector, TreeScope.Element, (sender, args) => MessageBox.Show("Hello World" + args.OldValue), new[] { RangeValuePatternIdentifiers.ValueProperty });
		}

		private void LocateElement()
		{
			IntPtr handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			AutomationElement windowElt = AutomationElement.FromHandle(handle);

			// Condition
			PropertyCondition c = new PropertyCondition(AutomationElement.ClassNameProperty, "RangeSelector");
			windowElt.FindAll(TreeScope.Descendants, c);
		}

		private void ListeningToEvents()
		{
			IntPtr handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			var elt = AutomationElement.FromHandle(handle);

			var rangeSelector = elt.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "RangeSelector"));
			Automation.AddAutomationPropertyChangedEventHandler(rangeSelector, TreeScope.Element, 
				(sender, args) =>
					{
						MessageBox.Show("Hello World" + args.OldValue);
					}, 
					new[] { ValuePatternIdentifiers.ValueProperty });
		}

		private void NavigateAutomationTree()
		{
			// Loop through all application windows (children)
			AutomationElement elt = TreeWalker.RawViewWalker.GetFirstChild(AutomationElement.RootElement);
			while (elt != null)
			{
				// print elt.Current.Name
				elt = TreeWalker.RawViewWalker.GetNextSibling(elt);
			}

			// Walk up the tree
			IntPtr handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			AutomationElement windowElt = AutomationElement.FromHandle(handle);
			PropertyCondition c = new PropertyCondition(AutomationElement.ClassNameProperty, "RangeSelector");
			AutomationElement rangeElt = windowElt.FindFirst(TreeScope.Descendants, c);

			while (elt != null)
			{
				// print elt.Current.Name;
				elt = TreeWalker.RawViewWalker.GetParent(elt);
			}

		}

		private void PerformInvokeOnControl()
		{
			IntPtr handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			AutomationElement windowElt = AutomationElement.FromHandle(handle);
			PropertyCondition c = new PropertyCondition(AutomationElement.ClassNameProperty, "Button");
			AutomationElement elt = windowElt.FindFirst(TreeScope.Descendants, c);

			// Must have IInovkeProvider
			var patterns = from pattern in elt.GetSupportedPatterns()
						   where pattern.Id == InvokePattern.Pattern.Id
			               select pattern;
			if (patterns.Count() == 1)
			{
				AutomationPattern pattern = patterns.First();

				object objPattern;
				InvokePattern invokePattern;
				if (elt.TryGetCurrentPattern(InvokePattern.Pattern, out objPattern))
				{
					invokePattern = objPattern as InvokePattern;
				}

			}
		}
	}

}
