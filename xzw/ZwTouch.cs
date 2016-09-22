using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZwTouch : MonoBehaviour {
    Vector3 mousePostion = Vector3.zero;
    Vector3 lastMousePosition = Vector3.zero;
    public GameObject fingerSprite = null;
    List<GameObject> listUsedPoint = new List<GameObject>();
    List<GameObject> listUnusedDrawPoint = new List<GameObject>();
    List<GameObject> listTempPoint1 = new List<GameObject>();
    List<Vector3> listOrgPoint = new List<Vector3>();
    List<Vector3> listTemp = new List<Vector3>();
    Vector3 lastScale = Vector3.one;
    float bezierDis = 4f;
    Transform pointsParent;
    int touchKind = 0;
    Vector3 tempVec;
    bool blnCanDraw = false;
    float nowScale = 0;
    float lerpScale = 2;
    float scaleMin = 0.7f;
    bool blnStopLerp = false;
    float pointStartColorSpeed = 2.5f;
    float pointStartScale = 2.0f;
    float singleTouchTime = 0;
    RaycastHit2D hit2d;
    GameObject click;

    void Start()
    {
        ZwController.Ins.zwTouch = this;
        bezierDis = Mathf.Max(4f, bezierDis * Screen.width / 1024);

        if (!pointsParent)
            pointsParent = GameObject.FindGameObjectWithTag("PointsParent").transform;
        if (fingerSprite)
            fingerSprite.SetActive(false);
    }
    void CheckTouchKind()
    {
        touchKind = 0;
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                    touchKind = 1;
                else if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    touchKind = 2;
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    touchKind = 3;
                mousePostion = Input.GetTouch(0).position;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                touchKind = 1;
            else if (Input.GetMouseButton(0))
                touchKind = 2;
            else if (Input.GetMouseButtonUp(0))
                touchKind = 3;
            mousePostion = Input.mousePosition;
        }
    }
    void Update()
    {
        if (CV.allStatus != CV.Status_Cs)
        {
            CheckTouchKind();
            if (CV.allStatus == CV.Status_Xxz)
            {
                TouchDraw();
            }
            else if (CV.allStatus == CV.Status_Xz_Over)
            {
                TouchComplate();
            }
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home))
                {
                    Application.Quit();
                }
            }
        }
    }
    void TouchDraw()
    {
        if (touchKind == 1)
        {
            lastMousePosition = mousePostion;
            tempVec = Camera.main.ScreenToWorldPoint(mousePostion);
            tempVec.z = -4;
            fingerSprite.transform.position = tempVec;
            fingerSprite.SetActive(true);
            AddTouchPoint(mousePostion, 0);
            blnCanDraw = true;
        }
        else if (touchKind == 2)
        {
            if (blnCanDraw)
            {
                tempVec = Camera.main.ScreenToWorldPoint(mousePostion);
                tempVec.z = -4;
                fingerSprite.transform.position = tempVec;
                //AddFingerFlare(mousePostion);
                AddTouchPoint(mousePostion, 1);
            }
        }
        else if (touchKind == 3)
        {
            if (blnCanDraw)
            {
                tempVec = Camera.main.ScreenToWorldPoint(mousePostion);
                tempVec.z = -4;
                fingerSprite.transform.position = tempVec;
                AddTouchPoint(mousePostion, 2);

                if (listTempPoint1.Count > 0 && listUsedPoint.Count > 0)
                {
                    lastScale = Vector3.one;
                    for (int i = 0; i < listTempPoint1.Count; i++)
                    {
                        listOrgPoint.Add(listTempPoint1[i].transform.position);
                        DealPoint(i == listTempPoint1.Count - 1 ? 2 : 1);
                    }
                }
                if (ZwClear.CheckClear(listOrgPoint[0], listOrgPoint[listOrgPoint.Count - 1]))
                {
                    ZwController.Ins.ResetWord();
                    CV.allStatus = CV.Status_Xxz;
                    //ZwController.Ins.AddExplode(mousePostion);
                }
                else
                {
                    if (listUsedPoint.Count > 0)
                    {
                        ZwController.Ins.CompareBihua(listOrgPoint);
                    }
                }
                if (listUsedPoint.Count == 0)
                    ResetTempPoint1();
                SetEndPoint(false);
            }
            fingerSprite.SetActive(false);
            lastScale = Vector3.one;
            blnCanDraw = false;
        }
    }
    void TouchComplate()
    {
        if (touchKind == 1)
        {
            lastMousePosition = mousePostion;
            singleTouchTime = Time.time;
        }
        else if (touchKind == 2)
        {

        }
        else if (touchKind == 3)
        {
            float dis = Vector3.Distance(lastMousePosition, mousePostion);
            if (Time.time - singleTouchTime <= 0.2f || dis < (float)Screen.width / 10)
            {
                tempVec = Camera.main.ScreenToWorldPoint(mousePostion);
                hit2d = Physics2D.Raycast(tempVec, Vector2.zero, 100, LayerMask.GetMask(ZwWords.WordLayerName));
                if (hit2d.collider)
                {
                    click = hit2d.collider.gameObject;
                    if (click.tag == ZwWords.WordTagBihua)
                        ZwController.Ins.ClickWord(click.transform.parent.parent.gameObject, click.name, mousePostion);
                    else
                        ZwController.Ins.ClickWord(null, "", mousePostion);
                }
                else
                    ZwController.Ins.ClickWord(null, "", mousePostion);
            }
        }
    }
    void AddTouchPoint(Vector3 mousePos, int kind)
    {
        if (kind == 0)
        {
            lastMousePosition = mousePos;
            ResetDrawPint();
            listOrgPoint.Add(mousePos);
        }
        else
        {
            Vector3 direction = mousePos - lastMousePosition;
            float s = direction.magnitude;
            float d = 0;
            if (s < 1)
                d = 0;
            else if (s < 2)
                d = s;
            else if (s <= 10)
                d = 1 + Mathf.Lerp(0.0f, 2.0f, s / 10);
            else
                d = 3;
            int count = (int)(s / d);
            Vector3 pos = Vector3.zero;
            Vector3 scale = Vector3.one * scaleMin;
            float ts = scaleMin;
            if (d == 0)
                ts = Mathf.Lerp(4, scaleMin, (d - 1) / 2);
            else
                ts = Mathf.Lerp(2, scaleMin, (d - 1) / 2);
            float lastNow = nowScale;
            nowScale = ts;
            if (d > 0)
            {
                if (blnStopLerp && listUsedPoint.Count > 0)
                {
                    blnStopLerp = false;
                    lastScale = listUsedPoint[listUsedPoint.Count - 1].transform.localScale;
                }
                listOrgPoint.Add(mousePos);
                DealPoint(1);
                if (kind == 2)
                {
                    for (int i = 0; i < count; i++)
                    {
                        pos = Vector3.Lerp(lastMousePosition, mousePos, (float)i / count);
                        scale = Vector3.one * Mathf.Lerp(lastScale.x, nowScale, lerpScale * Time.deltaTime);
                        scale.z = 1;
                        lastScale = scale;
                        AddUsePoint(pos, scale);
                    }
                }
                else
                {
                    scale = Vector3.one * Mathf.Lerp(lastScale.x, nowScale, lerpScale * Time.deltaTime);
                    scale.z = 1;
                    lastScale = scale;
                }
                lastMousePosition = mousePos;
                if (listUsedPoint.Count > 600)
                {
                    blnCanDraw = false;
                    fingerSprite.SetActive(false);
                }
                if (kind == 2 || !blnCanDraw)
                {
                    DealPoint(2);
                    if (!blnCanDraw)
                    {
                        SetEndPoint(true);
                        //BishunInit.Ins.AddExplode(mousePos);
                    }
                }
            }
            else
            {
                if (listUsedPoint.Count > 0)
                {
                    scale = Vector3.one * Mathf.Lerp(lastScale.x, nowScale, lerpScale * Time.deltaTime);
                    scale.z = 1;
                    listUsedPoint[listUsedPoint.Count - 1].GetComponent<ZwScale>().SetNowScale(scale.x);
                    lastScale = scale;
                    blnStopLerp = true;
                }
                else
                {
                    nowScale = lastNow;
                }
            }
        }
    }
    void ResetDrawPint()
    {
        for (int i = 0; i < listUsedPoint.Count; i++)
        {
            listUsedPoint[i].SetActive(false);
            listUnusedDrawPoint.Add(listUsedPoint[i]);
        }
        listUsedPoint.Clear();
        ResetTempPoint1();
        listOrgPoint.Clear();
        listTemp.Clear();
    }
    void DealPoint(int kind)
    {
        if (listOrgPoint.Count > 2)
        {
            listTemp.Clear();
            if (kind == 1)
            {
                if (listOrgPoint.Count == 3)
                    listTemp.Add(listOrgPoint[listOrgPoint.Count - 3]);
                listTemp.Add(listOrgPoint[listOrgPoint.Count - 3]);
                listTemp.Add(listOrgPoint[listOrgPoint.Count - 2]);
                listTemp.Add(listOrgPoint[listOrgPoint.Count - 1]);
            }
            else
            {
                listTemp.Add(listOrgPoint[listOrgPoint.Count - 2]);
                listTemp.Add(listOrgPoint[listOrgPoint.Count - 1]);
                listTemp.Add(listOrgPoint[listOrgPoint.Count - 1]);
            }
            BSpline2Smooth(listTemp, false);
            AddBezierPoint(listTemp);
        }
    }
    void ResetTempPoint1()
    {
        for (int i = 0; i < listTempPoint1.Count; i++)
        {
            listTempPoint1[i].SetActive(false);
            listUnusedDrawPoint.Add(listTempPoint1[i]);
        }
        listTempPoint1.Clear();
    }
    void SetEndPoint(bool blnExplode)
    {
        for (int i = 0; i < listUsedPoint.Count; i++)
        {
            if (blnExplode)
                listUsedPoint[i].GetComponent<ZwScale>().SetEndColor(0, 8);
            else
                listUsedPoint[i].GetComponent<ZwScale>().SetEndColor(Time.deltaTime / 50 * i, Mathf.Min((float)i / 50, 1));
        }
    }
    void AddUsePoint(Vector3 pos, Vector3 scale)
    {
        GameObject point = null;
        if (listUnusedDrawPoint.Count > 0)
        {
            point = listUnusedDrawPoint[listUnusedDrawPoint.Count - 1];
            listUnusedDrawPoint.RemoveAt(listUnusedDrawPoint.Count - 1);
        }
        if (point == null)
        {
            point = (GameObject)Instantiate(ZwController.Ins.GetPrefab("BasePoint"));
            point.transform.parent = pointsParent;
        }
        Vector3 tempVec = Camera.main.ScreenToWorldPoint(pos);
        tempVec.z = 0;
        point.transform.position = tempVec;
        point.transform.localScale = scale;
        point.transform.rotation = Quaternion.identity;
        point.SetActive(true);
        point.GetComponent<ZwScale>().ResetColor();
        listTempPoint1.Add(point);
    }
    void AddBezierPoint(List<Vector3> list)
    {
        GameObject point = null;
        Vector3 scale = Vector3.one;
        for (int i = 0; i < list.Count; i++)
        {
            point = null;
            if (listTempPoint1.Count > 0)
            {
                point = listTempPoint1[0];
                listTempPoint1.RemoveAt(0);
                scale = Vector3.one * Mathf.Lerp(lastScale.x, nowScale, lerpScale * Time.deltaTime);
                scale.z = 1;
                point.transform.localScale = scale;
                lastScale = scale;
            }
            if (point == null)
            {
                if (listUnusedDrawPoint.Count > 0)
                {
                    point = listUnusedDrawPoint[listUnusedDrawPoint.Count - 1];
                    listUnusedDrawPoint.RemoveAt(listUnusedDrawPoint.Count - 1);
                    scale = Vector3.one * Mathf.Lerp(lastScale.x, nowScale, lerpScale * Time.deltaTime);
                    scale.z = 1;
                    point.transform.localScale = scale;
                    lastScale = scale;
                }
            }
            if (point == null)
            {
                point = (GameObject)Instantiate(ZwController.Ins.GetPrefab("BasePoint"));
                point.transform.parent = pointsParent;
                scale = Vector3.one * Mathf.Lerp(lastScale.x, nowScale, lerpScale * Time.deltaTime);
                scale.z = 1;
                point.transform.localScale = scale;
                lastScale = scale;
            }
            Vector3 tempVec = Camera.main.ScreenToWorldPoint(list[i]);
            tempVec.z = 0;
            point.transform.position = tempVec;
            point.transform.rotation = Quaternion.identity;
            if (!point.activeSelf)
            {
                point.SetActive(true);
                point.GetComponent<ZwScale>().ResetColor();
            }
            point.GetComponent<ZwScale>().SetStartScale(pointStartScale);
            point.GetComponent<ZwScale>().SetStartColor(pointStartColorSpeed);
            listUsedPoint.Add(point);
        }
        ResetTempPoint1();
    }
    void BSpline2Smooth(List<Vector3> list, bool blnSet)
    {
        List<Vector3> aList = new List<Vector3>();
        if (blnSet)
            aList.Add(list[0]);
        aList.AddRange(list);
        if (blnSet)
            aList.Add(list[list.Count - 1]);
        List<Vector3> tList = new List<Vector3>();
        int loc1 = 1;
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;
        while (loc1 < aList.Count - 1)
        {
            start = aList[loc1 - 1];
            end = aList[loc1 + 1];
            tList.Add((aList[loc1 - 1] + aList[loc1]) * 0.5f);
            BSpline2Smooth(tList, start, aList[loc1], end);
            tList.Add((aList[loc1] + aList[loc1 + 1]) * 0.5f);
            ++loc1;
        }
        list.Clear();
        List<Vector3> rl = CompareLine.ResampleByLen(tList, bezierDis);
        if (rl != null)
            list.AddRange(rl);
        else
            list.AddRange(tList);
    }
    void BSpline2Smooth(List<Vector3> list, Vector3 arg1, Vector3 arg2, Vector3 arg3)
    {
        List<float> locx = new List<float>();
        List<float> locy = new List<float>();
        locx.Add((arg1.x + arg2.x) * 0.5f);
        locx.Add(arg2.x - arg1.x);
        locx.Add((arg1.x - 2 * arg2.x + arg3.x) * 0.5f);
        locy.Add((arg1.y + arg2.y) * 0.5f);
        locy.Add(arg2.y - arg1.y);
        locy.Add((arg1.y - 2 * arg2.y + arg3.y) * 0.5f);
        int loc6 = (int)(CountDistance(arg1, arg3));
        int loc7 = 0;
        float loc8 = 0;
        Vector3 loc5 = Vector3.zero;
        while (loc7 < loc6)
        {
            loc8 = (float)loc7 / loc6;
            loc5.x = locx[0] + loc8 * (locx[1] + locx[2] * loc8);
            loc5.y = locy[0] + loc8 * (locy[1] + locy[2] * loc8);
            loc5.z = 0;
            list.Add(loc5);
            loc7++;
        }
    }
    float CountDistance(Vector3 arg1, Vector3 arg2)
    {
        return Mathf.Round(Mathf.Sqrt(Mathf.Pow(arg1.x - arg2.x, 2) + Mathf.Pow(arg1.y - arg2.y, 2)));
    }
}

public class CompareLine
{
    public static int NormalizedPointCount = 32;
    private static bool[] matched = null;
    static bool blnGesture = false;

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
                    float distance = Vector2.Distance(points1[i], points2[j]); //OPTIMIZEME: replace by square distance compare

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
