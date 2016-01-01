using UnityEngine;
using System;

/// <summary>
/// From http://stackoverflow.com/a/3487761 converted by CSharpatron and @Arakade.
/// </summary>
public class PredictiveAimJSConverted {

	/**
	 * <summary>
	 * Return the firing solution for a projectile starting at 'src' with
	 * velocity 'v', to hit a target, 'targetPos' with velocity 'targetVelocity'.
	 * N.b. converted to use X and *Z* (not Y).
	 * E.g.
	 * >>> intercept({x:2, z:4}, {x:5, z:7}, {x: 2, z:1}, 5)
	 * = {x: 8, z: 8.5}
	 * </summary>
	 * <param name="src"> position of shooter</param>
	 * <param name="targetPos"> position of target</param>
	 * <param name="targetVelocity"> velocity of target</param>
	 * <param name="v">speed of projectile</param>
	 * <returns>Coordinate at which to fire (and where intercept occurs) or null if no solution found.</returns>
	 */
	public static Vector3? intercept(Vector3 src,
									Vector3 targetPos,
									Vector3 targetVelocity,
									float v) {
	var tx = targetPos.x - src.x;
	var ty = targetPos.z - src.z;
	var tvx = targetVelocity.x;
	var tvy = targetVelocity.z;

	// Get quadratic equation components
	var a = tvx * tvx + tvy * tvy - v * v;
	var b = 2 * (tvx * tx + tvy * ty);
	var c = tx * tx + ty * ty;

	// Solve quadratic
	var ts = quad(a, b, c); // See quad(), below

	// Find smallest positive solution
	if (null != ts) {
		var t0 = ts[0];
		var t1 = ts[1];
		var t = Mathf.Min(t0, t1);
		if (t < 0)
			t = Mathf.Max(t0, t1);
		if (t > 0) {
			return new Vector3(targetPos.x + targetVelocity.x * t, 0f, targetPos.z + targetVelocity.z * t);
		}
	}

	return null; // { x: tx, y: ty};
}


/**
 * Return solutions for quadratic
 */
private static float[] quad(float a, float b, float c) {
	if (Mathf.Abs(a) < 1e-6f) {
		if (Mathf.Abs(b) < 1e-6f) {
			return Mathf.Abs(c) < 1e-6f ? new float[] { 0f, 0f } : null;
		} else {
			return new float[] { -c / b, -c / b };
		}
	} else {
		var disc = b * b - 4 * a * c;
		if (disc >= 0) {
			disc = Mathf.Sqrt(disc);
			a = 2 * a;
			return new float[] { (-b - disc) / a, (-b + disc) / a };
		}
	}
	return null;
}

}
