using System.Collections.Generic;
using UnityEngine;


namespace FlashlightGame {
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

	public static class Settings {
		#region Fields

		//* DebugHandler *//
		public const DebugHandler.DebugLevel DebugLevel = DebugHandler.DebugLevel.Debug;
		public static readonly List<string> LogFilter = new List<string>() {
			"PerformMove", // Spammy
			"ChaseTarget"  // Spammy
		};

		#endregion
	}
}