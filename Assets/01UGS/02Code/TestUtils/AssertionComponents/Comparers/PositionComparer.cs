using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTest;

public class PositionComparer : VectorComparerBase<Vector3> {
	public enum CompareType {
		Equal,
		NotEqual
	}
	
	public CompareType compareType = CompareType.Equal;
	public double acceptableError = 0.0001f;
	
	protected override bool Compare(Vector3 a, Vector3 b) {
		var distance = Vector3.Distance(a, b);
		switch (compareType) {
			case CompareType.Equal:
				return Math.Abs(distance) <= acceptableError;
			case CompareType.NotEqual:
				return Math.Abs(distance) > acceptableError;
		}
		throw new Exception();
	}

	public override string GetFailureMessage() {
		var msg = base.GetFailureMessage();
		switch (compareType) {
			case CompareType.Equal:
				msg += ". Should be within " + acceptableError + "m.";
				break;
			case CompareType.NotEqual:
				msg += ". Should be at least " + acceptableError + "m away.";
				break;
		}
		return msg;
	}
}
