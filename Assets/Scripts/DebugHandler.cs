using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FlashlightGame;

namespace FlashlightGame {
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
}

public class DebugHandler {
	#region Fields

	//* Settings *//
	private static DebugLevel   DbgLevel  => Preferences.DebugHandler.DbgLevel;
	private static List<string> LogFilter => Preferences.DebugHandler.LogFilter;

	//* Instance *//
	private readonly string scriptName;

	#endregion

	#region Constructor

	/// <summary>
	/// Initialize a DebugHandler instance with a script name.
	/// </summary>
	/// <param name="scriptName">Name of the script for logging context</param>
	public DebugHandler(string scriptName) {
		this.scriptName = scriptName;
	}

	#endregion

	#region Functions

	/// <summary>
	/// Check if the specified debug level is permitted by current settings.
	/// </summary>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <returns>True/False depending on the level</returns>
	public static bool LevelPermitted(DebugLevel level) {
		if (DbgLevel == DebugLevel.None) return false;
		return level <= DbgLevel;
	}

	private static bool IsFiltered(string message) {
		var firstWord = message.Split(' ')[0];
		return LogFilter.Contains(firstWord);
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

		LogInternal(logMessage, level);
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

		LogInternal(logMessage, level);
	}

	/// <summary>
	/// Log a message together with no level. Defaults to DebugLevel.Info.
	/// </summary>
	/// <param name="message">string</param>
	public void Log(string message) => LogInternal(message, DebugLevel.Info);

	/// <summary>
	/// Handle cases where the given object isn't a string.
	/// </summary>
	/// <param name="message">Object that can be converted using ToString()</param>
	public void Log(object message) => LogInternal(message.ToString(), DebugLevel.Info);

	/// <summary>
	/// Log a simple message for the specified level.
	/// </summary>
	/// <param name="message">string</param>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <exception cref="ArgumentOutOfRangeException">Invalid DebugLevel</exception>
	private void LogInternal(string message, DebugLevel level) {
		if (!LevelPermitted(level)) return;

		var logMessage = $"[{level.ToString().ToUpper()}] [{scriptName}] {message} ";

		if (IsFiltered(logMessage)) return;

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