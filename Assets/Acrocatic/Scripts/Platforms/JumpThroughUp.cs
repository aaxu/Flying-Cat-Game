using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class used to make the player jump through the platform.
	public class JumpThroughUp : MonoBehaviour {
		// Private variables.
		private PlatformJumpThrough platform;		// The platform's main script.

		// Use this for initialization.
		void Start() {
			// Setting up references.
			platform = GetComponentInParent<PlatformJumpThrough>();
		}
		
		// Make sure the player and platform can't collide with each other, allowing the player to jump through the platform.
		void OnTriggerStay2D(Collider2D other) {
			// If the collider is the player's trigger...
			if (other.gameObject.tag == "Player") {
				// ... disable the collisions.
				DisableCollisions();
				// Make sure the player is considered 'inside' the platform.
				platform.insidePlatform = true;
			}
		}

		// Make sure the player and platform can collide with each other again.
		void OnTriggerExit2D(Collider2D other) {
			// If the collider is the player's trigger...
			if (other.gameObject.tag == "Player") {
				// ... start the enable timer.
				platform.StartEnableTimer();
				// Make sure the player is considered 'outside' the platform.
				platform.insidePlatform = false;
			}
		}

		// Function to disable all collisions between the platforms and the player.
		void DisableCollisions() {
			// Disable the collisions.
			platform.DisableCollisions();
		}
	}
}
