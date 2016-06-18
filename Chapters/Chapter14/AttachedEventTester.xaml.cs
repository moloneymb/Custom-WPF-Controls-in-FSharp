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
	/// Interaction logic for AttachedEventTester.xaml
	/// </summary>
	public partial class AttachedEventTester : UserControl
	{
		public AttachedEventTester()
		{
			InitializeComponent();
		}

		private void OnButtonClick(object sender, RoutedEventArgs e)
		{
			Button b = e.Source as Button;
			var message = string.Empty;
			switch (b.Name)
			{
				case "first":
					message = "You have clicked the First button";
					break;
				case "second":
					message = "You have clicked on the Second button";
					break;
				default:
					message = "You have clicked the Third, Fourth or Fifth button"; 
					break;
			}
			MessageBox.Show(message);
		}
	}
}



