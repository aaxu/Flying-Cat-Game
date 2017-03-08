using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// This class handles the jumping through the platform. It uses the JumpThroughUp and JumpThroughDown classes.
	public class PlatformJumpThrough : MonoBehaviour {
		// Public variables hidden in inspector.
		[HideInInspector]
		public bool collisions = true;		// Check if the collisions are enabled for the platform.
		[HideInInspector]
		public bool insidePlatform = false;	// Check if the player is 'inside' the platform.

		// Public variables.
		[Tooltip("Select if the platform is the top of a ladder.")]
		public bool topOfLadder = false;
		[Tooltip("Select the platforms that should act as a jump through platform.")]
		public GameObject[] platforms;

		// Private variables.
		private GameObject player;					// Variable used for the player's game object.
		private Player playerScript;				// Get the player's script.
		private Collider2D[] cols;					// The player's colliders.
		private bool startEnableTimer = false;		// Used to determine if the countdown should run.
		private float enableTimer;					// Used to count down before the collisions are enabled.

		// Use this for initialization.
		void Start () {
			// Setting up references.
			player = GameObject.FindGameObjectWithTag("Player");
			playerScript = player.GetComponent<Player>();
			cols = player.GetComponents<Collider2D>();
		}

		// Update is called once per frame.
		void Update() {
			// If the enable timer is running...
			if (startEnableTimer) {
				// ... descrease the enable timer with Time.deltaTime.
				enableTimer -= Time.deltaTime;
				// If the timer is finished...
				if (enableTimer <= 0) {
					// ... stop the timer.
					startEnableTimer = false;
					// Enable the collisions between the player and the platforms.
					EnableCollisions();
				}
			}
		}

		// Function to enable all collisions.
		public void EnableCollisions() {
			// If there aren't any collisions...
			if (!collisions) {
				// ... loop through all player colliders.
				foreach (Collider2D col in cols) {
					// Loop through all platforms associated to the JumpThroughUp object.
					foreach (GameObject platform in platforms) {
						// Stop ignoring the collisions between the player and the platforms.
						Physics2D.IgnoreCollision(platform.GetComponent<Collider2D>(), col, false);
					}
				}

				// Set the collisions variable to true.
				collisions = true;
				
				// Make sure the player isn't jumping through a platform.
				playerScript.jumpingThrough = false;
			}
		}

		// Function to disable all collisions.
		public void DisableCollisions() {
			// Make sure the player is jumping through a platform when he's jumping.
			if (playerScript.jumping) { playerScript.jumpingThrough = true; }
			
			// If the enable timer is running...
			if (startEnableTimer) {
				// ... stop the enable timer.
				startEnableTimer = false;
			}

			// If the collisions are enabled...
			if (collisions) {
				// ... loop through all player colliders.
				foreach (Collider2D col in cols) {
					// Loop through all platforms associated to the JumpThroughUp object.
					foreach (GameObject platform in platforms) {
						// Ignore the collisions between the player and the platforms.
						Physics2D.IgnoreCollision(platform.GetComponent<Collider2D>(), col, true);
					}
				}
				
				// Set the collisions variable to false.
				collisions = false;
			}
		}

		// Function to set the enable timer and start it.
		public void StartEnableTimer() {
			enableTimer = 0.05f;
			startEnableTimer = true;
		}
	}
}