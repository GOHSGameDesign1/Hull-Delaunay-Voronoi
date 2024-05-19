using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCurveGenerator : MonoBehaviour
{

    public List<Vector2> controlPoints = new List<Vector2>();

    private Vector2 currentPositionOnCurve;

    private Vector2[] coefficients = new Vector2[4];

    private BCurve[] curves = new BCurve[2];

    // Start is called before the first frame update
    void Start()
    {
        coefficients[0] = controlPoints[0];
        coefficients[1] = -3f * controlPoints[0] + 3f * controlPoints[1];
        coefficients[2] = 3f * (controlPoints[0] + -2f * controlPoints[1] + controlPoints[2]);
        coefficients[3] = -1f * controlPoints[0] + 3f * controlPoints[1] + -3f * controlPoints[2] + controlPoints[3];

        for(int i = 0; i < curves.Length; i++)
        {
            Vector2[] curveControls = new Vector2[4];

            int startIndex = i * 3;
            for(int j = 0; j < 4; j++)
            {
                curveControls[j] = controlPoints[startIndex + j];
            }

            curves[i] = new BCurve(curveControls);
        }

        StartCoroutine(LerpThroughCurve());
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    IEnumerator LerpThroughCurve()
    {
        float t = 0;

        while (true)
        {
            BCurve currentCurve = curves[(int)t];
            Debug.Log(currentCurve.GetControlPoints()[0]);
            Vector2[] currentCoefficients = currentCurve.GetCoefficients();
            float t2 = t - (int)t; // Only the float value is used to lerp through each curve
            currentPositionOnCurve = currentCoefficients[0] + (t2 * currentCoefficients[1]) + (t2 * t2 * currentCoefficients[2]) + (t2 * t2 * t2 * currentCoefficients[3]);

            //currentPositionOnCurve = t * coefficients[0];
            transform.position = currentPositionOnCurve;
            t += Time.deltaTime;
            t = Mathf.Repeat(t, 2);
            yield return null;
        }
    }

    private void OnPostRender()
    {
        //GL
    }
}
