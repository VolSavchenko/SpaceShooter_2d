using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public float speed = 1f; 			// Speed of the bullet

	public enum bulletType { 			// Bullet type enum
		spaceship,
		saucer,
	}

	public bulletType type; 			// Bullet type instance
	
	private Vector3 screenSW; 			// The South West corner of the screen in world space
	private Vector3 screenNE; 			// The North East corner of the screen in world space
	private float destroyPadding = 1; 	// Amount that the saucer can go off screen before it is destroyed

	// Use this for initialization
	void Start () {
		screenSW = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.transform.localPosition.y));
		screenNE = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.localPosition.y));
	}
	
	// Update is called once per frame
	void Update () {
		rigidbody2D.AddForce(transform.up * speed);
		
		// Clean up bullet if it goes off screen
		if(transform.localPosition.x < screenSW.x - destroyPadding ||
		   transform.localPosition.x > screenNE.x + destroyPadding ||
		   transform.localPosition.y < screenSW.y - destroyPadding ||
		   transform.localPosition.y > screenNE.y + destroyPadding){
			Destroy(gameObject);
		}
	}
	
	void OnTriggerEnter2D(Collider2D other){
		switch(type){
		case bulletType.spaceship:
			if(other.gameObject.tag == "Rock" || other.gameObject.tag == "Saucer"){
				Destroy(gameObject);
			}
			break;
		case bulletType.saucer:
			if(other.gameObject.tag == "Player"){
				Destroy(gameObject);
			}
			break;
		}
	}
}
