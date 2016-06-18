using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Chapters.Chapter02
{
	/// <summary>
	/// Interaction logic for InkCanvasExample.xaml
	/// </summary>
	public partial class InkCanvasExample : UserControl
	{
		public InkCanvasExample()
		{
			InitializeComponent();

			var inkDA = new DrawingAttributes();
			inkDA.Color = Colors.LightBlue;
			inkDA.Height = 5;
			inkDA.Width = 5;
			inkDA.FitToCurve = false;
			_tablet.DefaultDrawingAttributes = inkDA;
		}
	}
}
