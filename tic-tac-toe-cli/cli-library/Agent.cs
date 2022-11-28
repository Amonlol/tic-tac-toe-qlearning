using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace cli_library
{
	public class Agent
	{
		#region Константы

		const double GAMMA = 0.75; //gamma
		const double ALPHA = 0.01; //alpha
		const double EPS = 0.2; //epsilon

		const double WIN_REWARD = 10;
		const double DRAW_REWARD = 1;
		const double LOSE_REWARD = -20;

		const string JSON_POLICY_FILE = @"cli-library\policy.json";
		static string PATH_TO_JSON_POLICY = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName,
																		 JSON_POLICY_FILE);

		#endregion

		#region Объекты

		public Dictionary<string, double> Policy;

		#endregion

		public Agent()
		{
			Initialize();
		}

		#region Методы

		/// <summary>
		/// Инициализация основных данных агента
		/// </summary>
		private void Initialize()
		{
			GetPolicyData();
			SavePolicyData();
		}

		/// <summary>
		/// Получение основной информации по агенту
		/// </summary>
		/// <returns>Формитированная строка с информацией</returns>
		public string GetAgentInfo()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine($"Gamma: {GAMMA}");
			sb.AppendLine($"Alpha: {ALPHA}");
			sb.AppendLine($"Epsilon: {EPS}");
			sb.AppendLine($"Win reward: {WIN_REWARD}");
			sb.AppendLine($"Draw reward: {DRAW_REWARD}");
			sb.AppendLine($"Lose reward: {LOSE_REWARD}");
			sb.AppendLine($"Policy path: {PATH_TO_JSON_POLICY}");

			return sb.ToString();
		}

		/// <summary>
		/// Загрузка политик из файла policy.json
		/// </summary>
		/// <returns>Строка с десериализованным словарем типа string:double</returns>
		public string GetPolicyData()
		{
			string json;
			this.Policy = new Dictionary<string, double>();

			try
			{
				json = File.ReadAllText(PATH_TO_JSON_POLICY);
				this.Policy = JsonSerializer.Deserialize<Dictionary<string, double>>(json);
				return json;
			}
			catch (Exception)
			{
				json = JsonSerializer.Serialize(this.Policy, new JsonSerializerOptions { WriteIndented = true });
				File.Delete(PATH_TO_JSON_POLICY);
				File.WriteAllText(PATH_TO_JSON_POLICY, json);
				return json;
			}
		}

		/// <summary>
		/// Сохранение словаря с политиками в файл policy.json
		/// </summary>
		public void SavePolicyData()
		{
			string json = JsonSerializer.Serialize(this.Policy, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(PATH_TO_JSON_POLICY, json);
		}

		public Game.Cell MakeMove(List<Game.Cell> AvailableCellsList)
		{

			//exploration
			//var rnd = new Random();
			//if (rnd.Next(1, 100) <= EPS)
			//{

			//}

			//exploitation

			var rnd = new Random();
			return AvailableCellsList[rnd.Next(0, AvailableCellsList.Count - 1)];
		}
		#endregion
	}
}
