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

namespace Chapters.Chapter12
{
	/// <summary>
	/// Interaction logic for MeshMorphAnimationExample.xaml
	/// </summary>
	public partial class FallingObjects : UserControl
	{
		Random _random = new Random();
		private double Damping = 0.95;
		private double Gravity = 0.7;
		private double Resistance = 0.995;
		private const int TotalBalls = 10;

		public FallingObjects()
		{
			InitializeComponent();

			Loaded += Window1_Loaded;
		}

		void Window1_Loaded(object sender, RoutedEventArgs e)
		{
			CreateBalls();

			CompositionTarget.Rendering += CompositionTarget_Rendering;
		}

		private void CreateBalls()
		{
			for (int i = 0; i < TotalBalls; i++)
			{
				Ellipse e = new Ellipse();
				e.Width = 20;
				e.Height = 20;
				_container.Children.Add(e);

				PhysicsInfo info = InitPhysicsForBall();
				e.Tag = info;
			}
		}

		private PhysicsInfo InitPhysicsForBall()
		{
			return new PhysicsInfo()
			       	{
			       		X = _random.NextDouble() * ActualWidth,
			       		Y = _random.NextDouble() * ActualHeight,
			       		AX = _random.NextDouble() * 10,
			       		AY = _random.NextDouble() * 10,
			       	};
		}

		void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			for (int i = 0; i < TotalBalls; i++)
			{
				Ellipse ball = _container.Children[i] as Ellipse;

				PhysicsInfo info = ball.Tag as PhysicsInfo;
				UpdateBall(info);
				SetConstraints(ball, info);

				ball.SetValue(Canvas.LeftProperty, info.X);
				ball.SetValue(Canvas.TopProperty, info.Y);
			}

		}

		private void SetConstraints(Ellipse ball, PhysicsInfo info)
		{
			Size bounds = _container.RenderSize;

			if (info.X <= 0)
			{
				info.X = 0;
				info.AX *= -1 * Damping;
			}
			else if (info.X >= bounds.Width - ball.Width)
			{
				info.X = bounds.Width - ball.Width;
				info.AX *= -1 * Damping;
			}

			if (info.Y <= 0)
			{
				info.Y = 0;
				info.AY *= -1 * Damping;
			}
			else if (info.Y >= bounds.Height - ball.Height)
			{
				info.Y = bounds.Height - ball.Height;
				info.AY *= -1 * Damping;
			}
		}

		private void UpdateBall(PhysicsInfo info)
		{
			info.AX *= Resistance;
			info.AY += Gravity;
			info.AY *= Resistance;

			info.X += info.AX;
			info.Y += info.AY;
		}

		private void Randomize(object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < TotalBalls; i++)
			{
				PhysicsInfo info = InitPhysicsForBall();
				(_container.Children[i] as Ellipse).Tag = info;
			}
		}
	}

	internal class PhysicsInfo
	{
		public double AY;
		public double AX;
		public double X;
		public double Y;
		public double OldX;
		public double OldY;
	}
}