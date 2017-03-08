using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class that controls the running mechanic for the player.
	public class PlayerRun : MonoBehaviour {
		// Public variables.
		[Tooltip("When you enable this variable, the player only 'runs' when pressing the Run input. If you disable this variable, running is the default state while moving.")]
		public bool pressToRun = true;
		[Tooltip("Set the player's maximum walking speed. This is only used when Press To Run is enabled.")]
		public float walkSpeed = 3f;
		[Tooltip("Set the player's maximum running speed.")]
		public float runSpeed = 7f;
		[Tooltip("Set the total force that should be added to the player when walking or running.")]
		public float moveForce = 50f;

		// Private variables.
		private Player player;					// Get the Player class.
		
		// Use this for initialization.
		void Start () {
			// Setting up references.
			player = GetComponent<Player>();
		}

		// This function is called every fixed framerate frame.
		void FixedUpdate () {
			// Cache the run input.
			bool running = Input.GetButton("Run");

			// Get the current speed.
			float speed = GetSpeed();

			// Only make the player walk when pressToRun is active and the player isn't running.
			player.Walk(pressToRun && !running);

			// If the player is not stuck to the wall and the player is not dashing, sliding, crouching and/or on a ladder...
			if (!player.stuckToWall && !player.dashing && !player.sliding && !player.crouching && !player.onLadder) {
				// ... if the player is changing direction (h has a different sign to velocity.x) or hasn't reached speed yet...
				if(player.hor * player.rigidbody.velocity.x < speed) {
					// ... add a force to the player.
					player.rigidbody.AddForce(transform.rotation * Vector2.right * player.hor * player.GetMovementForce(moveForce));
				}
				// If the player's horizontal velocity is greater than the speed and the player isn't stuck to the X of a platform...
				if(Mathf.Abs(player.rigidbody.velocity.x) > speed && !player.IsStuckToPlatformX()) {
					// ... set the player's velocity to the speed in the x axis.
					player.SetXVelocity(Mathf.Sign(player.rigidbody.velocity.x) * speed);
				}
			}
		}
		
		// Get the current speed for the player.
		public float GetSpeed(bool withPlatform = true) {
			// Cache the horizontal input, run input and speed.
			bool running = Input.GetButton("Run");
			float speed = runSpeed;

			// If a button needs to be pressed to run...
			if (pressToRun) {
				// If the player is in the air and can't change between walking and running in the air...
				if (!player.grounded && !player.CanWalkAndRunInAir()) {
					// ... set the speed based on if the player was walking or running before jumping.
					speed = player.AirWalk() ? walkSpeed : runSpeed;
				} else {
					// ... set the speed to walking or running, depending on the run input.
					speed = running ? runSpeed : walkSpeed;
				}
			}

			// If the player is crouching...
			if (player.crouching) {
				// ... set the speed to the crouch speed.
				speed = player.GetCrouchSpeed();
			}
			
			// Set the player's speed based on the speed of the platform.
			if (withPlatform) { speed = player.GetSpeedOnMovingPlatform(speed); }

			// Get the player's air movement speed.
			speed = player.GetSpeedInAir(speed);

			// Return the speed.
			return speed;
		}
	}
}