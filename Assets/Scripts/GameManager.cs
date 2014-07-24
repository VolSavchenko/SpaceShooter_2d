using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	//Prefabs
	public GameObject spaceshipPrefab;
	public GameObject saucerPrefab;
	public GameObject startingRockPrefab;

	public enum gameState { 			// Current game state enum
		main,
		game,
		gameOver,
	}

	public gameState state;

	public GameObject mainUI; 			// Reference to the main menu's UI
	public GameObject gameUI; 			// Reference to the game's UI
	public GameObject scoreText; 		// Reference to the score UI text
	public GameObject livesText; 		// Reference to the lives left UI text
	public GameObject gameOverUI; 		// Reference to the game over UI
	public GameObject finalScoreText; 	// Reference to the final score UI text

	// Game Properties
	public int playerLives = 3; 		// Number of lives the player has
	public int score = 0; 				// Initial Score for the player
	public int numStartingRocks = 4;	// Number of rocks to intially populate the screen with
	public float saucerSpawnRate = 10;  // In seconds, how frequently a saucer will spawn

	private GameObject player; 			// Reference to the player's spaceship
	private int rockSpawnRadius = 4; 	// The radius from within the initial rocks will spawn
	private Vector3 screenSW; 			// The South West corner of the screen in world space
	private Vector3 screenNE; 			// The North East corner of the screen in world space
	private Vector3 screenSE; 			// The South East corner of the screen in world space
	private Vector3 screenNW; 			// The North West corner of the screen in world space
	private int startingScore;			// Store off the starting score
	private int startingLives; 			// Store off the starting lives

	// Use this for initialization
	void Start () {
		mainUI.SetActive(false);
		gameUI.SetActive(false);
		gameOverUI.SetActive(false);

		switch(state){
		case gameState.main:
			mainUI.SetActive(true);
			break;
		case gameState.game:
			gameUI.SetActive(true);
			break;
		case gameState.gameOver:
			gameOverUI.SetActive(true);
			break;
		}

		startingScore = score;
		startingLives = playerLives;
		UpdateScore(0);
		UpdateLives(0);
	}

	// Update is called once per frame
	void Update () {

		switch(state){
		case gameState.main:
			if (Input.GetKeyDown("enter") || Input.GetKeyDown("return")){
				StartCoroutine(GameStart());
			}
			break;
		case gameState.game:
			float translation = Input.GetAxis("Vertical");
			float rotation = Input.GetAxis("Horizontal");

			// Turning ship
			if (rotation > 0){
				player.GetComponent<Spaceship>().TurnRight(rotation);
			}
			else if (rotation < 0){
				player.GetComponent<Spaceship>().TurnLeft(rotation);
			}

			// Thrusting ship
			if (translation >= 0.9f){
				player.GetComponent<Spaceship>().Move(translation);
			}
			else{
				player.GetComponent<Spaceship>().Idle();
			}

			// Shoot bullets
			if(Input.GetButton("Jump")){
				player.GetComponent<Spaceship>().ShootBullet();
			}

			// Warp to random spot
			if(Input.GetButton("Fire1")){
				player.GetComponent<Spaceship>().Warp();
			}

			// Check if there are no more objects on screen and spawn new ones
			// NOTE: This is an inefficient call for an update function but is being left in for an example of finding all items with a tag
			// 			see the Rocks tutorial video for more info on this
			GameObject[] rocks = GameObject.FindGameObjectsWithTag("Rock");
			if(rocks.Length <= 0){
				for (int i = 0; i < numStartingRocks; i++){
					float rockPosX = rockSpawnRadius * Mathf.Cos(Random.Range(0f, 360f));
					float rockPosY = rockSpawnRadius * Mathf.Sin(Random.Range(0f, 360f));
					GameObject rockClone = Instantiate(startingRockPrefab, new Vector3(rockPosX, rockPosY, 0), Quaternion.identity) as GameObject;
					rockClone.GetComponent<Rock>().SetGameManager(this.gameObject);
				}
			}
			break;
		case gameState.gameOver:
			if (Input.GetKeyDown("enter") || Input.GetKeyDown("return")){
				// Clear out existing rocks on screen
				GameObject[] rocksToDestroy = GameObject.FindGameObjectsWithTag("Rock");
				for(int i = 0; i < rocksToDestroy.Length; i++){
					Destroy(rocksToDestroy[i]);
				}

				StartCoroutine(GameStart());
			}
			break;
		}
	}

	//Resets the Ship back to its starting position
	public void ResetShip(){
		player.transform.localPosition = new Vector3(0, 0, 0);
	}

	public void UpdateScore(int scoreToAdd){
		score += scoreToAdd;
		scoreText.guiText.text = "Score " + score;
	}

	// Updates the current count of lives
	public void UpdateLives(int livesLost){
		playerLives -= livesLost;
		livesText.guiText.text = "Lives " + playerLives;

		if(playerLives < 1){
			StartCoroutine(GameEnd());
		}
	}

	// Game start sequence
	IEnumerator GameStart(){
		mainUI.SetActive(false);
		gameUI.SetActive(true);
		gameOverUI.SetActive(false);
		state = gameState.game;

		scoreText.guiText.text = "Score " + score;
		livesText.guiText.text = "Lives " + playerLives;

		// Spawn the player's ship
		player = Instantiate(spaceshipPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
		player.GetComponent<Spaceship>().SetGameManager(this.gameObject);

		// Spawn the initial Rocks
		for (int i = 0; i < numStartingRocks; i++){
			float rockPosX = rockSpawnRadius * Mathf.Cos(Random.Range(0f, 360f));
			float rockPosY = rockSpawnRadius * Mathf.Sin(Random.Range(0f, 360f));
			GameObject rockClone = Instantiate(startingRockPrefab, new Vector3(rockPosX, rockPosY, 0), Quaternion.identity) as GameObject;
			rockClone.GetComponent<Rock>().SetGameManager(this.gameObject);
		}
		
		// Save off for spawning Saucer
		screenSW = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.transform.localPosition.y));
		screenNE = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.localPosition.y));
		screenSE = new Vector3(screenSW.x, screenNE.y, 0);
		screenNW = new Vector3(screenNE.x, screenSW.y, 0);
		
		StartCoroutine(SaucerSpawn());

		yield return null;
	}

	// Game End sequence
	IEnumerator GameEnd(){
		mainUI.SetActive(false);
		gameUI.SetActive(false);
		gameOverUI.SetActive(true);
		state = gameState.gameOver;

		finalScoreText.guiText.text = "Final Score " + score;

		Destroy(player);
		StopAllCoroutines();

		score = startingScore;
		playerLives = startingLives;

		yield return null;
	}

	// Spawns the saucer
	IEnumerator SaucerSpawn(){

		//Waits till its time to spawn a new saucer
		for (float timer = saucerSpawnRate; timer >= 0; timer -= Time.deltaTime){
			yield return null;
		}

		// Picks a corner to spawn from
		int cornerSelection = Random.Range(0, 4);
		Vector3 saucerSpawnPos = new Vector3(0, 0, 0); 

		// Botom left corner
		if (cornerSelection == 0){
			saucerSpawnPos = new Vector3(screenSW.x, screenSW.y, 0);
		}// Botom right corner
		else if (cornerSelection == 1){
			saucerSpawnPos = new Vector3(screenSE.x, screenSE.y, 0);
		}// Top right corner
		else if (cornerSelection == 2){
			saucerSpawnPos = new Vector3(screenNE.x, screenNE.y, 0);
		}// Top left corner
		else if (cornerSelection == 3){
			saucerSpawnPos = new Vector3(screenNW.x, screenNW.y, 0);
		}
		
		GameObject saucerClone = Instantiate(saucerPrefab, saucerSpawnPos, Quaternion.identity) as GameObject;
		saucerClone.GetComponent<Saucer>().SetGameManager(this.gameObject);

		// Point the saucer at a random angle into the play area
		if (cornerSelection == 0){
			saucerClone.transform.Rotate(Vector3.back * Random.Range(0f, 90f));
		}
		else if (cornerSelection == 1){
			saucerClone.transform.Rotate(Vector3.back * Random.Range(90f, 180f));
		}
		else if (cornerSelection == 2){
			saucerClone.transform.Rotate(Vector3.back * Random.Range(190f, 270f));
		}
		else if (cornerSelection == 3){
			saucerClone.transform.Rotate(Vector3.back * Random.Range(270f, 360f));
		}

		// Restart the Saucer Spawn
		StartCoroutine(SaucerSpawn());
	}
}
