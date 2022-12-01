using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public delegate void PlayerHandler(Players player);
		public event PlayerHandler PlayerChanged;

		public delegate void GameStateHandler(GameStates gameState);
		public event GameStateHandler GameStateChanged;

		//public event 
	}
}
