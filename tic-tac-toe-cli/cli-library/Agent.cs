using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using static cli_library.IPlayer;

namespace cli_library
{
	public class Agent : Player
	{
		#region Константы

		const double GAMMA = 0.75; //gamma - коэффициент дисконтирования показывает значимость наград в долгосрочной перспективе
		const double ALPHA = 0.01; //alpha - скорость обучения агента
		const double EPS = 0.3; //epsilon - шанс случайного выбора - exploration

		const double WIN_REWARD = 10;
		const double DRAW_REWARD = 1;
		const double LOSE_REWARD = -20;

		const string JSON_QTABLE_FILE = @"cli-library\qtable.json";
		static string PATH_TO_JSON_QTABLE = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName,
																		 JSON_QTABLE_FILE);

		#endregion

		#region Поля

		public bool canPlay;

		#endregion

		#region Объекты

		public Dictionary<string, Dictionary<Game.Cell, double>> QTable;
		public Game.Cell[][] GameField;
		public Queue<State> StateHistory;

		#endregion

		public Agent() : base() { }
		public Agent(Shapes myShape) : base(myShape, Players.Bot)
		{
			Initialize();
			GetAgentInfo();
		}

		#region Методы

		/// <summary>
		/// Инициализация основных данных агента
		/// </summary>
		private void Initialize()
		{
			GetPolicyData();
			SavePolicyData();
			ClearHistoryMoves();
		}

		/// <summary>
		/// Очистка истории ходов для данной игры
		/// </summary>
		public void ClearHistoryMoves()
		{
			StateHistory = new Queue<State>();
		}

		/// <summary>
		/// Получение основной информации по агенту
		/// </summary>
		/// <returns>Формитированная строка с информацией</returns>
		public string GetAgentInfo()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine($"Gamma: {GAMMA}");
			sb.AppendLine($"Alpha: {ALPHA}");
			sb.AppendLine($"Epsilon: {EPS}");
			sb.AppendLine($"Win reward: {WIN_REWARD}");
			sb.AppendLine($"Draw reward: {DRAW_REWARD}");
			sb.AppendLine($"Lose reward: {LOSE_REWARD}");
			sb.AppendLine($"Policy path: {PATH_TO_JSON_QTABLE}");

