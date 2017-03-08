using UnityEngine;
using System.Collections;

namespace Acrocatic {
// This class is used for moving platforms.
	public class PlatformMove : MonoBehaviour {
		// Public variables.
		[Tooltip("Set the platform's speed while moving.")]
		public float speed = 2;
		[Tooltip("This allows you to control if the platform should stop.")]
		public bool stop = false;
		[Tooltip("This allows you to only make the platform move while standing on it. Disabling this variable will make the platform move either way.")]
		public bool moveOnHit = false;
		[Tooltip("Make the platform loop through all waypoints. If this is disabled and the platform is at the last waypoint, the platform will move backwards towards the previous waypoints.")]
		public bool loop = true;
		[Tooltip("This allows you to specify if the platform should move forward or backwards through the waypoints. When looping is disabled, the platform will automatically move backwards when the last waypoint is reached.")]
		public bool moveForward = true;
		[Tooltip("This allows you to specify which waypoint should be the first waypoint the platform has to move towards.")]
		public int currentIndex = 0;
		[Tooltip("This allows you to control when the platform should stop. You can set the total amount of waypoints that need to be passed before the platform stops.")]
		public int stopAfterTotal = 0;
		[Tooltip("This allows you to control how long the platform stops at a waypoint. Default is 0 (directly moves to next waypoint).")]
		public float waitAtWaypointTime = 0;
		[Tooltip("Select all the waypoints that should be used for the platform's movement. You can drag the waypoint from the Hierarchy. Make sure you add the waypoints in the right order.")]
		public GameObject[] waypoints;

		// Private variables.
		private Platform platform;							// Get the Platform class.
		private bool movingForward = true;					// Determines if the platform is moving forwards.
		private int waypointsPassed = 0;					// Counts how many waypoints have been passed.
		private GameObject nextWaypoint;					// Used to store the next waypoint's game object.
		private GameObject parent;							// The parent game object.
		private Vector2 nextVelocity = new Vector2(0, 0);	// Used to store the velocity needed to go to the next waypoint.
		private bool waitAtWaypoint = false;				// Boolean to check if the platform is waiting at the current waypoint.
		private float nextWaypointTimer;					// Timer used to regulate how long the platform waits at the current waypoint.
		private int initialIndex;							// Used to remember the initial index.

		// Use this for initialization.
		void Start () {
			// Setting up references.
			platform = transform.parent.GetComponent<Platform>();
			movingForward = moveForward;
			nextWaypoint = waypoints[currentIndex];
			parent = transform.parent.gameObject;
			initialIndex = currentIndex;
		}
		
		// Update is called once per frame.
		void Update () {
			// Change the current waypoint if the movement direction changes.
			if (movingForward != moveForward) {
				movingForward = moveForward;
				DetermineNextWaypoint();
			}

			// Set the next waypoint.
			nextWaypoint = waypoints[currentIndex];

			// If there is a next waypoint...
			if (nextWaypoint) {
				// ... set up the velocity to move towards the next waypoint.
				Transform waypointTransform = nextWaypoint.transform;
				nextVelocity = (waypointTransform.position-transform.position).normalized;
			}
			
			// If the total amount of waypoints have been passed, stop the platform.
			if (stopAfterTotal != 0 && waypointsPassed >= stopAfterTotal) {
				stop = true;
			} else if (moveOnHit) {
				if (platform.playerOnPlatform) {
					stop = false;
				} else {
					stop = true;
				}
			}

			// If the platform is waiting at a waypoint...
			if (waitAtWaypointTime > 0 && waitAtWaypoint) {
				// ... run the timer.
				if (nextWaypointTimer > 0) {
					nextWaypointTimer -= Time.deltaTime;
				} else {
					// When the timer is finished, make sure the waitAtWaypoint variable is false and the platform starts moving again.
					waitAtWaypoint = false;
					stop = false;
				}
			}
		}
		
		// This function is called every fixed framerate frame.
		void FixedUpdate() {
			if (stop) {
				// Stop the platform.
				platform.rigidbody.velocity = new Vector2(0, 0);
			} else {
				// Move the platform to the next waypoint.
				platform.rigidbody.velocity = nextVelocity * speed;
			}
		}

		// Waypoints have triggers. This function checks if the platform enters that trigger.
		void OnTriggerEnter2D(Collider2D collider) {
			// If the moving platform collides with the waypoint...
			if (collider.gameObject == nextWaypoint) {
				// ... determine the next waypoint.
				DetermineNextWaypoint();

				// Add 1 to the amount of waypoints passed.
				waypointsPassed++;
			}
		}

		// Determine the next waypoint the platform should move towards.
		void DetermineNextWaypoint() {
			// If the platform is moving forward and it's at the last waypoint...
			if (moveForward && waypoints.Length == currentIndex+1) {
				// If the platform is looping...
				if (loop) {
					// ... set the waypoint index to -1, so it'll be 0.
					currentIndex = -1;
					// Or else...
				} else {
					// ... move backwards.
					moveForward = movingForward = false;
				}
			// Or else if the platform is moving backwards and it's at the first waypoint...
			} else if (!moveForward && currentIndex == 0) {
				// If the platform is looping...
				if (loop) {
					// ... set the waypoint index to the total amount of waypoints.
					currentIndex = waypoints.Length;
				// Or else...
				} else {
					// ... move forward.
					moveForward = movingForward = true;
				}
			}
			
			// If the platform is moving forward...
			if (moveForward) {
				// Add 1 to the current index.
				currentIndex++;
			// Or else...
			} else {
				// Detract 1 from the current index.
				currentIndex--;
			}

			// If the platform should wait at the waypoint...
			if (waitAtWaypointTime > 0) {
				// ... set the waypoint timer and make the platform stop.
				nextWaypointTimer = waitAtWaypointTime;
				waitAtWaypoint = true;
				stop = true;
			}
		}

		// Reset the currentIndex value.
		public void ResetIndex() {
			currentIndex = initialIndex;
		}
	}
}