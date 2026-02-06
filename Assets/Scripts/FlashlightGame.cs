using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FlashlightGame {
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

	public static class Lib {
		#region Fields

		private static LayerMask GroundLayerMask    => LayerMask.GetMask("Ground");
		private static LayerMask ClimbWallLayerMask => LayerMask.GetMask("ClimbWall");

		//* Constants
		private const float WallCheckDistance   = 0.8f;
		private const float WallRayDistance     = 3f;
		private const float WallRayHeight       = 1.2f;
		private const float WallTeleportOffsetY = 2f;


		//* Refs
		private static bool GizmosEnabled => true;

		#endregion

		#region Structs

		public struct WallClimbPoint {
			public Vector3 Position;
			public float   Distance;
		}

		#endregion

		#region Classes

		public static class Movement {
			/// <summary>
			/// Checks if there is ground below the given origin within the specified distance.
			/// </summary>
			/// <param name="origin">Where to check</param>
			/// <param name="distance">How big of an area to check</param>
			/// <returns>RaycastHit2D</returns>
			public static RaycastHit2D LedgeCheck(Vector3 origin, float distance) =>
				Physics2D.Raycast(origin, Vector2.down, distance, GroundLayerMask);

			/// <summary>
			/// Checks if there is ground within a *circle* at the given position and radius.
			/// </summary>
			/// <param name="origin">Where to check</param>
			/// <param name="radius">How big of a circle to check with</param>
			/// <returns>Collider2D</returns>
			public static Collider2D GroundCheck(Vector3 origin, float radius) =>
				Physics2D.OverlapCircle(origin, radius, GroundLayerMask);

			/// <summary>
			/// Check if a climbable wall is in front of the origin points look direction.
			/// </summary>
			/// <param name="origin">Where to check from</param>
			/// <param name="positiveX">Positive X means isLookingRight</param>
			/// <returns>RaycastHit2D</returns>
			public static RaycastHit2D WallCheck(Vector3 origin, bool positiveX) {
				var wallHit = Physics2D.Raycast(origin, positiveX ? Vector2.right : Vector2.left, WallCheckDistance);

				if (GizmosEnabled)
					Debug.DrawRay(origin, (positiveX ? Vector2.right : Vector2.left) * WallCheckDistance,
					              wallHit.collider ? Color.green : Color.red);

				return wallHit;
			}

			/// <summary>
			/// Returns the position and distance to the top of the nearest climbable wall from the given base position.
			/// </summary>
			/// <param name="basePosition">Where to originate from</param>
			/// <param name="positiveX">Positive X means isLookingRight</param>
			/// <returns>Lib.WallClimbPoint | Returns Position as Vector3.zero and Distance as 0f if no available point.</returns>
			public static WallClimbPoint GetWallClimbPoint(Vector3 basePosition, bool positiveX) {
				var origin    = (Vector2)basePosition + Vector2.up * WallRayHeight;
				var direction = positiveX ? Vector2.right : Vector2.left;

				if (GizmosEnabled) Debug.DrawRay(origin, direction * WallRayDistance, Color.red);

				var climbableWallHit = Physics2D.Raycast(origin, direction, WallRayDistance, ClimbWallLayerMask);

				if (!climbableWallHit)
					return new WallClimbPoint() {
						Position = Vector3.zero,
						Distance = 0f
					};

				var bounds = climbableWallHit.collider.bounds;

				if (GizmosEnabled) Debug.DrawLine(origin, climbableWallHit.point, Color.green);

				return new WallClimbPoint() {
					Position = new Vector3(
					                       bounds.center.x,
					                       bounds.max.y + WallTeleportOffsetY,
					                       basePosition.z
					                      ),
					Distance = climbableWallHit.distance
				};
			}
		}

		#endregion
	}

	public static class DebugHandler {
		#region Fields

		//* Settings *//
		public static DebugLevel   DbgLevel        = DebugLevel.Error;
		public static List<string> LogFilter       = new List<string>();
		public static string       LogFileLocation = Application.consoleLogPath;

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
		public static void LogKv(string message, DebugLevel level, params object[] context) {
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
		public static void Log(string message, DebugLevel level, params object[] context) {
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
		public static void Log(string message) => Log(message, DebugLevel.Info);

		/// <summary>
		/// Handle cases where the given object isn't a string.
		/// </summary>
		/// <param name="message">Object that can be converted using ToString()</param>
		public static void Log(object message) => Log(message.ToString(), DebugLevel.Info);

		/// <summary>
		/// Log a simple message for the specified level.
		/// </summary>
		/// <param name="message">string</param>
		/// <param name="level">DebugHandler.DebugLevel.*</param>
		/// <exception cref="ArgumentOutOfRangeException">Invalid DebugLevel</exception>
		public static void Log(string message, DebugLevel level) {
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
}