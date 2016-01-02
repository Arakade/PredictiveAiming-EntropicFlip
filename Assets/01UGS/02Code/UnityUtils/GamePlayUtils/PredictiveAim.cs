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
	//////////////////////////////////////////////////////////////////////////////
	//This implies that no solution exists for this situation as the target may literally outrun the projectile with its current direction
	//In cases like that, we simply aim at the place where the target will be 1 to 5 seconds from now.
	//Feel free to randomize t at your discretion for your specific game situation if you want that guess to feel appropriately noisier
	static float PredictiveAimWildGuessAtImpactTime()
	{
		return Random.Range(1, 5);
	}

	//////////////////////////////////////////////////////////////////////////////
	//Return a non-normalized vector representing the initial velocity
	//of a lobbed projectile in 3D space
	//meant to hit a target moving at constant velocity
	//Full derivation by Kain Shin exists here:
	//http://www.gamasutra.com/blogs/KainShin/20090515/83954/Predictive_Aim_Mathematics_for_AI_Targeting.php
	//gravity is assumed to be a positive number. It will be calculated in the downward direction
	static public Vector3 PredictiveAim(Vector3 muzzlePosition, float projectileSpeed, Vector3 targetPosition, Vector3 targetVelocity, float gravity)
	{
		Debug.Assert(projectileSpeed > 0, "What are you doing shooting at something with a projectile that doesn't move?");
		if (muzzlePosition == targetPosition)
		{
			//Why dost thou hate thyself so?
			//Do something smart here. I dunno... whatever.
			return projectileSpeed * (Random.rotation * Vector3.forward);
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

		//Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = C*C
		//A is distance from muzzle to target (known value: targetToMuzzleDist)
		//B is distance traveled by target until impact (targetSpeed * t)
		//C is distance traveled by projectile until impact (projectileSpeed * t)
		float cosTheta = Vector3.Dot(targetToMuzzleDir, targetVelocityDir);

		float t;
		if (Mathf.Approximately(projectileSpeedSq, targetSpeedSq))
		{
			//a = projectileSpeedSq - targetSpeedSq = 0
			//We want to avoid div/0 that can result from target and projectile traveling at the same speed
			//We know that C and B are the same length because the target and projectile will travel the same distance to impact
			//Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = C*C
			//Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = B*B
			//Law of Cosines: A*A - 2*A*B*cos(theta) = 0
			//Law of Cosines: A*A = 2*A*B*cos(theta)
			//Law of Cosines: A = 2*B*cos(theta)
			//Law of Cosines: A/(2*cos(theta)) = B
			//Law of Cosines: 0.5f*A/cos(theta) = B
			//Law of Cosines: 0.5f * targetToMuzzleDist / cos(theta) = targetSpeed * t
			//We know that cos(theta) of zero or less means there is no solution, since that would mean B goes backwards or leads to div/0 (infinity)
			if (cosTheta > 0)
			{
				t = 0.5f * targetToMuzzleDist / (targetSpeed * cosTheta);
			}
			else
			{
				t = PredictiveAimWildGuessAtImpactTime();
			}
		}
		else
		{
			//Quadratic formula: Note that lower case 'a' is a completely different derived variable from capital 'A' used in Law of Cosines (sorry):
			//t = [ -b � Sqrt( b*b - 4*a*c ) ] / (2*a)
			float a = projectileSpeedSq - targetSpeedSq;
			float b = 2.0f * targetToMuzzleDist * targetSpeed * cosTheta;
			float c = -targetToMuzzleDistSq;
			float discriminant = b * b - 4.0f * a * c;

			if (discriminant < 0)
			{
				//Square root of a negative number is an imaginary number (NaN)
				//Special thanks to Rupert Key (Twitter: @Arakade) for exposing NaN values that occur when target speed is faster than or equal to projectile speed
				t = PredictiveAimWildGuessAtImpactTime();
			}
			else
			{
				//a will never be zero because we protect against that with "if (Mathf.Approximately(projectileSpeedSq, targetSpeedSq))" above
				float uglyNumber = Mathf.Sqrt(discriminant);
				float t0 = 0.5f * (-b + uglyNumber) / a;
				float t1 = 0.5f * (-b - uglyNumber) / a;
				//Assign the lowest positive time to t to aim at the earliest hit
				t = Mathf.Min(t0, t1);
				if (t < Mathf.Epsilon)
				{
					t = Mathf.Max(t0, t1);
				}

				if (t < Mathf.Epsilon)
				{
					//Time can't flow backwards when it comes to aiming.
					//No real solution was found, take a wild shot at the target's future location
					t = PredictiveAimWildGuessAtImpactTime();
				}
			}
		}

		//Vb = Vt - 0.5*Ab*t + [(Pti - Pbi) / t]
		//By adding gravity as projectile acceleration, we are essentially breaking real world rules by saying that the projectile
		// gets any upwards/downwards gravity compensation velocity for free, since the projectileSpeed passed in is a constant that assumes zero gravity
		Vector3 projectileAcceleration = gravity * Vector3.down;
		Vector3 Vb = targetVelocity - (0.5f * projectileAcceleration * t) + (-targetToMuzzle / t);
		//FOR CHECKING ONLY (valid only if gravity is 0)...
		//float calculatedprojectilespeed = Vb.magnitude;
		//bool projectilespeedmatchesexpectations = (projectileSpeed == calculatedprojectilespeed);
		//...FOR CHECKING ONLY
		return Vb;
	}
}
