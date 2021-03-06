using UnityEngine;
using System.Collections.Generic;

public class MoveRigidbody : MonoBehaviour {

	[SerializeField]
	private float initialDelay = 1f; // tests benefit from little start time

	[SerializeField]
	private Vector3 force = new Vector3(0f, 10f, 0f); // silly value so spot if not assigned

	[SerializeField]
	private ForceMode forceMode = ForceMode.VelocityChange;

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
		var rb = GetComponent<Rigidbody>();
		rb.AddForce(force, forceMode);
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
		float deltaTime = Time.deltaTime;
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
		msg += "\nTargetRigidBodySpeed: " + GetComponent<Rigidbody>().velocity.magnitude.ToString();
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
