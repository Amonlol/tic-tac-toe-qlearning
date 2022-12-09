using static cli_library.IPlayer;

namespace cli_library
{
	public class RandomPlayer : Player
	{
		public RandomPlayer() : base() { }
		public RandomPlayer(Shapes myShape) : base (myShape, Players.Random)
		{
			MyShape = myShape;
		}

		public override Game.Cell MakeMove(List<Game.Cell> AvailableCellsList, Game.Cell[][] GameField)
		{
			var rnd = new System.Random();
			int i = rnd.Next(0, AvailableCellsList.Count);

			Game.Cell cell = AvailableCellsList[i];
			cell.ChangeCellValue(MyShape);
			return cell;
		}
	}
}
