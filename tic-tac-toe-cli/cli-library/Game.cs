using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cli_library
{
	public class Game : IPlayer
	{
		#region Список доступных (пустых) ячеек

		public List<Cell> AvailableCellsList;

		#endregion

		#region Поля

		private Cell[][] gameField;

		private IPlayer.GameStates state;
		private IPlayer.Players currentPlayer;
		private IPlayer.Players winner;
		private IPlayer.GameTypes gameType;

		#endregion

		#region Свойства

		public IPlayer.GameStates State { get => state; }
		public IPlayer.Players CurrentPlayer { get => currentPlayer; }
		public IPlayer.Players Winner { get => winner; }
		public IPlayer.GameTypes GameType { get => gameType; }
		public bool BotMadeMove { get; set; } = false;
		public Cell BotLatestMove { get; set; }
		public Cell[][] GameField { get => gameField; }

		#endregion

		#region Структуры

		public struct Cell
		{
			private int x, y;
			private IPlayer.Players value;

			public int X { get => x; }
			public int Y { get => y; }
			public IPlayer.Players Value { get => value; }

			public Cell(int x, int y, IPlayer.Players value)
			{
				this.x = x;
				this.y = y;
				this.value = value;
			}

			public bool ChangeCellValue(IPlayer.Players newValue)
			{
				if (value == IPlayer.Players.N)
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

		#region События

		public event IPlayer.PlayerHandler PlayerChanged;
		public event IPlayer.GameStateHandler GameStateChanged;

		#endregion

		public Game()
		{
			GenerateNewGame();
		}

		public Game(IPlayer.GameTypes gameType) : this()
		{
			this.gameType = gameType;
		}

		#region Методы

		/// <summary>
		/// Метод для генерации новой игры
		/// </summary>
		private void GenerateNewGame()
		{

			gameField = new Cell[3][]
									{
										new Cell[3] {new Cell(0,0,IPlayer.Players.N),new Cell(0,1,IPlayer.Players.N),new Cell(0,2,IPlayer.Players.N)},
										new Cell[3] {new Cell(1,0,IPlayer.Players.N),new Cell(1,1,IPlayer.Players.N),new Cell(1,2,IPlayer.Players.N)},
										new Cell[3] {new Cell(2,0,IPlayer.Players.N),new Cell(2,1,IPlayer.Players.N),new Cell(2,2,IPlayer.Players.N)}
									};

			state = IPlayer.GameStates.Playing;
			currentPlayer = IPlayer.Players.X;
			winner = IPlayer.Players.N;
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
				Console.WriteLine($"Введите координаты ячейки (в формате <Строка>,<Столбец>, где 0,0 - левый верхний угол), в которую желаете поставить {current}");
				Console.WriteLine("Доступны ячейки:");
				PrintAvailableCells();

				//string[] coordinates = new string[2];

				//Проверяем является ли текущий ход ходом бота
				if ((this.CurrentPlayer == IPlayer.Players.X && this.GameType == IPlayer.GameTypes.BotTrainingAsX) ||
					(this.CurrentPlayer == IPlayer.Players.O && this.GameType == IPlayer.GameTypes.BotTrainingAsO))
				{
					while (!BotMadeMove)
					{
						Thread.Sleep(100);
					}

					if (UpdateAvailableCells(BotLatestMove))
					{
						CheckWinner();
					}

					BotMadeMove = false;
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

							if (currentPlayer == IPlayer.Players.N || state != IPlayer.GameStates.Playing)
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
					winner = IPlayer.Players.X;
				}
				else
				{
					winner = IPlayer.Players.O;
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

					if (cell.Value == IPlayer.Players.N)
					{
						charField[j] = '-';
					}
					else if (cell.Value == IPlayer.Players.X)
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
				if (cell.Value == IPlayer.Players.N)
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
			if (AvailableCellsList.Contains(new Cell(myCell.X, myCell.Y, IPlayer.Players.N)))
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
				currentPlayer = IPlayer.Players.N;
			}

			//Проверка горизонтальных линий
			IPlayer.Players winnerHorizontal = CheckHorizontal();
			if (winnerHorizontal != IPlayer.Players.N)
			{
				if (winnerHorizontal == IPlayer.Players.X)
				{
					state = IPlayer.GameStates.Ended_With_X_Win;
				}
				else if (winnerHorizontal == IPlayer.Players.O)
				{
					state = IPlayer.GameStates.Ended_With_O_Win;
				}

				return;
			}

			//Проверка вертикальных линий
			IPlayer.Players winnerVertical = CheckVertical();
			if (winnerVertical != IPlayer.Players.N)
			{
				if (winnerVertical == IPlayer.Players.X)
				{
					state = IPlayer.GameStates.Ended_With_X_Win;
				}
				else if (winnerVertical == IPlayer.Players.O)
				{
					state = IPlayer.GameStates.Ended_With_O_Win;
				}

				return;
			}

			//Проверка диагональных линий
			IPlayer.Players winnerDiagonal = CheckDiagonal();
			if (winnerDiagonal != IPlayer.Players.N)
			{
				if (winnerDiagonal == IPlayer.Players.X)
				{
					state = IPlayer.GameStates.Ended_With_X_Win;
				}
				else if (winnerDiagonal == IPlayer.Players.O)
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
		private IPlayer.Players CheckHorizontal()
		{
			int countX = 0, countO = 0;

			for (int i = 0; i < gameField.Length; i++)
			{
				for (int j = 0; j < gameField[i].Length; j++)
				{
					if (gameField[i][j].Value == IPlayer.Players.X)
					{
						countX++;
					}
					else if (gameField[i][j].Value == IPlayer.Players.O)
					{
						countO++;
					}
				}

				if (countX == 3)
				{
					return IPlayer.Players.X;
				}
				else if (countX == 3)
				{
					return IPlayer.Players.O;
				}

				countX = 0;
				countO = 0;
			}

			return IPlayer.Players.N;
		}

		/// <summary>
		/// Проверка вертикальных линий
		/// </summary>
		/// <returns>Ничья, Х или O в формате перечисления IPlayer.Players</returns>
		private IPlayer.Players CheckVertical()
		{
			int countX = 0, countO = 0;

			//Прогонка игрового поля, как транспонированной матрицы i -> j, j -> i
			for (int i = 0; i < gameField.Length; i++)
			{
				for (int j = 0; j < gameField[i].Length; j++)
				{
					if (gameField[j][i].Value == IPlayer.Players.X)
					{
						countX++;
					}
					else if (gameField[j][i].Value == IPlayer.Players.O)
					{
						countO++;
					}
				}

				if (countX == 3)
				{
					return IPlayer.Players.X;
				}
				else if (countX == 3)
				{
					return IPlayer.Players.O;
				}

				countX = 0;
				countO = 0;
			}

			return IPlayer.Players.N;
		}

		/// <summary>
		/// Проверка двух диагональных линий
		/// </summary>
		/// <returns>Ничья, Х или O в формате перечисления IPlayer.Players</returns>
		private IPlayer.Players CheckDiagonal()
		{
			int countX = 0, countO = 0;

			//Проверка \ диагонали
			for (int i = 0; i < gameField.Length; i++)
			{
				if (gameField[i][i].Value == IPlayer.Players.X)
				{
					countX++;
				}
				else if (gameField[i][i].Value == IPlayer.Players.O)
				{
					countO++;
				}
			}

			if (countX == 3)
			{
				return IPlayer.Players.X;
			}
			else if (countX == 3)
			{
				return IPlayer.Players.O;
			}

			countO = 0;
			countX = 0;

			//Проверка / диагонали
			for (int i = gameField.Length - 1; i >= 0; i--)
			{
				if (gameField[i][Math.Abs(i - 2)].Value == IPlayer.Players.X)
				{
					countX++;
				}
				else if (gameField[i][Math.Abs(i - 2)].Value == IPlayer.Players.O)
				{
					countO++;
				}

			}

			if (countX == 3)
			{
				return IPlayer.Players.X;
			}
			else if (countX == 3)
			{
				return IPlayer.Players.O;
			}

			return IPlayer.Players.N;
		}

		/// <summary>
		/// Метод перехода хода к другому игроку
		/// </summary>
		private void ChangePlayer()
		{
			if (currentPlayer == IPlayer.Players.X)
			{
				currentPlayer = IPlayer.Players.O;
			}
			else if (currentPlayer == IPlayer.Players.O)
			{
				currentPlayer = IPlayer.Players.X;
			}
		}

		#endregion
	}
}
