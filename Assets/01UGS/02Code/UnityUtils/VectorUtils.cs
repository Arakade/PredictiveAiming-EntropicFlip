using UnityEngine;
using System.Collections;

namespace UGS {
public static class VectorUtils {

	public static Vector3 randomVectorInRange(Vector3 min, Vector3 max) {
		Vector3 result = new Vector3(
			Random.Range(min.x, max.x),
			Random.Range(min.y, max.y),
			Random.Range(min.z, max.z)
			);
		return result;
	}
	
	/// <summary>
	/// Find component of vector in plane -- not necessarily normalized.
	/// Finds vector perpendicular to both (i.e. sideways to vector and plane).  Then finds vector perpendicular to that and plane.
	/// Using some maths I forgot from http://www.euclideanspace.com/maths/geometry/elements/plane/lineOnPlane/
	/// See unit tests for example values.
	/// </summary>
	/// <param name="vector"></param>
	/// <param name="planeNormal"></param>
	/// <returns></returns>
	public static Vector3 vectorComponentInPlane(Vector3 vector, Vector3 planeNormal) {
		// N.b. Need to check whether vector needs normalizing?
		return Vector3.Cross(planeNormal, Vector3.Cross(vector, planeNormal));
	}
	
	/// <summary>
	/// Find component of vector in plane -- post-normalized!
	/// Finds vector perpendicular to both (i.e. sideways to vector and plane).  Then finds vector perpendicular to that and plane.
	/// Using some maths I forgot from http://www.euclideanspace.com/maths/geometry/elements/plane/lineOnPlane/
	/// See unit tests for example values.
	/// </summary>
	/// <param name="vector"></param>
	/// <param name="planeNormal"></param>
	/// <returns></returns>
	public static Vector3 vectorComponentInPlaneNormalized(Vector3 vector, Vector3 planeNormal) {
		// N.b. Need to check whether vector needs normalizing?
		return Vector3.Cross(planeNormal, Vector3.Cross(vector, planeNormal)).normalized;
	}

	public static string ToStringAccurate(this Vector3 v) {
//		float[] values = { 0, 1, 10, 1234.56789f, 123.45e-5f, 123.45e-9f };
//		foreach (var v in values) string.Format("{0:G} = {0:E} = {0:#0.##E0} = {0:0.##E0}", v).Dump();
//		return string.Format("({0:0.##E0}, {1:0.##E0}, {2:0.##E0})", v.x, v.y, v.z);
		return string.Format("({0:g}, {1:g}, {2:g})", v.x, v.y, v.z); // see http://msdn.microsoft.com/en-us/library/dwhawy9k%28v=vs.100%29.aspx#GFormatString
	}
	
	public static bool isNaN(this Vector3 v) {
		return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
	}
	
}
}
