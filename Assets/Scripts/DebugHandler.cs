using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugHandler : MonoBehaviour {
	#region Types

	/// <summary>
	/// Defines the level of debug message. Messages with a level equal to or lower than the current setting will be logged.
	/// </summary>
	public enum DebugLevel {
		None,
		Fatal,
		Error,
		Warning,
		Info,
		Debug
	}

	#endregion

	#region Fields

	//* Instance *//
	public static DebugHandler Instance;

	//* Settings *//
	[Header("Settings")]
	public DebugLevel debugLevel = DebugLevel.Warning;
	public List<string> filteredLogs;

	#endregion

	#region Unity Functions

	private static void RegisterInstance(DebugHandler instance) {
		if (Instance != null && Instance != instance) {
			Destroy(instance.gameObject);
		} else {
			Instance = instance;

			instance.LogKv("DebugHandler initialized.", DebugLevel.Info,
			               new object[] { "Debug Level", instance.debugLevel.ToString() });
		}
	}

	private void Awake() {
		RegisterInstance(this);
	}

	#endregion

	#region Functions

	/// <summary>
	/// Check if the specified debug level is permitted by current settings.
	/// </summary>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <returns>True/False depending on the level</returns>
	public bool LevelPermitted(DebugLevel level) {
		if (debugLevel == DebugLevel.None) return false;
		return level   <= debugLevel;
	}

	private bool IsFiltered(string message) {
		var firstWord = message.Split(' ')[0];
		return filteredLogs.Contains(firstWord);
	}

	/// <summary>
	/// Log a message together with "key-value pairs" as context.
	/// </summary>
	/// <param name="message">string</param>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <param name="context">{ "Key", "Value", "Key", "Value" ... }</param>
	public void LogKv(string message, DebugLevel level, params object[] context) {
		if (!LevelPermitted(level)) return;
		if (IsFiltered(message)) return;

		var logMessage = message;

		if (context is { Length: > 0 }) {
			for (var i = 0; i < context.Length; i += 2) {
				logMessage += $" | {context[i]}: {context[i + 1]}";
			}
		}

		Log(logMessage, level);
	}

	/// <summary>
	/// Log a message together with unlabeled context.
	/// </summary>
	/// <param name="message">string</param>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <param name="context">new object[] { "Something", 132, false ... }</param>
	public void Log(string message, DebugLevel level, params object[] context) {
		if (!LevelPermitted(level)) return;
		if (IsFiltered(message)) return;

		var logMessage = message;

		if (context is { Length: > 0 }) {
			logMessage = context.Aggregate(logMessage, (current, ctx) => current + $" | {ctx}");
		}

		Log(logMessage, level);
	}

	/// <summary>
	/// Log a message together with no level. Defaults to DebugLevel.Info.
	/// </summary>
	/// <param name="message">string</param>
	public void Log(string message) => Log(message, DebugLevel.Info);

	/// <summary>
	/// Log a simple message for the specified level.
	/// </summary>
	/// <param name="message">string</param>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <exception cref="ArgumentOutOfRangeException">Invalid DebugLevel</exception>
	public void Log(string message, DebugLevel level) {
		if (!LevelPermitted(level)) return;
		if (IsFiltered(message)) return;

		var logMessage = $"[{level.ToString().ToUpper()}] {message} ";

		switch (level) {
			case DebugLevel.None: break;
			case DebugLevel.Fatal:
				Debug.LogException(new Exception(logMessage));
				return;
			case DebugLevel.Error:
				Debug.LogError(logMessage);
				return;
			case DebugLevel.Info:
			case DebugLevel.Debug:
				Debug.Log(logMessage);
				return;
			case DebugLevel.Warning:
				Debug.LogWarning(logMessage);
				return;

			default: throw new ArgumentOutOfRangeException(nameof(level), level, null);
		}
	}

	#endregion
}