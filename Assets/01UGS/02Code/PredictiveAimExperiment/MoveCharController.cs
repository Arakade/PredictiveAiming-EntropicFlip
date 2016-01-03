using UnityEngine;
using System.Collections.Generic;

public class MoveCharController : MonoBehaviour {

	[SerializeField]
	private float initialDelay = 1f; // tests benefit from little start time

	[SerializeField]
	private Vector3 m_characterVelocity = new Vector3(0f, 10f, 0f); // silly value so spot if not assigned

	[SerializeField]
	private bool m_applyGravity = true;

	private bool m_movementHasStarted = false;
	private CharacterController m_charController = null;
	private Vector3 m_fallingVelocity = Vector3.zero;
	private static Vector3 kTerminalVelocity = 2.0f*Physics.gravity;

	public void OnEnable() {
		if (initialDelay <= 0f) {
			move();
		} else {
			Invoke(moveName, initialDelay);
		}
	}

	private void OnDisable() {
		CancelInvoke();
	}

	private const string moveName = "move";
	private void move() {
		m_movementHasStarted = true;
	}

	//////////////////////////////////////////////////////////////////////////////
	//Additions from Kain for observation purposes
	//Sorry for the different formatting, my Visual studio settings are different from yours
	//////////////////////////////////////////////////////////////////////////////
	Vector3 m_lastPosition;
	float m_measuredSpeed = 0;
	string m_nameOfLastHitRigidBody = "";

	//////////////////////////////////////////////////////////////////////////////
	void Awake()
	{
		m_lastPosition = transform.position;
		m_charController = GetComponent<CharacterController>();
	}

	//////////////////////////////////////////////////////////////////////////////
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body == null || body.isKinematic)
		{
			return;
		}

		m_nameOfLastHitRigidBody = body.name;
	}
	
	//////////////////////////////////////////////////////////////////////////////
	void Update()
	{
		if (!m_movementHasStarted)
		{
			return;
		}

		float deltaTime = Time.deltaTime;

		Vector3 movementThisFrame = m_characterVelocity;

		if (m_applyGravity)
		{
			if (m_charController.isGrounded)
			{
				m_fallingVelocity = Vector3.zero;
			}
			else
			{
				m_fallingVelocity += deltaTime * Physics.gravity;
				if (m_fallingVelocity.magnitude > kTerminalVelocity.magnitude)
				{
					m_fallingVelocity = kTerminalVelocity;
				}
				movementThisFrame += m_fallingVelocity;
			}
		}

		movementThisFrame *= deltaTime;
		m_charController.Move(movementThisFrame);

		if (deltaTime < Mathf.Epsilon)
		{
			//avoid div/0 or div/tinyNumber issues
			return;
		}

		Vector3 currentPosition = transform.position;
		m_measuredSpeed = (currentPosition - m_lastPosition).magnitude / deltaTime;
		m_lastPosition = currentPosition;
	}

	//////////////////////////////////////////////////////////////////////////////
	void OnGUI()
	{
		Camera mainCam = Camera.main;
		string msg = "targetPos";
		msg += "\nTargetCharControllerSpeed: " + m_charController.velocity.magnitude.ToString();
		msg += "\nTargetMeasuredSpeed: " + m_measuredSpeed.ToString();
		msg += "\nLastHitRigidBody: ";
		if (m_nameOfLastHitRigidBody.Length > 0)
		{
			msg += m_nameOfLastHitRigidBody;
		}
		else
		{
			msg += "None";
		}
		DebuggingUtils.DrawTextInWorld(mainCam, transform.position, msg, Color.blue);
	}
}
