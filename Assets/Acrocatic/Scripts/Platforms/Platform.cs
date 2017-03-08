using UnityEngine;
using System.Collections;

namespace Acrocatic {
	public class Platform : MonoBehaviour {
		[HideInInspector]
		public bool playerOnPlatform = false;		// Check if the player is standing on the platform.
		[HideInInspector]
		public Rigidbody2D rigidbody;				// Cache the rigidbody of the platform.

		// Public variables.
		[Tooltip("Select the platform's types (more than one can be chosen).")]
		public PlatformTypes[] platformTypes;

		// Private variables.
		private PlatformSink sinking;				// Get the platform sinking class.
		private Vector2 initialPosition;			// Used to store the platform's initial position.

		// Use this for initialization.
		void Start () {
			// Setting up references.
			rigidbody = GetComponent<Rigidbody2D>();
			sinking = GetComponent<PlatformSink>();
			initialPosition = transform.position;
		}
		
		// Update is called once per frame.
		void Update () {
		
		}

		// Check if the player is on the platform.
		public void SetPlayerOnPlatform(bool onPlatform) {
			playerOnPlatform = onPlatform;

			// If the platform is a sinking platform...
			if (PlatformTypeIs(PlatformTypes.Sinking) && sinking) {
				// Start or reset the timer based on the sinking variables for the platform.
				if (onPlatform) {
					sinking.StartSinkTimer();
				} else {
					sinking.ResetSinkTimer();
				}
			}
		}

		// Sink the platform.
		public void Sink() {
			// Make sure the platform stops.
			rigidbody.velocity = new Vector2(0, 0);
			// Make sure the platform is kinematic, so it falls down.
			rigidbody.isKinematic = false;
			// If the platform is a moving platform...
			if (PlatformTypeIs(PlatformTypes.Moving)) {
				// ... make sure the movement stops.
				GetComponentInChildren<PlatformMove>().enabled = false;
			}
		}

		// Reset the platform's position.
		public void ResetPosition() {
			// Make sure the platform stops.
			rigidbody.velocity = new Vector2(0, 0);
			// Make the platform kinematic again.
			rigidbody.isKinematic = true;
			// Set the platform's initial position.
			transform.position = initialPosition;
			// If the platform is a moving platform...
			if (PlatformTypeIs(PlatformTypes.Moving)) {
				// ... make sure the movement starts again from the beginning.
				PlatformMove move = GetComponentInChildren<PlatformMove>();
				move.ResetIndex();
				move.enabled = true;
			}
		}

		// Shake the camera.
		public void ShakeCamera() {
			sinking.ShakeCamera();
		}

		// Check platformTypes variable if the platform type is available.
		public bool PlatformTypeIs(PlatformTypes platformType) {
			bool isAvailable = false;
			for (int i=0; i<platformTypes.Length; i++) {
				if (platformTypes[i] == platformType) {
					isAvailable = true;
					break;
				}
			}
			return isAvailable;
		}

		// Check if the platform is actually sinking.
		public bool IsSinking() {
			return sinking.IsSinking();
		}
	}
}
