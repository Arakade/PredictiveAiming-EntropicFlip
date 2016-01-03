using UnityEngine;
using System.Collections.Generic;

public class ShootCommand : MonoBehaviour {

	[SerializeField]
	private float initialDelay = 1f; // tests benefit from little start time

	public void OnEnable() {
		if (initialDelay <= 0f) {
			shoot();
		} else {
			Invoke(shootName, initialDelay);
		}
	}

	private void OnDisable() {
		CancelInvoke();
	}

	private const string shootName = "shoot";
	private void shoot() {
		var shooter = GetComponent<Shooter>();
		shooter.shootAtTarget();
	}


}
