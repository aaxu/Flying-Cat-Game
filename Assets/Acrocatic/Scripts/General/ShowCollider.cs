using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class to show or hide the color for the colliders.
	public class ShowCollider : MonoBehaviour {
		// Private variables
		private SpriteRenderer spriteRenderer;

		// Use this for initialization
		void Start () {
			// Setting up references.
			spriteRenderer = GetComponent<SpriteRenderer> ();
			
			// If the showColliders setting is false...
			if (!GameSettings.Instance.showColliders) {
				// ... disable the sprite renderer.
				spriteRenderer.enabled = false;
			}
		}
	}
}
