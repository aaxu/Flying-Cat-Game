using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class that handles the player dashing.
	public class PlayerDash : MonoBehaviour {
		// Public variables.
		[Header("Dash settings")]
		[Tooltip("Set the force of the dash.")]
		public float dashForce = 600f;
		[Tooltip("Set the duration of the dash. The player can't move during this duration.")]
		public float dashTime = 0.2f;
		[Tooltip("Set the cooldown duration after performing a dash. The player can't dash again while the cooldown is active.")]
		public float cooldownTime = 0.5f;
		[Header("Vertical movement")]
		[Tooltip("Enable or disable gravity while performing a dash.")]
		public bool dashGravity = false;
		[Tooltip("When gravity is disabled, you can set the player's Y velocity to make sure the vertical position doesn't change. Or you can use it to add vertical movement to the dash.")]
		public float dashVelocityY = 0.4f;
		[Header("Air dashing")]
		[Tooltip("Enable or disable dashing while in the air.")]
		public bool airDash = true;
		[Tooltip("You can set a limit for the amount of dashes in the air by enabling this variable and changing the variable below.")]
		public bool airDashLimit = true;
		[Tooltip("When there is an air dashing limit, you can set the amount of air dashes here.")]
		public int airDashTotal = 1;

		// Private variables.
		private bool dash = false;					// Boolean that determines if a dash should be performed.
		private bool dashAllowed = false;			// Boolean that determines if a dash is allowed.
		private float dashTimer;					// Timer used to count down the dashTime.
		private float cooldownTimer;				// Timer used to count down the cooldownTime.
		private bool runCooldownTimer = false;		// Boolean that determines if the cooldown timer should run.
		private int totalAirDashes;					// Determines how many air dashes are currently allowed.
		private Player player;						// Get the Player class.

		// Use this for initialization.
		void Start () {
			// Setting up references.
			player = GetComponent<Player>();
		}
		
		// Update is called once per frame.
		void Update () {
			// If the player is grounded...
			if (player.grounded) {
				// ... reset the total air dashes allowed.
				totalAirDashes = airDashTotal;
			}

			// Call the SetDashAllowed function to make sure if a dash is allowed.
			SetDashAllowed();

			// If the player is currently dashing...
			if (player.dashing) {
				// ... make sure dashing isn't allowed.
				dashAllowed = false;

				// Reset the dash timer if the X velocity is 0.
				if (player.rigidbody.velocity.x == 0) {
					dashTimer = 0;
				}

				// Run the dash timer.
				if (dashTimer > 0) {
					dashTimer -= Time.deltaTime;
				// When the timer is finished...
				} else {
					// ... run the cooldown timer.
					runCooldownTimer = true;

					// Player isn't dashing anymore, so reset the variable.
					player.Dash(false);
				}
			}

			// If the cooldown timer is running...
			if (runCooldownTimer) {
				// ... make sure dashing isn't allowed.
				dashAllowed = false;

				// Run the cooldown timer.
				if (cooldownTimer > 0) {
					cooldownTimer -= Time.deltaTime;
				// When the timer is finished...
				} else {
					// ... reset the runCooldownTimer variable.
					runCooldownTimer = false;

					// Call the SetDashAllowed function to make sure if a dash is allowed.
					SetDashAllowed();
				}
			}

			// If the dash button is pressed and if a dash is allowed...
			if (Input.GetButtonDown("Dash") && dashAllowed && !player.dashing && !player.crouching && !player.stuckToWall && !player.falling && !player.sliding && !player.onLadder) {
				// ... perform the dash.
				dash = true;
			}
		}

		// This function is called every fixed framerate frame.
		void FixedUpdate () {
			// If a dash should be performed...
			if (dash) {
				// ... reset the dash variable.
				dash = false;

				// Unstick from the current platform.
				player.UnstickFromPlatform();

				// Set dashing to true.
				player.Dash(true);

				// Reset the X velocity.
				player.SetXVelocity(0);

				// Add an X force to the rigid body to actually perform the dash.
				player.rigidbody.AddForce(new Vector2((player.facingRight ? 1 : -1) * dashForce, 0f));

				// Reset the dash timers.
				dashTimer = dashTime;
				cooldownTimer = cooldownTime;

				// If there is an air dash limit, make sure it gets decreased.
				if (airDashLimit && airDashTotal > 0) {
					totalAirDashes--;
				}
			}

			// If the player is currently dashing...
			if (player.dashing) {
				// ... make sure the Y velocity is set if the gravity is disabled for the dash.
				if (!dashGravity) {
					// Set the speed.
					float speed = dashVelocityY;

					// If the player is on a moving platform, make sure the extra Y velocity is added.
					if (player.OnMovingPlatform()) {
						GameObject platform = player.GetPlatform();
						float yVel = platform.GetComponent<Rigidbody2D>().velocity.y;
						speed = speed + yVel;
					}

					// Set the Y velocity.
					player.SetYVelocity(speed);
				}
			}
		}

		// Set the dashAllowed variable.
		void SetDashAllowed() {
			// Make sure dashing in the air isn't allowed when airDash is set to false.
			if (!player.grounded && !airDash) {
				dashAllowed = false;
			// Or else if air dashing is allowed and player is currently in the air and there is an air dash limit...
			} else if (!player.grounded && airDashLimit) {
				// ... only allow a dash if totalAirDashes is higher than 0.
				if (totalAirDashes > 0) {
					dashAllowed = true;
				} else {
					dashAllowed = false;
				}
			// Or else...
			} else {
				// ... allow the dash.
				dashAllowed = true;
			}
		}

		// Reset the air dashing limit.
		public void ResetAirDashLimit() {
			totalAirDashes = airDashTotal;
		}
	}
}
