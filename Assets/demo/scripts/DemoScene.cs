using UnityEngine;
using System.Collections;
using Prime31;


public class DemoScene : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;

	//Custom Added
	public int jumpsAllowed = 1;
	public float dashDistance = 1.5f;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;

	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;

	private int jumpCount = 0;
	private float dashCooldown = 0;
	private float dashDistanceThisFrame = 1f;

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_controller = GetComponent<CharacterController2D>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f )
			return;
		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider2D col )
	{
		Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		if (_controller.isGrounded)
		{
			_velocity.y = 0;
			jumpCount = 0;
			dashCooldown = 0;
		} 
		else if (_controller.isWalled) //Reset for double jump
		{
			jumpCount = 0;
		}
						
		if (Input.GetKey (KeyCode.RightArrow) || Input.GetKey (KeyCode.D))
		{
			normalizedHorizontalSpeed = 1;
			
			if (transform.localScale.x < 0f)
				transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z); //Flip animation around

			if (_controller.isGrounded)
				_animator.Play (Animator.StringToHash ("Run"));
		} else if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey (KeyCode.A))
		{
			normalizedHorizontalSpeed = -1;
			if (transform.localScale.x > 0f)
				transform.localScale = new Vector3 (-transform.localScale.x, transform.localScale.y, transform.localScale.z);

			if (_controller.isGrounded)
				_animator.Play (Animator.StringToHash ("Run"));
		} else 
		{
			normalizedHorizontalSpeed = 0;
			if (_controller.isGrounded)
					_animator.Play (Animator.StringToHash ("Idle"));
		}

		if (Input.GetKeyDown (KeyCode.LeftShift))
		{
			if (dashCooldown <= 0 && dashDistanceThisFrame == 1) 
			{
				dashDistanceThisFrame = dashDistance;
				dashCooldown = 5f;
			}
		}

		if (dashCooldown > 0)
			dashCooldown -= 1 * Time.deltaTime;

			// we can only jump whilst grounded OR belew set double Jump limit
			if ((jumpCount < jumpsAllowed) && (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.Space) || Input.GetKeyDown (KeyCode.W))) {
				++jumpCount;
				_velocity.y = Mathf.Sqrt (2f * jumpHeight * -gravity);
				_animator.Play (Animator.StringToHash ("Jump"));
			}
			
		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?

		smoothedMovementFactor = dashDistanceThisFrame > 1 ? 0f : smoothedMovementFactor; //Dashing Smoothing

		_velocity.x = Mathf.Lerp( _velocity.x * dashDistanceThisFrame, normalizedHorizontalSpeed * runSpeed, Time.deltaTime * smoothedMovementFactor );

		if (dashDistanceThisFrame > 1)
			dashDistanceThisFrame--;

		// apply gravity before moving
		_velocity.y += gravity * Time.deltaTime;

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets uf jump down through one way platforms
		if( _controller.isGrounded && (Input.GetKey( KeyCode.DownArrow ) || Input.GetKey( KeyCode.S ) ) )
		{
			_velocity.y *= 3f;
			_controller.ignoreOneWayPlatformsThisFrame = true;
			//_animator.Play (Animator.StringToHash("Duck") );
		}

		_controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
	}

}