			return sb.ToString();
		}

		/// <summary>
		/// Загрузка политик из файла policy.json
		/// </summary>
		/// <returns>Строка с десериализованным словарем типа string:double</returns>
		public string GetPolicyData()
		{
			string json;
			this.QTable = new Dictionary<string, Dictionary<Game.Cell, double>>();

			try
			{
				json = File.ReadAllText(PATH_TO_JSON_QTABLE);
				this.QTable = JsonSerializer.Deserialize<Dictionary<string, Dictionary<Game.Cell, double>>>(json);
				return json;
			}
			catch (Exception)
			{
				json = JsonSerializer.Serialize(this.QTable, new JsonSerializerOptions { WriteIndented = true });
				File.Delete(PATH_TO_JSON_QTABLE);
				File.WriteAllText(PATH_TO_JSON_QTABLE, json);
				return json;
			}
		}

		/// <summary>
		/// Сохранение словаря с политиками в файл qtable.json
		/// </summary>
		public void SavePolicyData()
		{
			string json = JsonSerializer.Serialize(this.QTable, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(PATH_TO_JSON_QTABLE, json);
		}

		/// <summary>
		/// Генерация нового хода
		/// </summary>
		/// <param name="AvailableCellsList">Список доступных ячеек</param>
		/// <returns>Игровую ячейку, найденную по алгоритму</returns>
		public Game.Cell MakeMove(List<Game.Cell> AvailableCellsList)
		{
			var rnd = new Random();

			//Попытка получить следующий ход
			string currentState = GenerateStringOfCurrentState();
			//exploration
			double q;
			Game.Cell NextMoveCell = GetBestNextMoveCell(currentState, out q);

			//Случайный выбор хода из возможных пустых ячеек с шансом EPS
			//или если в таблице qtable нет записи следующего хода
			if (rnd.Next(1, 100) <= EPS || (NextMoveCell.X == -1 && NextMoveCell.Y == -1 && NextMoveCell.Value == IPlayer.Shapes.N))
			{
				//exploration
				NextMoveCell = AvailableCellsList[rnd.Next(0, AvailableCellsList.Count - 1)];
				NextMoveCell.ChangeCellValue(MyShape);
			}

			//Попытка добавить действие в словарь (если такого еще не было)
			if (!QTable.ContainsKey(currentState))
			{
				QTable.TryAdd(currentState, new Dictionary<Game.Cell, double>() { { NextMoveCell, q } });
			}
			else
			{
				foreach (var state in QTable)
				{
					if (state.Key == currentState && !state.Value.ContainsKey(NextMoveCell))
					{
						QTable[currentState].Add(NextMoveCell, q);
					}
				}
			}

			//Сохранение старого состояния в кэше
			StateHistory.Enqueue(new State(q, currentState, NextMoveCell));
			return NextMoveCell;
		}

		/// <summary>
		/// Генерация строки из информации о текущем состоянии
		/// </summary>
		/// <returns>Формитированную строку</returns>
		private string GenerateStringOfCurrentState()
		{
			StringBuilder sb = new StringBuilder();

			if (MyShape == Shapes.X)
			{
				sb.Append(1);
			}
			else
			{
				sb.Append(2);
			}

			foreach (var cells in GameField)
			{
				foreach (var cell in cells)
				{
					sb.Append(cell.Value);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Получение ячейки для лучшего хода по известной таблице
		/// </summary>
		/// <param name="currentState">Строка с текущим состоянием</param>
		/// <returns>Ячейку с лучшим показателем Q, либо ячейку для рандомной генерации хода</returns>
		private Game.Cell GetBestNextMoveCell(string currentState, out double q)
		{
			Game.Cell bestMoveCell = new Game.Cell(-1, -1, IPlayer.Shapes.N);
			q = 0;

			//Есть ли данное состояние в qtable
			if (QTable.ContainsKey(currentState))
			{
				foreach (var state in QTable)
				{
					if (state.Key == currentState)
					{
						bestMoveCell = state.Value.MaxBy(kvp => kvp.Value).Key;
						q = state.Value[bestMoveCell];
						break;
					}
				}
			}

			return bestMoveCell;
		}

		/// <summary>
		/// Финальный подсчет значений в таблице Q
		/// </summary>
		/// <param name="endGameState">Результат игры</param>
		public void CalculateQValues(IPlayer.GameStates endGameState)
		{
			double reward = double.NaN;
			double nextQ = 0;

			if (endGameState == IPlayer.GameStates.Ended_With_Draw)
			{
				reward = DRAW_REWARD;
			}
			else if (endGameState == IPlayer.GameStates.Ended_With_X_Win && MyShape == IPlayer.Shapes.X ||
						endGameState == IPlayer.GameStates.Ended_With_O_Win && MyShape == IPlayer.Shapes.O)
			{
				reward = WIN_REWARD;
			}
			else
			{
				reward = LOSE_REWARD;
			}

			//Рассчет Q-значений по факту окончания игры из кэша
			while (StateHistory.Count > 1)
			{
				State state = StateHistory.Dequeue();

				foreach (var qTableStates in QTable)
				{
					if (qTableStates.Key == state.currentState)
					{
						foreach (var actions in qTableStates.Value)
						{
							if (actions.Key == state.myMoveCell)
							{
								//Получаем следующее состояние, если оно есть
								State nextState;
								if (StateHistory.TryPeek(out nextState))
								{
									nextQ = qTableStates.Value.MaxBy(kvp => kvp.Value).Value;
								}
								double q = ((1 - ALPHA) * actions.Value + ALPHA * (reward + GAMMA * (nextState.q)));
								qTableStates.Value[actions.Key] = q;
								break;
							}
						}
					}
				}
			}
			SavePolicyData();
			ClearHistoryMoves();
		}
		#endregion
	}
}
