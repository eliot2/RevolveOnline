﻿using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Reflection;

public class GameMaster : MonoBehaviour {

	[Serializable]
	public struct GAME_VALUES{

		//###################################
		//PLAYER VARIABLES
		//###################################
		public float movementSpeed; 
		public float runningSpeed;
		public float mouseSensetivity;// = 10.0f;
		public float upDownRange;// = 70.0f;
		public float jumpHeight;// = 10.0f;
		public float jumpCount;
		public float maxHealth;
		public bool invertControls;
		public bool invertMouseY;
		public bool noStrafe;
		public bool canZoom;
		public int playerLives;		

		//###################################
		//ENVIRONMENT VARIABLES
		//###################################
		public float gravity;
		public bool jumpingAllowed;
		public bool runningAllowed;
		public bool miniMapEnabled;
        public bool drunkMode;
        public bool orthographic;

		//###################################
		//MOD CONTENT ENABLED
		//###################################
		//GUNS/WEAPONS-----------------------
		//basic gun mods
		public float fireInterval_Basic;
		public float bulletSpeed_Basic;
		public float gunWeight_Basic;
		public float bulletLife_Basic;

		//Characters-------------------------
		public bool robots;

		//Maps-------------------------------
		public bool ColumnArena;
		public bool GridArena;
	}

	private string magicWinButton = "\\";
	public GAME_VALUES _M;
	private int sizeX = 150;
	private int sizeY = 150;
	private bool gameOver = false;

	private KillStreak kills;
    private Camera[] cameraList;
    public GameObject[] selectedCharacters = new GameObject[4];
    public GameObject[] allPossibleCharacters = new GameObject[8];
    public int currentScene;
	//private KillStreak kills;

	void Awake()
	{
		DontDestroyOnLoad(transform.gameObject);
		if (FindObjectsOfType(GetType()).Length > 1){
			Destroy(gameObject);
		}
	}

	void Start(){
		kills = transform.GetComponent<KillStreak>();
        currentScene = Application.loadedLevel;
		//kills = transform.GetComponent<KillStreak>();
        DontDestroyOnLoad(this);

		if(Application.isEditor)Save_Values();
		//Screen.lockCursor = true;
		
		Load_Values();
		gameObject.GetComponent<Camera>().pixelRect = new Rect(Screen.width/2 - sizeX/2, Screen.height/2 - sizeY/2, sizeX, sizeY);
		if(_M.miniMapEnabled){
			gameObject.GetComponent<Camera>().enabled = true;
		}
	}
		




    public void SetOrthographic(bool orth)
    {
        cameraList = GameObject.FindObjectsOfType<Camera>();
        foreach (Camera x in cameraList)
        {
                x.orthographic = orth;
                x.orthographicSize = 163.7f;
        }
        
        //if (_M.orthographic)
        //{
        //    foreach (Camera x in cameraList)
        //    {
        //        x.orthographic = true;
        //        x.orthographicSize = 2.35f;
        //    }
        //}
        //else
        //{
        //    foreach (Camera x in cameraList)
        //    {
        //        x.orthographic = false;
        //    }
        //}
    }


    public void SpawnPlayers()
    {
        SetOrthographic(true);
       // Screen.lockCursor = true;
        GameObject[] spawnPoints = new GameObject[4];
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        Debug.Log("Selected CHar size: " + selectedCharacters.Length);
		Debug.Log("Spawn point size: " + spawnPoints.Length);
        for (int i = 0; i < 4; i++)
        {
			Debug.Log("Accessor i: " + i);
            Instantiate(
            selectedCharacters[i], 
            spawnPoints[i].transform.position, 
            spawnPoints[i].transform.rotation);
        }
    }

	void Update(){
        if (currentScene != Application.loadedLevel && Application.loadedLevel >= 0)
        {
			Debug.Log("current scene changed, level num: " + Application.loadedLevel);
			GameObject[] spawnPoints = new GameObject[4];
			spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
			Start();
			if(spawnPoints.Length == 4){SpawnPlayers();}
            //currentScene = Application.loadedLevel;
        }


		if(Input.GetKeyDown("q") || Input.GetKeyDown("escape"))
		{
			Save_Values();
			Manager.say("Attempting to quit game now, goodbye!", "always");
			Application.Quit();

		}
		if(Input.GetKeyDown(magicWinButton)){
			Manager.say("You pressed the magic win button!", "eliot");
			GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
			for(int i = 0; i < allPlayers.Length-1; i++){
				allPlayers[i].SetActive(false); //disables all but one player, making a winner.
			}
		}
		if (GameObject.FindGameObjectsWithTag ("Player").Length == 1) {
						gameOver = true;
						//Screen.lockCursor = false;
				} 
		else {
			gameOver = false;
		}

        
		if (kills.getPlayer1Kills() > 2) {
			Manager.say ("Player1 has killStreak", "jed");
		}
		if (kills.getPlayer2Kills() >2) {
			Manager.say ("Player2 has killStreak", "jed");
		}
		if (kills.getPlayer3Kills() > 2) {
			Manager.say ("Player3 has killStreak", "jed");
		}
		if (kills.getPlayer4Kills() > 1) {
			Manager.say ("Player4 has killStreak", "jed");
		}
	}


	public void Save_Values(){
		GameMaster masterScript;
		masterScript = GameObject.Find("Game Master").GetComponent<GameMaster>();
		Manager.say("Attempting to save GM", "always");
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream("GM.bin", FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, masterScript._M);
		stream.Close();
		Manager.say("Saving GM likely successful", "always");
	}

	private void Load_Values(){
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream("GM.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
		_M = (GAME_VALUES) formatter.Deserialize(stream);
		stream.Close();
		Manager.say("Loading GM likely successful", "always");
	}
	
	public bool isGameOver(){
		return gameOver;
	}

	public void SetGameOver(){
		gameOver = false;
	}


}
