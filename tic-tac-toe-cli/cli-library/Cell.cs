using Newtonsoft.Json;

namespace cli_library
{
	public partial class Game
	{
		/// <summary>
		/// Структура, которая используется для хранения клеток
		/// </summary>
		[Serializable]
		public struct Cell
		{
			[JsonProperty(Required = Required.Always)]
			public int X { get; set; }
			[JsonProperty(Required = Required.Always)]
			public int Y { get; set; }
			[JsonProperty(Required = Required.Always)]
			public IPlayer.Shapes Value { get; set; }

			[JsonConstructor]
			public Cell(int x, int y, IPlayer.Shapes value)
			{
				X = x;
				Y = y;
				Value = value;
			}

			public bool ChangeCellValue(IPlayer.Shapes newValue)
			{
				if (Value == IPlayer.Shapes.N)
				{
					Value = newValue;
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

		/// <summary>
		/// Объект, который используется для хранения данных из таблицы Q 
		/// и последующей де-\сериализации данных
		/// </summary>
		public class MyJsonDictionary
		{
			public struct CellPairs
			{
				public Cell cell { get; set; }
				public double value { get; set; }

				[JsonConstructor]
				public CellPairs(Cell cell, double value)
				{
					this.cell = cell;
					this.value = value;
				}
			}

			[JsonProperty(Required = Required.Always)]
			public Dictionary<string, List<CellPairs>> Cells { get; set; }

			public MyJsonDictionary()
			{
				Cells = new Dictionary<string, List<CellPairs>>();
			}

			[JsonConstructor]
			public MyJsonDictionary(Dictionary<string, List<CellPairs>> cells)
			{
				Cells = cells;
			}

			/// <summary>
			/// Метод добавления данных в словарь (таблицу) Q
			/// </summary>
			/// <param name="key">Статус игрового поля, состоящее из 9 цифр</param>
			/// <param name="cell">Клетка, которую нужно добавить</param>
			/// <param name="value">Значение параметра Q</param>
			public void Add(string key, Cell cell, double value)
			{
				if (Cells.ContainsKey(key))
				{
					int i = Cells[key].FindIndex(e => e.cell == cell);

					if (i == -1)
					{
						Cells[key].Add(new CellPairs(cell, value));
					}
				}
				else
				{
					Cells.Add(key, new List<CellPairs>() { new CellPairs(cell, value) });
				}
			}

			/// <summary>
			/// Метод для изменения значения Q клетки в словаре (таблице) Q
			/// </summary>
			/// <param name="key">Статус игрового поля, состоящее из 9 цифр</param>
			/// <param name="cell">Клетка, значение которой нужно изменить</param>
			/// <param name="value">Новое значение параметра Q</param>
			public void ChangeValueOfElement(string key, Cell cell, double value)
			{
				if (Cells.ContainsKey(key))
				{
					int i = Cells[key].FindIndex(e => e.cell == cell);
					CellPairs _ = Cells[key][i];
					_.value = value;
					Cells[key][i] = _;
				}
			}
		}
	}
}
