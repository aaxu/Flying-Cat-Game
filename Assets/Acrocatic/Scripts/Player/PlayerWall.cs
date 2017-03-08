using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class that controls the wall running/sliding/jumping mechanic for the player.
	public class PlayerWall : MonoBehaviour {
		// Public variables that shouldn't be shown in the inspector.
		[HideInInspector]
		public bool isWallJumping = false;			// Determines if the player is wall jumping. 

		// Public variables.
		[Tooltip("Select the layer for all wall interaction.")]
		public LayerMask wallLayer;
		[Tooltip("When the player fails to wall jump, you can make the player 'fall down' and unable to interact with other objects until he's on the ground.")]
		public bool shouldFall = true;
		[Tooltip("Enable this variable to only allow wall interaction while the player is in the air. You can disable this to allow the player to run up a wall when running against it while on the ground.")]
		public bool jumpToStick = true;
		[Tooltip("This is used for a timer that determines if the player should be disconnected from the wall. It's mainly used as a 'dead zone' to make sure the player can perform a wall jump.")]
		public float wallUnstickTime = 0.1f;

		// Group wall jumpings variables.
		[System.Serializable]
		public class WallJumping {
			[Tooltip("Enable or disable wall jumping.")]
			public bool enabled = true;
			[Tooltip("Determines how long the player is allowed to be stuck on the wall in 'wall jumping' state.")]
			public float wallStickTime = 0.25f;
			[Tooltip("Set the player's Y velocity while in the 'wall jumping' state.")]
			public float wallStickVelocity = 0f;
			[Tooltip("Enable or disable double jumping when performing a wall jump.")]
			public bool allowDoubleJump = true;
			[Tooltip("A 'boomerang wall jump' is a wall jump that directly returns you to the same wall (like a boomerang). Like in Super Meat Boy!")]
			public bool boomerangJump = true;
			[Tooltip("This determines how long the boomerang jump should take in the opposite direction of the wall.")]
			public float boomerangTime = 0.3f;
			[Tooltip("This allows you to change the player's air movement X speed while performing a boomerang wall jump.")]
			public float boomerangFactorX = 1.0f;
			[Tooltip("This allows you to change the player's air movement Y speed while performing a boomerang wall jump.")]
			public float boomerangFactorY = 1.0f;
			[Tooltip("Enable or disable multiple wall jumps on the same wall.")]
			public bool allowSameWall = false;
		}

		// Group wall running variables.
		[System.Serializable]
		public class WallRunning {
			[Tooltip("Enable or disable wall running.")]
			public bool enabled = true;
			[Tooltip("Set the total duration of the wall run.")]
			public float runTime = 1.5f;
			[Tooltip("Set the speed of the wall run.")]
			public float runSpeed = 5f;
			[Tooltip("Want the player to run on a wall for as long as they like? Enable this variable to set the duration to 'infinite'.")]
			public bool infiniteRun = false;
			[Tooltip("Slow down the player's wall run over the duration. This decreases the speed until the player comes to a stop. This doesn't work in combination with infinite running.")]
			public bool runSlowdown = true;
			[Tooltip("Enable or disable multiple wall runs on the same wall.")]
			public bool allowSameWall = false;
		}

		// Group wall sliding variables.
		[System.Serializable]
		public class WallSliding {
			[Tooltip("Enable or disable wall sliding.")]
			public bool enabled = true;
			[Tooltip("Set the total duration of the wall slide.")]
			public float slideTime = 1.5f;
			[Tooltip("Set the speed of the wall slide.")]
			public float slideSpeed = 5f;
			[Tooltip("Want the player to slide on a wall until he reaches the ground? Enable this variable to set the duration to 'infinite'.")]
			public bool infiniteSlide = false;
			[Tooltip("Speed up the wall slide until it reaches the slide speed. It starts very slow and gradually increases. This works in combination with the infinite slide.")]
			public bool slideSpeedup = true;
			[Tooltip("Enable or disable multiple wall slides on the same wall.")]
			public bool allowSameWall = false;
		}

		[Tooltip("Wall jumping is handled in this section. You have normal wall jumping and so called 'boomerang wall jumping'. Choose what you like best (or choose both).")]
		public WallJumping wallJump;
		[Tooltip("Wall running is handled in this section. You can also disable this to make the player slide down when interacting with a wall.")]
		public WallRunning wallRun;
		[Tooltip("Wall sliding is handled in this section. The player will perform a wall slide when wall running is completed or disabled.")]
		public WallSliding wallSlide;

		// Private variables.
		private Player player;						// Get the Player class.
		private float wallRunTimer;					// Timer used to determine how long the player can run up the wall.
		private float wallSlideTimer;				// Timer used to determine how long the player can slide down the wall.
		private float wallStickTimer;				// Timer used to determine how long the player can stick to the wall.
		private float wallUnstickTimer;				// Timer used to make the player unstick when not moving.
		private int currentWall;					// Remembers the current wall's instance ID.
		private int lastWall;						// Remembers the last wall's instance ID.
		private bool facingWall = false;			// Whether or not the player is facing a wall.
		private bool wallRunning;					// Determines if the player is currently running on a wall.
		private bool wallSliding;					// Determines if the player is currently sliding down a wall.
		private bool stuckToWall;					// Determines if the player is currently stuck on a wall.

		// Boomerang wall jumping variables.
		[HideInInspector]
		public bool boomerangJump;					// Determines if the player is performing the boomerang wall jump.
		private bool performBoomerangJump;			// Determines if the boomerang wall jump should be performed.
		private float boomerangTimer;				// Timer used to make the player move in the opposite direction of the wall before boomeranging.
		private float boomerangDirection;			// Used to remember the horizontal input when performing the wall jump.

		// runSlowdown and runSpeed private variables.
		private float runSlowdown;					// Determines the incremented slowdown for the wall running.
		private float runSlowdownSpeed;				// Determines the current wall run speed when slowing down.
		private float slideSpeedup;					// Determines the incremented speedup for the wall sliding.
		private float slideSpeedupSpeed;			// Determines the current wall slide speed when speeding up.

		// allowSameWall private variables.
		private bool onRightWall = false;			// Determines if the player is on the right wall.
		private bool onLeftWall = false;			// Determines if the player is on the left wall.
		private bool jumpedRightWall = false;		// Determines if the player jumped from the right wall.
		private bool jumpedLeftWall = false;		// Determines if the player jumped from the left wall.

		// Use this for initialization.
		void Start () {
			// Setting up references.
			player = GetComponent<Player>();
		}

		// This function is called every fixed framerate frame.
		void FixedUpdate () {
			// Cache the player speed and horizontal input.
			float speed = player.GetSpeed();

			// Make the player performs a boomerang wall jump.
			if (boomerangJump) {
				// Perform the jump once.
				if (performBoomerangJump) {
					performBoomerangJump = false;
					boomerangDirection = player.hor;
					player.SetYVelocity(0);
					PerformWallJump();
				}

				// If the boomerang wall jumping direction is different, stop the boomerang wall jump.
				if (player.hor != boomerangDirection) {
					boomerangJump = false;
				// Or else set the speed in the opposite direction.
				} else {
					player.SetXVelocity(speed * (player.hor * -1));
				}
			} else {
				// Set the velocity to make the player run on a wall.
				if (wallRunning) {
					if (!wallRun.infiniteRun && wallRun.runSlowdown) {
						player.SetYVelocity(runSlowdownSpeed);
					} else {
						player.SetYVelocity(wallRun.runSpeed);
					}
				}
				
				// Set the velocity to make the player slide down a wall.
				if (wallSliding) {
					if (wallSlide.slideSpeedup) {
						player.SetYVelocity(-slideSpeedupSpeed);
					} else {
						player.SetYVelocity(-wallSlide.slideSpeed);
					}
				}
				
				// Make the player stuck on the wall to wall jump.
				if (stuckToWall) {
					player.SetXVelocity(0);
					player.SetYVelocity(wallJump.wallStickVelocity);
				}
			}
		}
		
		// Update is called once per frame.
		void Update () {
			// Reset the wallRunning, wallSliding and stuckToWall variables.
			wallRunning = false;
			wallSliding = false;
			stuckToWall = false;

			// Reset the boomerang wall jump when the timer is completed.
			if (boomerangJump) {
				if (boomerangTimer > 0) {
					boomerangTimer -= Time.deltaTime;
				} else {
					boomerangJump = false;
				}
			}

			// Set runSlowdown and slideSpeedup.
			if (!wallRun.infiniteRun && wallRun.runSlowdown) {
				runSlowdown = (Time.deltaTime / wallRun.runTime) * wallRun.runSpeed;
			}
			if (wallSlide.slideSpeedup) {
				slideSpeedup = (Time.deltaTime / wallSlide.slideTime) * wallSlide.slideSpeed;
			}

			// If there can be interacted with the wall and the player is allowed to interact...
			if (((jumpToStick && !player.grounded) || (!jumpToStick && !player.crouching)) && (wallJump.enabled || wallRun.enabled || wallSlide.enabled) && !player.onLadder) {
				// The player is facing a wall if a linecast between the 'frontCheck' objects hits anything on the wall layer.
				Collider2D hitWall = Physics2D.OverlapArea(player.frontCheckTop.transform.position, player.frontCheckBot.transform.position, wallLayer);
				facingWall = hitWall != null;

				// Set the last wall when not on a wall.
				if (!facingWall && currentWall != 0 && !player.stuckToWall) {
					lastWall = currentWall;
					currentWall = 0;
				}

				// Only allow interaction with the wall when player isn't falling.
				if (!player.falling) {
					// If the player is facing a wall...
					if (facingWall) {
						// If the player is currently moving towards the wall...
						if ((player.facingRight && player.hor > 0) || (!player.facingRight && player.hor < 0)) {
							// If not stuck on the wall yet...
							if (!player.stuckToWall) {
								// ... disable horizontal movement.
								player.StuckToWall(true);
								
								// Reset the air dash limit.
								player.ResetAirDashLimit();

								// Stick the player to the wall, reset the wallStickTimer and wallUnstickTimer.
								wallStickTimer = wallJump.wallStickTime;
								wallUnstickTimer = wallUnstickTime;

								// Set which wall the player is using.
								onLeftWall = !player.facingRight;
								onRightWall = player.facingRight;
								
								// Remember the current wall.
								currentWall = hitWall.GetInstanceID();

								// Set jumps to 0 if wall jumping isn't allowed.
								if (!wallJump.enabled || !CanSameWallJump()) {
									player.SetJumps(0);
								}
							}

							// Handle wall running.
							if (wallRun.enabled) {
								HandleWallRunning();
							}

							// If not wall running...
							if (!wallRunning) {
								// Handle wall sliding.
								if (wallSlide.enabled) {
									HandleWallSliding();
								// If wall sliding is off and wall running is on and the player is not running...
								} else if (wallRun.enabled) {
									// ... if the player can wall jump on the same wall and the player is on the same wall again, start the wall unstick timer.
									if (wallJump.enabled && wallJump.allowSameWall && AgainstSameWall()) {
										RunWallUnstickTimer();
									// Or else make sure the player falls.
									} else {
										Unstick();
										Fall();
									}
								// Or else if the player can wall jump...
								} else if (wallJump.enabled) {
									RunWallUnstickTimer();
								}
							}

							// If boomerang wall jumping is allowed and the jump button is being pressed...
							if (wallJump.enabled && wallJump.boomerangJump && CanSameWallJump() && Input.GetButtonDown("Jump")) {
								// ... reset the boomerang jumping timer.
								boomerangTimer = wallJump.boomerangTime;
								// Make sure the boomerangJump and performBoomerangJump variables are set to true.
								boomerangJump = true;
								performBoomerangJump = true;
							}
						// Or else if the player can wall jump...
						} else if (wallJump.enabled && player.stuckToWall) {
							// ... run the unstick timer.
							RunWallUnstickTimer();
						}
					// Or else...
					} else if(player.stuckToWall) {
						// Handle wall jumping.
						if (wallJump.enabled) {
							HandleWallJumping();
						} else {
							if (player.stuckToWall && player.hor == 0)
								Fall();
							Unstick();
						}
					}
				}
			}

			// Set the wall variables used by the animator.
			player.SetWallAnimation(wallRunning, wallSliding, stuckToWall);

			// If the player is grounded...
			if (player.grounded) {
				// ... reset the allowSameWall variables when the player is grounded.
				onRightWall = false;
				onLeftWall = false;

				// Reset current and last wall.
				currentWall = 0;
				lastWall = 0;

				// Unstick the player.
				Unstick();
			}
		}

		// Handle the wall running.
		void HandleWallRunning() {
			// If the wallRunTimer is currently running and wall running is allowed...
			if (CanSameWallRun() && (wallRun.infiniteRun || (!wallRun.infiniteRun && wallRunTimer > 0))) {
				// Decrease the timer (if not infinite) and set wallRunning to true.
				if (!wallRun.infiniteRun) {
					wallRunTimer -= Time.deltaTime;
					// Slow down the speed if wallRunSlowdown is true.
					if (wallRun.runSlowdown) {
						runSlowdownSpeed -= runSlowdown;
					}
				}
				wallRunning = true;
			}
		}

		// Handle wall sliding.
		void HandleWallSliding() {
			// Make the player slide down the wall if it's allowed.
			if (CanSameWallSlide() && (wallSlide.infiniteSlide || (!wallSlide.infiniteSlide && wallSlideTimer > 0))) {
				// Decrease the wall sliding timer to make the player slide down the wall.
				if (!wallSlide.infiniteSlide) { 
					wallSlideTimer -= Time.deltaTime;
				}
				// Speed up the speed if wallSlideSpeedup is true.
				if (wallSlide.slideSpeedup && slideSpeedupSpeed < wallSlide.slideSpeed) {
					slideSpeedupSpeed += slideSpeedup;
				}
				wallSliding = true;
			// Or else make the player unsticky and fall down.
			} else {
				// If the player can wall jump on the same wall and the player is on the same wall again, start the wall unstick timer.
				if (wallJump.enabled && wallJump.allowSameWall && AgainstSameWall()) {
					RunWallUnstickTimer();
				// Or else make sure the player falls.
				} else {
					Unstick();
					Fall();
				}
			}
		}

		// Handle the wall jumping.
		void HandleWallJumping() {
			// If in the air, stuck on the wall and moving in the opposite direction...
			if (!player.grounded && player.stuckToWall && CanSameWallJump() && ((onRightWall && !player.facingRight && player.hor < 0) || (onLeftWall && player.facingRight && player.hor > 0))) {
				// ... start the wallStickTimer.
				if (wallStickTimer > 0) {
					wallStickTimer -= Time.deltaTime;
					stuckToWall = true;

					// When jumping while being stuck, make sure the jumps are reset and the player jumps.
					if(Input.GetButtonDown("Jump")) {
						PerformWallJump();
					}
				} else {
					// Make the player fall down when the timer is completed.
					Unstick();
					Fall();
				}
			// Or else make the player fall down (when not moving) and then unstick the player.
			} else {
				if (player.stuckToWall && player.hor == 0)
					Fall();
				Unstick();
			}
		}

		// Function to perform the wall jump. Resets several variables and performs a jump.
		void PerformWallJump() {
			stuckToWall = false;
			wallRunning = false;
			wallSliding = false;
			Unstick();
			isWallJumping = true;
			if (wallJump.allowDoubleJump) {
				player.ResetJumps();
			} else {
				player.SetJumps(1);
			}
			player.Jump();
		}

		// Function to unstick the player from the wall.
		void Unstick() {
			// Make the player no longer stuck to the wall.
			player.StuckToWall(false);
			
			// Set allowSameWall variables.
			jumpedLeftWall = onLeftWall;
			jumpedRightWall = onRightWall;

			// Reset wall running properties.
			if (wallRun.enabled && !wallRunning && !wallRun.infiniteRun) {
				// Reset the runSlowdownSpeed.
				if (wallRun.runSlowdown)
					runSlowdownSpeed = wallRun.runSpeed;
				
				// Reset the wallRunTimer.
				wallRunTimer = wallRun.runTime;
			}
			
			// Reset wall sliding properties.
			if (wallSlide.enabled && !wallSliding) {
				// Reset the slideSpeedupSpeed.
				if (wallSlide.slideSpeedup)
					slideSpeedupSpeed = 0f;
				
				// Reset the wallSlideTimer.
				wallSlideTimer = wallSlide.slideTime;
			}

			// Reset isWallJumping.
			isWallJumping = false;
		}

		// Fall down if shouldFall is true.
		void Fall() {
			if (shouldFall) {
				// Make sure the player is falling.
				player.Fall();
			}
		}

		// Function to run the wallUnstickTimer.
		void RunWallUnstickTimer() {
			// When the player is not moving, make sure the player is unstuck when the timer is completed.
			if (wallUnstickTimer > 0) {
				wallUnstickTimer -= Time.deltaTime;
			} else {
				Unstick();
				Fall();
			}
		}

		// Check to see if the player is on the same wall and is allowed to jump.
		bool CanSameWallJump() {
			return wallJump.allowSameWall || (!wallJump.allowSameWall && !AgainstSameWall());
		}

		// Check to see if the player is on the same wall and is allowed to run.
		bool CanSameWallRun() {
			return wallRun.allowSameWall || (!wallRun.allowSameWall && !AgainstSameWall());
		}

		// Check to see if the player is on the same wall and is allowed to slide.
		bool CanSameWallSlide() {
			return wallSlide.allowSameWall || (!wallSlide.allowSameWall && !AgainstSameWall());
		}

		// Check to see if the player is against the same wall.
		bool AgainstSameWall() {
			if ((onRightWall && jumpedRightWall) || (onLeftWall && jumpedLeftWall)) {
				return currentWall == lastWall;
			} else {
				return false;
			}
		}
	}
}