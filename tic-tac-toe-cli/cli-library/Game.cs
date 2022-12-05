using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cli_library
{
	public class Game
	{
		#region Список доступных (пустых) ячеек

		public List<Cell> AvailableCellsList;

		#endregion

		#region Поля

		private Cell[][] gameField;

		private IPlayer.GameStates state;
		private IPlayer.Shapes currentPlayer;
		private IPlayer.Shapes winner;
		private IPlayer.GameTypes gameType;

		private IPlayer player1;
		private IPlayer player2;

		#endregion

		#region Свойства

		public IPlayer.GameStates State { get => state; }
		public IPlayer.Shapes CurrentPlayer { get => currentPlayer; }
		public IPlayer.Shapes Winner { get => winner; }
		public IPlayer.GameTypes GameType { get => gameType; }
		//public bool BotMadeMove { get; set; } = false;
		//public Cell BotLatestMove { get; set; }
		public Cell[][] GameField { get => gameField; }

		#endregion

		#region Структуры

		public struct Cell
		{
			private int x, y;
			private IPlayer.Shapes value;

			public int X { get => x; }
			public int Y { get => y; }
			public IPlayer.Shapes Value { get => value; }

			public Cell(int x, int y, IPlayer.Shapes value)
			{
				this.x = x;
				this.y = y;
				this.value = value;
			}

			public bool ChangeCellValue(IPlayer.Shapes newValue)
			{
				if (value == IPlayer.Shapes.N)
				{
					value = newValue;
					return true;
				}

				return false;
			}

			public static bool operator ==(Cell first, Cell second)
			{
				if (first.X == second.X && first.Y == second.Y && first.Value == second.Value)
				{
					return true;
				}

				return false;
			}

			public static bool operator !=(Cell first, Cell second)
			{
				if (first.X == second.X && first.Y == second.Y && first.Value == second.Value)
				{
					return false;
				}

				return true;
			}

		}

		#endregion

		public Game()
		{
			GenerateNewGame();
		}

		public Game(IPlayer.GameTypes gameType) : this()
		{
			this.gameType = gameType;
		}

		public Game(IPlayer.GameTypes gameType, IPlayer player1, IPlayer player2) : this(gameType)
		{
			this.player1 = player1;
			this.player2 = player2;

			this.player1.PlayerChanged += PlayerChanged;
			this.player2.PlayerChanged += PlayerChanged;
		}

		private void PlayerChanged(IPlayer.Shapes shape, IPlayer.Players player)
		{
			throw new NotImplementedException();
		}

		#region Методы

		/// <summary>
		/// Метод для генерации новой игры
		/// </summary>
		private void GenerateNewGame()
		{

			gameField = new Cell[3][]
									{
										new Cell[3] {new Cell(0,0,IPlayer.Shapes.N),new Cell(0,1,IPlayer.Shapes.N),new Cell(0,2,IPlayer.Shapes.N)},
										new Cell[3] {new Cell(1,0,IPlayer.Shapes.N),new Cell(1,1,IPlayer.Shapes.N),new Cell(1,2,IPlayer.Shapes.N)},
										new Cell[3] {new Cell(2,0,IPlayer.Shapes.N),new Cell(2,1,IPlayer.Shapes.N),new Cell(2,2,IPlayer.Shapes.N)}
									};

			state = IPlayer.GameStates.Playing;
			currentPlayer = IPlayer.Shapes.X;
			winner = IPlayer.Shapes.N;
			AvailableCellsList = new List<Cell>(9);

			InitializeAvailableCells();
		}

		/// <summary>
		/// Метод для начала игры
		/// </summary>
		public bool StartGame()
		{
			while (state == IPlayer.GameStates.Playing)
			{
				Console.Clear();
				//Выводим текущее игровое поле
				PrintGameField();

				//Узнаем чей ход
				string current = currentPlayer.ToString();

				//Просим игрока ввести координаты ячейки
				Console.WriteLine("\n");
				Console.WriteLine($"Ход игрока: {current}");
				Console.WriteLine("\n");
				//Console.WriteLine($"Введите координаты ячейки (в формате <Строка>,<Столбец>, где 0,0 - левый верхний угол), в которую желаете поставить {current}");
				Console.WriteLine("Доступны ячейки:");
				PrintAvailableCells();

				//string[] coordinates = new string[2];

				//Проверяем является ли текущий ход ходом бота
				if ((this.CurrentPlayer == IPlayer.Shapes.X && this.GameType == IPlayer.GameTypes.BotTrainingAsX) ||
					(this.CurrentPlayer == IPlayer.Shapes.O && this.GameType == IPlayer.GameTypes.BotTrainingAsO))
				{
					//while (!BotMadeMove)
					//{
					//	Thread.Sleep(100);
					//}

					//if (UpdateAvailableCells(BotLatestMove))
					//{
					//	CheckWinner();
					//}

					//BotMadeMove = false;
				}
				else
				{
					//Считывание ввода с клавиатуры
					string[] coordinates = Console.ReadLine().Split(',');

					try
					{
						Cell myCell = new Cell(Convert.ToInt32(coordinates[0]), Convert.ToInt32(coordinates[1]), currentPlayer);

						//Обновление доступных ячеек
						if (UpdateAvailableCells(myCell))
						{
							//Поиск победителя или взаимоблокировки (ничьи)
							CheckWinner();

							if (currentPlayer == IPlayer.Shapes.N || state != IPlayer.GameStates.Playing)
							{
								break;
							}
						}
						else
						{
							Console.WriteLine("Данная ячейка уже заполнена, выберите другую!");
						}

					}
					catch (Exception)
					{
						Console.WriteLine("!!!НЕВЕРНО ВВЕДЕНЫ КООРДИНАТЫ!!!");
					}
				}

			}

			Console.Clear();
			Console.WriteLine("Игра окончена!");
			PrintGameField();

			if (state == IPlayer.GameStates.Ended_With_Draw)
			{
				Console.WriteLine("Ничья!");
			}
			else
			{
				if (state == IPlayer.GameStates.Ended_With_X_Win)
				{
					winner = IPlayer.Shapes.X;
				}
				else
				{
					winner = IPlayer.Shapes.O;
				}
				Console.WriteLine($"Победитель: {winner}!");
			}

			return true;
		}

		/// <summary>
		/// Метод для вывода игрового поля в консоль
		/// </summary>
		/// <returns>Возвращает форматированную строку с игровым полем с заменой чисел на символы</returns>
		public string PrintGameField()
		{
			StringBuilder field = new StringBuilder();

			for (int i = 0; i < gameField.Length; i++)
			{
				Cell[] line = gameField[i];

				char[] charField = new char[3];

				if (i == 0)
				{
					field.AppendLine("    0     1     2  ");
					field.AppendLine("  _________________ ");
				}

				for (int j = 0; j < line.Length; j++)
				{
					Cell cell = line[j];

					if (cell.Value == IPlayer.Shapes.N)
					{
						charField[j] = '-';
					}
					else if (cell.Value == IPlayer.Shapes.X)
					{
						charField[j] = 'X';
					}
					else
					{
						charField[j] = 'O';
					}
				}

				field.AppendLine(" |     |     |     |");
				field.AppendLine(string.Format("{3}|  {0}  |  {1}  |  {2}  |", charField[0], charField[1], charField[2], i));
				field.AppendLine(" |_____|_____|_____|");
			}

			Console.WriteLine(field);
			return field.ToString();
		}

		/// <summary>
		/// Заполнение пустого списка доступных ячеек на старте игры
		/// </summary>
		private void InitializeAvailableCells()
		{
			for (int i = 0; i < gameField.Length; i++)
			{
				for (int j = 0; j < gameField[i].Length; j++)
				{
					AvailableCellsList.Add(gameField[i][j]);
				}
			}
		}

		/// <summary>
		/// Получение форматированной строки с доступными ячейками
		/// </summary>
		/// <returns>Форматированная строка с доступными ячейками для хода</returns>
		private string GetAvailableCells()
		{
			StringBuilder cells = new StringBuilder();

			foreach (var cell in AvailableCellsList)
			{
				if (cell.Value == IPlayer.Shapes.N)
				{
					cells.Append(string.Format($"[{cell.X},{cell.Y}],"));
				}
			}

			return cells.ToString().Remove(cells.Length - 1);
		}

		/// <summary>
		/// Вывод доступных ячеек в консоль
		/// </summary>
		private void PrintAvailableCells()
		{
			Console.WriteLine(GetAvailableCells());
		}

		/// <summary>
		/// Обновление списка доступных ячеек
		/// </summary>
		private bool UpdateAvailableCells(Cell myCell)
		{
			if (AvailableCellsList.Contains(new Cell(myCell.X, myCell.Y, IPlayer.Shapes.N)))
			{
				foreach (Cell cell in AvailableCellsList)
				{
					if (cell.X == myCell.X && cell.Y == myCell.Y)
					{
						AvailableCellsList.Remove(cell);
						break;
					}
				}
			}

			return gameField[myCell.X][myCell.Y].ChangeCellValue(myCell.Value);
		}

		/// <summary>
		/// Нахождение победившего игрока или ничьи
		/// </summary>
		private void CheckWinner()
		{
			if (AvailableCellsList.Count < 1)
			{
				currentPlayer = IPlayer.Shapes.N;
			}

			//Проверка горизонтальных линий
			IPlayer.Shapes winnerHorizontal = CheckHorizontal();
			if (winnerHorizontal != IPlayer.Shapes.N)
			{
				if (winnerHorizontal == IPlayer.Shapes.X)
				{
					state = IPlayer.GameStates.Ended_With_X_Win;
				}
				else if (winnerHorizontal == IPlayer.Shapes.O)
				{
					state = IPlayer.GameStates.Ended_With_O_Win;
				}

				return;
			}

			//Проверка вертикальных линий
			IPlayer.Shapes winnerVertical = CheckVertical();
			if (winnerVertical != IPlayer.Shapes.N)
			{
				if (winnerVertical == IPlayer.Shapes.X)
				{
					state = IPlayer.GameStates.Ended_With_X_Win;
				}
				else if (winnerVertical == IPlayer.Shapes.O)
				{
					state = IPlayer.GameStates.Ended_With_O_Win;
				}

				return;
			}

			//Проверка диагональных линий
			IPlayer.Shapes winnerDiagonal = CheckDiagonal();
			if (winnerDiagonal != IPlayer.Shapes.N)
			{
				if (winnerDiagonal == IPlayer.Shapes.X)
				{
					state = IPlayer.GameStates.Ended_With_X_Win;
				}
				else if (winnerDiagonal == IPlayer.Shapes.O)
				{
					state = IPlayer.GameStates.Ended_With_O_Win;
				}

				return;
			}

			ChangePlayer();
		}

		/// <summary>
		/// Проверка горизонтальных линий
		/// </summary>
		/// <returns>Ничья, Х или O в формате перечисления IPlayer.Players</returns>
		private IPlayer.Shapes CheckHorizontal()
		{
			int countX = 0, countO = 0;

			for (int i = 0; i < gameField.Length; i++)
			{
				for (int j = 0; j < gameField[i].Length; j++)
				{
					if (gameField[i][j].Value == IPlayer.Shapes.X)
					{
						countX++;
					}
					else if (gameField[i][j].Value == IPlayer.Shapes.O)
					{
						countO++;
					}
				}

				if (countX == 3)
				{
					return IPlayer.Shapes.X;
				}
				else if (countX == 3)
				{
					return IPlayer.Shapes.O;
				}

				countX = 0;
				countO = 0;
			}

			return IPlayer.Shapes.N;
		}

		/// <summary>
		/// Проверка вертикальных линий
		/// </summary>
		/// <returns>Ничья, Х или O в формате перечисления IPlayer.Players</returns>
		private IPlayer.Shapes CheckVertical()
		{
			int countX = 0, countO = 0;

			//Прогонка игрового поля, как транспонированной матрицы i -> j, j -> i
			for (int i = 0; i < gameField.Length; i++)
			{
				for (int j = 0; j < gameField[i].Length; j++)
				{
					if (gameField[j][i].Value == IPlayer.Shapes.X)
					{
						countX++;
					}
					else if (gameField[j][i].Value == IPlayer.Shapes.O)
					{
						countO++;
					}
				}

				if (countX == 3)
				{
					return IPlayer.Shapes.X;
				}
				else if (countX == 3)
				{
					return IPlayer.Shapes.O;
				}

				countX = 0;
				countO = 0;
			}

			return IPlayer.Shapes.N;
		}

		/// <summary>
		/// Проверка двух диагональных линий
		/// </summary>
		/// <returns>Ничья, Х или O в формате перечисления IPlayer.Players</returns>
		private IPlayer.Shapes CheckDiagonal()
		{
			int countX = 0, countO = 0;

			//Проверка \ диагонали
			for (int i = 0; i < gameField.Length; i++)
			{
				if (gameField[i][i].Value == IPlayer.Shapes.X)
				{
					countX++;
				}
				else if (gameField[i][i].Value == IPlayer.Shapes.O)
				{
					countO++;
				}
			}

			if (countX == 3)
			{
				return IPlayer.Shapes.X;
			}
			else if (countX == 3)
			{
				return IPlayer.Shapes.O;
			}

			countO = 0;
			countX = 0;

			//Проверка / диагонали
			for (int i = gameField.Length - 1; i >= 0; i--)
			{
				if (gameField[i][Math.Abs(i - 2)].Value == IPlayer.Shapes.X)
				{
					countX++;
				}
				else if (gameField[i][Math.Abs(i - 2)].Value == IPlayer.Shapes.O)
				{
					countO++;
				}

			}

			if (countX == 3)
			{
				return IPlayer.Shapes.X;
			}
			else if (countX == 3)
			{
				return IPlayer.Shapes.O;
			}

			return IPlayer.Shapes.N;
		}

		/// <summary>
		/// Метод перехода хода к другому игроку
		/// </summary>
		private void ChangePlayer()
		{
			if (currentPlayer == IPlayer.Shapes.X)
			{
				currentPlayer = IPlayer.Shapes.O;
			}
			else if (currentPlayer == IPlayer.Shapes.O)
			{
				currentPlayer = IPlayer.Shapes.X;
			}
		}

		#endregion
	}
}
