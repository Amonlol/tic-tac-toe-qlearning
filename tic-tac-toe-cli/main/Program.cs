using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

using cli_library;

namespace main
{
	internal class Program
	{
		public static Game game;
		//public static IPlayer player1;
		//public static IPlayer player2;

		static void Main(string[] args)
		{
			//IPlayer.GameTypes gameType = GetGameType();

			StartGame(GetGameType());

			//if (gameType == IPlayer.GameTypes.BotTrainingAsX)
			//{
			//	InitializeAgentTraining(IPlayer.Shapes.X);
			//	StartAgentTrainingAsX();
			//}
		}

		/// <summary>
		/// Метод генерации новой игры
		/// </summary>
		static void StartGame(IPlayer.GameTypes gameType)
		{
			//game = new Game(IPlayer.GameTypes.BotTrainingAsX);
			IPlayer player1 = new Player();
			IPlayer player2 = new Player();

			if (gameType == IPlayer.GameTypes.TwoPlayers)
			{
				player1 = new Human(IPlayer.Shapes.X);
				player2 = new Human(IPlayer.Shapes.O);
			}
			else if (gameType == IPlayer.GameTypes.BotTrainingAsX)
			{
				player1 = new Agent(IPlayer.Shapes.X);
				player2 = new Human(IPlayer.Shapes.O);
			}
			else if (gameType == IPlayer.GameTypes.BotPlayingAsX)
			{

			}

			game = new Game(gameType, player1, player2);
			game.StartGame();
		}

		/// <summary>
		/// Выбор режима игры
		/// </summary>
		/// <returns>Тип выбранной игры</returns>
		static IPlayer.GameTypes GetGameType()
		{
			Console.WriteLine("Выберите режим игры:");

			var gameTypes = Enum.GetValues(typeof(IPlayer.GameTypes));

			foreach (var type in gameTypes)
			{
				Console.WriteLine($"[{(int)type} - {type}]");
			}

			int i = 0;
			while (true)
			{
				try
				{
					i = Convert.ToInt32(Console.ReadLine());
					break;
				}
				catch (Exception)
				{
					Console.WriteLine("!!!НЕВЕРНО ВЫБРАН РЕЖИМ ИГРЫ!!!");
				}
			}

			return (IPlayer.GameTypes)i;
		}

		/// <summary>
		/// Метод для обучения агента через метод с закреплением "Q-Learning"
		/// </summary>
		//	static void InitializeAgentTraining(IPlayer.Shapes player)
		//	{
		//		agent = new Agent(player);

		//		if (player == IPlayer.Shapes.X)
		//		{
		//			StartAgentTrainingAsX();
		//		}
		//		else if (player == IPlayer.Shapes.O)
		//		{
		//			//StartAgentTrainingAsO();
		//		}

		//		Console.WriteLine("Начало обучения агента");
		//		Console.WriteLine(agent.GetAgentInfo());
		//		Console.WriteLine(agent.GetPolicyData());
		//	}

		//	/// <summary>
		//	/// Обучение бота игре за Х
		//	/// </summary>
		//	static void StartAgentTrainingAsX()
		//	{
		//		while (game.State == IPlayer.GameStates.Playing)
		//		{
		//			if (game.CurrentPlayer == IPlayer.Shapes.X && game.GameType == IPlayer.GameTypes.BotTrainingAsX ||
		//				game.CurrentPlayer == IPlayer.Shapes.O && game.GameType == IPlayer.GameTypes.BotTrainingAsO)
		//			{
		//				agent.GameField = game.GameField;
		//				//game.BotLatestMove = agent.MakeMove(game.AvailableCellsList);
		//				//game.BotMadeMove = true;
		//			}
		//		}
		//		agent.CalculateQValues(game.State);
		//	}
	}
}