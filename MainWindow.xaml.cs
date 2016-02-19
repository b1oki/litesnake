using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LiteSnake
{
	public partial class MainWindow : Window
	{
		enum Direction { Up, Left, Right, Down };
		Direction lastDirection;
		int snakeLength, score, speed;
		bool play, game, keyPushed;
		Timer snakeTimer;
		Random rnd;
		Rectangle food;
		Point foodCoord;
		List<Rectangle> snakeBody;
		List<Point> snakeCoords;

		/// <summary>
		/// Конструктор окна приложения
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			InitGame();
		}
		/// <summary>
		/// Инициализация игровых переменных
		/// </summary>
		private void InitGame()
		{
			play = true;
			game = true;
			keyPushed = false;
			score = -1;
			speed = 1;
			snakeLength = 3;
			InitSnake();
			InitApple();
			lbPause.Content = "PAUSE";
			UpdateLabel();
		}

		/// <summary>
		/// Обновление текстовой информации в окне
		/// </summary>
		void UpdateLabel()
		{
			lbScore.Content = score.ToString();
			lbSpeed.Content = speed.ToString();
		}
		/// <summary>
		/// Раскрашивание яблока
		/// </summary>
		/// <param name="dot"></param>
		void InitRectangleApple(Rectangle dot)
		{
			dot.Height = 20;
			dot.Width = 20;
			dot.Fill = Brushes.Red;
			dot.Stroke = Brushes.Yellow;
			dot.StrokeThickness = 2;
			canvaz.Children.Add(dot);
		}
		/// <summary>
		/// Раскрашивание клетки тела змеи
		/// </summary>
		/// <param name="dot"></param>
		void InitRectangleSnake(Rectangle dot)
		{
			dot.Height = 20;
			dot.Width = 20;
			dot.Fill = Brushes.Green;
			dot.Stroke = Brushes.GreenYellow;
			dot.StrokeThickness = 2;
			canvaz.Children.Add(dot);
		}
		/// <summary>
		/// Инициализация переменных яблока
		/// </summary>
		void InitApple()
		{
			rnd = new Random(DateTime.Now.Millisecond);
			food = new Rectangle();
			InitRectangleApple(food);
			NextApple();
		}

		private void NextRandomApple()
		{
			foodCoord.X = 20 * rnd.Next(0, 13);
			foodCoord.Y = 20 * rnd.Next(0, 13);
		}

		void NextApple()
		{
			if (play)
			{
				do { NextRandomApple(); }
				while (snakeCoords.Contains(foodCoord));
				DrawApple();
				score++;
				if (score > 0 && score % 5 == 0)
				{
					speed++;
					snakeTimer.Stop();
					snakeTimer.Interval -= speed * 3;
					snakeTimer.Start();
				}
				UpdateLabel();
			}
		}

		void InitSnakeTimer()
		{
			snakeTimer = new Timer(150.0);
			snakeTimer.Enabled = true;
			snakeTimer.Elapsed += new ElapsedEventHandler(UpdateSnake);
			snakeTimer.Start();
		}

		private void InitSnake()
		{
			snakeBody = new List<Rectangle>();
			snakeCoords = new List<Point>();
			for (int i = snakeLength; i > 0; i--)
			{
				snakeBody.Add(new Rectangle());
				snakeCoords.Add(new Point(i * 20 + 20, 40));
			}
			foreach (Rectangle dot in snakeBody)
			{ InitRectangleSnake(dot); }
			lastDirection = Direction.Right;
			InitSnakeTimer();
		}

		void DrawApple()
		{
			Canvas.SetLeft(food, foodCoord.X);
			Canvas.SetTop(food, foodCoord.Y);
		}

		void DrawSnake()
		{
			for (int i = 0; i < snakeLength; i++)
			{
				Canvas.SetLeft(snakeBody[i], snakeCoords[i].X);
				Canvas.SetTop(snakeBody[i], snakeCoords[i].Y);
			}
		}

		void DrawGameOver()
		{
			lbPause.Content = "GAME OVER";
			lbPause.Visibility = Visibility.Visible;
		}

		private void RestartGame()
		{
			snakeTimer.Stop();
			canvaz.Children.Clear();
			lbPause.Visibility = Visibility.Hidden;
			InitGame();
		}

		void UpdateSnake(object source, ElapsedEventArgs e)
		{
			if (game)
			{
				if (play)
				{
					if (CheckCollisions())
					{
						Point lastDot = snakeCoords[snakeLength - 1];
						for (int i = snakeLength - 1; i > 0; i--)
						{ snakeCoords[i] = snakeCoords[i - 1]; }
						switch (lastDirection)
						{
							case Direction.Right:
								snakeCoords[0] = new Point(snakeCoords[0].X + 20, snakeCoords[0].Y);
								break;
							case Direction.Left:
								snakeCoords[0] = new Point(snakeCoords[0].X - 20, snakeCoords[0].Y);
								break;
							case Direction.Down:
								snakeCoords[0] = new Point(snakeCoords[0].X, snakeCoords[0].Y + 20);
								break;
							case Direction.Up:
								snakeCoords[0] = new Point(snakeCoords[0].X, snakeCoords[0].Y - 20);
								break;
						}
						if (snakeCoords[0] == foodCoord)
						{
							Dispatcher.Invoke(new Action(addNewDot));
							Dispatcher.Invoke(new Action(NextApple));
							snakeLength++;
							snakeCoords.Add(lastDot);
						}
						Dispatcher.Invoke(new Action(DrawSnake));
					}
					else
					{
						play = false;
						game = false;
						Dispatcher.Invoke(new Action(DrawGameOver));
					}
					keyPushed = false;
				}
			}
		}

		bool CheckCollisions()
		{
			bool result = false;
			if (snakeCoords[0].X <= 260 &&
				snakeCoords[0].X >= 0 &&
				snakeCoords[0].Y <= 260 &&
				snakeCoords[0].Y >= 0 &&
				snakeCoords.LastIndexOf(snakeCoords[0]) == 0)
			{ result = true; }

			return result;
		}

		void addNewDot()
		{
			Rectangle dot = new Rectangle();
			InitRectangleSnake(dot);
			snakeBody.Add(dot);
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					if (lastDirection != Direction.Down && keyPushed == false)
					{
						lastDirection = Direction.Up;
						keyPushed = true;
					}
					break;
				case Key.Down:
					if (lastDirection != Direction.Up && keyPushed == false)
					{
						lastDirection = Direction.Down;
						keyPushed = true;
					}
					break;
				case Key.Left:
					if (lastDirection != Direction.Right && keyPushed == false)
					{
						lastDirection = Direction.Left;
						keyPushed = true;
					}
					break;
				case Key.Right:
					if (lastDirection != Direction.Left && keyPushed == false)
					{
						lastDirection = Direction.Right;
						keyPushed = true;
					}
					break;
				case Key.Space:
					if (game)
					{
						if (play)
						{
							lbPause.Visibility = Visibility.Visible;
							play = false;
						}
						else
						{
							lbPause.Visibility = Visibility.Hidden;
							play = true;
						}
					}
					else
					{ RestartGame(); }
					break;
				case Key.Escape:
					this.Close();
					break;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{ snakeTimer.Stop(); }
	}
}