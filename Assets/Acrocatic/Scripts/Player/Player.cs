using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Enums for all classes.
	public enum Direction { Left, Right };

	// This class is used to communicate between the different Player movement classes.
	public class Player : MonoBehaviour {
		// Public variables that shouldn't be shown in the inspector.
		[HideInInspector]
		public float hor;							// Get the Horizontal input.
		[HideInInspector]
		public bool facingRight;					// For determining which way the player is currently facing.
		[HideInInspector]
		public bool grounded = true;				// Whether or not the player is grounded.
		[HideInInspector]
		public bool walking = false;				// Determines if the player is walking.
		[HideInInspector]
		public bool stuckToWall = false;			// Used to stop the player from moving when stuck to a wall.
		[HideInInspector]
		public bool falling = false;				// Determines if the player is falling.
		[HideInInspector]
		public bool dashing = false;				// Determines if the player is dashing.
		[HideInInspector]
		public bool crouching = false;				// Determines if the player is crouching.
		[HideInInspector]
		public bool sliding = false;				// Determines if the player is sliding.
		[HideInInspector]
		public bool onLadder = false;				// Determines if the player is on a ladder.
		[HideInInspector]
		public GameObject frontCheckTop;			// A position marking to check if the player's front.
		[HideInInspector]
		public GameObject frontCheckBot;			// A position marking to check if the player's front.
		[HideInInspector]
		public Transform groundCheck;				// A position marking to check if the player is grounded.
		[HideInInspector]
		public Collider2D groundCollider;			// Ground collider object.
		[HideInInspector]
		public bool jumpDown = false;				// Determines if the player is about to jump down.
		[HideInInspector]
		public bool jumping = false;				// Determines if the player is jumping.
		[HideInInspector]
		public bool jumpingThrough = false;			// Determines if the player is jumping through a platform.
		[HideInInspector]
		public Rigidbody2D rigidbody;				// Cache the rigidbody of the player.
		
		// Public variables.
		[Tooltip("Select the direction in which the sprites are facing.")]
		public Direction spriteDirection;
		[Tooltip("Select the layer that should be used for platforms.")]
		public LayerMask platformLayer;
		[Tooltip("Select the layer that should be used for the ground.")]
		public LayerMask groundLayer;
		[Tooltip("Set the radius for the ground check. This creates a circle at the feet of the player, checking if anything from the ground or platform layer collides with the circle.")]
		public float groundRadius = 0.12f;
		[Tooltip("Enable or disable player rotation when on a rotated platform (slope).")]
		public bool rotateOnSlope = true;
		[Tooltip("Enable or disable player movement after death.")]
		public bool moveAfterDeath = true;
		[Tooltip("You can enable this to keep the player's velocity when hitting the ground. This is experimental and currently defaults to false.")]
		public bool keepVelocityOnGround = false;
		[Tooltip("The timer used for the remembering the player's velocity when hitting the ground. Don't give this a high value.")]
		public float groundedVelocityTime = 0.05f;

		// Private variables.
		private PlayerJump playerJump;				// Get the PlayerJump class.
		private PlayerWall playerWall; 				// Get the PlayerWall class.
		private PlayerRun playerRun; 				// Get the PlayerRun class.
		private PlayerCrouch playerCrouch; 			// Get the PlayerCrouch class.
		private PlayerDash playerDash; 				// Get the PlayerDash class.
		private PlayerHitbox playerHitbox; 			// Get the PlayerHitbox class.
		private PlayerPlatform playerPlatform;		// Get the PlayerPlatform class.
		private Quaternion normalRotation;			// The default rotation for the player.
		private Animator animator;					// The player's animator.
		private bool flipAgain = false;				// Used to fix a bug.
		private float gravityScale;					// Cache the initial gravity scale.
		private bool isDead = false;				// Check if the player is dead.
		private float groundedXVelocity;			// Remember the X velocity when the player hits the ground.
		private float groundedTimer;				// Timer for how long the player should ignore the friction when hitting the ground.

		// Wall variables.
		private bool wallSliding = false;			// These variables are used to remember the wall sliding, running and jumping variables.
		private bool wallRunning = false;			// That's needed to show the correct animations.
		private bool wallJumping = false;

		// Use this for initialization.
		void Start () {
			// Setting up references.
			rigidbody = GetComponent<Rigidbody2D>();
			groundCheck = transform.Find("groundCheck");
			frontCheckTop = new GameObject("frontCheckTop");
			frontCheckBot = new GameObject("frontCheckBot");
			playerJump = GetComponent<PlayerJump>();
			playerWall = GetComponent<PlayerWall>();
			playerRun = GetComponent<PlayerRun>();
			playerCrouch = GetComponent<PlayerCrouch>();
			playerDash = GetComponent<PlayerDash>();
			playerHitbox = GetComponent<PlayerHitbox>();
			playerPlatform = GetComponent<PlayerPlatform>();
			normalRotation = transform.localRotation;
			animator = GetComponent<Animator>();
			gravityScale = rigidbody.gravityScale;
			
			// Set the frontCheck and backCheck transform parent to the player's transform.
			frontCheckTop.transform.parent = transform;
			frontCheckBot.transform.parent = transform;

			// Check which direction the player is facing based on the spriteDirection and flip when the spriteDirection is Left.
			if (spriteDirection == Direction.Right) {
				facingRight = true;
			} else {
				facingRight = false;
				Flip();
			}

			// Change the side marker positions.
			playerHitbox.ChangeSidePositions();
		}

		// This function is called every fixed framerate frame.
		void FixedUpdate() {
			// Set the player's hitbox.
			if (grounded && !dashing) {
				if (crouching) {
					ChangeHitbox("crouch");
				} else {
					ChangeHitbox("default");
				}
			} else {
				ChangeHitbox("jump");
			}

			// If the player is stuck to a wall or on a ladder.
			if (stuckToWall || onLadder) {
				// ... set the gravity scale to 0.
				rigidbody.gravityScale = 0;
			} else {
				// Reset the gravity scale.
				rigidbody.gravityScale = gravityScale;
			}

			// If the groundedXVelocity is active and keepVelocityOnGround is activated...
			if (keepVelocityOnGround && groundedXVelocity != 0) {
				// ... set the player's X velocity to the initial velocity when hitting the ground.
				SetXVelocity(groundedXVelocity);
			}
		}

		// Update is called once per frame.
		void Update() {
			// Cache the horizontal input.
			hor = Input.GetAxis("Horizontal");

			// Set the animator values.
			animator.SetBool("grounded", grounded);
			animator.SetBool("walking", walking);
			animator.SetBool("crouching", crouching);
			animator.SetBool("sliding", sliding);
			animator.SetBool("dashing", dashing);
			animator.SetBool("falling", falling);
			animator.SetBool("wall", stuckToWall);
			animator.SetBool("onLadder", onLadder);
			animator.SetBool("jumpingThrough", jumpingThrough);
			animator.SetFloat("horizontal", Mathf.Abs(hor));
			animator.SetFloat("xSpeed",  Mathf.Abs(rigidbody.velocity.x));
			animator.SetFloat("ySpeed", rigidbody.velocity.y);

			// The player is grounded if a circle at the groundcheck position overlaps anything on the ground layer.
			// Only perform the check if the player is not on a platform.
			if (!OnPlatform()) {
				groundCollider = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
			}

			// If the groundedXVelocity is higher than 0 and keepVelocityOnGround is activated...
			if (keepVelocityOnGround && groundedXVelocity != 0) {
				// ... run the timer.
				if (groundedTimer > 0) {
					groundedTimer -= Time.deltaTime;
				} else {
					// Make sure the groundedXVelocity is set to 0.
					groundedXVelocity = 0;
				}
			}

			// If player is grounded and not jumping through a platform...
			if (groundCollider && !jumpingThrough) {
				// If the player isn't grounded yet and keepVelocityOnGround is activated...
				if (keepVelocityOnGround && !grounded) {
					// ... set the groundedXVelocity to the current X velocity.
					groundedXVelocity = rigidbody.velocity.x;
					// Start the groundedTimer.
					groundedTimer = groundedVelocityTime;
				}

				// Set grounded to true.
				grounded = true;
				// Set jumping to false.
				jumping = false;
				// Rotate player to collider's position if not on a platform.
				if (rotateOnSlope && !OnPlatform()) { transform.rotation = groundCollider.transform.localRotation; }
				// Call the stateComplete trigger if the player was falling.
				if (falling) {
					animator.SetTrigger("stateComplete");
				}
				falling = false;
			// Or else...
			} else {
				// ... rotate player to original position and set grounded to true.
				transform.rotation = normalRotation;
				grounded = false;
			}

			// If the player is moving in the opposite direction compared to the direction the player is facing...
			if((hor > 0 && !facingRight) || (hor < 0 && facingRight) || flipAgain) {
				// ... flip the player.
				Flip();
			}
		}

		// Make sure the player is dead.
		public void Dead() {
			isDead = true;
			SetJumps(0);
			falling = true;
			animator.SetTrigger("startFalling");

			// Disable player movement if it isn't allowed after death.
			if (!moveAfterDeath) { 
				GetComponent<PlayerRun>().enabled = false;
			}
		}

		// ################
		// ### MOVEMENT ###
		// ###          ###
		// Function to flip the character.
		void Flip() {
			// Only flip the player when not dashing or sliding and the player isn't dead and not allowed to move while dead.
			if (!dashing && !sliding && !(isDead && !moveAfterDeath)) {
				flipAgain = false;

				// These if-statements are used to fix a bug where the player would be flipped before changing the animation.
				// This caused a weird flicker.
				AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
				if (stuckToWall && (info.IsName("PlayerWallRun") || info.IsName("PlayerWallSlide"))) {
					flipAgain = true;
					if (playerWall.wallJump.enabled) {
						animator.CrossFade("PlayerWallJump", 0f);
					} else {
						animator.CrossFade("Jump/Fall", 0f);
					}
				} else if (!stuckToWall && info.IsName("PlayerWallJump")) {
					flipAgain = true;
				} else {
					// Switch the way the player is labelled as facing.
					facingRight = !facingRight;
					
					// Multiply the player's x local scale by -1.
					Vector3 theScale = transform.localScale;
					theScale.x *= -1;
					transform.localScale = theScale;
				}
			}
		}

		// Set the X velocity for the player.
		public void SetXVelocity(float xVel) {
			// If the player isn't grounded.
			if (!grounded) {
				// Check if the player is wall jumping and reset the variable if the player is wall jumping.
				bool isWallJumping = playerWall && playerWall.wallJump.enabled && playerWall.isWallJumping;
				if (isWallJumping)
					playerWall.isWallJumping = false;
			}

			// Set X velocity.
			rigidbody.velocity = new Vector2(xVel, rigidbody.velocity.y);
		}

		// Set the Y velocity for the player.
		public void SetYVelocity(float yVel) {
			rigidbody.velocity = new Vector2(rigidbody.velocity.x, yVel);
		}

		// Check if the player is allowed to change between walking and running while in the air.
		public bool CanWalkAndRunInAir() {
			if (playerJump) {
				return playerJump.airMovement.walkAndRun;
			} else {
				return true;
			}
		}

		// Check if the player was walking or running before jumping.
		public bool AirWalk() {
			return playerJump && playerJump.walkingOnJump;
		}

		// Get the player's air movement factor.
		public float GetAirSpeedFactor() {
			if (playerJump) {
				return playerJump.airMovement.speedFactor;
			} else {
				return 1.0f;
			}
		}

		// Function to set the walking variable.
		public void Walk(bool walk) {
			// Don't change the walking variable if the player is jumping and can't change from walking to running in the air.
			if (!grounded && playerJump && !playerJump.airMovement.resetOnWall && !CanWalkAndRunInAir())
				return;
			
			walking = walk;
		}

		// Get the speed.
		public float GetSpeed(bool withPlatform = true) {
			return playerRun.GetSpeed(withPlatform);
		}

		// Get the speed on a moving platform.
		public float GetSpeedOnMovingPlatform(float speed) {
			// If player is on a moving platform...
			if (OnMovingPlatform()) {
				// ... get the platform's X velocity.
				float xVel = GetPlatform().GetComponent<Rigidbody2D>().velocity.x;
				// If the player is moving left while the platform is moving to the left or when the player is moving to the right and the platform is moving to the right...
				if ((hor < 0 && xVel < 0) || (hor > 0 && xVel > 0)) {
					// ... add the platform's velocity to the speed of the player.
					speed = Mathf.Abs(xVel) + speed;
				// Or else if the player is moving to the right while the platform is moving to the left or when the player is moving to the left and the platform is moving to the right...
				} else if ((hor > 0 && xVel < 0) || (hor < 0 && xVel > 0)) {
					// ... make sure the player's speed is the platform's speed minus the normal speed.
					speed = Mathf.Abs(Mathf.Abs(xVel) - speed);
				} else if (hor == 0) {
					speed = Mathf.Abs(xVel);
				}
			}

			// Return the new speed.
			return speed;
		}

		// Get the player's speed in the air if movement in the air is using force.
		public float GetSpeedInAir(float speed) {
			// If the player isn't grounded.
			if (!grounded) {
				// Change the speed based on the boomerang X factor when the player is boomerang wall jumping.
				if (playerWall && playerWall.boomerangJump) {
					speed = speed * playerWall.wallJump.boomerangFactorX;
				// Or else change the speed based on the speed factor in the air.
				} else {
					speed = speed * GetAirSpeedFactor();
				}
			}

			// Return the speed.
			return speed;
		}

		// Use the change factor to determine the amount of control for the player.
		public float GetMovementForce(float force) {
			// If the player isn't grounded, not wall jumping and the force should be changed by a factor...
			if (!grounded && playerJump && playerJump.airMovement.changeFactor != 1.0 && !(playerWall && playerWall.wallJump.enabled && playerWall.isWallJumping)) {
				// ... set the new force.
				force = force * playerJump.airMovement.changeFactor;
			}
			// Return the force.
			return force;
		}
		// ###          ###
		// ################


		// #################
		// ### PLATFORMS ###
		// ###           ###
		// Unstick the player from the platform.
		public void UnstickFromPlatform() {
			playerPlatform.Unstick();
		}

		// Get the current platform.
		public GameObject GetPlatform() {
			return playerPlatform.currentPlatform;
		}

		// Check if the player is on a platform.
		public bool OnPlatform() {
			return playerPlatform.OnPlatform();
		}

		// Check if the player is on a moving platform.
		public bool OnMovingPlatform() {
			return playerPlatform.OnMovingPlatform();
		}
		
		// Check if the player is on a sinking platform.
		public bool OnSinkingPlatform() {
			return playerPlatform.OnSinkingPlatform();
		}

		// Check if the player is stuck to the platform.
		public bool IsStuckToPlatform() {
			return playerPlatform.IsStuckToPlatform();
		}
		
		// Check if the player is on a platform where the player is stuck to the X velocity.
		public bool IsStuckToPlatformX() {
			return playerPlatform.IsStuckToPlatformX();
		}

		// Return if the player should keep its speed when jumping from the platform.
		public bool KeepSpeedOnJump() {
			return playerPlatform.KeepSpeedOnJump();
		}
		// ###          ###
		// ################
		// ###          ###
		// ################


		// ###############
		// ### JUMPING ###
		// ###         ###
		// Change the total amount of jumps.
		public void SetJumps(int jumps) {
			if (playerJump) { playerJump.jumps = jumps; }
		}

		// Reset the total amount of jumps to the default value.
		public void ResetJumps() {
			if (playerJump) { SetJumps(playerJump.doubleJumping.totalJumps); }
		}

		// Make the player jump.
		public void Jump() {
			if (playerJump) { playerJump.InitJump(); }
		}

		// Make the player fall down.
		public void Fall() {
			SetJumps(0);
			falling = true;
			animator.SetTrigger("startFalling");
		}

		// Get the jump factor for the player.
		public float GetJumpFactor() {
			// Get the boomerang jump Y factor if the player is boomerang jumping.
			if (playerWall && playerWall.boomerangJump) {
				return playerWall.wallJump.boomerangFactorY;
			// Or else just return 1.0.
			} else {
				return 1.0f;
			}
		}
		// ###         ###
		// ###############


		// ############
		// ### WALL ###
		// ###      ###
		// Set the stuckToWall variable to make sure the player is stuck against a wall.
		public void StuckToWall(bool stuck) {
			stuckToWall = stuck;
		}

		// Set the wall sliding variables used by the animator.
		public void SetWallAnimation(bool run, bool slide, bool jump) {
			// Call the trigger 'stateComplete' when wall interaction is finished.
			if (((wallRunning && !run) || (wallSliding && !slide) || (wallJumping && !jump)) && !run && !slide && !jump) {
				animator.SetTrigger("stateComplete");
			}

			// Trigger the wall running animation when the player is wall running.
			if (run && !wallRunning) {
				animator.SetTrigger("startWallRun");
			}
			
			// Trigger the wall sliding animation when the player is wall sliding.
			if (slide && !wallSliding) {
				animator.SetTrigger("startWallSlide");
			}
			
			// Trigger the wall jumping animation when the player is wall jumping.
			if (jump && !wallJumping) {
				animator.SetTrigger("startWallJump");
			}

			// Set the wall interaction variables to remember what the player is doing.
			wallRunning = run;
			wallSliding = slide;
			wallJumping = jump;

			// Set the animator values for all wall interaction.
			animator.SetBool("wallRunning", run);
			animator.SetBool("wallSliding", slide);
			animator.SetBool("wallJumping", jump);
		}
		// ###      ###
		// ############


		// ###############
		// ### DASHING ###
		// ###         ###
		// Function to set the dashing variable.
		public void Dash(bool dash) {
			// Call the trigger 'stateComplete' when dashing is finished.
			if (dashing && !dash) {
				animator.SetTrigger("stateComplete");
			// Start the dash when dashing.
			} else if (dash && !dashing) {
				animator.SetTrigger("startDash");
			}
			dashing = dash;
		}

		// Function to reset the air dashing limit.
		public void ResetAirDashLimit() {
			if (playerDash) { playerDash.ResetAirDashLimit(); }
		}
		// ###         ###
		// ###############

		// ##############
		// ### HITBOX ###
		// ###        ###
		// Function to change the player's hitbox.
		public void ChangeHitbox(string type) {
			if (playerHitbox) { playerHitbox.ChangeHitbox(type); }
		}

		// Function to check if the player is allowed to stand up.
		public bool AllowedToStandUp() {
			return playerHitbox ? playerHitbox.AllowedToStandUp() : true;
		}
		// ###        ###
		// ##############

		// #################
		// ### CROUCHING ###
		// ###           ###
		// Function to set the crouching variable based on if the player is crouching.
		public void Crouch(bool crouch) {
			crouching = crouch;
		}
		// Function to set the sliding variable based on if the player is sliding.
		public void Slide(bool slide) {
			// Call the trigger 'startSlide' when sliding.
			if (!sliding && slide) {
				animator.SetTrigger("startSlide");
			}
			sliding = slide;
		}
		// Function to get the crouch speed.
		public float GetCrouchSpeed() {
			if (playerCrouch) {
				return playerCrouch.crouchSpeed;
			} else {
				return 0;
			}
		}
		// ###           ###
		// #################

		// #################
		// ### CROUCHING ###
		// ###           ###
		public void OnLadder() {
			onLadder = true;
			animator.SetTrigger("triggerLadder");
		}
		// ###           ###
		// #################
	}
}
