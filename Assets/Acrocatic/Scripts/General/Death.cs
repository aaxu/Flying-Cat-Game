using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class that handles the player's death.
	public class Death : MonoBehaviour {
		// Private variables.
		private bool isDead = false;
		private float deathTimer = 1f;

		// Check if the player is entering the trigger.
		void OnTriggerEnter2D(Collider2D other) {
			if (other.gameObject.tag == "Player" && !isDead) {
				// Make sure the player is dead.
				isDead = true;

				// Make sure the player knows he's dead.
				other.gameObject.GetComponent<Player>().Dead();
			}
		}

		void Update() {
			// If the player is dead...
			if (isDead) {
				// ... run a timer.
				if (deathTimer > 0) {
					deathTimer -= Time.deltaTime;
				} else {
					// When the timer is complete: reload the same level.
					Application.LoadLevel(Application.loadedLevelName);
				}
			}

		}
	}
}