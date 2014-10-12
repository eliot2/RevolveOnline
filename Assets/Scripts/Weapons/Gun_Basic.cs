﻿using UnityEngine;
using System.Collections;

public class Gun_Basic : MonoBehaviour {

	
	//private Vector3 startRotation = new Vector3(0f,0f,0f);
	//private Vector3 startScale = new Vector3(0.17989f, .13f, 0.56067f);

	private bool equipped;
    public bool spawnedEquipped;
	private string Fire_str = " "; 

	private float fireSpd;

	public GameObject bullet_prefab;

	private GameMaster GM;

	private string owner;

	// Use this for initialization
	void Start () {
		fireSpd = 0;

		GM  = GameObject.Find("Game Master").GetComponent<GameMaster>();

		if( !spawnedEquipped){
			equipped = false;
		}else{
			equipped = true;
			setEquips(transform.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		fireSpd -= Time.deltaTime*60;

		Quaternion tempRot = gameObject.transform.rotation;

		if(equipped && Input.GetButton(Fire_str) && fireSpd < 0){
			Manager.say("I FIRED", "eliot");
			GameObject tempBullet;
            //the gun has a bullet spawn component found via getchild(0).transform.position 
			tempBullet = Instantiate(bullet_prefab, transform.GetChild(0).transform.position,  tempRot*Quaternion.Euler(new Vector3(90f,0f,0f))) as GameObject;
			tempBullet.GetComponent<Bullet_Basic>().setSpeedandOwner(Vector3.up * GM._M.bulletSpeed_Basic, owner);
			fireSpd = GM._M.fireInterval_Basic;
		}
	}


	//METHOD USED IF PLAYER SPAWNS WITH THIS GUN
	private void setEquips(GameObject player){
		if(!equipped && player.transform.tag == "Player" && !player.GetComponent<FirstPersonController>().gunEquipped){
            player.GetComponent<FirstPersonController>().gunEquipped = true;
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            player.GetComponent<GunSetup>().attachGun(gameObject, player);
            equipped = true;
            //transform.localScale.Set(startScale.x, startScale.y, startScale.z);
            Fire_str = transform.GetComponentInParent<FirstPersonController>().GetFire_Str();
            owner = transform.parent.parent.name;
		}
	}

	//METHOD USED IF NO SPAWNING WITH GUN
	void OnTriggerEnter(Collider player){
		if(!equipped && player.transform.tag == "Player" && !player.GetComponent<FirstPersonController>().gunEquipped){
			player.GetComponent<FirstPersonController>().gunEquipped = true;
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            player.GetComponent<GunSetup>().attachGun(gameObject, player.gameObject);
			equipped = true;
			//transform.localScale.Set(startScale.x, startScale.y, startScale.z);
            Fire_str = transform.GetComponentInParent<FirstPersonController>().GetFire_Str();
			owner = transform.parent.parent.name;
		}
	}
}
