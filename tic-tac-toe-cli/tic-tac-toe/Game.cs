using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tic_tac_toe
{
	public class Game
	{
		#region Перечисления

		private enum GameStates
		{
			Playing,
			Draw,
			X_Won,
			O_Won
		}

		private enum Players
		{
			N,
			X,
			O
		}

		#endregion

		#region Список доступных (пустых) ячеек

		List<Cell> AvailableCellsList;

		#endregion


		#region Поля

		private int[][] gameField;
		private int[] xCells, oCells;

		private GameStates state;
		private Players currentPlayer;
		private Players winner;

		#endregion

		#region Структуры

		private struct Cell
		{
			private int x;
			private int y;
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
				if (this.value == Players.N)
				{
					this.value = newValue;
					return true;
				}

				return false;
			}
		}

		#endregion

		public Game()
		{
			GenerateNewGame();
		}

		#region Методы

		/// <summary>
		/// Метод для генерации новой игры
		/// </summary>
		private void GenerateNewGame()
		{
			this.gameField = new int[3][]
									{
										new int[] {0,0,0},
										new int[] {0,0,0},
										new int[] {0,0,0}
									};

			this.state = GameStates.Playing;
			this.currentPlayer = Players.X;
			this.winner = Players.N;
			this.AvailableCellsList = new List<Cell>(9);

			this.xCells = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			this.oCells = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

			InitializeAvailableCells();

		}

		/// <summary>
		/// Метод для начала игры
		/// </summary>
		public void StartGame()
		{
			while (state == GameStates.Playing)
			{
				//Выводим текущее игровое поле
				PrintGameField();

				//Узнаем чей ход
				string current = this.currentPlayer.ToString();

				//Просим игрока ввести координаты ячейки
				Console.WriteLine($"Ход игрока {current}");
				Console.WriteLine($"Введите координаты ячейки (в формате X,Y, где 0,0 - левый верхний угол), в которую желаете поставить {current}");
				Console.WriteLine("Доступны ячейки:");
				PrintAvailableCells();

				//Считывание ввода с клавиатуры
				string? _ = Console.ReadLine();

				//Обновление доступных ячеек
				UpdateAvailableCells();

				//Поиск победителя или взаимоблокировки (ничьи)
				CheckWinner();
			}

			Console.WriteLine("Игра окончена!");

			if (state == GameStates.Draw)
			{
				Console.WriteLine("Ничья!");
			}
			else
			{
				Console.WriteLine($"Победитель: {currentPlayer}!");
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
				int[] line = gameField[i];

				char[] charField = new char[3];

				for (int j = 0; j < line.Length; j++)
				{
					int _ = line[j];

					if (_ == 0)
					{
						charField[j] = '-';
					}
					else if (_ == 1)
					{
						charField[j] = 'X';
					}
					else
					{
						charField[j] = 'O';
					}
				}

				field.AppendLine("     |     |     ");
				field.AppendLine(String.Format("  {0}  |  {1}  |  {2}  ", charField[0], charField[1], charField[2]));
				field.AppendLine("_____|_____|_____");
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
					Cell cell = new Cell(i, j, Players.N);
					AvailableCellsList.Add(cell);
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
					cells.Append(String.Format($"[{cell.X},{cell.Y}],"));
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
		private void UpdateAvailableCells()
		{
			foreach (var cell in AvailableCellsList)
			{
				if (cell.Value != Players.N)
				{
					AvailableCellsList.Remove(cell);
				}
			}
		}

		private void CheckWinner()
		{
			int countX, countY;

			if (AvailableCellsList.Count < 1)
			{
				this.currentPlayer = Players.N;

				//Проверка горизонтальных и вертикальных линий для ячейки [0,0]

			}

		}

		#endregion
	}
}
