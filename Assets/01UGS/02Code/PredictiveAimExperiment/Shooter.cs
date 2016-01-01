using UnityEngine;
using System.Collections;
using UGS;

public class Shooter : MonoBehaviour {

	[SerializeField]
	public Transform targetXfrm = default(Transform);

	[SerializeField]
	private Transform gunTip = default(Transform);

	[SerializeField]
	private Transform shotPrefab = default(Transform);

	[SerializeField]
	private ForceMode shotForceMode = ForceMode.VelocityChange;

	[SerializeField]
	private float shotForce = 1f;

	[Tooltip("Which predictive aiming code to use.")]
	[SerializeField]
	private PredictiveCodeToUse whichPredictiveCodeToUse = PredictiveCodeToUse.Kain;

	private enum PredictiveCodeToUse { Rupert, Kain, JavaScript }

	private Vector3 targetPos {
		get { return targetXfrm.position; }
	}

	private Vector3 bulletStartPos {
		get { return gunTip.position; }
	}

	private Vector3 targetVelocity {
		get { return targetXfrm.GetComponent<Rigidbody>().velocity; } // Assume it's a RB!
	}

	private Vector3 ownVelocity {
		get { return Vector3.zero; } // TODO: Will need actual velocity in time
	}

	private bool shotObeysGravity {
		get { return shotPrefab.GetComponent<Rigidbody>().useGravity; }
	}

	public void shootAtTarget() {
		var aimVector = aim();
		launch(aimVector);
	}

	private Vector3 aim() {
		var aimVector = aimImpl();
		var aimFailure = !aimVector.HasValue;
		if (aimFailure) {
			Debug.LogFormat(this, "{0} aimVector:{1} so using gunTip.forward", this, aimVector);
			aimVector = gunTip.forward * shotForce;
		}
		Debug.LogFormat(this, "{0} aimVector:{1}", this, aimVector);
		Debug.DrawRay(bulletStartPos, aimVector.Value, aimFailure ? Color.magenta : Color.red, 4f, false);
		return aimVector.Value;
	}

	private Vector3? aimImpl() {
		switch (whichPredictiveCodeToUse) {
			case PredictiveCodeToUse.Rupert: {
					var aimVector = ProjectileAimingUtils.getLaunchVector(targetPos, targetVelocity, bulletStartPos, shotForce, ownVelocity, shotObeysGravity ? Physics.gravity : (Vector3?) null);
					return aimVector.isNaN() ? (Vector3?) null : aimVector;
				}

			case PredictiveCodeToUse.Kain: {
					Debug.LogFormat(this, "{0} shooting with Kain's code", this);
					var aimVector = GameUtilities.PredictiveAim(bulletStartPos, shotForce, targetPos, targetVelocity, shotObeysGravity ? -Physics.gravity.y : 0f); // negate gravity since multiplied by Vector3.down
					return aimVector.isNaN() ? (Vector3?) null : aimVector;
				}

			case PredictiveCodeToUse.JavaScript:
				return PredictiveAimJSConverted.intercept(bulletStartPos, targetPos, targetVelocity, shotForce);
				
			default:
				throw new System.NotSupportedException(string.Format("Implement {0} support", whichPredictiveCodeToUse));
		}
	}

	private void launch(Vector3 aimVector) {
		var shot = (Transform) Instantiate(shotPrefab, bulletStartPos, gunTip.rotation);
		shot.parent = transform.parent; // same parent so tidied by integration tests
		var shotRb = shot.GetComponent<Rigidbody>();
		//var launchVector = shot.forward * shotForce;
		var launchVector = aimVector;
		shotRb.AddForce(launchVector, shotForceMode);
	}

}
