using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCurve
{
    private Vector2[] controlPoints = new Vector2[4];
    private Vector2[] coefficients = new Vector2[4];

    public BCurve(Vector2[] controlPoints)
    {
        for(int i = 0; i < controlPoints.Length; i++)
        {
            this.controlPoints[i] = controlPoints[i];
        }

        CalculateCoefficients();
    }

    private void CalculateCoefficients()
    {
        coefficients[0] = controlPoints[0];
        coefficients[1] = -3f * controlPoints[0] + 3f * controlPoints[1];
        coefficients[2] = 3f * (controlPoints[0] + -2f * controlPoints[1] + controlPoints[2]);
        coefficients[3] = -1f * controlPoints[0] + 3f * controlPoints[1] + -3f * controlPoints[2] + controlPoints[3];
    }

    public Vector2[] GetCoefficients()
    {
        return coefficients;
    }

    public Vector2[] GetControlPoints()
    {
        return controlPoints;
    }
}
