using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Enums for all classes.
	public enum PlatformTypes { None, Normal, Moving, Sinking, JumpThrough };

	// This class controls all player behaviour on platforms.
	public class PlayerPlatform : MonoBehaviour {
		// Public variables that shouldn't be shown in the inspector.
		[HideInInspector]
		public GameObject currentPlatform;			// Current platform's game object the player is standing on.
		[HideInInspector]
		public Platform platformClass;				// Get the current platform's script.

		// Moving platform variables for the player.
		[System.Serializable]
		public class MovingPlatform {
			[Tooltip("Make the player stick to the X movement of a moving platform.")]
			public bool stickToX = true;
			[Tooltip("Make the player stick to the Y movement of a moving platform.")]
			public bool stickToY = true;
			[Tooltip("Use the friction of the platform when stuck to the X movement of the platform. Use this to create 'icy' moving platforms.")]
			public bool useFriction = true;
			[Tooltip("Keep the moving platform's X velocity when you perform a jump. Disabling this will make the player jump straight up while on a moving platform.")]
			public bool keepSpeedOnJump = true;
		}

		// Public variables.
		[Tooltip("All variables related to moving platforms are located here.")]
		public MovingPlatform movingPlatform;

		// Private variables.
		private Player player;						// Get the Player class.
		private Collider2D platformCollider;		// Platform collider object.
		private bool unstick = false;				// Make the player not stick to the platform.
		private float unstickTime = 0.1f;			// The time the player shouldn't be stuck to the platform.
		private float unstickTimer;					// The timer for being unstuck.
		private float playerFriction;				// Get the player's friction.
		private float platformFriction;				// Get the platform's friction.
		private float slowdown;						// Determines the slowdown based on the friction of the player and platform.
		private float totalSlowdown = 0;			// Calculate the slowdown based on the friction of the platform. This is used to simulate the friction.
		private bool stoppedMoving = false;			// This is used to check if the player stopped moving.
		private float previousHor = 0;				// Remember the previous horizontal input.
		private float lastXVel;						// Get the previous X velocity.

		// Use this for initialization.
		void Start () {
			// Setting up references.
			player = GetComponent<Player>();
		}
		
		// Update is called once per frame.
		void Update () {
			// Check if the player is on a platform.
			platformCollider = Physics2D.OverlapCircle(player.groundCheck.position, player.groundRadius, player.platformLayer);
			
			// If the player is on a platform and not jumping through it...
			if (platformCollider && !currentPlatform && !player.jumpingThrough) {
				// ... set the current platform.
				currentPlatform = platformCollider.transform.parent.transform.parent.gameObject;

				// Set the player's rotation based on the current platform's rotation if rotateOnSlope is enabled.
				if (player.rotateOnSlope) { transform.rotation = currentPlatform.transform.rotation; }

				// Make sure the player is considered grounded.
				player.groundCollider = platformCollider;

				// Get the platform's script.
				platformClass = currentPlatform.GetComponent<Platform>();

				// Shake the camera when on a sinking platform if camera shaking is enabled.
				if (platformClass.PlatformTypeIs(PlatformTypes.Sinking)) {
					platformClass.ShakeCamera();
				}
				
				// If the player is on a moving platform and should use the platform's friction...
				if (platformClass.PlatformTypeIs(PlatformTypes.Moving) && movingPlatform.useFriction) {
					// ... set the friction of the platform.
					platformFriction = platformCollider.gameObject.GetComponent<BoxCollider2D>().sharedMaterial.friction;
					
					// Set the friction of the player.
					playerFriction = GetComponent<BoxCollider2D>().sharedMaterial.friction;
					
					// Set the slowdown based on the friction from both the player and platform.
					slowdown = platformFriction * playerFriction;
				}

				// Let the platform know that the player is standing on it.
				platformClass.SetPlayerOnPlatform(true);
			// Or else...
			} else if (!platformCollider && currentPlatform) {
				// .. let the platform know the player isn't standing on it anymore.
				platformClass.SetPlayerOnPlatform(false);

				// Reset the variables.
				currentPlatform = null;
				platformClass = null;
				unstick = false;
			}

			// Timer for resetting the unstick variable.
			if (unstick) {
				if (unstickTimer > 0) {
					unstickTimer -= Time.deltaTime;
				} else {
					unstick = false;
				}
			}
		}
		
		// This function is called every fixed framerate frame.
		void FixedUpdate() {
			// Make the player sticky to the moving platform.
			if (OnMovingPlatform()) {
				// Cache the platform's velocity.
				Vector2 platformVelocity = platformClass.rigidbody.velocity;
				
				// If the player should stick to the X velocity of the platform...
				if (movingPlatform.stickToX && !unstick) {
					// ... cache the player's X velocity.
					float xVel = player.rigidbody.velocity.x;
					// Get the player's speed.
					float speed = player.GetSpeed(false);
					// Set the min and max velocity.
					float minVel = platformVelocity.x;
					float maxVel = platformVelocity.x;

					// If the player should stop...
					if (player.hor == 0) {
						// If the player should use the friction of the platform...
						if (movingPlatform.useFriction) {
							// Simulate friction on the platform.
							// Doesn't work as good as non-moving platforms. 
							// If you have a better solution, let us know: support@battlebrothers.io.

							// If stoppedMoving isn't set to true yet...
							if (!stoppedMoving) {
								// ... set stoppedMoving to true.
								stoppedMoving = true;
								// Set the total slowdown to the player's current X velocity.
								totalSlowdown = Mathf.Abs(platformVelocity.x - lastXVel);
							}

							// If the total slowdown is below 0 (and the player should stop moving)...
							if (totalSlowdown <= 0) {
								// ... set the previousHor variable to 0.
								previousHor = 0;
							}

							// If the previousHor variable isn't 0 (so the player stopped moving, but hasn't fully stopped yet due to friction)...
							if (previousHor != 0) {
								// ... decrease the total slowdown by the slowdown set by the frictions of the player and platform.
								totalSlowdown -= slowdown;
								// If the player was moving to the right...
								if (previousHor == 1) {
									// ... make sure the player's velocity is the same as the platform's plus the total slowdown.
									player.SetXVelocity(platformVelocity.x + totalSlowdown);
								// Or else if the player was moving to the left...
								} else if (previousHor == -1) {
									// ... make sure the player's velocity is the same as the platform's minus the total slowdown.
									player.SetXVelocity(platformVelocity.x - totalSlowdown);
								}
							// Or when the previousHor variable is 0 (so the player actually stopped moving, because totalSlowdown is below 0)...
							} else {
								// ... set the player's X velocity to that of the platform, so the player doesn't fall off.
								player.SetXVelocity(platformVelocity.x);
							}
						// Or else...
						} else {
							// Directly set the player's velocity to the platform's velocity.
							player.SetXVelocity(platformVelocity.x);
						}
					// Or else if the player is moving...
					} else {
						// ... make sure the player sticks to the platform during movement.

						// If the player should use the platform's friction...
						if (movingPlatform.useFriction) {
							// ... make sure stoppedMoving is false and remember the previous horizontal input.
							stoppedMoving = false;
							previousHor = player.hor;
						}

						// Set the minimum and maximum velocity based on the player's and platform's direction.
						if ((platformVelocity.x < 0 && player.hor > 0) || (platformVelocity.x > 0 && player.hor > 0)) {
							minVel = platformVelocity.x;
							maxVel = platformVelocity.x + speed;
						} else if ((platformVelocity.x < 0 && player.hor < 0) || (platformVelocity.x > 0 && player.hor < 0)) {
							minVel = platformVelocity.x - speed;
							maxVel = platformVelocity.x;
						}

						// Remember the current X velocity on the platform.
						lastXVel = platformVelocity.x + (player.hor * speed);
						// Set the player's velocity to this velocity if the player's current velocity is lower than the minimum or higher than the maximum.
						if(xVel > maxVel || xVel < minVel) {
							player.SetXVelocity(lastXVel);
						}
					}
				}

				// If the player should stick to the Y velocity of the platform...
				if (movingPlatform.stickToY && !unstick) {
					// ... set the Y velocity based on the platform's Y velocity.
					player.SetYVelocity(platformVelocity.y);
				}
			}
		}

		// Function to unstick the player from the platform.
		public void Unstick() {
			unstickTimer = unstickTime;
			unstick = true;
		}

		// Return if the player is stuck to the platform.
		public bool IsStuckToPlatform() {
			return OnPlatform() && !unstick;
		}

		// Return if the player is stuck to the X speed of the platform.
		public bool IsStuckToPlatformX() {
			return OnPlatform() && !unstick && movingPlatform.stickToX;
		}

		// Return if the player is on a platform.
		public bool OnPlatform() {
			return currentPlatform && !platformClass.PlatformTypeIs(PlatformTypes.None);
		}

		// Return if the player is on a moving platform.
		public bool OnMovingPlatform() {
			return OnPlatform() && platformClass.PlatformTypeIs(PlatformTypes.Moving);
		}
		
		// Return if the player is on a sinking platform and the platform is sinking.
		public bool OnSinkingPlatform() {
			return OnPlatform() && platformClass.PlatformTypeIs(PlatformTypes.Sinking) && platformClass.IsSinking();
		}

		// Return if the player should keep the platform's speed when jumping.
		public bool KeepSpeedOnJump() {
			return movingPlatform.keepSpeedOnJump;
		}
	}
}