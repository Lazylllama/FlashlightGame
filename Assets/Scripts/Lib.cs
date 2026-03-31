using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable CheckNamespace
namespace FlashlightGame {
	public static class Lib {
		#region Fields

		private static LayerMask BoxLayerMask        => LayerMask.GetMask("Box");
		private static LayerMask GroundLayerMask     => LayerMask.GetMask("Ground");
		private static LayerMask MantleWallLayerMask => LayerMask.GetMask("MantleWall");

		//* Refs
		private static bool GizmosEnabled => true;
		
		#endregion

		#region Structs, Enums, Constants

		public struct WallMantlePoint {
			public Vector3 Position;
			public float   Distance;
		}

		public enum InputType {
			KeyboardMouse,
			Xbox,
			PlayStation,
			SteamDeck,
			Unknown
		}

		public static Dictionary<InputType, string> InputTypeDisplayName = new() {
			[InputType.KeyboardMouse] = "Keyboard & Mouse",
			[InputType.Xbox]          = "Xbox",
			[InputType.PlayStation]   = "PlayStation",
			[InputType.SteamDeck]     = "Steam Deck",
			[InputType.Unknown]       = "Unknown"
		};

		#endregion

		#region Classes

		public static class Input {
			public static InputType RevealDevice(InputDevice device) {
				switch (device) {
					//* Gamepad Inputs
					case Gamepad gamepad: {
						var name         = gamepad.description.product?.ToLowerInvariant()      ?? "";
						var manufacturer = gamepad.description.manufacturer?.ToLowerInvariant() ?? "";

						//* Steam Deck
						if (name.Contains("steam") || name.Contains("deck")) return InputType.SteamDeck;

						//* PS Dualshock/Dualsense
						// TODO(@lazylllama): Maybe make diff input type for dualsense for some features idk
						if (device is UnityEngine.InputSystem.DualShock.DualShockGamepad
						    || name.Contains("dualsense")
						    || name.Contains("dualshock")
						    || manufacturer.Contains("sony"))
							return InputType.PlayStation;

						//* Generic Shit Controller (or Xbox)
						return InputType.Xbox;
					}

					//* Keyboard Mouse if not gamepad
					default: return InputType.KeyboardMouse;
				}
			}
		}


		public static class Game {
		}

		public static class Movement {
			//* Constants
			//? Public for gizmos etc
			public const float WallCheckDistance   = 1.0f;
			private const float WallTeleportOffsetY = 2f;

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
			/// Check if a mantle-able wall is in front of the origin points look direction.
			/// </summary>
			/// <param name="origin">Where to check from</param>
			/// <param name="positiveX">Positive X means isWalkingRight</param>
			/// <returns>RaycastHit2D</returns>
			public static RaycastHit2D MantleWallCheck(Vector3 origin, bool positiveX) {
				var hit = Physics2D.Raycast(origin, positiveX ? Vector2.right : Vector2.left, WallCheckDistance,
				                            MantleWallLayerMask);
				if (!hit.collider)
					Debug.DrawRay(origin, positiveX ? Vector2.right : Vector2.left * WallCheckDistance, Color.blue);
				else Debug.DrawLine(origin, hit.point, Color.purple);
				return hit;
			}

			/// <summary>
			/// Check if a ground layer wall is in front of the origin points look direction.
			/// </summary>
			/// <param name="origin">Where to check from</param>
			/// <param name="positiveX">Positive X means isWalkingRight</param>
			/// <returns>RaycastHit2D</returns>
			public static RaycastHit2D WallCheck(Vector3 origin, bool positiveX) =>
				Physics2D.Raycast(origin, positiveX ? Vector2.right : Vector2.left, WallCheckDistance,
				                  GroundLayerMask);

			/// <summary>
			/// Returns the position and distance to the top of the nearest mantle-able wall from the given base position.
			/// </summary>
			/// <param name="basePosition">Where to originate from</param>
			/// <param name="positiveX">Positive X means isWalkingRight</param>
			/// <returns>Lib.WallMantlePoint | Returns Position as Vector3.zero and Distance as 0f if no available point.</returns>
			public static WallMantlePoint GetWallMantlePoint(Vector3 basePosition, bool positiveX) {
				var origin        = (Vector2)basePosition + Vector2.up * WallCheckDistance;
				var direction     = positiveX ? Vector2.right : Vector2.left;
				var mantleWallHit = Physics2D.Raycast(origin, direction, WallCheckDistance, MantleWallLayerMask);
				if (!mantleWallHit.collider) Debug.DrawRay(origin, direction * WallCheckDistance, Color.blue);
				else Debug.DrawLine(origin, mantleWallHit.point, Color.purple);

				var defaultReturn = new WallMantlePoint() {
					Position = Vector3.zero,
					Distance = 0f
				};

				if (!mantleWallHit) return defaultReturn;

				var centerX   = mantleWallHit.point.x + (positiveX ? 0.5f : -0.5f);
				var rayOrigin = new Vector2(centerX, mantleWallHit.point.y + 300f);

				var getGroundY = Physics2D.Raycast(rayOrigin, Vector2.down, 350f, GroundLayerMask);

				if (!getGroundY)
					getGroundY = Physics2D.Raycast(rayOrigin, Vector2.down, 350f, BoxLayerMask);

				if (!getGroundY.collider) return defaultReturn;

				return new WallMantlePoint() {
					Position = new Vector3(
					                       centerX,
					                       getGroundY.point.y + WallTeleportOffsetY,
					                       basePosition.z
					                      ),
					Distance = mantleWallHit.distance
				};
			}
		}

		#endregion
	}
}