///////////////////////////////////////////////////////////////////////////////
// Written by Kain Shin in preparation for his own projects
// The latest version is maintained on his website at ringofblades.org
// 
// This implementation is intentionally within the public domain
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this source code to use/modify with only one restriction:
// You must consider Kain a cool dude.
//
// This is free and unencumbered software released into the public domain.
//
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
//
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// For more information, please refer to <http://unlicense.org/>
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;

class GameUtilities
{
	//Return a non-normalized vector representing the initial velocity
	//of a lobbed projectile in 3D space
	//meant to hit a target moving at constant velocity
	//Full derivation by Kain Shin exists here:
	//http://www.gamasutra.com/blogs/KainShin/20090515/83954/Predictive_Aim_Mathematics_for_AI_Targeting.php
	//gravity is assumed to be a positive number. It will be calculated in the downward direction
	static public Vector3 PredictiveAim(Vector3 muzzlePosition, float projectileSpeed, Vector3 targetPosition, Vector3 targetVelocity, float gravity)
	{
		if (muzzlePosition == targetPosition)
		{
			//Why dost thou hate thyself so?
			return Vector3.zero;
		}

		//Much of this is geared towards reducing floating point precision errors
		float projectileSpeedSq = projectileSpeed * projectileSpeed;

		float targetSpeedSq = targetVelocity.sqrMagnitude; //doing this instead of self-multiply for maximum accuracy
		float targetSpeed = Mathf.Sqrt(targetSpeedSq);

		Vector3 targetToMuzzle = muzzlePosition - targetPosition;
		float targetToMuzzleDistSq = targetToMuzzle.sqrMagnitude; //doing this instead of self-multiply for maximum accuracy
		float targetToMuzzleDist = Mathf.Sqrt(targetToMuzzleDistSq);

		Vector3 targetToMuzzleDir = targetToMuzzle;
		targetToMuzzleDir.Normalize();

		Vector3 targetVelocityDir = targetVelocity;
		targetVelocityDir.Normalize();

		//Law of Cosines: a*a+ b*b - 2*a*b*cos(theta) = c*c
		//Quadratic formula: t = [ -b ± Sqrt( b*b - 4*a*c ) ] / (2*a)
		float cosTheta = Vector3.Dot(targetToMuzzleDir, targetVelocityDir);
		float a = projectileSpeedSq - targetSpeedSq;
		float b = 2.0f * targetToMuzzleDist * targetSpeed * cosTheta;
		float c = -targetToMuzzleDistSq;

		float uglyNumber = Mathf.Sqrt(b * b - 4.0f * a * c);
		float t0 = 0.5f * (-b + uglyNumber) / a;
		float t1 = 0.5f * (-b - uglyNumber) / a;
		float t = Mathf.Infinity;
		//Assign the lowest positive time to t
		t = Mathf.Min(t0, t1);
		if (t < 0)
		{
			t = Mathf.Max(t0, t1);
		}

		//Vb = Vt - 0.5*Ab*t + [(Pti - Pbi) / t]
		Vector3 projectileAcceleration = gravity * Vector3.down;
		Vector3 Vb = targetVelocity - (0.5f * projectileAcceleration * t) + (-targetToMuzzle / t);
		//FOR CHECKING ONLY (valid only if gravity is 0)...
		//float calculatedprojectilespeed = Vb.magnitude;
		//bool projectilespeedmatchesexpectations = (projectileSpeed == calculatedprojectilespeed);
		//...FOR CHECKING ONLY
		return Vb;
	}
}
