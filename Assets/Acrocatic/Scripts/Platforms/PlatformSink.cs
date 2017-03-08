using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// This class is used for sinking platforms.
	public class PlatformSink : MonoBehaviour {
		// Public variables.
		[Header("General")]
		[Tooltip("Determines if the platform should sink when the player stands on it. If this is enabled, a timer will run. When the timer is completed, the platform will sink.")]
		public bool sinkOnHit = true;
		[Tooltip("Stop the timer when the player leaves the platform. If this is enabled, the platform will not sink when the player leaves the platform before the timer runs out.")]
		public bool stopTimerWhenGone = false;
		[Tooltip("Reset the timer when the player leaves the platform. This resets the timer to the original duration.")]
		public bool resetTimerWhenGone = false;
		[Tooltip("Set the sink timer's duration. When this is 0 and Sink On Hit is enabled, the platform will sink instantly when the player stands on it.")]
		public float time = 1.0f;
		[Header("Reset")]
		[Tooltip("Reset the sinking platform's position.")]
		public bool resetPosition = true;
		[Tooltip("This is the duration for when the platform should reset to its initial position. Only works if Reset Position is enabled.")]
		public float resetTime = 2.0f;
		[Header("Camera shake")]
		[Tooltip("Shake the camera when standing on a sinking platform.")]
		public bool shakeCamera = true;
		[Tooltip("Only shake the camera when the platform is actually sinking.")]
		public bool shakeWhenSinking = false;
		[Tooltip("Set the duration for the camera shake when the camera should shake on hit.")]
		public float shakeTime = 1.0f;
		[Tooltip("Set the shake's intensity.")]
		public float shakeAmount = 0.1f;
		// NOTE: You can set the platform's speed by changing its rigidbody2D components!

		// Private variables.
		private Platform platform;						// Get the platform class.
		private bool hasBeenStopped = false;			// Determines if the timer has been stopped at least once.
		private bool timerRunning = false;				// Determines if the timer is running.
		private float sinkTimer;						// This is used for the timer.
		private bool sinking = false;					// Determines if the platform is currently sinking.
		private bool shaken = false;					// Determines if the platform has already shaken (not stirred).
		private bool runResetTimer = false;				// Boolean to determine if the reset timer should run.
		private float resetTimer;						// Timer to reset the platform's position.

		// Use this for initialization.
		void Start () {
			platform = GetComponent<Platform>();
		}
		
		// Update is called once per frame.
		void Update () {
			// If the platform isn't sinking...
			if (!sinking) {
				// ... if the timer is running...
				if (timerRunning) {
					// ... run the timer and make the platform sink when it's completed.
					if (sinkTimer > 0) {
						sinkTimer -= Time.deltaTime;
					} else {
						Sink();
					}
				// Or else...
				} else {
					// ... start the timer when the platform should sink automatically.
					if (!sinkOnHit) {
						StartSinkTimer();
					}
				}
			}

			// If the reset timer is running...
			if (runResetTimer) {
				// ... run the timer and reset the platform's position when it's finished.
				if (resetTimer > 0) {
					resetTimer -= Time.deltaTime;
				} else {
					runResetTimer = false;
					ResetPosition();
				}
				// Or else...
			}
		}

		// Function to make the platform sink.
		public void Sink() {
			sinking = true;
			ShakeCamera();
			platform.Sink();
			if (resetPosition) {
				StartResetTimer();
			}
		}

		// Function to start the sinking timer.
		public void StartSinkTimer() {
			// If the timer is not yet running and the platform isn't sinking...
			if (!sinking && !timerRunning) {
				// ... only reset the timer if it's the first time the player interacts or when it should reset after jumping off.
				if (!hasBeenStopped || (hasBeenStopped && resetTimerWhenGone)) {
					sinkTimer = time;
				}
				// Start the timer.
				timerRunning = true;
			}
		}
		
		// Function to reset the sinking timer based on the public variables.
		public void ResetSinkTimer() {
			// If the platform isn't sinking and the timer should stop or reset when the player jumps off the platform.
			if (!sinking && (stopTimerWhenGone || resetTimerWhenGone)) {
				// Remember that the platform has been stopped at least once.
				hasBeenStopped = true;
				// Stop the timer.
				timerRunning = false;
			}
		}

		// Shake the camera.
		public void ShakeCamera() {
			// If the platform hasn't shaken and the shake amount and time is higher than 0...
			if (shakeCamera && !shaken && ((shakeWhenSinking && sinking) || !shakeWhenSinking) && shakeAmount > 0 && shakeTime > 0) {
				// Set shaken to true, so this platform doesn't trigger a shake again.
				shaken = true;
				// Get the Camera and CameraShake class.
				GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
				CameraShake shake = camera.GetComponent<CameraShake>();
				// Set the shake amount and shake time.
				shake.shakeAmount = shakeAmount;
				shake.shake = shakeTime;
				// Enable the CameraShake class.
				shake.enabled = true;
			}
		}

		// Start reset timer.
		public void StartResetTimer() {
			// Set the reset timer's time based on the resetTime variable.
			resetTimer = resetTime;
			// Run the reset timer.
			runResetTimer = true;
		}

		// Reset the platform's position.
		public void ResetPosition() {
			// Reset private variables.
			sinking = false;
			hasBeenStopped = false;
			shaken = false;
			timerRunning = false;
			runResetTimer = false;
			// Reset the position of the platform.
			platform.ResetPosition();
		}

		// Check if the platform is actually sinking.
		public bool IsSinking() {
			return sinking;
		}
	}
}