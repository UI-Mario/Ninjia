using UnityEngine;
using System.Collections;

public class NinjaMovementScript : MonoBehaviour {
	
	//Player speed and JumpForce. You can tweak these to change the game dynamics. 
	public float PlayerSpeed;
	public float JumpForce;

	//Do you want player to have double jump? Then make this DoubleJump boolean true :)
	public bool DoubleJump;
	

	//These variables are for the code. They track the current events of the player character.
	//You don't need to change or worry about them :)
	private MainEventsLog MainEventsLog_script;
	private bool DJ_available;
	private float JumpForceCount;
	private bool IsGrounded;
	private GameObject GroundedToOBJ;

	private float walljump_count;
	private bool WallTouch;
	private bool WallGripJustStarted;
	private GameObject WallOBJ;

	private bool PlayerLooksRight;

	//Checkpoint related things:
	public GameObject ActiveCheckpoint;



	//These booleans keep track which button is being pressed or not.
	private bool Btn_Left_bool;
	private bool Btn_Right_bool;
	private bool Btn_Jump_bool;

	//Here are reference slots for AnimationController and Player Sprite Object.
	public Animator AnimatorController;
	public GameObject MySpriteOBJ;
	private Vector3 MySpriteOriginalScale;

	//Here are reference slots for Player Particle Emitters
	public ParticleSystem WallGripParticles;
	private int WallGripEmissionRate;
	public ParticleSystem JumpParticles_floor;
	public ParticleSystem JumpParticles_wall;
	public ParticleSystem JumpParticles_doublejump;
	public ParticleSystem Particles_DeathBoom;


	//AudioSources play the audios of the scene.
	public AudioSource AudioSource_Jump;



	
	// Use this for initialization
	void Start () {

		//Just some default values for WallGrip Particle Emitter.
		WallGripEmissionRate = 10;
		WallGripParticles.emissionRate = 0;

		//Player characters looks right in the start of the scene.
		PlayerLooksRight = true;
		MySpriteOriginalScale = MySpriteOBJ.transform.localScale;

	}
	
	// Update is called once per frame
	void Update () {

		//Button commands from the keyboard
		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			Button_Left_press();		
		}
		if(Input.GetKeyUp (KeyCode.LeftArrow)) {
			Button_Left_release();		
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			Button_Right_press();		
		}
		if(Input.GetKeyUp (KeyCode.RightArrow)) {
			Button_Right_release();		
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			Button_Jump_press();		
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			Button_Jump_release();		
		}

		if (Input.GetKeyDown (KeyCode.A)) {
			Button_Jump_press();		
		}
		if (Input.GetKeyUp (KeyCode.A)) {
			Button_Jump_release();		
		}

