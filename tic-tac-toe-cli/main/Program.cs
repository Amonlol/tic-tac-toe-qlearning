using cli_library;

namespace main
{
	internal class Program
	{
		public static Game game;
		public static Agent agent;

		static void Main(string[] args)
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

			StartGame();

			if ((IPlayer.GameTypes)i == IPlayer.GameTypes.BotTrainingAsX)
			{
				InitializeAgentTraining(IPlayer.Players.X);
				StartAgentTrainingAsX();
			}
		}

		/// <summary>
		/// Метод генерации новой игры
		/// </summary>
		static void StartGame()
		{
			game = new Game(IPlayer.GameTypes.BotTrainingAsX);
			game.StartGame();
		}

		/// <summary>
		/// Метод для обучения агента через метод с закреплением "Q-Learning"
		/// </summary>
		static void InitializeAgentTraining(IPlayer.Players player)
		{
			agent = new Agent(player);

			if (player == IPlayer.Players.X)
			{
				StartAgentTrainingAsX();
			}
			else if (player == IPlayer.Players.O)
			{
				//StartAgentTrainingAsO();
			}

			Console.WriteLine("Начало обучения агента");
			Console.WriteLine(agent.GetAgentInfo());
			Console.WriteLine(agent.GetPolicyData());
		}

		/// <summary>
		/// Обучение бота игре за Х
		/// </summary>
		static void StartAgentTrainingAsX()
		{
			while (game.State == IPlayer.GameStates.Playing)
			{
				if (game.CurrentPlayer == IPlayer.Players.X && game.GameType == IPlayer.GameTypes.BotTrainingAsX ||
					game.CurrentPlayer == IPlayer.Players.O && game.GameType == IPlayer.GameTypes.BotTrainingAsO)
				{
					agent.GameField = game.GameField;
					game.BotLatestMove = agent.MakeMove(game.AvailableCellsList);
					game.BotMadeMove = true;
				}
			}
			agent.CalculateQValues(game.State);
		}
	}
}