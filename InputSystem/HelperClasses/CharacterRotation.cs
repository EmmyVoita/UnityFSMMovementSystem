using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlayerStates
{
    public static class CharacterRotation 
    {
        public static Quaternion ClampRotation(Quaternion rotation, float startAngle, float minAngle, float maxAngle)
        {
            /*
            Quaternion operations in most programming languages, including Unity's C#, typically 
            expect angles to be in radians rather than degrees. Converting angles to radians 
            ensures that the calculations involving quaternions are performed correctly.
            */

            float startAngleRad = startAngle * Mathf.Deg2Rad;
            float minAngleRad = minAngle * Mathf.Deg2Rad;
            float maxAngleRad = maxAngle * Mathf.Deg2Rad;



            //Debug.Log("Converting angles to radians. StartAngle: " + startAngleRad + " MinAngle: " + minAngleRad + " MaxAngle: " + maxAngleRad);

            /*
                This line creates a quaternion (startQuaternion) that represents a rotation 
                around the Vector3.up axis. Quaternion.AngleAxis(angle, axis) is a Unity 
                function that generates a quaternion for a specified angle around a given axis.
            */

            // Convert the start angle to a quaternion
            Quaternion startQuaternion = Quaternion.AngleAxis(startAngle, Vector3.up);

            /*
                The relativeRotation represents the rotation relative to a specified start angle. 
                In other words, it gives you the rotation in relation to the initial orientation 
                defined by the startAngle.This is useful in scenarios where you want to perform 
                rotations relative to a specific reference direction or angle. It allows you to 
                keep track of how an object has been rotated with respect to its initial state, 
                rather than in absolute terms.
            */

            // Convert the rotation to be relative to the start angle
            Quaternion relativeRotation = Quaternion.Inverse(startQuaternion) * rotation;

            // Convert the relative rotation to euler angles for easier manipulation
            Vector3 relativeEulerAngles = relativeRotation.eulerAngles;


            // Convert relativeEulerAngles.y to radians before clamping
            float relativeAngleRad = relativeRotation.eulerAngles.y * Mathf.Deg2Rad;

            //Debug.Log("relativeEulerAngles.y (rad) '" + relativeAngleRad + "' clamped to '" + Mathf.Clamp(relativeEulerAngles.y, minAngleRad, maxAngleRad) + "'. \n Using min: '" + minAngleRad + "' and max: '" + maxAngleRad + "'.");


            // Clamp the relative rotation's y (vertical) angle
            relativeAngleRad = Mathf.Clamp(relativeAngleRad, minAngleRad, maxAngleRad);

            // Convert back to degrees
            relativeRotation.eulerAngles = new Vector3(0, relativeAngleRad * Mathf.Rad2Deg, 0);

            // Convert the clamped relative rotation back to a quaternion
            Quaternion clampedRelativeRotation = Quaternion.Euler(relativeEulerAngles);

            // Combine the clamped relative rotation with the start angle to get the final rotation
            Quaternion finalRotation = startQuaternion * clampedRelativeRotation;

            /*Debug.Log("StartQuaternion.eulerAngles.y '" + startQuaternion.eulerAngles.y +
                        "' \n clampedRelativeRotation.eulerAngles.y '" + clampedRelativeRotation.eulerAngles.y + 
                        "' \n finalRotation.eulerAngles.y '" + finalRotation.eulerAngles.y + "' .");*/

            return finalRotation;
        }

        public static float NormalizeAngle(float angle)
        {
            if (angle > 180)
            {
                return angle - 360;
            }
            return angle;
        }
    }
}
