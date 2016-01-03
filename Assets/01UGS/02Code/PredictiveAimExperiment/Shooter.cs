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
	[Tooltip("This is not actually a force (mass * acceleration) as much as it is the constant velocity of the shot")]
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
		get { return m_targetUsesCharController ? targetXfrm.GetComponent<CharacterController>().velocity : targetXfrm.GetComponent<Rigidbody>().velocity; } // Supporth either CharController or RB!
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
					//Alteration from Kain for testing...
					//ORIGINAL var aimVector = GameUtilities.PredictiveAim(bulletStartPos, shotForce, targetPos, targetVelocity, shotObeysGravity ? -Physics.gravity.y : 0f); // negate gravity since multiplied by Vector3.down
					//Vector3 measuredTargetVelocity = targetVelocity.normalized * m_measuredSpeed;
					m_targetSpeedAtTimeOfCalculation = targetVelocity.magnitude;
					m_projectileSpeedAtTimeOfCalculation = shotForce;
					Vector3 aimVector;
					m_lastPredictionFoundValidSolution = GameUtilities.PredictiveAim(bulletStartPos, shotForce, targetPos, targetVelocity, shotObeysGravity ? -Physics.gravity.y : 0f, out aimVector); // negate gravity since multiplied by Vector3.down
					//...Alteration from Kain for testing
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
		
		//Alteration from Kain...
		shotRb.AddForce(launchVector, shotForceMode);
		m_lastProjectile = shotRb;
		m_lastProjectilePosition = bulletStartPos;
		//...Alteration from Kain
	}

	//////////////////////////////////////////////////////////////////////////////
	//Additions from Kain for observation purposes
	//Sorry for the different formatting, my Visual studio settings are different from yours
	//////////////////////////////////////////////////////////////////////////////
	float m_targetSpeedAtTimeOfCalculation = -1;
	float m_projectileSpeedAtTimeOfCalculation = -1;
	float m_accumulatedTime = 0;

	bool m_targetUsesCharController = false;
	bool m_lastPredictionFoundValidSolution = true;

	Rigidbody m_lastProjectile = null;
	Vector3 m_lastProjectilePosition;
	float m_measuredProjectileSpeed = 0;

	//////////////////////////////////////////////////////////////////////////////
	void Awake()
	{
		m_targetUsesCharController = (null != targetXfrm.GetComponent<CharacterController>());
		m_accumulatedTime = 0;
	}

	//////////////////////////////////////////////////////////////////////////////
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (Time.timeScale > 0)
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = 1;
			}
			return;
		}

		float deltaTime = Time.deltaTime;
		m_accumulatedTime += deltaTime;
		if (deltaTime < Mathf.Epsilon)
		{
			//avoid div/0 or div/tinyNumber issues
			return;
		}

		if (m_lastProjectile)
		{
			Vector3 currentPosition = m_lastProjectile.transform.position;
			m_measuredProjectileSpeed = (currentPosition - m_lastProjectilePosition).magnitude / deltaTime;
			m_lastProjectilePosition = currentPosition;
		}
	}

	//////////////////////////////////////////////////////////////////////////////
	static Rect s_infoRect = new Rect(100, 100, 500, 500);
	static Rect s_infoRectShadow = new Rect(101, 101, 500, 500);
	void OnGUI()
	{
		string msg = "Press [Space] to toggle pause";
		if (m_lastProjectile)
		{
			msg += "\nTargetSpeedAtTimeOfCalculation: " + m_targetSpeedAtTimeOfCalculation.ToString();
			msg += "\nProjectileSpeedAtTimeOfCalculation: " + m_projectileSpeedAtTimeOfCalculation.ToString();
			msg += "\nLastPredictionFoundValidSolution: " + m_lastPredictionFoundValidSolution.ToString();
		}
		else
		{
			msg += "\nAccumulatedTime: " + m_accumulatedTime.ToString();			
		}
		GUI.color = Color.black;
		GUI.Label(s_infoRectShadow, msg);
		GUI.color = Color.white;
		GUI.Label(s_infoRect, msg);

		Camera mainCam = Camera.main;

		msg = "bulletStartPos";
		DebuggingUtils.DrawTextInWorld(mainCam, bulletStartPos, msg, Color.red);

		if (m_lastProjectile)
		{
			msg = "lastProjectile";
			msg += "\nProjectileRigidBodySpeed: " + m_lastProjectile.velocity.magnitude.ToString();
			msg += "\nProjectileMeasuredSpeed: " + m_measuredProjectileSpeed.ToString();
			DebuggingUtils.DrawTextInWorld(mainCam, m_lastProjectile.transform.position, msg, Color.red);
		}
	}
}
