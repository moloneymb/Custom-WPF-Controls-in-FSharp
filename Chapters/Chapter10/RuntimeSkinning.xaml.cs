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

namespace Chapters.Chapter10
{
	/// <summary>
	/// Interaction logic for RuntimeSkinning.xaml
	/// </summary>
	public partial class RuntimeSkinning : UserControl
	{
		public RuntimeSkinning()
		{
			InitializeComponent();
			Loaded += RuntimeSkinning_Loaded;
		}

		void RuntimeSkinning_Loaded(object sender, RoutedEventArgs e)
		{
			_skinChanger.AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(OnSkinChanged));
		}

		private void OnSkinChanged(object sender, RoutedEventArgs e)
		{
			string name = (e.OriginalSource as RadioButton).Name;
			_contactForm.Resources.Clear();
			_contactForm.Resources.MergedDictionaries.Clear();

			if (name == "None") return;

			ResourceDictionary skin =
				Application.LoadComponent(new Uri("/Chapters;component/Chapter10/Skins/" + name + ".xaml", UriKind.Relative)) as ResourceDictionary;
			_contactForm.Resources.MergedDictionaries.Add(skin);

			e.Handled = true;
		}
	}
}
