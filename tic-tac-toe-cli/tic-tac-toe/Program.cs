namespace tic_tac_toe_cli
{
	internal class Program
	{
		static void Main(string[] args)
		{
			//Console.WriteLine("Hello, World!");

			Game game = new Game();
			game.StartGame();

			//game.printGameField();
		}
	}
}