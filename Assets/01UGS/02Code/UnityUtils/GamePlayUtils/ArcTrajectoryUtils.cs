using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public static class ArcTrajectoryUtils {
	
	public static readonly float gravity = Mathf.Abs(Physics.gravity.y); // N.b. will not update if gravity changed
	
	public static Vector3 calculateLaunchVelocityVector(Vector3 vectorToTarget, float maxHeight = float.NaN) {
		if (0 != vectorToTarget.y) {
			if (0 > vectorToTarget.y) {
				// launch point above target
				float horizontalLaunchVelocity = calculateDownwardLaunchVelocity(vectorToTarget);
				Vector3 launchVector = convertHorizontalScalarsToVector(vectorToTarget, horizontalLaunchVelocity);
				Debug.LogFormat("launchVector:{0} (for lower vectorToTarget:{1})", launchVector, vectorToTarget);
				return launchVector;
			} else {
				// Launch point below target
				Debug.LogFormat("{0}: Launch point below target not implemented, vectorToTarget:{1}", typeof (ArcTrajectoryUtils).Name, vectorToTarget);
				return vectorToTarget;
				// TODO: When fixed, update PowerUpManager.isInvalidLandingPoint()
			}
		} else {
			// Launch point at same height as target
			Assert.IsTrue(!float.IsNaN(maxHeight), string.Format("No maxHeight supplied for horizontal launch with vector:{0}", vectorToTarget));
			return calculateHorizontalLaunchVector(vectorToTarget, maxHeight);
		}
	}
	
#region HorizontalLaunch
	private static Vector3 calculateHorizontalLaunchVector(Vector3 vectorToTarget, float maxHeight) {
		Vector2 planeVectorToTarget = new Vector2(vectorToTarget.x, vectorToTarget.z);
		float range = planeVectorToTarget.magnitude;
		float launchAngle = calculateHorizontalLaunchElevation(range, maxHeight);
		float launchVelocity = calculateHorizontalLaunchVelocity(range, launchAngle);
		Vector3 launchVector = convert3SpaceScalarsToVector(vectorToTarget, launchVelocity, launchAngle);
		Debug.LogFormat("launchVector:{0} (for vectorToTarget:{1}, maxHeight:{2})", launchVector, vectorToTarget, maxHeight);
		return launchVector;
	}
	
	private static float calculateHorizontalLaunchElevation(float range, float maxHeight) {
		float launchAngle = Mathf.Atan(4 * maxHeight / range);
		Debug.LogFormat("launchAngle:{0} (for range:{1}, maxHeight:{2})", launchAngle, range, maxHeight);
		return launchAngle;
	}
	
	private static float calculateHorizontalLaunchVelocity(float range, float launchElevation) {
		float launchVelocity = Mathf.Sqrt(range * gravity / Mathf.Sin(2 * launchElevation)); // assumes gravity is only in y axis
		Debug.LogFormat("launchVelocity:{0} (for range:{1}, launchElevation:{2})", launchVelocity, range, launchElevation);
		return launchVelocity;
	}
#endregion
	
#region DownwardLaunch
	private static float calculateDownwardLaunchVelocity(Vector3 vectorToTarget) {
		float height = -vectorToTarget.y;
		Assert.IsTrue(0 <= height, string.Format("Launching horizontally only valid when above target, vectorToTarget:{0}", vectorToTarget));
		Vector2 planeVectorToTarget = new Vector2(vectorToTarget.x, vectorToTarget.z);
		float range = planeVectorToTarget.magnitude;
		float launchVelocity = range * Mathf.Sqrt(gravity / (2 * height));
		Debug.LogFormat("Horizontal launchVelocity:{0} (for vectorToTarget:{1})", launchVelocity, vectorToTarget);
		return launchVelocity;
	}
#endregion
	
#region Utils
	private static Vector3 convert3SpaceScalarsToVector(Vector3 vectorToTarget, float launchVelocity, float launchElevation) {
		float verticalLaunchVelocity = launchVelocity * Mathf.Sin(launchElevation);
		float horizLaunchVelocity = launchVelocity * Mathf.Cos(launchElevation);
		Vector3 launchVector = convertHorizontalScalarsToVector(vectorToTarget, horizLaunchVelocity, verticalLaunchVelocity);
		return launchVector;
	}
	
	private static Vector3 convertHorizontalScalarsToVector(Vector3 vectorToTarget, float horizLaunchVelocity, float verticalLaunchVelocity = 0f) {
		float horizAngleToTarget = Mathf.Atan2(vectorToTarget.z, vectorToTarget.x);
		float x = horizLaunchVelocity * Mathf.Cos(horizAngleToTarget);
		float z = horizLaunchVelocity * Mathf.Sin(horizAngleToTarget);
		Vector3 launchVector = new Vector3(x, verticalLaunchVelocity, z);
		return launchVector;
	}
#endregion
	
}

