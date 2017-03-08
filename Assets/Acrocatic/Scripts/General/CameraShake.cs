using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Used to shake camera when on a sinking platform.
	// Based on: https://gist.github.com/ftvs/5822103.
	public class CameraShake : MonoBehaviour {
		// Public variables.
		[Tooltip("The time it should take for the Camera to shake.")]
		public float shake = 0f;
		[Tooltip("Amplitude of the shake. A larger value shakes the camera harder.")]
		public float shakeAmount = 0.1f;

		// Private variables.
		float cameraZ; 		// Remember the camera's Z position.

		// When the script is enabled.
		void OnEnable() {
			// Remember the camera's Z position.
			cameraZ = transform.localPosition.z;
		}

		// Update is called once per frame.
		void Update() {
			// If the shake timer is higher than 0...
			if (shake > 0) {
				// Set the camera's local position to a random value inside a unit sphere times the shake amount.
				// And make sure the Z value stays the same.
				Vector3 position = transform.localPosition + Random.insideUnitSphere * shakeAmount;
				transform.localPosition = new Vector3(position.x, position.y, cameraZ);

				// Decrease the timer by Time.deltaTime.
				shake -= Time.deltaTime;
			// When the timer is finished.
			} else {
				// Set shake to 0.
				shake = 0f;
				// Disable the class.
				enabled = false;
			}
		}
	}
}