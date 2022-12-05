using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public Game.Cell MakeMove(List<Game.Cell> AvailableCellsList);

		public delegate void PlayerHandler(Shapes shape, Players player);
		public event PlayerHandler PlayerChanged;

		public delegate void GameStateHandler(GameStates gameState);
		public event GameStateHandler GameStateChanged;
	}

	public class Player : IPlayer
	{
		public Shapes MyShape;
		public Players WhoAmI;

		public event PlayerHandler PlayerChanged;
		public event GameStateHandler GameStateChanged;

		public Player() { }

		public Player(Shapes myShape, Players whoAmI)
		{
			MyShape = myShape;
			WhoAmI = whoAmI;
		}

		public virtual Game.Cell MakeMove(List<Game.Cell> AvailableCellsList)
		{
			return new Game.Cell();
		}
	}
}
