using UnityEngine;
using System.Collections;
using System;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]

public class FirstPersonController : MonoBehaviour {

	public bool gunEquipped = false; //equiped gun?


	private GameMaster GM;

	private ZoomIn zoom;

	private float rotUpDown;// = 0;
	//private Vector3 speed;
	private float verticalSpeed;
	private float rotLeftRight;
	private float maxVelocityChange = 10.0f;

	private Vector3 playerPos;
	private Ray	ray;
	private RaycastHit rayHitDown;
	private bool isGrounded = true;
	private float moveSpeed;
	private float totalJumpsAllowed;
	private float totalJumpsMade;
	private float floorInclineThreshold = 0.2f;
    private bool allowAirMovement;

	private bool runningToggle = false;
    private bool canCheckForJump;

    public bool isHueBot;

	//ACTION STRINGS
	//==================================================================
	private string Haim_str = "_Look Rotation";
	private string Vaim_str = "_Look UpDown";
	private string Strf_str = "_Strafe";
	private string FWmv_str = "_Forward";
	private string  Fire_str = "_Fire";
	private string Jump_str = "_Jump";
	private string Dash_str = "_Run";
	private string Zoom_str = "_Zoom";
    private string Drop_str = "_Drop";
	//==================================================================

    //PERSONAL CHARACTER MODIFIERS
    public float runSpeedModifier;
    public float walkSpeedModifier;
    public float weightModifier;
    public float healthModifier;
    public float jumpHeightModifier;
    public float armorModifier;

    private bool speedBuffKillStreakPowerUp = false;
	public bool isZoomed = false;

	private Vector3 spawn;

    //For looking, we are assigning rotations, but we need original values
    //that aren't getting modified, so we can re-assign them.
    private Vector3 startingCameraRotation;
    private Vector3 newRotationAngle;
    private Vector3 targetVelocity;

	void Awake () {
		GetComponent<Rigidbody>().freezeRotation = true;
		GetComponent<Rigidbody>().useGravity = false;
	}
	
	
	// Use this for initialization
	void Start () {
        canCheckForJump = true;
        newRotationAngle = new Vector3();
        startingCameraRotation = transform.GetChild(0).transform.localRotation.eulerAngles;
        totalJumpsMade = 0;
		setControlStrings();
		GM  = GameObject.Find("Game Master").GetComponent<GameMaster>();
		moveSpeed = GM._M.movementSpeed;
		rotLeftRight = 0.0f; 
		totalJumpsAllowed = GM._M.jumpCount;
        transform.GetComponentInChildren<Healthbar>().modifyMaxHealth(GM, healthModifier, armorModifier);
		zoom = transform.GetComponentInChildren<ZoomIn> ();
		spawn = transform.position;
		//speed = Vector3.zero;
	}


	
	// Update is called once per frame
	void FixedUpdate () {
		//player rotation
		//left and right
        
		rotLeftRight = Input.GetAxis(Haim_str)*GM._M.mouseSensetivity;
		transform.Rotate(0, rotLeftRight, 0);
		//up and down (with camera)
		if(GM._M.invertMouseY) rotUpDown -= -Input.GetAxis(Vaim_str)*GM._M.mouseSensetivity;
		else rotUpDown -= Input.GetAxis(Vaim_str)*GM._M.mouseSensetivity;
		rotUpDown = Mathf.Clamp(rotUpDown, -GM._M.upDownRange, GM._M.upDownRange);
        newRotationAngle.x = rotUpDown;
        newRotationAngle.y = startingCameraRotation.y;
        newRotationAngle.z = startingCameraRotation.z;
		transform.GetChild(0).transform.localRotation = Quaternion.Euler(newRotationAngle);


		//Zooming
		if (GM._M.canZoom && Input.GetButton (Zoom_str)) {
			zoom.dozoom ();
		} 
		else {
			zoom.undozoom();
		}

        if (Input.GetButtonDown(Drop_str))
        {
            DropGun();
        }
		//if (GM._M.canZoom && Input.GetButtonUp (Zoom_str)) {
		//	zoom.undozoom();
		//}

		//Movement
		//Running!!
		if (GM._M.runningAllowed && Input.GetButtonDown (Dash_str)) {
			runningToggle = !runningToggle;
		 }
		moveSpeed = runningToggle? GM._M.runningSpeed+runSpeedModifier :  GM._M.movementSpeed+walkSpeedModifier;
        if (speedBuffKillStreakPowerUp)
        {
            moveSpeed *= 2;
        }
		
		//Jumping!!
        if (GM._M.jumpingAllowed && totalJumpsMade < totalJumpsAllowed  && Input.GetButtonDown(Jump_str))
        {
            totalJumpsMade += 1;
            isGrounded = false;
            canCheckForJump = false;
            Manager.say("Jumping action go. Jumps Made: " + totalJumpsMade + " Jumps Allowed: " + totalJumpsAllowed, "eliot");
            GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, CalculateJumpVerticalSpeed(), GetComponent<Rigidbody>().velocity.z);

            Invoke("AllowJumpCheck", 2);
        }

