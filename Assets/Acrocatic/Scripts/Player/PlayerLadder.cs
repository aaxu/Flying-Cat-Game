using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class that controls ladder movement.
	public class PlayerLadder : MonoBehaviour {
		// Public variables.
		[Header("General")]
		[Tooltip("Set the layer that should be used for ladders.")]
		public LayerMask ladderLayer;
		[Tooltip("Set the speed multiplier for snapping to the middle of the ladder. Setting this to a high value will make the player instantly snap to the middle of the ladder. Lower values will show a slower animation towards the middle.")]
		public float snapSpeed = 5f;
		[Header("Climbing speed")]
		[Tooltip("Set the player's speed on a ladder when moving up.")]
		public float climingSpeedUp = 3f;
		[Tooltip("Set the player's speed on a ladder when moving down.")]
		public float climingSpeedDown = 3f;
		[Tooltip("Set the player's speed on a ladder when moving to the left.")]
		public float climingSpeedLeft = 2f;
		[Tooltip("Set the player's speed on a ladder when moving to the right.")]
		public float climingSpeedRight = 2f;

		// Private variables.
		private Player player;						// Get the Player class.
		private BoxCollider2D boxCollider;			// Get the player's box collider.
		private Collider2D hitLadder;				// Remember the ladder the player is currently on.
		private Ladder ladder;						// Get the ladder's Ladder class (for the variables).
		private bool snappedToMiddle = false;		// Variable that determines if the player is snapped to the middle.

		// Use this for initialization.
		void Start () {
			// Setting up references.
			player = GetComponent<Player>();
			boxCollider = GetComponent<BoxCollider2D>();
		}

		// This function is called every fixed framerate frame.
		void FixedUpdate () {
			// Cache the vertical and horizontal input.
			float v = Input.GetAxis("Vertical");
			float h = Input.GetAxis("Horizontal");

			// If the player is on a ladder and the Ladder class is set...
			if (player.onLadder && ladder) {
				// Create the variables used to check if the player is almost falling off the ladder.
				bool stillOnLadder = true;
				Vector2 pos = transform.position;
				Vector2 pointA = new Vector2(pos.x + boxCollider.offset.x - (boxCollider.size.x / 2), pos.y + boxCollider.offset.y - (boxCollider.size.y / 2));
				Vector2 pointB = new Vector2(pos.x + boxCollider.offset.x + (boxCollider.size.x / 2), pos.y + boxCollider.offset.y + (boxCollider.size.y / 2));

				// If the player is moving down and movement down is allowed...
				if (v < 0 && ladder.allowDown) {
					// ... check if the player is almost falling off the ladder if the player isn't allowed to fall off.
					if (!ladder.fallOffBottom) {
						stillOnLadder = Physics2D.OverlapArea(pointA, new Vector2(pointB.x, pointB.y - 0.1f), ladderLayer);
					}

					// If the player isn't allowed to fall off and is almost falling off the ladder...
					if (!ladder.fallOffBottom && !stillOnLadder) {
						// ... make sure there is no Y movement.
						player.SetYVelocity(0);
					// Or else...
					} else {
						// ... set the player's Y velocity based on the climbing and default speed.
						player.SetYVelocity(v * (climingSpeedDown + (ladder.defaultSpeedY <= 0 ? ladder.defaultSpeedY : -ladder.defaultSpeedY)));
					}
				// Or else if the player is moving up and movement down is allowed...
				} else if (v > 0 && ladder.allowUp) {
					// ... check if the player is almost falling off the ladder if the player isn't allowed to fall off.
					if (!ladder.fallOffTop) {
						stillOnLadder = Physics2D.OverlapArea(new Vector2(pointA.x, pointA.y + 0.1f), pointB, ladderLayer);
					}

					// If the player isn't allowed to fall off and is almost falling off the ladder...
					if (!ladder.fallOffTop && !stillOnLadder) {
						// ... make sure there is no Y movement.
						player.SetYVelocity(0);
					// Or else...
					} else {
						// ... set the player's Y velocity based on the climbing and default speed.
						player.SetYVelocity(v * (climingSpeedUp + ladder.defaultSpeedY));
					}
				// Or else ...
				} else {
					// ... set the player's speed to the ladder's default speed.
					player.SetYVelocity(ladder.defaultSpeedY);
				}

				// If the player is grounded and is moving left or right...
				if (player.grounded && (h < 0 || h > 0)) {
					// ... unstick the player from the ladder.
					Unstick();
				// Or else if the player is snapped to the middle...
				} else if (ladder.snapToMiddle) {
					// ... if the player is snapped to the middle and allowed to fall off the ladder and moving left or right...
					if (snappedToMiddle && (v == 0) && ((ladder.fallOffLeft && h < 0) || (ladder.fallOffRight && h > 0))) {
						// ... unstick the player from the ladder.
						Unstick();
					// Or else...
					} else {
						// ... if the player is currently in the middle of the ladder...
						if (transform.position.x > (hitLadder.transform.position.x - 0.05f) && transform.position.x < (hitLadder.transform.position.x + 0.05f)) {
							// Make sure the X velocity is set to 0.
							player.SetXVelocity(0);
							// Make sure the player is snapped to the middle.
							snappedToMiddle = true;
						// Or else...
						} else {
							// ... calculate the velocity needed to move to the center of the ladder.
							Vector2 gotoCenter = (new Vector3(hitLadder.transform.position.x, transform.position.y, 0) - transform.position).normalized * snapSpeed;
							// Set the X velocity to this velocity.
							player.SetXVelocity(gotoCenter.x);
							// Make sure the player isnt't snapped to the middle.
							snappedToMiddle = false;
						}
					}
				// Or else...
				} else {
					// ... if the player is moving to the left and is allowed to move to the left...
					if (h < 0 && ladder.allowLeft) {
						// ... check if the player is almost falling off the ladder if the player isn't allowed to fall off.
						if (!ladder.fallOffLeft) {
							// 
							stillOnLadder = Physics2D.OverlapArea(pointA, new Vector2(pointB.x - 0.1f, pointB.y), ladderLayer);
						}

						// If the player isn't allowed to fall off and is almost falling off the ladder...
						if (!ladder.fallOffLeft && !stillOnLadder) {
							// ... make sure the X velocity is set to 0.
							player.SetXVelocity(0);
						// Or else...
						} else {
							// ... set the player's X velocity based on the climbing and default speed.
							player.SetXVelocity(h * (climingSpeedLeft + (ladder.defaultSpeedX <= 0 ? ladder.defaultSpeedX : -ladder.defaultSpeedX)));
						}
					// Or else if the player is moving to the right and is allowed to move to the right...
					} else if (h > 0 && ladder.allowRight) {
						// ... check if the player is almost falling off the ladder if the player isn't allowed to fall off.
						if (!ladder.fallOffRight) {
							stillOnLadder = Physics2D.OverlapArea(new Vector2(pointA.x + 0.1f, pointA.y), pointB, ladderLayer);
						}

						// If the player isn't allowed to fall off and is almost falling off the ladder...
						if (!ladder.fallOffRight && !stillOnLadder) {
							// ... make sure the X velocity is set to 0.
							player.SetXVelocity(0);
						// Or else...
						} else {
							// ... set the player's X velocity based on the climbing and default speed.
							player.SetXVelocity(h * (climingSpeedRight + ladder.defaultSpeedX));
						}
					// Or else...
					} else {
						// ... set the player's X velocity to the ladder's X default speed.
						player.SetXVelocity(ladder.defaultSpeedX);
					}
				}
			}
		}
		
		// Update is called once per frame.
		void Update () {
			// Check if the player's hitbox collides with a ladder.
			Vector2 pos = transform.position;
			Vector2 pointA = new Vector2(pos.x + boxCollider.offset.x - (boxCollider.size.x / 2), pos.y + boxCollider.offset.y - (boxCollider.size.y / 2));
			Vector2 pointB = new Vector2(pos.x + boxCollider.offset.x + (boxCollider.size.x / 2), pos.y + boxCollider.offset.y + (boxCollider.size.y / 2));
			hitLadder = Physics2D.OverlapArea(pointA, pointB, ladderLayer);

			// If the player isn't currently on a ladder, there is a ladder collission and the player isn't falling, dashing, crouching, sliding or stuck on a wall...
			if (!player.onLadder && hitLadder && !player.falling && !player.dashing && !player.crouching && !player.sliding && !player.stuckToWall) {
				// ... cache the vertical input.
				float v = Input.GetAxis("Vertical");

				// If the player isn't on a ladder and the vertical movement is either moving up or down...
				if (!player.onLadder && (v > 0.1 || v < -0.1)) {
					// ... make sure the player is stuck to the ladder.
					Stick();
				}
			} 

			// If there isn't a collission with a ladder and the player is considered on a ladder...
			if (!hitLadder && player.onLadder) {
				// ... make the player unstuck from the ladder.
				Unstick();
			}

			// If the player is on a ladder, there is a ladder object and the player is allowed to jump...
			if (player.onLadder && ladder && ladder.allowJump && Input.GetButtonDown("Jump")) {
				// ... unstick the player from the ladder and perform the jump.
				Unstick();
				player.Jump();
			}
		}

		// Stick to ladder.
		void Stick() {
			// Put the Ladder class of the ladder in the ladder variable.
			ladder = hitLadder.GetComponent<Ladder>();

			// Make sure the player is considered on a ladder.
			player.OnLadder();

			// If the player is allowed to jump...
			if (ladder.allowJump) {
				// ... if the player is allowed to double jump...
				if (ladder.allowDoubleJump) {
					// ... reset the amount of jumps that can be performed.
					player.ResetJumps();
				// Or else...
				} else {
					// ... set the total jumps allowed to 1.
					player.SetJumps(1);
				}
			// Or else...
			} else {
				// ... set the total amount of jumps to 0.
				player.SetJumps(0);
			}
		}

		// Unstick from ladder.
		void Unstick() {
			player.onLadder = false;
			ladder = null;
			snappedToMiddle = false;
		}
	}
}