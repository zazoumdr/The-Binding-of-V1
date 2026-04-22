using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class UciChessEngine
{
	private Process engineProcess;

	private static string EngineDirectory => Path.Combine(Application.streamingAssetsPath, "ChessEngine");

	public UciChessEngine()
	{
		string fileName = FindExecutableInDirectory(EngineDirectory);
		engineProcess = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = fileName,
				UseShellExecute = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				CreateNoWindow = true
			}
		};
	}

	private string FindExecutableInDirectory(string directoryPath)
	{
		try
		{
			string[] files = Directory.GetFiles(directoryPath, "*.exe");
			if (files.Length != 0)
			{
				return files[0];
			}
			return null;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Error finding executable: " + ex.Message);
			return null;
		}
	}

	public async Task InitializeUciModeAsync(bool whiteIsBot, int elo)
	{
		engineProcess.Start();
		await Task.Delay(1);
		await SendCommandAsync("uci");
		while (!(await ReadResponseAsync()).StartsWith("uciok"))
		{
		}
		await SendCommandAsync("isready");
		while (await ReadResponseAsync() != "readyok")
		{
		}
		if (elo >= 1500)
		{
			await SetEloRatingAsync(elo);
		}
		else
		{
			await SetEloRatingAsync(500);
		}
		await SendCommandAsync("ucinewgame");
		await SendCommandAsync("position startpos");
		if (whiteIsBot)
		{
			MonoSingleton<ChessManager>.Instance.BotStartGame();
		}
	}

	public async Task SendPlayerMoveAndGetEngineResponseAsync(string moves, Action<string> callback, int moveTimeInMilliseconds = 2000)
	{
		await SendCommandAsync("position startpos moves " + moves);
		await SendCommandAsync("go movetime " + moveTimeInMilliseconds);
		string text;
		do
		{
			text = await ReadResponseAsync();
		}
		while (!text.StartsWith("bestmove"));
		callback(text);
	}

	public async Task SetEloRatingAsync(int eloRating)
	{
		if (eloRating < 0 || eloRating > 3200)
		{
			UnityEngine.Debug.LogError("Elo rating must be between 0 and 3200.");
		}
		await SendCommandAsync("setoption name UCI_LimitStrength value true");
		await SendCommandAsync($"setoption name UCI_Elo value {eloRating}");
	}

	public async Task SendCommandAsync(string command)
	{
		await engineProcess.StandardInput.WriteLineAsync(command);
	}

	public async Task<string> ReadResponseAsync()
	{
		return await engineProcess.StandardOutput.ReadLineAsync();
	}

	public async Task StopEngine()
	{
		await SendCommandAsync("quit");
		if (!engineProcess.HasExited)
		{
			engineProcess.Close();
		}
	}
}
