using UnityEngine;
using System.Collections;

public class Saucer : MonoBehaviour {
	// References
	public GameObject saucerBulletPrefab; // The bullet that the saucer will fire
	public AudioClip bulletSfx;
	public AudioClip hitSfx;

	// Properties
	public float speed = 1f; 			// Speed that the saucer moves
	public float maxFireWaitTime = 5; 	// In seconds, the maximum amount of time the saucer will wait to fire
	public int score; 					// Score to add when this rock is destroyed 

	private Animator animator; 			// Reference to the saucer animator
	private GameObject gameManager; 	// Reference to the Game Manager
	private Vector3 screenSW; 			// The South West corner of the screen in world space
	private Vector3 screenNE; 			// The North East corner of the screen in world space
	private float destroyPadding = 1; 	// Amount that the saucer can go off screen before it is destroyed
	private AudioSource audioSource; 	// Reference to the ships audio source

	// Use this for initialization
	void Start () {
		animator = this.GetComponent<Animator>();
		audioSource = this.GetComponent<AudioSource>();

		screenSW = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.transform.localPosition.y));
		screenNE = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.localPosition.y));

		StartCoroutine(Attack());

		animator.SetInteger("State", 0);
	}
	
	// Update is called once per frame
	void Update () {
		rigidbody2D.AddForce(transform.up * speed);
		
		// Clean up Saucer if it goes off screen
		if(transform.localPosition.x < screenSW.x - destroyPadding ||
		   transform.localPosition.x > screenNE.x + destroyPadding ||
		   transform.localPosition.y < screenSW.y - destroyPadding ||
		   transform.localPosition.y > screenNE.y + destroyPadding){
			Destroy(gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D other){
		if(other.gameObject.tag == "SpaceshipBullet" || other.gameObject.tag == "Player"){
			StartCoroutine(Hit());
		}
	}

	// Set the reference to the Game Manager
	public void SetGameManager(GameObject gameManagerObject){
		gameManager = gameManagerObject;
	}

	// Starts the attack phase of the Saucer
	IEnumerator Attack(){
		// Saucer picks a random time based on its given range and then fires
		for (float timer = Random.Range(0, maxFireWaitTime); timer >= 0; timer -= Time.deltaTime){
			yield return null;
		}

		Instantiate(saucerBulletPrefab, transform.localPosition, transform.localRotation);
		audioSource.PlayOneShot(bulletSfx);
	}

	// Runs through the saucers' hit state
	IEnumerator Hit(){
		animator.SetInteger("State", 1);
		audioSource.PlayOneShot(hitSfx);
		gameManager.GetComponent<GameManager>().UpdateScore(score);

		yield return new WaitForSeconds(0.3f);

		Destroy(gameObject);
	}
}
