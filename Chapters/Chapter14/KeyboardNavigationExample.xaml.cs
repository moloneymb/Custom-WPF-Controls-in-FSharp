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
	/// Interaction logic for KeyboardNavigationExample.xaml
	/// </summary>
	public partial class KeyboardNavigationExample : UserControl
	{
		public KeyboardNavigationExample()
		{
			InitializeComponent();

			EventManager.RegisterClassHandler(typeof(Window), Keyboard.GotKeyboardFocusEvent, new RoutedEventHandler(OnKeyboardFocus), true);
		}

		private void OnKeyboardFocus(object sender, RoutedEventArgs e)
		{
			_focusedElement.Text = Keyboard.FocusedElement.ToString();
		}
	}
}