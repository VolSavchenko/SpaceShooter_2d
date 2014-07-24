using UnityEngine;
using System.Collections;

public class Spaceship : MonoBehaviour {

	//References
	public GameObject spaceshipBulletPrefab; 	// The bullet that the spaceship will fire
	public AudioClip bulletSfx;  				// Reference to the bullet sfx
	public AudioClip shieldUpSfx; 				// Reference to the shield up sfx
	public AudioClip shieldDownSfx; 			// Reference to the shield down sfx
	public AudioClip hitSfx; 					// Reference to the hit sfx

	//Properties
	public float speed = 1f; 			// Speed that the ship moves
	public float turnSpeed = 1f; 		// Speed that the ship turns
	public float fireRate = 0.5f; 		// In seconds, how quickly the ships bullets can be fired
	public float respawnRate = 1f; 		// In seconds, how long after death till the ship respawns
	public float shieldTime = 3f; 		// In seconds, how long the shield lasts
	public float warpCoolDown = 0.5f; 	// In seconds, how long till the next warp can be activated

	private float accelRate = 0f; 		// The percentage that the ships speed should currently be at
	private Animator animator; 			// Reference to the ships animator
	private GameObject gameManager; 	// Reference to the Game Manager
	private Vector3 screenSW; 			// The South West corner of the screen in world space
	private Vector3 screenNE; 			// The North East corner of the screen in world space
	private float wrapPadding = 1f; 	// Amount that the ship can go off screen before it wraps to the other side
	private bool hit = false; 			// Has the ship been hit
	private float nextFire; 			// Time that the next bullet can be fired
	private bool shielded = true; 		// Is the shield currently active
	private float nextWarp; 			// Time that the next warp can occur
	private AudioSource audioSource; 	// Reference to the ships audio source

	// Use this for initialization
	void Start () {
		animator = this.GetComponent<Animator>();
		audioSource = this.GetComponent<AudioSource>();

		screenSW = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.transform.localPosition.y));
		screenNE = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.localPosition.y));

		StartCoroutine(ShieldActive());
	}
	
	// Update is called once per frame
	void Update () {
		rigidbody2D.AddForce(transform.up * (speed * accelRate));

		// Wrap for the sides 
		if(transform.localPosition.x < screenSW.x - wrapPadding){
			transform.localPosition = new Vector3(screenNE.x, transform.localPosition.y, transform.localPosition.z);
		}
		else if( transform.localPosition.x > screenNE.x + wrapPadding){
			transform.localPosition = new Vector3(screenSW.x, transform.localPosition.y, transform.localPosition.z);
		}
		
		// Wrap for the bottom and top 
		if(transform.localPosition.y < screenSW.y - wrapPadding){
			transform.localPosition = new Vector3(transform.localPosition.x, screenNE.y, transform.localPosition.z);
		}
		else if( transform.localPosition.y > screenNE.y + wrapPadding){
			transform.localPosition = new Vector3(transform.localPosition.x, screenSW.y, transform.localPosition.z);
		}
	}

	void OnTriggerEnter2D(Collider2D other){
		if(hit || shielded){
			return;
		}

		if(other.gameObject.tag == "Rock" || other.gameObject.tag == "Saucer" || other.gameObject.tag == "SaucerBullet"){
			StartCoroutine(Hit());
		}
	}

	// Set the reference to the Game Manager
	public void SetGameManager(GameObject gameManagerObject){
		gameManager = gameManagerObject;
	}

	// Turns the ship to the right
	public void TurnRight(float rotation = 0f){
		// Don't move if the ship has been hit
		if(hit){
			return;
		}

		transform.Rotate(Vector3.back * (rotation * turnSpeed));
	}

	// Turns the ship to the left
	public void TurnLeft(float rotation = 0f){
		// Don't move if the ship has been hit
		if(hit){
			return;
		}

		transform.Rotate(Vector3.forward * (rotation * -turnSpeed));
	}

	// Moves the ship forward
	public void Move(float accel){
		// Don't move if the ship has been hit
		if(hit){
			return;
		}

		accelRate = accel;

		if(shielded){
			animator.SetInteger("State", 3);
		}
		else{
			animator.SetInteger("State", 1);
		}
	}

	// Sets the ship to its idle state
	public void Idle(){
		// Don't move if the ship has been hit
		if(hit){
			return;
		}

		accelRate = accelRate * 0.99f;

		if(shielded){
			animator.SetInteger("State", 2);
		}
		else{
			animator.SetInteger("State", 0);
		}
	}

	// Shoots a bullet
	public void ShootBullet(){
		if(hit){
			return;
		}

		if( Time.time > nextFire){
			nextFire = Time.time + fireRate;
			Instantiate(spaceshipBulletPrefab, transform.localPosition, transform.localRotation);
			audioSource.PlayOneShot(bulletSfx);
		}
	}

	// Warps the ship to a random location within the play area
	public void Warp(){
		if(hit){
			return;
		}

		if(Time.time > nextWarp){
			nextWarp = Time.time + warpCoolDown;

			float newXPos = Random.Range(screenSW.x, screenNE.x);
			float newYPos = Random.Range(screenSW.y, screenNE.y);

			transform.localPosition = new Vector3(newXPos, newYPos, 0);
		}
	}

	// Activates the ships shield
	IEnumerator ShieldActive(){
		shielded = true;
		animator.SetInteger("State", 2);
	audioSource.PlayOneShot(shieldUpSfx);

		yield return new WaitForSeconds(shieldTime);

		shielded = false;
		animator.SetInteger("State", 0);
		audioSource.PlayOneShot(shieldDownSfx);
	}

	// Runs through the ships' hit state
	IEnumerator Hit(){
		hit = true;

		accelRate = 0f;

		animator.SetInteger("State", 4);
		audioSource.PlayOneShot(hitSfx);
		gameManager.GetComponent<GameManager>().UpdateLives(1);
		
		yield return new WaitForSeconds(0.1f);

		// Make the ship invisible to imitate a respawning of the ship
		renderer.enabled = false;
		collider2D.enabled = false;
		yield return new WaitForSeconds(respawnRate);
		renderer.enabled = true;
		collider2D.enabled = true;

		hit = false;
		gameManager.GetComponent<GameManager>().ResetShip();
		StartCoroutine(ShieldActive());
	}
}
