using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompareLine : MonoBehaviour {
    public static int NormalizedPointCount = 32;
    private static bool[] matched = null;
    static bool blnGesture = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static float CompareNormal(List<Vector3> line1, List<Vector3> line2)
    {
        blnGesture = false;
        List<Vector3> nLine1 = Normalize(line1);
        List<Vector3> nLine2 = Normalize(line2);
        float min = CloundNormal(nLine1, nLine2);
        return min;
    }
    static float CloundNormal(List<Vector3> points1, List<Vector3> points2)
    {
#if UNITY_EDITER
        if (points1.Count != points2.Count)
        {
            Debug.LogError("Points1 != Points2 count: " + points1.Count + " vs " + points2.Count);
            return float.PositiveInfinity;
        }
#endif
        float sum = 0;
        for (int i = 0; i < points1.Count; i++)
        {
            float distance = Vector3.Distance(points1[i], points2[i]);
            sum += distance;
        }
        return sum;
    }
    public static float CompareGesture(List<Vector3> line1, List<Vector3> line2)
    {
        blnGesture = true;
        List<Vector3> nLine1 = Normalize(line1);
        List<Vector3> nLine2 = Normalize(line2);
        float e = 0.5f; // [0..1] controls the number of of tested alignments
        int step = Mathf.FloorToInt(Mathf.Pow(nLine1.Count, 1 - e));
        float min = float.PositiveInfinity;
        for (int i = 0; i < nLine1.Count; i += step)
        {
            float d1 = CloudDistance(nLine1, nLine2, i);
            float d2 = CloudDistance(nLine2, nLine1, i);
            min = Mathf.Min(min, d1, d2);
        }
        return min;
    }
    static float CloudDistance(List<Vector3> points1, List<Vector3> points2, int startIndex)
    {
        int numPoints = points1.Count;
        ResetMatched(numPoints);

#if UNITY_EDITOR
        if (points1.Count != points2.Count)
        {
            Debug.LogError("Points1 != Points2 count: " + points1.Count + " vs " + points2.Count);
            return float.PositiveInfinity;
        }
#endif

        float sum = 0;
        int i = startIndex;

        do
        {
            int index = -1;
            float minDistance = float.PositiveInfinity;

            for (int j = 0; j < numPoints; ++j)
            {
                if (!matched[j])
                {
                    float distance = Vector3.Distance(points1[i], points2[j]); //OPTIMIZEME: replace by square distance compare

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        index = j;
                    }
                }
            }

            matched[index] = true;

            float weight = 1 - ((i - startIndex + points1.Count) % points1.Count) / points1.Count;  // [0::1] assign decreasing confidence weights to point matchings.
            sum += weight * minDistance;


            i = (i + 1) % points1.Count;

        } while (i != startIndex);

        return sum;
    }
    static void ResetMatched(int count)
    {
        if (matched == null || matched.Length < count)
            matched = new bool[count];

        for (int i = 0; i < count; ++i)
            matched[i] = false;
    }
    static List<Vector3> Normalize(List<Vector3> points)
    {
        return new List<Vector3>(Apply(points, NormalizedPointCount));
    }
    static List<Vector3> Apply(List<Vector3> inputPoints, int normalizedPointsCount)
    {
        List<Vector3> normalizedPoints = Resample(inputPoints, normalizedPointsCount);
        if (blnGesture)
        {
            Scale(normalizedPoints);
            TranslateToOrigin(normalizedPoints);
        }
        return normalizedPoints;
    }
    // X points => normalizedPointsCount points
    static List<Vector3> Resample(List<Vector3> points, int normalizedPointsCount)
    {
        if (normalizedPointsCount < 5)
            normalizedPointsCount = 5;
        float intervalLength = PathLength(points) / (normalizedPointsCount - 1);
        float D = 0;
        Vector3 q = Vector3.zero;

        List<Vector3> normalizedPoints = new List<Vector3>();
        normalizedPoints.Add(points[0]);

        List<Vector3> pointBuffer = new List<Vector3>();
        pointBuffer.AddRange(points);

        for (int i = 1; i < pointBuffer.Count; ++i)
        {
            Vector3 a = pointBuffer[i - 1];
            Vector3 b = pointBuffer[i];
            float d = Vector3.Distance(a, b);
            if ((D + d) > intervalLength)
            {
                q = Vector3.Lerp(a, b, (intervalLength - D) / d);
                normalizedPoints.Add(q);
                pointBuffer.Insert(i, q); // q becomes the next "b" (point[i])
                D = 0;
            }
            else
            {
                D += d;
            }
        }

        // sometimes we fall a rounding-error short of adding the last point, so add it if so
        if (normalizedPoints.Count == normalizedPointsCount - 1)
            normalizedPoints.Add(pointBuffer[pointBuffer.Count - 1]);

        return normalizedPoints;
    }
    // compute total length of the set of strokes
    static float PathLength(List<Vector3> points)
    {
        float d = 0;
        for (int i = 1; i < points.Count; ++i)
        {
            d += Vector3.Distance(points[i - 1], points[i]);
        }
        return d;
    }
    // Rescale "points" in place with shape preservation so that the resulting bounding box will be within [0..1] range
    static void Scale(List<Vector3> points)
    {
        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < points.Count; ++i)
        {
            Vector3 p = points[i];
            min.x = Mathf.Min(min.x, p.x);
            min.y = Mathf.Min(min.y, p.y);
            max.x = Mathf.Max(max.x, p.x);
            max.y = Mathf.Max(max.y, p.y);
        }

        float size = Mathf.Max(max.x - min.x, max.y - min.y);
        float invSize = 1.0f / size;

        for (int i = 0; i < points.Count; ++i)
        {
            Vector3 p = points[i];
            p = (p - min) * invSize;
            points[i] = p;
        }
    }
    // translate points in place so the cloud is centered at the (0,0) origin
    static void TranslateToOrigin(List<Vector3> points)
    {
        Vector3 c = Centroid(points);

        for (int i = 0; i < points.Count; ++i)
        {
            Vector3 p = points[i];
            p -= c;
            points[i] = p;
        }
    }
    // compute the center of the points cloud
    static Vector3 Centroid(List<Vector3> points)
    {
        Vector3 c = Vector3.zero;

        for (int i = 0; i < points.Count; ++i)
            c += points[i];

        c /= points.Count;
        return c;
    }

    public static List<Vector3> ResampleByLen(List<Vector3> points, float len)
    {
        if (len < 2)
            len = 2;
        int normalizedPointsCount = (int)(PathLength(points) / len);
        if (normalizedPointsCount <= 0)
            return null;
        float intervalLength = len;
        float D = 0;
        Vector3 q = Vector3.zero;

        List<Vector3> normalizedPoints = new List<Vector3>();
        normalizedPoints.Add(points[0]);

        List<Vector3> pointBuffer = new List<Vector3>();
        pointBuffer.AddRange(points);

        for (int i = 1; i < pointBuffer.Count; ++i)
        {
            Vector3 a = pointBuffer[i - 1];
            Vector3 b = pointBuffer[i];
            float d = Vector3.Distance(a, b);
            if ((D + d) > intervalLength)
            {
                q = Vector3.Lerp(a, b, (intervalLength - D) / d);
                normalizedPoints.Add(q);
                pointBuffer.Insert(i, q); // q becomes the next "b" (point[i])
                D = 0;
            }
            else
            {
                D += d;
            }
        }

        // sometimes we fall a rounding-error short of adding the last point, so add it if so
        if (normalizedPoints.Count == normalizedPointsCount - 1)
            normalizedPoints.Add(pointBuffer[pointBuffer.Count - 1]);

        return normalizedPoints;
    }
}
