using UnityEngine;
using System.Collections;

/// <summary>
/// References:
/// http://www.gamasutra.com/blogs/KainShin/20090515/83954/Predictive_Aim_Mathematics_for_AI_Targeting.php
/// </summary>
public static class ProjectileAimingUtils {

	/// <summary>
	/// 
	/// </summary>
	/// <param name="targetPos">Current position of the target.</param>
	/// <param name="targetVelocity">Current velocity of the target.</param>
	/// <param name="bulletPos">Initial position of the projectile when fired.</param>
	/// <param name="bulletSpeed">The speed that the shot will fire at once launched.</param>
	/// <param name="shooterVelocity">How the barrel of the weapon is moving (since it's inherited by the projectile, this needs to be accounted for).</param>
	/// <param name="gravity">If supplied, gravity is accounted for with a slightly more expensive equation.</param>
	/// <returns></returns>
	// TODO: Account for shooterVelocity
	// TODO: Optimize for targetSpeed == 0 (or close to).
	public static Vector3 getLaunchVector(Vector3 targetPos, Vector3 targetVelocity, Vector3 bulletPos, float bulletSpeed, Vector3 shooterVelocity, Vector3? gravity = null) {
		// t = [ -2*D*St*cos(theta) ± Sqrt[ (2*D*St*cos(theta))^2 + 4*(Sb^2 - St^2)*D^2 ] ] / (2*(Sb^2 - St^2))
		var distanceSqr = Vector3.SqrMagnitude(targetPos - bulletPos);
		var distance = Mathf.Sqrt(distanceSqr);
		var targetSpeedSqr = targetVelocity.sqrMagnitude;
		var targetSpeed = Mathf.Sqrt(targetSpeedSqr);

		var cosTheta = Vector3.Dot(Vector3.Normalize(bulletPos - targetPos), Vector3.Normalize(targetVelocity)); // TODO: Optimize this twice: 1. first part is -distance-ish = re-use, 2. targetVelocity normalized needs its length which is calculated above

		var speedSqrDiff = (bulletSpeed * bulletSpeed) - targetSpeedSqr;
		var travelTime_P1 = -2f * distance * targetSpeed * cosTheta;
		var travelTime_P2 = Mathf.Sqrt( (travelTime_P1 * travelTime_P1) + (4f * speedSqrDiff * distanceSqr) );
		var travelTime_P3 = 2f * speedSqrDiff;

		// +/- for 2 intersections.  Choose lower
		var travelTime_Plus = (travelTime_P1 + travelTime_P2) / travelTime_P3;
		var travelTime_Minus = (travelTime_P1 - travelTime_P2) / travelTime_P3;
        var travelTime = Mathf.Min(travelTime_Plus, travelTime_Minus);
		if (0 >= travelTime) {
			travelTime = Mathf.Max(travelTime_Plus, travelTime_Minus);
        }

		var launchVector = gravity.HasValue ?
			targetVelocity - 0.5f * gravity.Value * travelTime + (targetPos - bulletPos) / travelTime
			:
			targetVelocity + (targetPos - bulletPos) / travelTime;

		return launchVector;
	}
		
}
