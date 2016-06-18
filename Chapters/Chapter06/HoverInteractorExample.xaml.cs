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
using FSharp.Chapter6;

namespace Chapters.Chapter06
{
	/// <summary>
	/// Interaction logic for HoverInteractorExample.xaml
	/// </summary>
	public partial class HoverInteractorExample : UserControl
	{
		public HoverInteractorExample()
		{
			InitializeComponent();

			StringDataSource src = new StringDataSource();
			for (int i = 0; i < 10; i++)
			{
				src.Add("Item - " + i);
			}

			_listBox.ItemsSource = src;
		}
	}
}