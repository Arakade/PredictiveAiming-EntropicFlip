#define DEBUG_UGS

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTest;

/// <summary>
/// Ray-cast from one point to another and check whether blocked.
/// Depending on <see cref="RayCastHelper.rayCastType"/>, casts from referenced Vector or '<see cref="RayCastHelper.point2"/>'
/// to referenced Vector, <see cref="RayCastHelper.point2"/> or <see cref="RayCastHelper.point3"/>.
/// Distance is slightly more than distance between points (to allow tolerance).
/// </summary>
public class RayCastComparer : ActionBaseGeneric<Vector3> {

	public RayCastHelper settings;

	/// <summary>
	/// Set by <see cref="RayCastHelper.compareOrGetErrorMessage(Vector3)"/>.
	/// Null when it succeeds or a message when it fails.
	/// </summary>
	private string failureMessage;

	[Serializable]
	public class RayCastHelper {
		[Tooltip(
			"FromSelfToPoint2:   ray-cast from above referenced Vector to 'point2'.\n" +
			"FromPoint2ToSelf:   ray-cast from 'point2' to above referenced Vector.\n" +
			"FromPoint2ToPoint3: ray-cast from 'point2' to 'point3' (and ignore info above).")]
		public RayCastType rayCastType = RayCastType.FromPoint2ToPoint3;

		public enum RayCastType { FromPoint2ToSelf, FromSelfToPoint2, FromPoint2ToPoint3 }

		public Vector3 point2;

		public Vector3 point3;

		public LayerMask layerMask = -1;
		
		public bool simpleHitIsSufficient;

		public float extraDistanceForHit = 1f;

		public float acceptableError = 0.001f;
		
		/// <summary>
		/// Last value supplied to <see cref="compareOrGetErrorMessage(Vector3)"/>.  Only set after calling.
		/// </summary>
		private Vector3? lastPoint1;

		/// <summary>
		/// Last hit output.  Only set after calling <see cref="compareOrGetErrorMessage(Vector3)"/>.
		/// </summary>
		private RaycastHit? lastHitInfo;

		/// <summary>
		/// Whether the last <see cref="compareOrGetErrorMessage(Vector3)"/> passed.
		/// </summary>
		private bool lastPassed;

		/// <summary>
		/// Returns null on success or an error message otherwise.
		/// </summary>
		/// <param name="point1"></param>
		/// <returns></returns>
		public string compareOrGetErrorMessage(Vector3 point1) {
			var retVal = inner(point1);
			lastPassed = (null == retVal);
			return retVal;
		}

		private string inner(Vector3 point1) {
			lastPoint1 = point1;
			var hitPoint = rayCastForPoint(point1);
			if (!hitPoint.HasValue)
				return "No point hit at all";

			if (simpleHitIsSufficient) { // already failed if no hitPoint so this is success
				return null; // success
			}

			var theEnd = end(point1);
			var hitProximity = (hitPoint.Value - theEnd).magnitude;
			var closeEnough = hitProximity < acceptableError;
			if (closeEnough) {
				return null;
			} else {
				return string.Format("ray hit at {0} which was {1} away from target:{2} and greater than acceptableError:{3}", hitPoint.Value, hitProximity, theEnd, acceptableError);
			}
		}

		private Vector3? rayCastForPoint(Vector3 point1) {
			var theRay = ray(point1);
			var theDistance = distance(point1);
			RaycastHit hitInfo;
			var isHit = Physics.Raycast(theRay, out hitInfo, theDistance, layerMask);
			lastHitInfo = isHit ? hitInfo : (RaycastHit?) null;
			if (!isHit)
				return null;

			return hitInfo.point;
		}

		private Ray ray(Vector3 point1) {
			var direction = delta(point1).normalized;
			var ray = new Ray(start(point1), direction);
			return ray;
		}
		
		private float distance(Vector3 point1) {
			var distance = delta(point1).magnitude;
			distance += extraDistanceForHit;
			return distance;
		}

		private Vector3 delta(Vector3 point1) {
			return end(point1) - start(point1);
		}

		private Vector3 start(Vector3 point1) {
			switch (rayCastType) {
				case RayCastType.FromSelfToPoint2:
					return point1;

				case RayCastType.FromPoint2ToSelf:
				case RayCastType.FromPoint2ToPoint3:
					return point2;

				default:
					throw new NotSupportedException("needs updating to handle " + rayCastType);
			}
		}

		private Vector3 end(Vector3 point1) {
			switch (rayCastType) {
				case RayCastType.FromSelfToPoint2:
					return point2;

				case RayCastType.FromPoint2ToSelf:
					return point1;

				case RayCastType.FromPoint2ToPoint3:
					return point3;

				default:
					throw new NotSupportedException("needs updating to handle " + rayCastType);
			}
		}

		public override string ToString() {
			if (!lastPoint1.HasValue) {
				return string.Format("{0}(NOT YET USED)", base.ToString());
			}

			var point1 = lastPoint1.Value;
			var hitMessage = lastHitInfo.HasValue ? string.Format("hit:{0}, at:{1}", lastHitInfo.Value.transform, lastHitInfo.Value.point) : "(nothing hit)";
			return string.Format("{0}(type:{1}, from:{2}, to:{3}, layerMask:{4}, simpleHitIsSufficient:{5}, extraDistanceForHit:{6}, acceptableError:{7}, {8}, lastPassed:{9})",
				base.ToString(),
				Enum.GetName(typeof (RayCastType), rayCastType),
				start(point1), end(point1),
				layerMask, simpleHitIsSufficient,
				extraDistanceForHit, acceptableError,
				hitMessage, lastPassed);
		}

		public void addDebugGizmo() {
			if (!lastPoint1.HasValue)
				return;

			var colour = lastPassed ? Color.green : new Color(0.5f, 0.5f, 0f);
			Debug.DrawLine(start(lastPoint1.Value), end(lastPoint1.Value), colour, 0.5f);
			//DebugAssistGizmos.singleton.addLine(ToString(), start(lastPoint1.Value), end(lastPoint1.Value), colour, 0.5f);
		}

#if UNITY_EDITOR
		public void OnDrawGizmosSelected() {
			if (!lastPoint1.HasValue)
				return;

			Gizmos.DrawLine(start(lastPoint1.Value), end(lastPoint1.Value));
		}
#endif // UNITY_EDITOR
    }

    protected override bool Compare(Vector3 point) {
		failureMessage = settings.compareOrGetErrorMessage(point);
		var result = (null == failureMessage);
#if DEBUG_UGS
		Debug.LogFormat(this, "{0}: {1}, failureMessage:{2}", this, settings, failureMessage);
		if (!result) {
			settings.addDebugGizmo();
		}
#endif // DEBUG_UGS
		return result;
	}

	public override string GetFailureMessage() {
		var msg = base.GetFailureMessage();
		msg += ". " + failureMessage;
		return msg;
	}

#if UNITY_EDITOR
    public void OnDrawGizmosSelected() {
		settings.OnDrawGizmosSelected();
	}
#endif // UNITY_EDITOR
}
