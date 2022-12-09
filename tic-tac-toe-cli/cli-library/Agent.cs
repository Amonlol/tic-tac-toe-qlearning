using System.Text;

using Newtonsoft.Json;

using static cli_library.IPlayer;

namespace cli_library
{
	public class Agent : Player
	{
		#region Константы

		const double GAMMA = 0.75; //gamma - коэффициент дисконтирования показывает значимость наград в долгосрочной перспективе
		const double ALPHA = 0.01; //alpha - скорость обучения агента
		const double EPS = 0.6;    //epsilon - шанс случайного выбора - exploration

		const double WIN_REWARD = 10;
		const double DRAW_REWARD = -1;
		const double LOSE_REWARD = -20;

		const string JSON_QTABLE_FILE = @"cli-library\qtable.json";
		static string PATH_TO_JSON_QTABLE = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName,
																		 JSON_QTABLE_FILE);

		#endregion

		#region Объекты

		public Game.MyJsonDictionary QTable;
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
		/// Метод делегата события IPlayer.GameEnded
		/// </summary>
		/// <param name="gameState">Статус окончания игры</param>
		/// <param name="winner">Победитель</param>
		public void GameIsEnded(object sender, GameStates gameState, Shapes winner)
		{
			CalculateQValues(winner);
			//SavePolicyData();
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
			this.QTable = new Game.MyJsonDictionary();

			try
			{
				json = File.ReadAllText(PATH_TO_JSON_QTABLE);
				this.QTable = JsonConvert.DeserializeObject<Game.MyJsonDictionary>(json);
				return json;
			}
			catch (Exception)
			{
				//json = JsonConvert.SerializeObject(this.QTable);
				File.Delete(PATH_TO_JSON_QTABLE);
				json = SavePolicyData();
				//File.WriteAllText(PATH_TO_JSON_QTABLE, json);
				return json;
			}
		}

		/// <summary>
		/// Сохранение словаря с политиками в файл qtable.json
		/// </summary>
		public string SavePolicyData()
		{
			string json = JsonConvert.SerializeObject(this.QTable, Formatting.Indented);
			File.WriteAllText(PATH_TO_JSON_QTABLE, json);
			return json;
		}

		/// <summary>
		/// Генерация нового хода
		/// </summary>
		/// <param name="AvailableCellsList">Список доступных ячеек</param>
		/// <returns>Игровую ячейку, найденную по алгоритму</returns>
		public override Game.Cell MakeMove(List<Game.Cell> AvailableCellsList, Game.Cell[][] GameField)
		{
			var rnd = new System.Random();

			//Попытка получить следующий ход
			string currentState = GenerateStringOfCurrentState(GameField);
			//exploitation
			double q;
			Game.Cell NextMoveCell = GetBestNextMoveCell(currentState, out q);

			//Случайный выбор хода из возможных пустых ячеек с шансом EPS
			//или если в таблице qtable нет записи следующего хода
			if (rnd.Next(1, 100) <= (EPS * 100) || (NextMoveCell.X == -1 && NextMoveCell.Y == -1 && NextMoveCell.Value == Shapes.N))
			{
				//exploration
				NextMoveCell = AvailableCellsList[rnd.Next(0, AvailableCellsList.Count - 1)];
				NextMoveCell.ChangeCellValue(MyShape);
			}

			//Попытка добавить действие в словарь (если такого еще не было)
			QTable.Add(currentState, NextMoveCell, q);
			//if (!QTable.Cells.ContainsKey(currentState))
			//{
			//	QTable.Cells.TryAdd(currentState, new Game.MyJsonDictionary.CellPairs().Add(NextMoveCell, q));
			//}
			//else
			//{
			//	foreach (var state in QTable.Cells)
			//	{
			//		if (state.Key == currentState && !state.Value.cellPairs.ContainsKey(NextMoveCell))
			//		{
			//			QTable.Cells[currentState].Add(NextMoveCell, q);
			//		}
			//	}
			//}

			//Сохранение старого состояния в кэше
			State s = new State(q, currentState, NextMoveCell);
			StateHistory.Enqueue(s);
			return NextMoveCell;
		}

		/// <summary>
		/// Генерация строки из информации о текущем состоянии
		/// </summary>
		/// <returns>Формитированную строку</returns>
		private string GenerateStringOfCurrentState(Game.Cell[][] GameField)
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
					sb.Append((int)Enum.Parse(typeof(Shapes), cell.Value.ToString()));
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
			if (QTable.Cells.ContainsKey(currentState))
			{
				Game.MyJsonDictionary.CellPairs cellPairs = QTable.Cells[currentState].MaxBy(kvp => kvp.value);
				bestMoveCell = cellPairs.cell;
				q = cellPairs.value;
			}

			return bestMoveCell;
		}

		/// <summary>
		/// Финальный подсчет значений в таблице Q
		/// </summary>
		/// <param name="endGameState">Результат игры</param>
		public void CalculateQValues(Shapes winner)
		{
			double reward = double.NaN;
			double nextMaxQ = 0;

			if (winner == Shapes.N)
			{
				reward = DRAW_REWARD;
			}
			else if (winner == this.GetMyShape())
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

				if (QTable.Cells.ContainsKey(state.currentState))
				{
					int i = QTable.Cells[state.currentState].FindIndex(e => e.cell == state.myMoveCell);
					var cellPair = QTable.Cells[state.currentState][i];

					//Получаем следующее состояние, если оно есть
					State nextState;

					if (StateHistory.TryPeek(out nextState))
					{
						nextMaxQ = QTable.Cells[state.currentState].MaxBy(kvp => kvp.value).value;
					}
					else
					{
						nextMaxQ = 0;
					}

					double q = (((1 - ALPHA) * cellPair.value) + (ALPHA * (reward + GAMMA * nextMaxQ)));
					QTable.ChangeValueOfElement(state.currentState, state.myMoveCell, q);
					continue;

				}
			}
			SavePolicyData();
			ClearHistoryMoves();
		}
		#endregion
	}
}
