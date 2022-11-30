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

			var gameTypes = Enum.GetValues(typeof(Game.GameTypes));

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

			if ((Game.GameTypes)i == Game.GameTypes.BotTrainingAsX)
			{
				InitializeAgentTraining();
				StartAgentTrainingAsX();
			}
		}

		/// <summary>
		/// Метод генерации новой игры
		/// </summary>
		static void StartGame()
		{
			game = new Game(Game.GameTypes.BotTrainingAsX);
			game.StartGame();
		}

		/// <summary>
		/// Метод для обучения агента через метод с закреплением "Q-Learning"
		/// </summary>
		static void InitializeAgentTraining()
		{
			agent = new Agent();
			Console.WriteLine("Начало обучения агента");
			Console.WriteLine(agent.GetAgentInfo());
			Console.WriteLine(agent.GetPolicyData());
		}

		static void StartAgentTrainingAsX()
		{
			while (game.State == Game.GameStates.Playing)
			{
				if (game.CurrentPlayer == Game.Players.X && game.GameType == Game.GameTypes.BotTrainingAsX)
				{
					game.BotLatestMove = agent.MakeMove(game.AvailableCellsList, Game.Players.X);
					game.BotMadeMove = true;
					agent.GameField = game.GameField;
				}
				else if (game.CurrentPlayer == Game.Players.O && game.GameType == Game.GameTypes.BotTrainingAsO)
				{
					game.BotLatestMove = agent.MakeMove(game.AvailableCellsList, Game.Players.O);
					game.BotMadeMove = true;
					agent.GameField = game.GameField;
				}
			}
		}
	}
}