        //ACTUAL MOVING
		if(totalJumpsMade==0.0f || GM._M.allowAirMovement){

            //Gram being a massive jerk. DONT EVER ENABLE THIS. IM WARNING YOU
			
			if(!GM._M.invertControls && !isHueBot)
				targetVelocity = new Vector3(Input.GetAxis(Strf_str), 0, Input.GetAxis(FWmv_str));
            else if (!GM._M.invertControls && isHueBot)
            {
                /*Manager.say("I AM HUEBOOT", "Eli");
                Vector3 tempTargetVelocity = new Vector3(Input.GetAxis(Strf_str), 0, Input.GetAxis(FWmv_str));
                if (tempTargetVelocity.magnitude > targetVelocity.magnitude)
                {
                    targetVelocity = tempTargetVelocity;
                    Manager.say("tempTargetVelocity.magnitude > targetVelocity.magnitude", "Eli");
                }*/
                targetVelocity = new Vector3(Input.GetAxis(Strf_str), 0, Input.GetAxis(FWmv_str));
            }
            else if (GM._M.invertControls && isHueBot)
            {
                Vector3 tempTargetVelocity = targetVelocity = new Vector3(-Input.GetAxis(Strf_str), 0, -Input.GetAxis(FWmv_str));
                if (tempTargetVelocity.magnitude > targetVelocity.magnitude)
                {
                    targetVelocity = tempTargetVelocity;
                }
            }
            else
				targetVelocity = new Vector3(-Input.GetAxis(Strf_str), 0, -Input.GetAxis(FWmv_str));
            // THis too. WHYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY?
			if(GM._M.noStrafe)
				targetVelocity.x = 0;

			targetVelocity = transform.TransformDirection(targetVelocity);
			targetVelocity *= moveSpeed;
			// Apply a force that attempts to reach our target velocity
			Vector3 velocity = GetComponent<Rigidbody>().velocity;
			Vector3 velocityChange = (targetVelocity - velocity);

			velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
			velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
			velocityChange.y = 0;

            GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);

