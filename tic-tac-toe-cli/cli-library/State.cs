namespace cli_library
{
	public struct State
	{
		public double q;
		public string currentState;
		public Game.Cell myMoveCell;

		public State(double q, string currentState, Game.Cell myMoveCell)
		{
			this.q = q;
			this.currentState = currentState;
			this.myMoveCell = myMoveCell;
		}
	}
}
