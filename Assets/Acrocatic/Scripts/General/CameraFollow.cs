using UnityEngine;
using System.Collections;

namespace Acrocatic {
	// Class that makes the camera follow the player. This is the same as the free 2D demo asset from Unity.
	// Source: https://www.assetstore.unity3d.com/#/content/11228
	public class CameraFollow : MonoBehaviour {
		[Tooltip("Distance in the x axis the player can move before the camera follows.")]
		public float xMargin = 1f;
		[Tooltip("Distance in the y axis the player can move before the camera follows.")]
		public float yMargin = 1f;
		[Tooltip("How smoothly the camera catches up with it's target movement in the x axis.")]
		public float xSmooth = 8f;
		[Tooltip("How smoothly the camera catches up with it's target movement in the y axis.")]
		public float ySmooth = 8f;
		[Tooltip("The maximum x and y coordinates the camera can have.")]
		public Vector2 maxXAndY;
		[Tooltip("The minimum x and y coordinates the camera can have.")]
		public Vector2 minXAndY;


		private Transform player;		// Reference to the player's transform.
		
		
		void Start ()
		{
			// Setting up the reference.
			player = GameObject.FindGameObjectWithTag("Player").transform;
		}
		
		
		bool CheckXMargin()
		{
			// Returns true if the distance between the camera and the player in the x axis is greater than the x margin.
			return Mathf.Abs(transform.position.x - player.position.x) > xMargin;
		}
		
		
		bool CheckYMargin()
		{
			// Returns true if the distance between the camera and the player in the y axis is greater than the y margin.
			return Mathf.Abs(transform.position.y - player.position.y) > yMargin;
		}
		
		
		void FixedUpdate ()
		{
			TrackPlayer();
		}
		
		
		void TrackPlayer ()
		{
			// By default the target x and y coordinates of the camera are its current x and y coordinates.
			float targetX = transform.position.x;
			float targetY = transform.position.y;
			
			// If the player has moved beyond the x margin...
			if(CheckXMargin())
				// ... the target x coordinate should be a Lerp between the camera's current x position and the player's current x position.
				targetX = Mathf.Lerp(transform.position.x, player.position.x, xSmooth * Time.deltaTime);
			
			// If the player has moved beyond the y margin...
			if(CheckYMargin())
				// ... the target y coordinate should be a Lerp between the camera's current y position and the player's current y position.
				targetY = Mathf.Lerp(transform.position.y, player.position.y, ySmooth * Time.deltaTime);
			
			// The target x and y coordinates should not be larger than the maximum or smaller than the minimum.
			targetX = Mathf.Clamp(targetX, minXAndY.x, maxXAndY.x);
			targetY = Mathf.Clamp(targetY, minXAndY.y, maxXAndY.y);
			
			// Set the camera's position to the target position with the same z component.
			transform.position = new Vector3(targetX, targetY, transform.position.z);
		}
	}
}