		if (walljump_count >= 0) {
			walljump_count -= Time.deltaTime;		
		}

	}


	void FixedUpdate(){

		//The actual movement happens here.

		//Checks is the player pressing left or right button.
		if(Btn_Left_bool == true && Btn_Right_bool == false){
			if(PlayerLooksRight == true && WallTouch == false){
				PlayerLooksRight = false;
				MySpriteOBJ.transform.localScale = new Vector3(-MySpriteOriginalScale.x,MySpriteOriginalScale.y,MySpriteOriginalScale.z);
			}
			this.GetComponent<Rigidbody2D>().AddForce(new Vector2(-PlayerSpeed*Time.deltaTime,0f));
		}else if(Btn_Left_bool == false && Btn_Right_bool == true){
			if(PlayerLooksRight == false && WallTouch == false){
				PlayerLooksRight = true;
				MySpriteOBJ.transform.localScale = MySpriteOriginalScale;
			}
			this.GetComponent<Rigidbody2D>().AddForce(new Vector2(PlayerSpeed*Time.deltaTime,0f));
		}


		//Slowdown the player fall if touching a wall
		if (IsGrounded == false && WallTouch == true) {
			this.GetComponent<Rigidbody2D>().velocity = new Vector2 (this.GetComponent<Rigidbody2D>().velocity.x, Physics2D.gravity.y * 0.01f);
		}


		//Lift player up if jump is happening
		if (Btn_Jump_bool == true && JumpForceCount > 0) {
			this.GetComponent<Rigidbody2D>().velocity = new Vector2(this.GetComponent<Rigidbody2D>().velocity.x,JumpForce);
			JumpForceCount -= 0.1f*Time.deltaTime;			
		}


		//Send variables to Animation Controller
		AnimatorController.SetFloat ("HorizontalSpeed", this.GetComponent<Rigidbody2D>().velocity.x*this.GetComponent<Rigidbody2D>().velocity.x);
		AnimatorController.SetFloat ("VerticalSpeed", this.GetComponent<Rigidbody2D>().velocity.y);
		AnimatorController.SetBool ("Grounded", IsGrounded);
		AnimatorController.SetBool ("Walled", WallTouch);

	}


	void OnCollisionEnter2D(Collision2D coll) {

		//Did the player hit the ground
		if (coll.gameObject.tag == "Ground" && IsGrounded == false) {
			DJ_available = false;
			GroundedToOBJ = coll.gameObject;
			this.transform.parent = coll.gameObject.transform;
			IsGrounded = true;
		}

		//Did the player hit the wall
		if(coll.gameObject.tag == "Wall" && this.GetComponent<Rigidbody2D>().velocity.y < 0f) {

			DJ_available = false;
			WallOBJ = coll.gameObject;
			this.transform.parent = coll.gameObject.transform;

			WallTouch = true;

			//Check that the player is facing to the right direction
			if(WallOBJ.transform.position.x < this.transform.position.x){
				PlayerLooksRight = true;
				MySpriteOBJ.transform.localScale = MySpriteOriginalScale;
			}else{
				PlayerLooksRight = false;
				MySpriteOBJ.transform.localScale = new Vector3(-MySpriteOriginalScale.x,MySpriteOriginalScale.y,MySpriteOriginalScale.z);
			}

			//Start emiting smoke particles when touching the wall
			WallGripParticles.emissionRate = WallGripEmissionRate;
		}

		if (coll.gameObject.tag == "Roof") {
			JumpForceCount = 0f;
		}

	}

	//OnCollisionStay we are making sure that Wall and Ground collisions are getting registered...
	void OnCollisionStay2D(Collision2D coll) {
		
		if(coll.gameObject.tag == "Wall" && WallTouch == false && this.GetComponent<Rigidbody2D>().velocity.y < 0f) {
			
			DJ_available = false;
			WallOBJ = coll.gameObject;
			WallTouch = true;
			
			if(WallOBJ.transform.position.x < this.transform.position.x){
				PlayerLooksRight = true;
				MySpriteOBJ.transform.localScale = MySpriteOriginalScale;
			}else{
				PlayerLooksRight = false;
				MySpriteOBJ.transform.localScale = new Vector3(-MySpriteOriginalScale.x,MySpriteOriginalScale.y,MySpriteOriginalScale.z);
			}
			
			WallGripParticles.emissionRate = WallGripEmissionRate;
			
		}
		
		if (coll.gameObject.tag == "Ground" && IsGrounded == false) {
			DJ_available = false;
			GroundedToOBJ = coll.gameObject;
			IsGrounded = true;
		}
	}


	//Here we check if the player is jumping or moving away from the wall or ground.
	void OnCollisionExit2D(Collision2D coll) {

		if (coll.gameObject.tag == "Ground" && coll.gameObject == GroundedToOBJ) {
			DJ_available = true;
			GroundedToOBJ = null;
			this.transform.parent = null;

			IsGrounded = false;
		}

		if (coll.gameObject.tag == "Wall" && coll.gameObject == WallOBJ) {
			//This makes the walljump easier. Player is able to do the wall jump even few miliseconds after he let go of the wall.
			DJ_available = true;
			walljump_count = 0.16f;

			this.transform.parent = null;
			WallOBJ = null;
			WallTouch = false;
			WallGripParticles.emissionRate = 0;
		}
	}


	public void NinjaDies(){
		Particles_DeathBoom.Emit (50);


		this.gameObject.transform.position = ActiveCheckpoint.transform.position;
		this.GetComponent<Rigidbody2D>().velocity = Vector2.zero;


		//Send message to MainEventsLog. First checks if the reference path is set. If not, it will MainEventsLog from the scene.
		if(MainEventsLog_script == null){
			MainEventsLog_script = GameObject.FindGameObjectWithTag("MainEventLog").GetComponent<MainEventsLog>();
		}
		MainEventsLog_script.PlayerDied();
	}



	//This region is for Button events. (These same events are called from Keyboard and Touch Buttons)
	#region ButtonVoids

	public void Button_Left_press(){
		Btn_Left_bool = true;
	}

	public void Button_Left_release(){
		Btn_Left_bool = false;
	}

	public void Button_Right_press(){
		Btn_Right_bool = true;
	}
		
	public void Button_Right_release(){
		Btn_Right_bool = false;
	}


	public void Button_Jump_press(){

		Btn_Jump_bool = true;


		//If you are on the ground. Do the Jump.
		if (IsGrounded == true) {
			DJ_available = true;
			AudioSource_Jump.Play();
			JumpForceCount = 0.02f;
			this.GetComponent<Rigidbody2D>().velocity = new Vector2(this.GetComponent<Rigidbody2D>().velocity.x,JumpForce);
			JumpParticles_floor.Emit(20);

		//If you are in the air and DoubleJump is available. Do it!
		}else if(DoubleJump == true && DJ_available == true && WallTouch == false){
			DJ_available = false;
			AudioSource_Jump.Play();
			JumpForceCount = 0.02f;
			this.GetComponent<Rigidbody2D>().velocity = new Vector2(this.GetComponent<Rigidbody2D>().velocity.x,JumpForce);
			JumpParticles_doublejump.Emit(10);
		}


		//If you touch the wall or just let go. And are defenitly not in the ground. Do the Wall Jump!
		if ((WallTouch == true || walljump_count > 0f) && IsGrounded == false) {
			DJ_available = true;
			AudioSource_Jump.Play();
			JumpForceCount = 0.02f;
			JumpParticles_wall.Emit(20);
			if(PlayerLooksRight == false){
				this.GetComponent<Rigidbody2D>().AddForce (new Vector2 (-JumpForce*32f, 0f));
			}else{
				this.GetComponent<Rigidbody2D>().AddForce (new Vector2 (JumpForce*32f, 0f));
			}
		}


	
	}

	public void Button_Jump_release(){
		JumpForceCount = 0f;
		Btn_Jump_bool = false;
	}
	
	#endregion


}
