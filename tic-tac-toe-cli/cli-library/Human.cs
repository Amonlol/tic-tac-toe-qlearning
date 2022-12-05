using System.Text;

using static cli_library.IPlayer;

namespace cli_library
{
	public class Human : Player
	{
		public Human() : base() { }
		public Human(Shapes myShape) : base(myShape, Players.Human) { }
		public override Game.Cell MakeMove(List<Game.Cell> AvailableCellsList)
		{
			//StringBuilder sb = new StringBuilder();
			//foreach (var cell in AvailableCellsList)
			//{
			//	sb.Append($"[{0}],[{1}]",cell.X, cell.Y);
			//}
			//Console.WriteLine($"Доступные ячейки: {0}",sb.ToString());
			//Console.WriteLine($"Введите координаты ячейки (в формате <Строка>,<Столбец>, где 0,0 - левый верхний угол), в которую желаете поставить {MyShape}");
			string[] _ = Console.ReadLine().Split(',');
			Game.Cell myMoveCell = new Game.Cell(Convert.ToInt32(_[0]), Convert.ToInt32(_[1]), MyShape);

			while (true)
			{
				foreach (var cell in AvailableCellsList)
				{
					if (cell.X == myMoveCell.X && cell.Y == myMoveCell.Y)
					{
						return myMoveCell;
					}
				}
				Console.WriteLine("Введены неверные данные!");
			}
		}
	}
}
