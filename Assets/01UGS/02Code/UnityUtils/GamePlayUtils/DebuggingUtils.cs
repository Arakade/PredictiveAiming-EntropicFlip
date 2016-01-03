using UnityEngine;


//////////////////////////////////////////////////////////////////////////////
//Additions from Kain for observation purposes
//Sorry for the different formatting, my Visual studio settings are different from yours
//////////////////////////////////////////////////////////////////////////////
public static class DebuggingUtils
{
	//////////////////////////////////////////////////////////////////////////////
	static Rect CovertScreenPositionToRect(Vector2 screenPosition, bool bHorizontallyCentered)
	{
		//Assume horizontally centered
		float fRectWidth = bHorizontallyCentered
						? Screen.width - 2.0f * screenPosition.x
						: Screen.width - screenPosition.x;
		Rect screenRect = new Rect(screenPosition.x, screenPosition.y, fRectWidth, Screen.height - screenPosition.y);
		return screenRect;
	}

	//////////////////////////////////////////////////////////////////////////////
	//This needs to be called within the scope of an OnGUI function
	public static void DrawTextInWorld(Camera whichCamera, Vector3 worldPosition, string msg, Color msgColor)
	{
		Vector3 targetScreenPoint = whichCamera.WorldToScreenPoint(worldPosition);
		if (targetScreenPoint.z > 0)
		{
			Color previousColor = GUI.color;

			//First we draw the shadow
			GUI.color = Color.black;
			Vector2 targetGUIPoint = new Vector2(targetScreenPoint.x, Screen.height - targetScreenPoint.y);
			Rect targetRect = CovertScreenPositionToRect(targetGUIPoint, false);
			GUI.Label(targetRect, msg);

			//Then we draw the text
			GUI.color = msgColor;
			targetGUIPoint.x -= 1;
			targetGUIPoint.y -= 1;
			targetRect = CovertScreenPositionToRect(targetGUIPoint, false);
			GUI.Label(targetRect, msg);

			GUI.color = previousColor;
		}
	}
	//////////////////////////////////////////////////////////////////////////////
}