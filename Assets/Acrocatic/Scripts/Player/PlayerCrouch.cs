using UnityEngine;
using System.Collections;

namespace Acrocatic {
	public class PlayerCrouch : MonoBehaviour {
		// Public variables.
		[Header("Crouching")]
		[Tooltip("Enable or disable movement while crouching.")]
		public bool allowMovement = true;
		[Tooltip("Set the movement speed while crouching.")]
		public float crouchSpeed = 3f;
		[Header("Sliding")]
		[Tooltip("Enable or disable sliding while running and pressing the crouch button.")]
		public bool allowRunningSlide = true;
		[Tooltip("Enable or disable sliding while crouching and pressing the dash button.")]
		public bool allowCrouchedSlide = true;
		[Tooltip("Set the force for a normal slide.")]
		public float slideForce = 600f;
		[Tooltip("Set the duration of the slide.")]
		public float slideTime = 0.3f;
		[Tooltip("Set a cooldown duration in which you can't perform a slide.")]
		public float cooldownTime = 0.5f;
		[Header("Air sliding")]
		[Tooltip("Enable or disable sliding while in the air and pressing the crouch button.")]
		public bool allowAirSlide = true;
		[Tooltip("Set the force for an air slide.")]
		public Vector2 airSlideForce;

		// Private variables.
		private Player player;						// Get the Player class.
		private bool slide = false;					// Boolean that determines if a slide should be performed.
		private bool slideAllowed = false;			// Boolean that determines if a slide is allowed.
		private float slideTimer;					// Timer used to count down the slideTime.
		private float cooldownTimer;				// Timer used to count down the cooldownTime.
		private bool runCooldownTimer = false;		// Boolean that determines if the cooldown timer should run.

		// Use this for initialization.
		void Start () {
			// Setting up references.
			player = GetComponent<Player>();
		}
		
		// Update is called once per frame.
		void Update () {
			// Cache Crouch button press.
			bool crouch = Input.GetButton("Crouch");
			bool crouchSlide = Input.GetButtonDown("Crouch");

			// Cache Dash button press.
			bool dash = Input.GetButtonDown("Dash");

			// Check if a slide is allowed.
			SetSlideAllowed();

			// If the player is grounded, not crouching, not performing a dash and the Crouch button is being pressed...
			if (player.grounded && !player.crouching && !player.dashing && !player.onLadder && crouch) {
				// Make sure the player is crouching.
				Crouch();

				// If the player is running, pressing the crouch button and is allowed to slide while running...
				if (allowRunningSlide && slideAllowed && (player.hor == 1 || player.hor == -1) && crouchSlide) {
					// ... perform the slide.
					Slide();
				}
			}

			// If the player is allowed to slide while crouching, currently crouching and pressing the Dash button...
			if (allowCrouchedSlide && slideAllowed && player.crouching && dash) {
				// ... perform the slide.
				Slide();
			}

			// If the player is not grounded, air sliding is allowed, not performing a dash, not on a ladder, not on a wall, not falling and the player is pressing the crouch button...
			if (!player.grounded && allowAirSlide && slideAllowed && !player.dashing && !player.onLadder && !player.stuckToWall && !player.falling && crouchSlide) {
				// ... perform the slide.
				Slide();
				
				// Make sure the player is crouching.
				Crouch();
			}

			// Check if the player should stand up.
			StandUp(crouch);
		}

		// This function is called every fixed framerate frame.
		void FixedUpdate() {
			// If crouching and movement is allowed...
			if (player.crouching && allowMovement && !player.sliding && player.hor != 0) {
				// ... set X velocity to the speed times the horizontal input (between -1 and 1).
				player.SetXVelocity(player.GetSpeed() * player.hor);
			}

			// If a slide should be performed...
			if (slide) {
				// ... reset the slide variable.
				slide = false;

				// Unstick from current platform.
				player.UnstickFromPlatform();

				// If the player is in the air and an air slide is allowed...
				if (allowAirSlide && !player.grounded) {
					// ... reset the X and Y velocity.
					player.SetXVelocity(0);
					player.SetYVelocity(0);
					
					// Add an X and Y force to the rigid body to actually perform the slide.
					player.rigidbody.AddForce(new Vector2((player.facingRight ? 1 : -1) * airSlideForce.x, airSlideForce.y));
				// Or else...
				} else {
					// ... reset the X velocity.
					player.SetXVelocity(0);
					
					// Add an X force to the rigid body to actually perform the slide.
					player.rigidbody.AddForce(new Vector2((player.facingRight ? 1 : -1) * slideForce, 0f));
				}
				
				// Reset the slide timers.
				slideTimer = slideTime;
				cooldownTimer = cooldownTime;
			}
		}

		// Perform a slide.
		void Slide() {
			// Set the slide variable to true.
			slide = true;

			// Set the player's sliding variable to true.
			player.Slide(true);
		}

		// Make sure the player should crouch.
		void Crouch() {
			// Set the crouching variable.
			player.Crouch(true);
		}

		// Make sure if the player should stand up.
		void StandUp(bool crouch) {
			// If the player is crouching, not sliding and the player is not grounded anymore OR if the player stops crouching...
			if (player.crouching && !player.sliding && (!player.grounded || (!crouch && player.AllowedToStandUp()))) {
				// Reset the crouching variable.
				player.Crouch(false);
			}
		}

		// Set the slideAllowed variable.
		void SetSlideAllowed() {
			// Set slideAllowed to true by default.
			slideAllowed = true;
			
			// If the player is currently sliding...
			if (player.sliding) {
				// ... make sure sliding isn't allowed.
				slideAllowed = false;
				
				// Run the slide timer.
				if (slideTimer > 0) {
					slideTimer -= Time.deltaTime;
				// When the timer is finished...
				} else {
					// ... run the cooldown timer.
					runCooldownTimer = true;
					
					// Player isn't sliding anymore, so reset the variable.
					player.Slide(false);
				}
			}
			
			// If the cooldown timer is running...
			if (runCooldownTimer) {
				// ... make sure sliding isn't allowed.
				slideAllowed = false;
				
				// Run the cooldown timer.
				if (cooldownTimer > 0) {
					cooldownTimer -= Time.deltaTime;
				// When the timer is finished...
				} else {
					// ... reset the runCooldownTimer variable.
					runCooldownTimer = false;
					
					// Make sure sliding is allowed again.
					slideAllowed = true;
				}
			}
		}
	}
}
