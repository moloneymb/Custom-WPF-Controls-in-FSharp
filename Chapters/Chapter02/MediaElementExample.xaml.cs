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
using System.Windows.Shapes;

namespace Chapters.Chapter02
{
	/// <summary>
	/// Interaction logic for MediaElementExample.xaml
	/// </summary>
	public partial class MediaElementExample : UserControl
	{
		private bool _isPlaying = false;
		public MediaElementExample()
		{
			InitializeComponent();
		}

		private void PlayPause(object sender, ExecutedRoutedEventArgs e)
		{
			if (_isPlaying)
			{
				_mediaElt.Pause();
				_isPlaying = false;
			}
			else
			{
				_mediaElt.Play();
				_isPlaying = true;
			}
		}

		private void CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Stop(object sender, ExecutedRoutedEventArgs e)
		{
			_mediaElt.Stop();
			_playBtn.IsChecked = false;
			_isPlaying = false;
		}
	}
}
