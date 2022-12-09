using cli_library;

namespace main
{
	internal class Program
	{
		public static Game game;
		public static IPlayer player1;
		public static IPlayer player2;

		static void Main(string[] args)
		{
			StartGame(GetGameType());
		}

		/// <summary>
		/// Метод генерации новой игры
		/// </summary>
		static void StartGame(IPlayer.GameTypes gamemode)
		{
			GenerateGamemode(gamemode);

			for (int i = 0; i < 1000; i++)
			{
				game = new Game(gamemode, player1, player2);
				LinkEventsWithPlayers();
				game.StartGame();
			}
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
		/// Генерация новой игры с указанным режимом игры (в данный момент доступны только первые 2)
		/// </summary>
		/// <param name="gamemode">Режим игры</param>
		static void GenerateGamemode(IPlayer.GameTypes gamemode)
		{
			player1 = new Player();
			player2 = new Player();

			if (gamemode == IPlayer.GameTypes.TwoPlayers)
			{
				player1 = new Human(IPlayer.Shapes.X);
				player2 = new Human(IPlayer.Shapes.O);
			}
			else if (gamemode == IPlayer.GameTypes.BotTrainingAsX)
			{
				player1 = new Agent(IPlayer.Shapes.X);
				player2 = new RandomPlayer(IPlayer.Shapes.O);
			}

			// TO DO: реализация остальных режимов игры

		}

		/// <summary>
		/// Метод для связи событий с экземплярами игроков (в данный момент реализовано только для подкласса Agent)
		/// </summary>
		static void LinkEventsWithPlayers()
		{
			if (player1.GetType().Equals(typeof(Agent)))
			{
				game.GameEnded += ((Agent)player1).GameIsEnded;
			}

			if (player2.GetType().Equals(typeof(Agent)))
			{
				game.GameEnded += ((Agent)player2).GameIsEnded;
			}

		}
	}
}