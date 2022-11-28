using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cli_library
{
	public class Game
	{
		#region Перечисления

		public enum GameStates
		{
			Playing,
			Ended_With_Draw,
			Ended_With_X_Win,
			Ended_With_O_Win
		}
		public enum Players
		{
			N,
			X,
			O
		}
		public enum GameTypes
		{
			TwoPlayers,
			BotTrainingAsX,
			BotTrainingAsO
		}

		#endregion

		#region Список доступных (пустых) ячеек

		public List<Cell> AvailableCellsList;

		#endregion

		#region Поля

		private Cell[][] gameField;

		private GameStates state;
		private Players currentPlayer;
		private Players winner;
		private GameTypes gameType;

		#endregion

		#region Свойства

		public GameStates State { get => state; }
		public Players CurrentPlayer { get => currentPlayer; }
		public Players Winner { get => winner; }
		public GameTypes GameType { get => gameType; }
		public bool BotMadeMove { get; set; } = false;
		public Cell BotLatestMove { get; set; }

		#endregion

		#region Структуры

		public struct Cell
		{
			private int x, y;
			private Players value;

			public int X { get => x; }
			public int Y { get => y; }
			public Players Value { get => value; }

			public Cell(int x, int y, Players value)
			{
				this.x = x;
				this.y = y;
				this.value = value;
			}

			public bool ChangeValue(Players newValue)
			{
				if (value == Players.N)
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

		public Game(GameTypes gameType) : this()
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
										new Cell[3] {new Cell(0,0,Players.N),new Cell(0,1,Players.N),new Cell(0,2,Players.N)},
										new Cell[3] {new Cell(1,0,Players.N),new Cell(1,1,Players.N),new Cell(1,2,Players.N)},
										new Cell[3] {new Cell(2,0,Players.N),new Cell(2,1,Players.N),new Cell(2,2,Players.N)}
									};

			state = GameStates.Playing;
			currentPlayer = Players.X;
			winner = Players.N;
			AvailableCellsList = new List<Cell>(9);

			InitializeAvailableCells();
		}

		/// <summary>
		/// Метод для начала игры
		/// </summary>
		public bool StartGame()
		{
			_ = Task.Run(async () =>
			{
				while (state == GameStates.Playing)
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
					if ((this.CurrentPlayer == Players.X && this.GameType == GameTypes.BotTrainingAsX) ||
						(this.CurrentPlayer == Players.O && this.GameType == GameTypes.BotTrainingAsO))
					{
						while (!BotMadeMove)
						{
							Thread.Sleep(1000);
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

								if (currentPlayer == Players.N || state != GameStates.Playing)
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

				if (state == GameStates.Ended_With_Draw)
				{
					Console.WriteLine("Ничья!");
				}
				else
				{
					if (state == GameStates.Ended_With_X_Win)
					{
						winner = Players.X;
					}
					else
					{
						winner = Players.O;
					}
					Console.WriteLine($"Победитель: {winner}!");
				}
			});
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

					if (cell.Value == Players.N)
					{
						charField[j] = '-';
					}
					else if (cell.Value == Players.X)
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
				if (cell.Value == Players.N)
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
			if (AvailableCellsList.Contains(new Cell(myCell.X, myCell.Y, Players.N)))
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

			return gameField[myCell.X][myCell.Y].ChangeValue(myCell.Value);
		}

		/// <summary>
		/// Нахождение победившего игрока или ничьи
		/// </summary>
		private void CheckWinner()
		{
			if (AvailableCellsList.Count < 1)
			{
				currentPlayer = Players.N;
			}

			//Проверка горизонтальных линий
			Players winnerHorizontal = CheckHorizontal();
			if (winnerHorizontal != Players.N)
			{
				if (winnerHorizontal == Players.X)
				{
					state = GameStates.Ended_With_X_Win;
				}
				else if (winnerHorizontal == Players.O)
				{
					state = GameStates.Ended_With_O_Win;
				}

				return;
			}

			//Проверка вертикальных линий
			Players winnerVertical = CheckVertical();
			if (winnerVertical != Players.N)
			{
				if (winnerVertical == Players.X)
				{
					state = GameStates.Ended_With_X_Win;
				}
				else if (winnerVertical == Players.O)
				{
					state = GameStates.Ended_With_O_Win;
				}

				return;
			}

			//Проверка диагональных линий
			Players winnerDiagonal = CheckDiagonal();
			if (winnerDiagonal != Players.N)
			{
				if (winnerDiagonal == Players.X)
				{
					state = GameStates.Ended_With_X_Win;
				}
				else if (winnerDiagonal == Players.O)
				{
					state = GameStates.Ended_With_O_Win;
				}

				return;
			}

			ChangePlayer();
		}

		/// <summary>
		/// Проверка горизонтальных линий
		/// </summary>
		/// <returns>Ничья, Х или O в формате перечисления Players</returns>
		private Players CheckHorizontal()
		{
			int countX = 0, countO = 0;

			for (int i = 0; i < gameField.Length; i++)
			{
				for (int j = 0; j < gameField[i].Length; j++)
				{
					if (gameField[i][j].Value == Players.X)
					{
						countX++;
					}
					else if (gameField[i][j].Value == Players.O)
					{
						countO++;
					}
				}

				if (countX == 3)
				{
					return Players.X;
				}
				else if (countX == 3)
				{
					return Players.O;
				}

				countX = 0;
				countO = 0;
			}

			return Players.N;
		}

		/// <summary>
		/// Проверка вертикальных линий
		/// </summary>
		/// <returns>Ничья, Х или O в формате перечисления Players</returns>
		private Players CheckVertical()
		{
			int countX = 0, countO = 0;

			//Прогонка игрового поля, как транспонированной матрицы i -> j, j -> i
			for (int i = 0; i < gameField.Length; i++)
			{
				for (int j = 0; j < gameField[i].Length; j++)
				{
					if (gameField[j][i].Value == Players.X)
					{
						countX++;
					}
					else if (gameField[j][i].Value == Players.O)
					{
						countO++;
					}
				}

				if (countX == 3)
				{
					return Players.X;
				}
				else if (countX == 3)
				{
					return Players.O;
				}

				countX = 0;
				countO = 0;
			}

			return Players.N;
		}

		/// <summary>
		/// Проверка двух диагональных линий
		/// </summary>
		/// <returns>Ничья, Х или O в формате перечисления Players</returns>
		private Players CheckDiagonal()
		{
			int countX = 0, countO = 0;

			//Проверка \ диагонали
			for (int i = 0; i < gameField.Length; i++)
			{
				if (gameField[i][i].Value == Players.X)
				{
					countX++;
				}
				else if (gameField[i][i].Value == Players.O)
				{
					countO++;
				}
			}

			if (countX == 3)
			{
				return Players.X;
			}
			else if (countX == 3)
			{
				return Players.O;
			}

			countO = 0;
			countX = 0;

			//Проверка / диагонали
			for (int i = gameField.Length - 1; i >= 0; i--)
			{
				if (gameField[i][Math.Abs(i - 2)].Value == Players.X)
				{
					countX++;
				}
				else if (gameField[i][Math.Abs(i - 2)].Value == Players.O)
				{
					countO++;
				}

			}

			if (countX == 3)
			{
				return Players.X;
			}
			else if (countX == 3)
			{
				return Players.O;
			}

			return Players.N;
		}

		/// <summary>
		/// Метод перехода хода к другому игроку
		/// </summary>
		private void ChangePlayer()
		{
			if (currentPlayer == Players.X)
			{
				currentPlayer = Players.O;
			}
			else if (currentPlayer == Players.O)
			{
				currentPlayer = Players.X;
			}
		}

		#endregion
	}
}
