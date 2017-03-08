using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class for ladders.
	public class Ladder : MonoBehaviour {
		// Public variables.
		[Header("General")]
		[Tooltip("Make the player snap to the middle of the ladder. The player's Snap Speed is used to determine how long this takes.")]
		public bool snapToMiddle = true;
		[Tooltip("Allow the player to jump from the ladder.")]
		public bool allowJump = true;
		[Tooltip("Allow the player to double jump from the ladder. This only works if Allow Jump is also enabled.")]
		public bool allowDoubleJump = true;
		[Header("Ladder speed")]
		[Tooltip("Set a default X speed for the ladder. This allows you to create something like an escalator.")]
		public float defaultSpeedX = 0f;
		[Tooltip("Set a default Y speed for the ladder. This allows you to create something like an escalator.")]
		public float defaultSpeedY = 0f;
		[Header("Allow movement")]
		[Tooltip("Allow movement up the ladder.")]
		public bool allowUp = true;
		[Tooltip("Allow movement down the ladder.")]
		public bool allowDown = true;
		[Tooltip("Allow movement to the left on the ladder.")]
		public bool allowLeft = true;
		[Tooltip("Allow movement to the right on the ladder.")]
		public bool allowRight = true;
		[Header("Falling off the ladder")]
		[Tooltip("Allow the player to move above the ladder to unstick from it.")]
		public bool fallOffTop = true;
		[Tooltip("Allow the player to fall off the ladder when at the utmost bottom of the ladder.")]
		public bool fallOffBottom = true;
		[Tooltip("Allow the player to fall off the ladder when he's at the utmost left side of the ladder.")]
		public bool fallOffLeft = true;
		[Tooltip("Allow the player to fall off the ladder when he's at the utmost right side of the ladder.")]
		public bool fallOffRight = true;
	}
}
