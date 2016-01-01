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

}
