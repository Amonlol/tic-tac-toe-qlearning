using System.Text;

namespace cli_library
{
	public partial class Game : IHandler
	{
		#region Список доступных (пустых) ячеек

		public List<Cell> AvailableCellsList;

		#endregion

		#region Поля

		private Cell[][] gameField;

		private IPlayer.GameStates state;
		private IPlayer.Shapes winner;
		private IPlayer.GameTypes gameType;

		private IPlayer currentPlayer;
		private IPlayer player1;
		private IPlayer player2;

		#endregion

		#region Свойства

		public IPlayer.GameStates State { get => state; }
		public IPlayer CurrentPlayer { get => currentPlayer; }
		public IPlayer.Shapes Winner { get => winner; }
		public IPlayer.GameTypes GameType { get => gameType; }
		//public bool BotMadeMove { get; set; } = false;
		//public Cell BotLatestMove { get; set; }
		public Cell[][] GameField { get => gameField; }

		#endregion

		#region События

		public event IHandler.GameStateHandler GameEnded;

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
			currentPlayer = player1;
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
			winner = IPlayer.Shapes.N;

			InitializeAvailableCells();
		}

		/// <summary>
		/// Метод для начала игры
		/// </summary>
		public void StartGame()
		{
			while (state == IPlayer.GameStates.Playing)
			{
				Console.Clear();

				//Выводим текущее игровое поле
				PrintGameField();

				//Просим игрока ввести координаты ячейки
				Console.WriteLine("\n");
				Console.WriteLine($"Ход игрока: {CurrentPlayer.GetMyShape()}");
				Console.WriteLine("\n");
				Console.WriteLine("Доступны ячейки:");
				PrintAvailableCells();

				//Считываем ход игрока (человек, рандом или бот)
				Cell nextMoveCell = CurrentPlayer.MakeMove(AvailableCellsList, GameField);

				//Обновление доступных ячеек
				UpdateAvailableCells(nextMoveCell);

				//Поиск победителя или взаимоблокировки (ничьи)
				FindWinner();

				if (state != IPlayer.GameStates.Playing)
				{
					Console.Clear();
					PrintGameField();
					Console.WriteLine("Игра окончена!");

					if (state == IPlayer.GameStates.Ended_With_Draw)
					{
						Console.WriteLine("Ничья!");
					}
					else
					{
						Console.WriteLine($"Победитель: {winner}!");
					}

					//Вызов события, означающего завершение текущей игры
					GameEnded?.Invoke(this, State, Winner);

					break;
				}
			}
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
			AvailableCellsList = new List<Cell>(9);
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
							Console.WriteLine("\n");
				Console.WriteLine($"Ход игрока: {CurrentPlayer.GetMyShape()}");
				Console.WriteLine("\n");
				//Console.WriteLine($"Введите координаты ячейки (в формате <Строка>,<Столбец>, где 0,0 - левый верхний угол), в которую желаете поставить {current}");
				Console.WriteLine("Доступны ячейки:");
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
		private void FindWinner()
		{
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

				winner = winnerHorizontal;
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

				winner = winnerVertical;
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

				winner = winnerDiagonal;
				return;
			}

			//Если никто не выиграл и больше нет пустых клеток
			if (AvailableCellsList.Count < 1)
			{
				winner = IPlayer.Shapes.N;
				state = IPlayer.GameStates.Ended_With_Draw;
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
				else if (countO == 3)
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
				else if (countO == 3)
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
			else if (countO == 3)
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
			else if (countO == 3)
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
			if (CurrentPlayer.GetMyShape() == player1.GetMyShape())
			{
				currentPlayer = player2;
			}
			else
			{
				currentPlayer = player1;
			}
		}

		#endregion
	}
}