			// Jump
			//Manager.say("Jumping action go. Jumps Made: " + totalJumpsMade + " Jumps Allowed: " + totalJumpsAllowed, "eliot");
        }
		

		GetComponent<Rigidbody>().AddForce(new Vector3 (0, -GM._M.gravity * GetComponent<Rigidbody>().mass, 0));
		// We apply gravity manually for more tuning control


        if (transform.position.y < -80.0f || transform.position.y > 200.0f)
        {
			transform.GetChild(2).GetComponent<Healthbar>().takePercentDamage(1.0f, "God");
		}
	}

    public void DropGun()
    {
        gunEquipped = false;
    }

	private float CalculateJumpVerticalSpeed () {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * (GM._M.jumpHeight+jumpHeightModifier-weightModifier) * GM._M.gravity);
	}

	private void setControlStrings(){
		string pName = gameObject.name;

		if(pName.Contains("1")){
			Fire_str = "p1" + Fire_str;
			FWmv_str = "p1" + FWmv_str;
			Strf_str = "p1" + Strf_str;
			Haim_str = "p1" + Haim_str;
			Vaim_str = "p1" + Vaim_str;
			Jump_str = "p1" + Jump_str;
			Dash_str = "p1" + Dash_str;
			Zoom_str = "p1" + Zoom_str;
            Drop_str = "p1" + Drop_str;
		}else if(pName.Contains("2")){
			Fire_str = "p2" + Fire_str;
			FWmv_str = "p2" + FWmv_str;
			Strf_str = "p2" + Strf_str;
			Haim_str = "p2" + Haim_str;
			Vaim_str = "p2" + Vaim_str;
			Jump_str = "p2" + Jump_str;
			Dash_str = "p2" + Dash_str;
			Zoom_str = "p2" + Zoom_str;
            Drop_str = "p2" + Drop_str;
		}else if(pName.Contains("3")){
			Fire_str = "p3" + Fire_str;
			FWmv_str = "p3" + FWmv_str;
			Strf_str = "p3" + Strf_str;
			Haim_str = "p3" + Haim_str;
			Vaim_str = "p3" + Vaim_str;
			Jump_str = "p3" + Jump_str;
			Dash_str = "p3" + Dash_str;
			Zoom_str = "p3" + Zoom_str;
            Drop_str = "p3" + Drop_str;
		}else if(pName.Contains("4")){
			Fire_str = "p4" + Fire_str;
			FWmv_str = "p4" + FWmv_str;
			Strf_str = "p4" + Strf_str;
			Haim_str = "p4" + Haim_str;
			Vaim_str = "p4" + Vaim_str;
			Jump_str = "p4" + Jump_str;
			Dash_str = "p4" + Dash_str;
			Zoom_str = "p4" + Zoom_str;
            Drop_str = "p4" + Drop_str;
		}
	}
    public string GetFire_Str(){
        return Fire_str;
    }

    public string GetDrop_Str()
    {
        Debug.Log("Sending Drop_str: " + Drop_str);
        return Drop_str;
    }

    // piece of delays OnCollisionStay's ground check so we can't jump for 2 seconds after
    public void AllowJumpCheck()
    {
        //Manager.say("CAN CJECK JUMP", "eliot");
        canCheckForJump = true;
    }

	void OnCollisionStay(Collision floor){
		Vector3 tempVect;
        // we want to prevent isGrounded from being true and totalJumpsMade = 0 until 2 seconds later
		if(isGrounded == false && canCheckForJump){
			for(int i = 0; i < floor.contacts.Length; i++){
				tempVect = floor.contacts[i].normal;
				if( tempVect.y > floorInclineThreshold){
					isGrounded = true;
					totalJumpsMade = 0;
					return;
					//Manager.say("Collision normal is: " + tempVect);
                }
                else
                {
                   /* if (isHueBot)
                    {
                        targetVelocity = Vector3.zero;
                    }*/
                }
			}
		}
	}

    public void increaseSpeed(int speedInc)
    {
        speedBuffKillStreakPowerUp = true;
    }

	public void respawn()
	{
		transform.position = spawn;
	}
}



/*
			if(isGrounded && Input.GetButtonDown(Jump_str)){
				rigidbody.AddForce(Vector3.up * GM._M.jumpHeight); 
				
				Debug.Log("Jumping attempted!");
			}
			else if(Input.GetButtonDown(Jump_str)){
				Debug.Log("Jumping attempted! and FAILED");
			}
			//Running!!
			if(GM._M.runningAllowed && isGrounded && Input.GetKeyDown(KeyCode.LeftShift)){
				GM._M.movementSpeed = 10.0f;
			}
			else if(GM._M.movementSpeed == 10.0f){
				GM._M.movementSpeed = 6.0f;
			}
			
			
			float forwardSpeed = Input.GetAxis(FWmv_str);
			float sideSpeed = Input.GetAxis(Strf_str);
			
			speed = new Vector3( sideSpeed*, 0, forwardSpeed*GM._M.movementSpeed);
			
			speed = transform.rotation * speed;
			
			rigidbody.velocity = speed*Time.deltaTime;//(rigidbody.position + speed * Time.deltaTime);*/