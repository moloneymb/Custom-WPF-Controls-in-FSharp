using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public class TestCommand : ICommand
	{
		Random _random = new Random();
		public void Execute(object parameter)
		{
			Debug.WriteLine("TestCommand Executed");
		}

		public bool CanExecute(object parameter)
		{
			return (_random.NextDouble() < 0.5);
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}
	}

	/// <summary>
	/// Interaction logic for SimpleCommandingExample.xaml
	/// </summary>
	public partial class SimpleCommandingExample : UserControl
	{
		public SimpleCommandingExample()
		{
			InitializeComponent();

			//var command = new TestCommand();
			//this.CommandBindings.Add(new CommandBinding(command));
			//CommandRaiserButton.Command = command;

			//var inputBinding = new InputBinding(command, new KeyGesture(Key.T,
			//                            ModifierKeys.Control | ModifierKeys.Shift));
			//InputBindings.Add(inputBinding);
		}
	}
}
