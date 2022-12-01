using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
