using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSplineGenerator
{

    private int numberOfSplines;

    private float size;
    private Vector2[] controlPoints;
    private BCurve[] curves;

    public BSplineGenerator(int numberOfSplines, float size)
    {
        this.numberOfSplines = numberOfSplines;
        this.size = size;

        InitializePoints();
    }

    private void InitializePoints()
    {
        controlPoints = new Vector2[numberOfSplines * 3 + 1];
        curves = new BCurve[numberOfSplines];

        for (int i = 0; i < controlPoints.Length; i++)
        {
            controlPoints[i].x = size * Random.Range(-1f, 1f);
            controlPoints[i].y = size * Random.Range(-1f, 1f);
        }

        for (int i = 0; i < curves.Length; i++)
        {
            Vector2[] curveControls = new Vector2[4];

            int startIndex = i * 3;
            for (int j = 0; j < 4; j++)
            {
                curveControls[j] = controlPoints[startIndex + j];
            }

            curves[i] = new BCurve(curveControls);
        }
    }

    public Vector2 getPostionFromSpline(float t)
    {
        t = Mathf.Repeat(t, numberOfSplines);
        BCurve currentCurve = curves[(int)t];
        Debug.Log(currentCurve.GetControlPoints()[0]);
        Vector2[] currentCoefficients = currentCurve.GetCoefficients();
        float t2 = t - (int)t; // Only the float value is used to lerp through each curve
        Vector2 currentPositionOnCurve = currentCoefficients[0] + (t2 * currentCoefficients[1]) + (t2 * t2 * currentCoefficients[2]) + 
            (t2 * t2 * t2 * currentCoefficients[3]);
        return currentPositionOnCurve;
    }
}
