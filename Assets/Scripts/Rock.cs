using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour {

	// References
	public GameObject childRockPrefab; 	// Reference to the child prefab this rock will produce when destroyed
	public AudioClip hitSfx;

	//Properties
	public int numChildRocks = 1; 		// Number of children rocks this rock will produce when destroyed
	public float speed = 1f; 			// Speed of the rock
	public int score; 					// Score to add when this rock is destroyed

	private GameObject gameManager; 	// Reference to the Game Manager
	private Vector3 screenSW; 			// The South West corner of the screen in world space
	private Vector3 screenNE; 			// The North East corner of the screen in world space
	private float wrapPadding = 1; 		// Amount that the ship can go off screen before it wraps to the other side
	private AudioSource audioSource; 	// Reference to the ships audio source

	// Use this for initialization
	void Start () {
		audioSource = this.GetComponent<AudioSource>();

		screenSW = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.transform.localPosition.y));
		screenNE = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.localPosition.y));

		transform.Rotate(Vector3.forward * Random.Range(0f, 360f));
	}
	
	// Update is called once per frame
	void Update () {
		rigidbody2D.AddForce(transform.up * speed);

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
		if(other.gameObject.tag == "SpaceshipBullet" || other.gameObject.tag == "Player"){
			StartCoroutine(DestroyRock());
		}
	}

	// Set the reference to the Game Manager
	public void SetGameManager(GameObject gameManagerObject){
		gameManager = gameManagerObject;
	}

	// Destroys the rock and instantiates a child if it has one
	IEnumerator DestroyRock(){
		if (childRockPrefab != null){
			for (int i = 0; i < numChildRocks; i++){
				GameObject rockClone = Instantiate(childRockPrefab, transform.localPosition, Quaternion.identity) as GameObject;
				rockClone.GetComponent<Rock>().SetGameManager(gameManager);
			}
		}
		
		audioSource.PlayOneShot(hitSfx);
		gameManager.GetComponent<GameManager>().UpdateScore(score);
		yield return new WaitForSeconds(0.2f);
		
		Destroy(gameObject);
	}
}
