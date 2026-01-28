using System;
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
	

	#endregion

	#region Unity Functions

	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(this.gameObject);
		} else {
			Instance = this;
			
			LogKv("DebugHandler initialized.", DebugLevel.Info, new object[] { "Debug Level", debugLevel.ToString() });
		}
	}

	#endregion

	#region Functions

	/// <summary>
	/// Check if the specified debug level is permitted by current settings.
	/// </summary>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <returns>True/False depending on level</returns>
	public bool LevelPermitted(DebugLevel level) {
		if (debugLevel == DebugLevel.None) return false;
		return level   <= debugLevel;
	}

	/// <summary>
	/// Log a message together with "key-value pairs" as context.
	/// </summary>
	/// <param name="message">string</param>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <param name="context">{ "Key", "Value", "Key", "Value" ... }</param>
	public void LogKv(string message, DebugLevel level, params object[] context) {
		if (!LevelPermitted(level)) return;

		var logMessage = $"[{level.ToString().ToUpper()}] {message}";

		if (context != null && context.Length > 0) {
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

		var logMessage = message;

		if (context != null && context.Length > 0) {
			logMessage = context.Aggregate(logMessage, (current, ctx) => current + $" | {ctx}");
		}

		Log(logMessage, level);
	}

	/// <summary>
	/// Log a simple message for specified level.
	/// </summary>
	/// <param name="message">string</param>
	/// <param name="level">DebugHandler.DebugLevel.*</param>
	/// <exception cref="ArgumentOutOfRangeException">Invalid DebugLevel</exception>
	public void Log(string message, DebugLevel level) {
		if (!LevelPermitted(level)) return;

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