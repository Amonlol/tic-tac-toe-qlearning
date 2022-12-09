using static cli_library.IPlayer;

namespace cli_library
{
	public interface IPlayer
	{
		public enum GameStates
		{
			Playing,
			Ended_With_Draw,
			Ended_With_X_Win,
			Ended_With_O_Win
		}
		public enum Shapes
		{
			N,
			X,
			O
		}
		public enum GameTypes
		{
			TwoPlayers,
			BotTrainingAsX,
			BotTrainingAsO,
			BotPlayingAsX,
			BotPlayingAsO
		}
		public enum Players
		{
			Human,
			Bot,
			Random
		}

		public Game.Cell MakeMove(List<Game.Cell> AvailableCellsList, Game.Cell[][] GameField);
		public Shapes GetMyShape();
		public Players GetPlayerType();

	}

	public interface IHandler
	{
		public delegate void GameStateHandler(object sender, GameStates gameState, Shapes winner);
		public event GameStateHandler GameEnded;
	}

	public class Player : IPlayer, IHandler
	{
		public Shapes MyShape;
		public Players WhoAmI;

		public event IHandler.GameStateHandler GameEnded;

		public Player() { }

		public Player(Shapes myShape, Players whoAmI)
		{
			MyShape = myShape;
			WhoAmI = whoAmI;
		}

		public virtual Game.Cell MakeMove(List<Game.Cell> AvailableCellsList, Game.Cell[][] GameField)
		{
			return new Game.Cell();
		}
		public Shapes GetMyShape()
		{
			return MyShape;
		}
		public Players GetPlayerType()
		{
			return WhoAmI;
		}

	}
}
