using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Chapters
{
	/// <summary>
	/// Interaction logic for AutomationClientAPIExample.xaml
	/// </summary>
	public partial class AutomationClientAPIExample
	{
		private IntPtr _handle;
		private Dispatcher _dispatcher;
		private bool _alreadySubscribed;

		public AutomationClientAPIExample()
		{
			this.InitializeComponent();
			Loaded += delegate
			{
				_dispatcher = Dispatcher;
				_handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
			};
		}

		private void PrintMessage(string msg)
		{
			ConsoleText.Text = msg;
		}

		private void LocateElements(object sender, RoutedEventArgs e)
		{
			WaitCallback callback = delegate(object state)
					{
						// Locate the application from the window handle
						var windowElt = AutomationElement.FromHandle(_handle);

						// Locate the UserControl with AutomationId
						var ucElt = windowElt.FindFirst(TreeScope.Descendants,
							new PropertyCondition(AutomationElement.AutomationIdProperty, "ID_AutomationClient"));

						// Find all its children who are Buttons
						var childElts = ucElt.FindAll(TreeScope.Descendants,
							new PropertyCondition(AutomationElement.ClassNameProperty, "Button"));

						// Construct the description message
						StringBuilder sb = new StringBuilder();
						sb.Append("---=== Locating Elements ===---\n");
						sb.Append(childElts.Count + " Buttons found with names: ");
						foreach (AutomationElement childElt in childElts)
						{
							sb.Append("<" + childElt.Current.Name + ">, ");
						}
						sb.Append(Environment.NewLine);

						// Fire on UI thread
						_dispatcher.BeginInvoke(DispatcherPriority.Normal,
													new Action<string>(PrintMessage),
													sb.ToString());
					};

			ThreadPool.QueueUserWorkItem(callback);

		}

		private void CheckForControlPattern(object sender, RoutedEventArgs e)
		{
			WaitCallback callback = (state) =>
					{
						AutomationElement windowElt = AutomationElement.FromHandle(_handle);

						// Locate this UserControl with AutomationId
						var ucElt = windowElt.FindFirst(TreeScope.Descendants,
														new PropertyCondition(
															AutomationElement.AutomationIdProperty,
															"ID_AutomationClient"));

						// Condition
						PropertyCondition c = new PropertyCondition(AutomationElement.IsControlElementProperty,
																	true);
						AutomationElementCollection elements = ucElt.FindAll(TreeScope.Descendants, c);

						StringBuilder sb = new StringBuilder();
						sb.Append("---=== Checking for Control Patterns ===---\n");
						sb.Append("Controls supporting the ScrollPattern:\n");
						foreach (AutomationElement elt in elements)
						{
							var patterns = from pattern in elt.GetSupportedPatterns()
										   where pattern.Id == ScrollPattern.Pattern.Id
										   select pattern.ProgrammaticName;

							if (patterns.Count() > 0)
							{
								sb.Append(elt.Current.ClassName);
								sb.Append(Environment.NewLine);
							}
						}

						// Fire on UI thread
						_dispatcher.BeginInvoke(DispatcherPriority.Normal,
												new Action<string>(PrintMessage),
												sb.ToString());
					};

			ThreadPool.QueueUserWorkItem(callback);
		}

		private void ReadProperties(object sender, RoutedEventArgs e)
		{
			WaitCallback callback = (state) =>
			{
				AutomationElement windowElt = AutomationElement.FromHandle(_handle);

				// Locate this UserControl with AutomationId
				var ucElt = windowElt.FindFirst(TreeScope.Descendants,
												new PropertyCondition(
													AutomationElement.AutomationIdProperty,
													"ID_AutomationClient"));

				// Condition
				PropertyCondition c = new PropertyCondition(AutomationElement.IsControlElementProperty,
															true);
				AutomationElementCollection elements = ucElt.FindAll(TreeScope.Descendants, c);

				StringBuilder sb = new StringBuilder();
				sb.Append("---=== Reading Automation Properties ===---\n");
				sb.Append(ucElt.Current.ClassName + ": \n");
				AutomationProperty[] properties = ucElt.GetSupportedProperties();
				foreach (AutomationProperty property in properties)
				{
					sb.Append("\t" + property.ProgrammaticName + ": " + ucElt.GetCurrentPropertyValue(property));
					sb.Append("\n");
				}

				// Fire on UI thread
				_dispatcher.BeginInvoke(DispatcherPriority.Normal,
										new Action<string>(PrintMessage),
										sb.ToString());
			};

			ThreadPool.QueueUserWorkItem(callback);
		}

		private void SubscribeToRangeSelectorEvents(object sender, RoutedEventArgs e)
		{
			WaitCallback callback =
				delegate
				{
					if (_alreadySubscribed) return;
					AutomationElement windowElt = AutomationElement.FromHandle(_handle);

					// Locate this UserControl with AutomationId
					var rangeSelectorElt = windowElt.FindFirst(TreeScope.Descendants,
															   new PropertyCondition(
																AutomationElement.AutomationIdProperty,
																"ID_RangeSelector"));

					Automation.AddAutomationPropertyChangedEventHandler(rangeSelectorElt, TreeScope.Element,
																		TakeActionForEvent,
																		new[] { ValuePattern.ValueProperty });

					_alreadySubscribed = true;
				};

			ThreadPool.QueueUserWorkItem(callback);
			PrintMessage("Subscribed to PropertyChange events on RangeSelector");
		}

		private void TakeActionForEvent(object sender, AutomationPropertyChangedEventArgs args)
		{
			_dispatcher.BeginInvoke(DispatcherPriority.Normal,
									new Action<string>(PrintMessage),
									"New Range: " + args.NewValue);
		}

		private void UnsubscribeFromRangeSelectorEvents(object sender, RoutedEventArgs e)
		{
			WaitCallback callback =
				delegate
				{
					if (!_alreadySubscribed) return;
					AutomationElement windowElt = AutomationElement.FromHandle(_handle);

					// Locate this UserControl with AutomationId
					var rangeSelectorElt = windowElt.FindFirst(TreeScope.Descendants,
													new PropertyCondition(
														AutomationElement.AutomationIdProperty,
														"ID_RangeSelector"));

					Automation.RemoveAutomationPropertyChangedEventHandler(rangeSelectorElt, TakeActionForEvent);
					_alreadySubscribed = false;
				};

			ThreadPool.QueueUserWorkItem(callback);
			PrintMessage("Unsubscribed from PropertyChange events on RangeSelector");
		}

		private void NavigateToAncestor(object sender, RoutedEventArgs e)
		{
			WaitCallback callback =
				delegate
				{
					AutomationElement windowElt = AutomationElement.FromHandle(_handle);

					var rangeSelectorElt = windowElt.FindFirst(TreeScope.Descendants,
													new PropertyCondition(
														AutomationElement.AutomationIdProperty,
														"ID_RangeSelector"));

					AutomationElement parent = rangeSelectorElt;
					StringBuilder sb = new StringBuilder();
					while (parent != null)
					{
						string className = parent == AutomationElement.RootElement ? "RootElement (Desktop)" : parent.Current.ClassName;
						sb.Append("\t" + className + "\n");
						parent = TreeWalker.RawViewWalker.GetParent(parent);
					}

					_dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<string>(PrintMessage), sb.ToString());
				};

			ThreadPool.QueueUserWorkItem(callback);
		}

		private void PerformOperation(object sender, RoutedEventArgs e)
		{
			WaitCallback callback =
				delegate
				{
					AutomationElement windowElt = AutomationElement.FromHandle(_handle);

					// Locate this UserControl with AutomationId
					var btnElt = windowElt.FindFirst(TreeScope.Descendants,
													new PropertyCondition(
														AutomationElement.AutomationIdProperty,
														"ID_InvokeButton"));

					InvokePattern pattern = btnElt.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
					if (pattern != null) // Button implements the IInvokeProvider interface
					{
						pattern.Invoke();
					}

				};

			ThreadPool.QueueUserWorkItem(callback);
		}

		private void PerformOperationOnWindow(object sender, RoutedEventArgs e)
		{
			WaitCallback callback =
				delegate
				{
					AutomationElement windowElt = AutomationElement.FromHandle(_handle);

					WindowPattern pattern = windowElt.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern;
					if (pattern != null) // Button implements the IInvokeProvider interface
					{
						pattern.SetWindowVisualState(WindowVisualState.Normal);
					}

				};

			ThreadPool.QueueUserWorkItem(callback);
		}

		private void InvokeOperationOnButton(object sender, RoutedEventArgs e)
		{
			PrintMessage("Operation on Button invoked");
		}
	}
}