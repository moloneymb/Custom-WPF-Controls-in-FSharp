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
using FSharp.Chapter9;

namespace Chapters.Chapter09
{
	/// <summary>
	/// Interaction logic for TransitionTester.xaml
	/// </summary>
	public partial class TransitionTester : UserControl
	{
		private bool _isPlaying;
		private UIElement _prev;
		private UIElement _next;

		public TransitionTester()
		{
			InitializeComponent();
			Loaded += TransitionTester_Loaded;
		}

		void TransitionTester_Loaded(object sender, RoutedEventArgs e)
		{
			_prev = _view1;
			_next = _view2;
            //_trans.TransitionCompleted += delegate
            //                                {
            //                                    _isPlaying = false;
            //                                    var temp = _prev;
            //                                    _prev = _next;
            //                                    _next = temp;
            //                                };

		}

		private void PlayTransition(object sender, RoutedEventArgs e)
		{
			if (_isPlaying) return;

			string trans = (sender as Button).Name;
			if (trans == "Fade")
			{
				_trans.Transition = new FadeTransition();
			}
			else if (trans == "Slide")
			{
				_trans.Transition = new SlideTransition();
			}

			_trans.ApplyTransition(_prev, _next);
		}
	}
}
