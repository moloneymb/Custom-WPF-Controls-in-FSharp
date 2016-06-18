using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FSharp.Chapter10;

namespace Chapters.Chapter10
{
	/// <summary>
	/// Interaction logic for CustomWindowTester.xaml
	/// </summary>
	public partial class CustomWindowTester : UserControl
	{
		public CustomWindowTester()
		{
			InitializeComponent();
			new BlackStoneWindow().ShowDialog();
		}
	}
}
