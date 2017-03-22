using Acrocatic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopPlatform : MonoBehaviour {
    Rigidbody2D rgbd;
    public float spawnRate;
    public float spawnDistanceOffset;
    public float timeSinceLastSpawned;
    public GameObject player;
    private float timeElapsed;
	// Use this for initialization
	void Start () {
        rgbd = GetComponent<Rigidbody2D>();
        Vector2 currentPos = rgbd.transform.position;
        rgbd.transform.position = new Vector2(currentPos.x, Random.Range(-1f, 4f));
	}
	
	// Update is called once per frame
	void Update () {
        timeSinceLastSpawned += Time.deltaTime;
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= 5)
        {
            timeElapsed = 0;
            spawnRate -= 0.5f;
            spawnDistanceOffset += 1;
            
        }
        if (timeSinceLastSpawned >= spawnRate)
        {
            Player playerScript = player.GetComponent<Player>();
            Rigidbody2D pos = playerScript.rigidbody;
            Vector2 newPosition = new Vector2(pos.transform.position.x + spawnDistanceOffset, Random.Range(-1f, 4f));
            rgbd.transform.position = newPosition;
            timeSinceLastSpawned = 0;
        }
	}
}
