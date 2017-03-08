using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class used to make the player jump through the platform.
	public class JumpThroughDown : MonoBehaviour {
		// Private variables.
		private GameObject player;					// Variable used for the player's game object.
		private PlatformJumpThrough platform;		// The platform's main script.
		private Player playerScript;				// Variable used for the Player script.
		private bool canJumpDown = false;			// Determines if the player is allowed to jump down.

		// Use this for initialization.
		void Start() {
			// Setting up references.
			player = GameObject.FindGameObjectWithTag("Player");
			playerScript = player.GetComponent<Player>();
			platform = GetComponentInParent<PlatformJumpThrough>();
		}

		// Update is called once per frame.
		void Update () {
			// If the player is allowed to jump down...
			if (canJumpDown) {
				// Get the vertical input.
				float v = Input.GetAxis("Vertical");
				// If the vertical input is equal to -1 (moving down)...
				if (v == -1) {
					// If the player isn't grounded...
					if (!playerScript.grounded) {
						// ... make sure the player falls through the platform.
						JumpDown();
					// Or else if the platform is the top of a ladder...
					} else if (platform.topOfLadder) {
						// ... disable the collisions (no jumping).
						platform.DisableCollisions();
					// Or else...
					} else {
						// ... get the jump input.
						bool jump = Input.GetButton("Jump");
						// If the jump button is being pressed...
						if (jump) { 
							// ... make sure the player knows he can jump down.
							playerScript.jumpDown = true;
							// And then: jump!
							JumpDown();
						}
					}
				}
			}
		}

		// Function to jump down.
		void JumpDown() {
			playerScript.jumping = true;
			platform.DisableCollisions();
		}

		// Function to determine if the player is allowed to jump down.
		void OnTriggerEnter2D(Collider2D other) {
			if (other.gameObject.tag == "Player") {
				canJumpDown = true;
			}
		}
		
		// Function to determine if the player is allowed to jump down.
		void OnTriggerExit2D(Collider2D other) {
			if (other.gameObject.tag == "Player") {
				canJumpDown = false;
				playerScript.jumpDown = false;

				// If the player isn't inside the platform, start the enable timer.
				if (!platform.insidePlatform) {
					platform.StartEnableTimer();
				}
			}
		}
	}
}