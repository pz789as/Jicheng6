using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;

public class XEDrawWord : MonoBehaviour {
    public static float Scale_Value = 1.5f;
    [HideInInspector]
    public Material mat = null;
    public Material immediate_mat = null;
    public Material immediate_side = null;
    public int imtColorCount = 45;
    [HideInInspector]
    public Color[] fillColors;
    Color[] sideColors;
    public List<Bihua> arrBihua = new List<Bihua>();
    public BetterList<Vector2> headQuad = new BetterList<Vector2>();
    public BetterList<Vector3> headPos = new BetterList<Vector3>();
    public int quadCount = 0;
    [HideInInspector]
    public List<string> arrNumber = new List<string>();
    List<Vector3> maxBushouV = new List<Vector3>();
    List<Vector3> minBushouV = new List<Vector3>();
    public bool isInited { get; set; }
    int nowColorIdx = 0;
    public float Offx { get; set; }
    public float Offy { get; set; }
    bool blnImmediate;
    public Color defaultColor = Color.white;
    public Color defaultSideCol = Color.black;
    public Color selectSideCol = Color.green;
    [HideInInspector]
    public int drawingBihuaIdx = 0;
    float minDisStart;
    int startIdx;
    float minDisEnd;
    int endIdx;
    LitNode starLit;
    LitNode endLit;
    public bool blnDrawQuad = true;
    public bool blnMoveQuad = false;
    public void SetDrawIdx(int sel)
    {
        immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), defaultSideCol);
        drawingBihuaIdx = sel;
        immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), selectSideCol);
    }
    public void Next()
    {
        immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), defaultSideCol);
        drawingBihuaIdx = (drawingBihuaIdx + 1) % arrBihua.Count;
        immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), selectSideCol);
        //drawIdx++;
        //if (drawIdx >= arrBihua.Count)
        //    drawIdx = -1;
    }
    public void Last()
    {
        immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), defaultSideCol);
        drawingBihuaIdx = (drawingBihuaIdx + arrBihua.Count - 1) % arrBihua.Count;
        immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), selectSideCol);
        //drawIdx--;
        //if (drawIdx < -1)
        //    drawIdx = arrBihua.Count - 1;
    }
    public bool MoveNext()
    {
        int changIdx = drawingBihuaIdx + 1;
        if (changIdx < arrBihua.Count)
        {
            arrBihua.Reverse(drawingBihuaIdx, 2);
            immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), defaultSideCol);
            drawingBihuaIdx = changIdx;
            immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), selectSideCol);
            return true;
        }
        return false;
    }
    public bool MoveLast()
    {
        int changIdx = drawingBihuaIdx - 1;
        if (changIdx >= 0)
        {
            arrBihua.Reverse(changIdx, 2);
            immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), defaultSideCol);
            drawingBihuaIdx = changIdx;
            immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), selectSideCol);
            return true;
        }
        return false;
    }

    // Use this for initialization
    void Start()
    {
        isInited = false;
        Offx = 0;// Screen.width - 600;
        Offy = 0;
        CreateMaterial();
        //InitColor();
    }
    public Color GetNowColor(int idx)
    {
        int tmp = (nowColorIdx + idx) % fillColors.Length;
        return fillColors[tmp];
    }
    void CreateMaterial()
    {
        InitColor();
        int colorCount = fillColors.Length + sideColors.Length + 1;
        string shaderStr = "Shader \"Custom/TmpColor\" {\n" +
            "Properties {\n";
        for (int i = 0; i < colorCount; i++)
        {
            if (i == 0)
            {
                string tmp = string.Format("_Color{0} (\"Color {1}\",Color) = ({2},{3},{4},{5})\n",
                    i, i, defaultColor.r, defaultColor.g, defaultColor.b, 1);
                shaderStr += tmp;
            }
            else if (i < fillColors.Length + 1)
            {
                string tmp = string.Format("_Color{0} (\"Color {1}\",Color) = ({2},{3},{4},{5})\n",
                    i, i, fillColors[i - 1].r, fillColors[i - 1].g, fillColors[i - 1].b, fillColors[i - 1].a);
                shaderStr += tmp;
            }
            else
            {
                string tmp = string.Format("_Color{0} (\"Color {1}\",Color) = ({2},{3},{4},{5})\n",
                    i, i, sideColors[i - fillColors.Length - 1].r, sideColors[i - fillColors.Length - 1].g, sideColors[i - fillColors.Length - 1].b, sideColors[i - fillColors.Length - 1].a);
                shaderStr += tmp;
            }
        }
        shaderStr += "}\n" +
            "SubShader {\n";
        for (int i = 0; i < colorCount; i++)
        {
            shaderStr += "Pass\n" +
                "{\n" +
                    "Color [_Color" + i + "]\n" +
                "}\n";
        }
        shaderStr += "}\n" +
            "}\n";
        mat = new Material(shaderStr);

        for (int m = 0; m < 2; m++)
        {
            colorCount = imtColorCount + 1;
            shaderStr = "Shader \"Custom/ImtColor\" {\n" +
                "Properties {\n";
            for (int i = 0; i < colorCount; i++)
            {
                if (i == 0)
                {
                    string tmp = string.Format("_Color{0} (\"Color {1}\",Color) = ({2},{3},{4},{5})\n",
                        i, i, m == 0 ? defaultColor.r : defaultSideCol.r,
                        m == 0 ? defaultColor.g : defaultSideCol.g,
                        m == 0 ? defaultColor.b : defaultSideCol.b, 1);
                    shaderStr += tmp;
                }
                else
                {
                    string tmp = string.Format("_Color{0} (\"Color {1}\",Color) = ({2},{3},{4},{5})\n",
                        i, i, 0, 0, 0, 1);
                    shaderStr += tmp;
                }
            }
            shaderStr += "}\n" +
                "SubShader {\n";
            for (int i = 0; i < colorCount; i++)
            {
                shaderStr += "Pass\n" +
                    "{\n" +
                        "Color [_Color" + i + "]\n" +
                    "}\n";
            }
            shaderStr += "}\n" +
                "}\n";
            if (m == 0)
                immediate_mat = new Material(shaderStr);
            else
                immediate_side = new Material(shaderStr);
        }
    }
    void InitColor()
    {
        float[] col = new float[]
        {
            //fill
            231,31,24,
            255,102,0,
            254,153,0,
            255,204,0,
            255,255,0,
            172,229,0,
            1,204,0,
            0,153,203,
            0,86,204,
            33,0,203,
            136,0,204,
            216,0,125,
            //side
            115,15,12,
            127,51,0,
            127,76,0,
            127,102,0,
            127,127,0,
            86,114,0,
            0,102,0,
            0,76,101,
            0,43,102,
            16,0,101,
            68,0,102,
            108,0,62
        };
        fillColors = new Color[12];
        sideColors = new Color[12];
        
        for (int i = 0; i < 12; i++)
        {
            fillColors[i] = new Color(col[i * 3 + 0] / 255, col[i * 3 + 1] / 255, col[i * 3 + 2] / 255);
            sideColors[i] = new Color(col[i * 3 + 0 + 36] / 255, col[i * 3 + 1 + 36] / 255, col[i * 3 + 2 + 36] / 255);
        }
    }
    public void InitWord(XmlDocument doc)
    {
        isInited = false;
        StopAllCoroutines();
        StartCoroutine(LoadWord(doc));
        nowColorIdx = UnityEngine.Random.Range(0, fillColors.Length);
    }
    string defaultName = "k";
    public void SetDefaultName(string name)
    {
        this.defaultName = name;
    }
    IEnumerator LoadWord(XmlDocument doc)
    {
        drawIdx = -1;
        XmlNodeList xnlPoints = doc.SelectNodes("character/points");
        arrBihua.Clear();
        arrNumber.Clear();
        maxBushouV.Clear();
        minBushouV.Clear();
        for (int k = 0; k < xnlPoints.Count; k++)
        {
            XmlNodeList xnl = xnlPoints[k].SelectNodes("point");
            List<Vector2> basePoint = new List<Vector2>();
            for (int a = 0; a < xnl.Count; a++)
            {
                LitNode ln = new LitNode((XmlElement)xnl[a]);
                basePoint.Add(ln.TrsVec);
                if (ln.pos == 1)
                    starLit = ln;
                else if (ln.pos == 2)
                    endLit = ln;
                else
                    ln = null;
            }

            List<LitNode> upPoint = new List<LitNode>();
            List<LitNode> downPoint = new List<LitNode>();
            List<LitNode> smoothBSPArr = new List<LitNode>();

            minDisStart = float.MaxValue;
            minDisEnd = float.MaxValue;
            startIdx = -1;
            endIdx = -1;
            int loc1 = 2;
            while (loc1 < xnl.Count - 1)
            {
                BSPLineSmooth(smoothBSPArr, (XmlElement)xnl[loc1 - 2], (XmlElement)xnl[loc1 - 1], (XmlElement)xnl[loc1], (XmlElement)xnl[loc1 + 1]);
                ++loc1;
            }
            if (xnl.Count >= 4)
            {
                BSPLineSmooth(smoothBSPArr, (XmlElement)xnl[xnl.Count - 3], (XmlElement)xnl[xnl.Count - 2], (XmlElement)xnl[xnl.Count - 1], (XmlElement)xnl[0]);
                BSPLineSmooth(smoothBSPArr, (XmlElement)xnl[xnl.Count - 2], (XmlElement)xnl[xnl.Count - 1], (XmlElement)xnl[0], (XmlElement)xnl[1]);
                BSPLineSmooth(smoothBSPArr, (XmlElement)xnl[xnl.Count - 1], (XmlElement)xnl[0], (XmlElement)xnl[1], (XmlElement)xnl[2]);
            }
            else
            {
                smoothBSPArr.Add(new LitNode((XmlElement)xnl[1]));
                smoothBSPArr.Add(new LitNode((XmlElement)xnl[2]));
            }
            smoothBSPArr[startIdx].pos = 1;
            smoothBSPArr[endIdx].pos = 2;

            int loc2 = -1;
            int loc3 = -1;
            List<LitNode> loc4 = smoothBSPArr;
            loc1 = 0;
            while (loc1 < loc4.Count)
            {
                if (loc4[loc1].pos != 1)
                {
                    if (loc4[loc1].pos == 2)
                        loc3 = loc1;
                }
                else
                    loc2 = loc1;
                ++loc1;
            }
            if (loc2 != -1 && loc3 != -1)
            {
                int loc5 = 0;
                loc1 = loc2;
                while (loc5 < loc4.Count)
                {
                    upPoint.Add(loc4[loc1]);
                    if (loc1 == loc3)
                    {
                        break;
                    }
                    if (loc1 == loc4.Count - 1)
                        loc1 = -1;
                    ++loc1;
                    ++loc5;
                }
                loc5 = 0;
                loc1 = loc3;
                while (loc5 < loc4.Count)
                {
                    downPoint.Insert(0, loc4[loc1]);
                    if (loc1 == loc2)
                    {
                        break;
                    }
                    if (loc1 == loc4.Count - 1)
                        loc1 = -1;
                    ++loc5;
                    ++loc1;
                }
            }

            int m_step = upPoint.Count < downPoint.Count ? upPoint.Count : downPoint.Count;
            int dot_step = 0;
            int up_step = 0;
            int down_step = 0;
            dot_step++;
            List<Vector3> orgList = new List<Vector3>();
            float x = (upPoint[0].x + downPoint[0].x) / 2 + Offx;
            float y = (upPoint[0].y + downPoint[0].y) / 2 + Offy;
            orgList.Add(new Vector3(x, -y));
            while (dot_step < m_step)
            {
                up_step = dot_step * upPoint.Count / m_step;
                down_step = dot_step * downPoint.Count / m_step;
                if (dot_step % 10 == 0)
                {
                    x = (upPoint[up_step].x + downPoint[down_step].x) / 2 + Offx;
                    y = (upPoint[up_step].y + downPoint[down_step].y) / 2 + Offy;
                    orgList.Add(new Vector3(x, -y));
                }
                ++dot_step;
            }
            x = (upPoint[up_step - 1].x + downPoint[down_step - 1].x) / 2 + Offx;
            y = (upPoint[up_step - 1].y + downPoint[down_step - 1].y) / 2 + Offy;
            orgList.Add(new Vector3(x, -y));

            Bihua bihua = new Bihua();
            bihua.smoothBSPArr = smoothBSPArr;
            bihua.upPoint = upPoint;
            bihua.downPoint = downPoint;
            bihua.orgList = orgList;
            bihua.dashList = CompareLine.ResampleByLen(orgList, 14);
            bihua.isShow = false;
            if (xnlPoints[k].Attributes != null && xnlPoints[k].Attributes.Count > 0)
            {
                bihua.name = xnlPoints[k].Attributes["name"].Value;
                if (xnlPoints[k].Attributes.Count > 1) //获取图片id，这个是运行比较图片程序后得到的。（XE（编辑）->XA（比较）->XE（编辑：得到图片）->XP（生成：得到图片））
                {
                    bihua.bhKey = xnlPoints[k].Attributes["picIdx"].Value;
                }
                else
                {
                    bihua.bhKey = defaultName;
                }
            }
            bihua.basePoint = basePoint;
            Vector3 max = Vector3.zero;
            Vector3 min = Vector3.zero;
            bihua.center = CaculateCenter(bihua, out max, out min);
            maxBushouV.Add(max);
            minBushouV.Add(min);
            CheckRim(bihua);
            arrBihua.Add(bihua);
            arrNumber.Add(string.Format("{0}", k + 1));
            yield return null;
        }
        isInited = true;
    }
    void CheckRim(Bihua bh)
    {
        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < bh.upPoint.Count; ++i)
        {
            Vector3 p = bh.upPoint[i].TrsVec;
            min.x = Mathf.Min(min.x, p.x);
            min.y = Mathf.Min(min.y, p.y);
            max.x = Mathf.Max(max.x, p.x);
            max.y = Mathf.Max(max.y, p.y);
        }
        for (int i = 0; i < bh.downPoint.Count; ++i)
        {
            Vector3 p = bh.downPoint[i].TrsVec;
            min.x = Mathf.Min(min.x, p.x);
            min.y = Mathf.Min(min.y, p.y);
            max.x = Mathf.Max(max.x, p.x);
            max.y = Mathf.Max(max.y, p.y);
        }
        max.x = Mathf.CeilToInt(max.x);
        max.y = Mathf.CeilToInt(max.y);
        min.x = Mathf.FloorToInt(min.x);
        min.y = Mathf.FloorToInt(min.y);
        bh.maxRimPoint = max;
        bh.minRimPoint = min;
    }
    Vector3 CaculateCenter(Bihua bh, out Vector3 max, out Vector3 min)
    {
        float minX = float.MaxValue;
        float maxX = 0;
        float minY = float.MaxValue;
        float maxY = 0;
        for (int i = 0; i < bh.upPoint.Count; i++)
        {
            if (minX > bh.upPoint[i].x)
                minX = bh.upPoint[i].x;
            if (maxX < bh.upPoint[i].x)
                maxX = bh.upPoint[i].x;
            if (minY > bh.upPoint[i].y)
                minY = bh.upPoint[i].y;
            if (maxY < bh.upPoint[i].y)
                maxY = bh.upPoint[i].y;
        }
        for (int i = 0; i < bh.downPoint.Count; i++)
        {
            if (minX > bh.downPoint[i].x)
                minX = bh.downPoint[i].x;
            if (maxX < bh.downPoint[i].x)
                maxX = bh.downPoint[i].x;
            if (minY > bh.downPoint[i].y)
                minY = bh.downPoint[i].y;
            if (maxY < bh.downPoint[i].y)
                maxY = bh.downPoint[i].y;
        }
        Vector3 center = new Vector3((minX + maxX) / 2, -(minY + maxY) / 2, 0);
        max = new Vector3(maxX, maxY, 0);
        min = new Vector3(minX, minY, 0);
        return center;
    }
    void BSPLineSmooth(List<LitNode> arg1, XmlElement xml2, XmlElement xml3, XmlElement xml4, XmlElement xml5)
    {
        LitNode arg2 = new LitNode(xml2);
        LitNode arg3 = new LitNode(xml3);
        LitNode arg4 = new LitNode(xml4);
        LitNode arg5 = new LitNode(xml5);
        BSPLineSmooth(arg1, arg2, arg3, arg4, arg5);
    }
    void BSPLineSmooth(List<LitNode> arg1, LitNode arg2, LitNode arg3, LitNode arg4, LitNode arg5)
    {
        LitNode loc5 = null;
        float loc8 = 0;
        List<float> loc1 = new List<float>();
        List<float> loc2 = new List<float>();
        loc1.Add((-arg2.x + 3 * arg3.x - 3 * arg4.x + arg5.x) / 6);
        loc1.Add((3 * arg2.x - 6 * arg3.x + 3 * arg4.x) / 6);
        loc1.Add((-3 * arg2.x + 3 * arg4.x) / 6);
        loc1.Add((arg2.x + 4 * arg3.x + arg4.x) / 6);
        loc2.Add((-arg2.y + 3 * arg3.y - 3 * arg4.y + arg5.y) / 6);
        loc2.Add((3 * arg2.y - 6 * arg3.y + 3 * arg4.y) / 6);
        loc2.Add((-3 * arg2.y + 3 * arg4.y) / 6);
        loc2.Add((arg2.y + 4 * arg3.y + arg4.y) / 6);
        List<float> loc3 = new List<float>();
        List<float> loc4 = new List<float>();
        loc3.Add(loc1[3]);
        loc4.Add(loc2[3]);
        loc5 = new LitNode();
        loc5.x = loc1[3];
        loc5.y = loc2[3];
        loc5.pos = arg3.pos;
        loc5.org = arg3.org;
        arg1.Add(loc5);

        float dis = Vector2.Distance(loc5.Vec, starLit.Vec);
        if (dis < minDisStart)
        {
            minDisStart = dis;
            startIdx = arg1.Count - 1;
        }
        dis = Vector2.Distance(loc5.Vec, endLit.Vec);
        if (dis < minDisEnd)
        {
            minDisEnd = dis;
            endIdx = arg1.Count - 1;
        }

        int loc6 = (int)(CountDistance(arg3, arg4) / Scale_Value);
        int loc7 = 1;
        while (loc7 < loc6)
        {
            loc8 = (float)loc7 / loc6;
            loc3.Add(loc1[3] + loc8 * (loc1[2] + loc8 * (loc1[1] + loc8 * loc1[0])));
            loc4.Add(loc2[3] + loc8 * (loc2[2] + loc8 * (loc2[1] + loc8 * loc2[0])));
            loc5 = new LitNode();
            loc5.x = loc3[loc7];
            loc5.y = loc4[loc7];
            loc5.pos = 0;
            if (arg3.pos == 3)
                loc5.org = 2;
            arg1.Add(loc5);
            ++loc7;

            dis = Vector2.Distance(loc5.Vec, starLit.Vec);
            if (dis < minDisStart)
            {
                minDisStart = dis;
                startIdx = arg1.Count - 1;
            }
            dis = Vector2.Distance(loc5.Vec, endLit.Vec);
            if (dis < minDisEnd)
            {
                minDisEnd = dis;
                endIdx = arg1.Count - 1;
            }
        }
    }
    float CountDistance(LitNode arg1, LitNode arg2)
    {
        return Mathf.Round(Mathf.Sqrt(Mathf.Pow(arg1.x - arg2.x, 2) + Mathf.Pow(arg1.y - arg2.y, 2)));
    }

    public void DrawBihua(bool immediate)
    {
        if (isInited)
        {
            immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), defaultSideCol);
            drawingBihuaIdx = 0;
            blnImmediate = immediate;
            for (int i = 0; i < arrBihua.Count; i++)
                arrBihua[i].quadList.Clear();
            headQuad.Clear();
            headPos.Clear();
            StopAllCoroutines();
            StartCoroutine(DrawBihuaing());
        }
    }
    int max_Step = 0;
    int now_Step = 0;
    /// <summary>
    ///   first SetBegindrawmsg
    ///   second DrawingPercent
    ///   next SetDrawNext
    ///   other ResetDraw
    /// </summary>
    public void SetBeginDrawMsg()
    {
        if (isInited && blnImmediate)
        {
            immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), defaultSideCol);
            drawingBihuaIdx = 0;
            immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), selectSideCol);
            Bihua bh = arrBihua[drawingBihuaIdx];
            max_Step = bh.upPoint.Count < bh.downPoint.Count ? bh.upPoint.Count : bh.downPoint.Count;
            now_Step = 0;
            ResetDraw();
            headQuad.Clear();
        }
    }
    /// <summary>
    ///   first SetBegindrawmsg
    ///   second DrawingPercent
    ///   next SetDrawNext
    ///   other ResetDraw
    /// </summary>
    public void DrawingPecent(int per, Color color)
    {
        if (isInited && blnImmediate)
        {
            immediate_mat.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), color);
            //immediate_side.SetColor(string.Format("_Color{0}", drawingBihuaIdx + 1), color * 0.5f);
            Bihua bh = arrBihua[drawingBihuaIdx];
            now_Step = per * 10;
            int up_step = now_Step * bh.upPoint.Count / max_Step;
            int down_step = now_Step * bh.downPoint.Count / max_Step;
            if (up_step > bh.upPoint.Count - 1)
                up_step = bh.upPoint.Count - 1;
            if (down_step > bh.downPoint.Count - 1)
                down_step = bh.downPoint.Count - 1;
            for (int i = 0; i < bh.quadDrawInfo.Count; i++)
            {
                if (bh.quadDrawInfo[i].drawStep <= now_Step)
                {
                    //bh.quadDrawInfo[i].quadColorIdx = (nowColorIdx + bh.quadDrawInfo[i].bihuaIdx) % fillColors.Length + 1;
                    bh.quadDrawInfo[i].quadColorIdx = drawingBihuaIdx % imtColorCount + 1;
                }
            }
            if (now_Step < max_Step - 5)
                AddHeadQuad(bh.upPoint, bh.downPoint, up_step, down_step);
            else
                headQuad.Clear();
        }
    }
    public void DrawingPecent(int per, int idx, Color color)
    {
        if (isInited && blnImmediate)
        {
            immediate_mat.SetColor(string.Format("_Color{0}", idx + 1), color);
            Bihua bh = arrBihua[idx];
            for (int i = 0; i < bh.quadDrawInfo.Count; i++)
            {
                bh.quadDrawInfo[i].quadColorIdx = idx % imtColorCount + 1;
            }
        }
    }
    public void SetBushou(int bs)
    {
        if (isInited)
        {
            arrBihua[drawingBihuaIdx].bushou = bs;
        }
    }
    /// <summary>
    ///   first SetBegindrawmsg
    ///   second DrawingPercent
    ///   next SetDrawNext
    ///   other ResetDraw
    /// </summary>
    public void SetDrawNext()
    {
        if (isInited && blnImmediate)
        {
            drawingBihuaIdx++;
            if (drawingBihuaIdx < arrBihua.Count)
            {
                Bihua bh = arrBihua[drawingBihuaIdx];
                max_Step = bh.upPoint.Count < bh.downPoint.Count ? bh.upPoint.Count : bh.downPoint.Count;
                now_Step = 0;
            }
            headQuad.Clear();
        }
    }
    /// <summary>
    ///   first SetBegindrawmsg
    ///   second DrawingPercent
    ///   next SetDrawNext
    ///   other ResetDraw
    /// </summary>
    public void ResetDraw()
    {
        if (isInited && blnImmediate)
        {
            for (int b = 0; b < arrBihua.Count; b++)
            {
                Bihua bh = arrBihua[b];
                for (int i = 0; i < bh.quadDrawInfo.Count; i++)
                {
                    bh.quadDrawInfo[i].quadColorIdx = 0;
                }
            }
        }
    }
    IEnumerator DrawBihuaing()
    {
        quadCount = 0;
        for (int i = 0; i < arrBihua.Count; i++)
        {
            Bihua bh = arrBihua[i];
            int m_step = bh.upPoint.Count < bh.downPoint.Count ? bh.upPoint.Count : bh.downPoint.Count;
            int dot_step = 0;
            int up_step = 0;
            int down_step = 0;
            LitNode lastUp = bh.upPoint[0];
            LitNode lastDown = bh.downPoint[0];
            dot_step++;
            while (dot_step < m_step)
            {
                up_step = dot_step * bh.upPoint.Count / m_step;
                down_step = dot_step * bh.downPoint.Count / m_step;
                if (dot_step % 5 == 0 || dot_step < 5 || dot_step >= m_step - 5 ||
                    bh.upPoint[up_step].org != 0 || bh.downPoint[down_step].org != 0)
                {
                    AddQuad(lastUp, bh.upPoint[up_step], bh.downPoint[down_step], lastDown, i, dot_step);
                    quadCount++;
                    lastUp = bh.upPoint[up_step];
                    lastDown = bh.downPoint[down_step];
                    if (!blnImmediate)
                    {
                        if (dot_step < m_step - 5)
                            AddHeadQuad(bh.upPoint, bh.downPoint, up_step, down_step);
                        else
                            headQuad.Clear();
                    }
                }
                if (!blnImmediate)
                {
                    if (dot_step % 5 == 0)
                    {
                        yield return null;
                    }
                }
                ++dot_step;
            }
            if (!blnImmediate)
            {
                yield return null;
            }
            drawingBihuaIdx++;
        }
    }
    void AddQuad(LitNode n1, LitNode n2, LitNode n3, LitNode n4, int bihuaIdx, int nowStep)
    {
        arrBihua[bihuaIdx].quadList.Add(new Vector3(n1.x, - n1.y, 0));
        arrBihua[bihuaIdx].quadList.Add(new Vector3(n2.x, - n2.y, 0));
        arrBihua[bihuaIdx].quadList.Add(new Vector3(n3.x, - n3.y, 0));
        arrBihua[bihuaIdx].quadList.Add(new Vector3(n4.x, - n4.y, 0));
        int colorIdx = (nowColorIdx + bihuaIdx) % fillColors.Length + 1;
        DrawInfo info = new DrawInfo();
        if (blnImmediate)
        {
            colorIdx = 0;
        }
        info.quadColorIdx = colorIdx;
        info.bihuaIdx = bihuaIdx;
        info.drawStep = nowStep;
        arrBihua[bihuaIdx].quadDrawInfo.Add(info);
    }
    void AddHeadQuad(List<LitNode> up, List<LitNode> down, int upStep, int downStep)
    {
        headPos.Clear();
        int control_d = Mathf.Clamp((int)(20f / Scale_Value), 10, 20);
        List<Vector3> tmpList = new List<Vector3>();
        tmpList.Add(new Vector2(up[upStep].x, - up[upStep].y));
        if (upStep + control_d < up.Count)
            tmpList.Add(new Vector2(up[upStep + control_d].x, - up[upStep + control_d].y));
        else
            tmpList.Add(new Vector2(up[up.Count - 1].x, - up[up.Count - 1].y));
        if (downStep + control_d < down.Count)
            tmpList.Add(new Vector2(down[downStep + control_d].x, - down[downStep + control_d].y));
        else
            tmpList.Add(new Vector2(down[down.Count - 1].x, - down[down.Count - 1].y));
        tmpList.Add(new Vector2(down[downStep].x, - down[downStep].y));
        Helper.ComputeBezier(tmpList, 16, headPos);

        headQuad.Clear();
        if (headPos.size >= 4)
        {
            Vector3 up0, up1, down0, down1;
            for (int i = 0; i < headPos.size / 2 - 1; i++)
            {
                up0 = headPos[i];
                up1 = headPos[i + 1];
                down0 = headPos[headPos.size - i - 1];
                down1 = headPos[headPos.size - i - 2];
                headQuad.Add(new Vector3(up0.x, up0.y, 0));
                headQuad.Add(new Vector3(up1.x, up1.y, 0));
                headQuad.Add(new Vector3(down1.x, down1.y, 0));
                headQuad.Add(new Vector3(down0.x, down0.y, 0));
            }
        }
    }
    void Update()
    {
        if (blnMoveQuad)
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 t = Input.mousePosition;
                moveVec += t - lastMousePos;
                lastMousePos = t;

                print(moveVec);
            }
        }
    }
    [HideInInspector]
    public Vector3 moveVec = Vector3.zero;
    Vector3 lastMousePos = Vector3.zero;
    [HideInInspector]
    public int drawIdx = -1;
    void OnGUI()
    {
        //
    }
    void OnEnable()
    {
        moveVec.x = PlayerPrefs.GetFloat("moveVec.x", Screen.width / 2 - 400 * Scale_Value / 2);
        moveVec.y = PlayerPrefs.GetFloat("moveVec.y", Screen.height / 2  + 400 * Scale_Value / 2);
        moveVec.z = 0;
    }
    void OnDisable()
    {
        //PlayerPrefs.SetFloat("moveVec.x", moveVec.x);
        //PlayerPrefs.SetFloat("moveVec.y", moveVec.y);
        PlayerPrefs.DeleteAll();
    }
    Vector3 tempMoveV = Vector3.zero;
    void OnPostRender()
    {
        if (!isInited)
            return;
        if (!mat)
        {
            CreateMaterial();
            return;
        }
        GL.PushMatrix();
        GL.LoadOrtho();
        
        //GL.Begin(GL.LINES);
        //immediate_side.SetPass(0);
        //GL.Vertex3(0 / Screen.width, 0 / Screen.height, 0);
        //GL.Vertex3(200f / Screen.width, 200f / Screen.height, 0);
        //GL.End();
        for (int d = 0; d < 2; d++)
        {
            for (int i = 0; i < arrBihua.Count; i++)
            {
                if (drawIdx != i && drawIdx != -1)
                    continue;
                if ((i == drawingBihuaIdx && d == 0) || (i != drawingBihuaIdx && d == 1))
                    continue;
                //tempMoveV = arrBihua[i].minRimPoint;
                if (blnDrawQuad)
                {
                    for (int q = 0; q < arrBihua[i].quadList.Count / 4; q++)
                    {
                        GL.Begin(GL.QUADS);
                        if (!blnImmediate)
                            mat.SetPass(arrBihua[i].quadDrawInfo[q].quadColorIdx);
                        else
                            immediate_mat.SetPass(arrBihua[i].quadDrawInfo[q].quadColorIdx);
                        DrawQuads(arrBihua[i].quadList[q * 4], arrBihua[i].quadList[q * 4 + 1], arrBihua[i].quadList[q * 4 + 2], arrBihua[i].quadList[q * 4 + 3]);
                        GL.End();
                    }
                }
                int nowIdx = (nowColorIdx + i) % fillColors.Length + 1;
                GL.Begin(GL.QUADS);
                if (i == drawingBihuaIdx && blnImmediate)
                    immediate_mat.SetPass(arrBihua[i].quadDrawInfo[0].quadColorIdx);
                else
                    mat.SetPass(nowIdx);
                for (int h = 0; h < headQuad.size / 4 && i == drawingBihuaIdx; h++)
                {
                    DrawQuads(headQuad[h * 4], headQuad[h * 4 + 1], headQuad[h * 4 + 2], headQuad[h * 4 + 3]);
                }
                GL.End();

                nowIdx = (nowColorIdx + i) % sideColors.Length + fillColors.Length + 1;
                GL.Begin(GL.LINES);
                //if (!blnImmediate)
                //    mat.SetPass(nowIdx);
                //else
                //{
                //    if (i < drawingBihuaIdx)
                //        immediate_side.SetPass(arrBihua[i].quadDrawInfo[0].quadColorIdx);
                //    else
                //        immediate_side.SetPass(0);
                //}
                immediate_side.SetPass(i + 1);
                for (int m = 0; m < arrBihua[i].upPoint.Count - 1; m++)
                {
                    DrawSingleLine(arrBihua[i].upPoint[m].TrsVec, arrBihua[i].upPoint[m + 1].TrsVec);
                }
                for (int m = 0; m < arrBihua[i].downPoint.Count - 1; m++)
                {
                    DrawSingleLine(arrBihua[i].downPoint[m].TrsVec, arrBihua[i].downPoint[m + 1].TrsVec);
                }
                //DrawSingleLine(arrBihua[i].upPoint[0].TrsVec, arrBihua[i].downPoint[0].TrsVec);
                //DrawSingleLine(arrBihua[i].upPoint[arrBihua[i].upPoint.Count - 1].TrsVec, arrBihua[i].downPoint[arrBihua[i].downPoint.Count - 1].TrsVec);
                GL.End();
            }
        }
        GL.PopMatrix();
    }
    void DrawSingleLine(Vector3 p1, Vector3 p2)
    {
        p1.x = p1.x + moveVec.x - tempMoveV.x;
        p1.y = p1.y + moveVec.y - tempMoveV.y;
        p2.x = p2.x + moveVec.x - tempMoveV.x;
        p2.y = p2.y + moveVec.y - tempMoveV.y;
        GL.Vertex3(p1.x / Screen.width, p1.y / Screen.height, 0);
        GL.Vertex3(p2.x / Screen.width, p2.y / Screen.height, 0);
    }
    void DrawSingleLine(Vector3 p1, Vector3 p2, float offx, float offy)
    {
        p1.x = p1.x + moveVec.x - tempMoveV.x;
        p1.y = p1.y + moveVec.y - tempMoveV.y;
        p2.x = p2.x + moveVec.x - tempMoveV.x;
        p2.y = p2.y + moveVec.y - tempMoveV.y;
        GL.Vertex3((p1.x + offx) / Screen.width, (p1.y + offy) / Screen.height, 0);
        GL.Vertex3((p2.x + offx) / Screen.width, (p2.y + offy) / Screen.height, 0);
    }
    void DrawQuads(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        p1.x = p1.x + moveVec.x - tempMoveV.x;
        p1.y = p1.y + moveVec.y - tempMoveV.y;
        p2.x = p2.x + moveVec.x - tempMoveV.x;
        p2.y = p2.y + moveVec.y - tempMoveV.y;
        p3.x = p3.x + moveVec.x - tempMoveV.x;
        p3.y = p3.y + moveVec.y - tempMoveV.y;
        p4.x = p4.x + moveVec.x - tempMoveV.x;
        p4.y = p4.y + moveVec.y - tempMoveV.y;
        GL.Vertex3(p1.x / Screen.width, p1.y / Screen.height, 0);
        GL.Vertex3(p2.x / Screen.width, p2.y / Screen.height, 0);
        GL.Vertex3(p3.x / Screen.width, p3.y / Screen.height, 0);
        GL.Vertex3(p4.x / Screen.width, p4.y / Screen.height, 0);
    }
    void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        p1.x = p1.x + moveVec.x - tempMoveV.x;
        p1.y = p1.y + moveVec.y - tempMoveV.y;
        p2.x = p2.x + moveVec.x - tempMoveV.x;
        p2.y = p2.y + moveVec.y - tempMoveV.y;
        p3.x = p3.x + moveVec.x - tempMoveV.x;
        p3.y = p3.y + moveVec.y - tempMoveV.y;
        GL.Vertex3(p1.x / Screen.width, p1.y / Screen.height, 0);
        GL.Vertex3(p2.x / Screen.width, p2.y / Screen.height, 0);
        GL.Vertex3(p3.x / Screen.width, p3.y / Screen.height, 0);
    }

    StringBuilder sBuild = new StringBuilder();
    public string GetBihuaOrder()
    {
        sBuild.Length = 0;
        for (int i = 0; i < arrBihua.Count; i++)
        {
            //sBuild.Append(arrBihua[i].name);
            for (int a = 0; a < XEControl.Ins.bhName.Length; a++)
            {
                if (arrBihua[i].name == XEControl.Ins.bhName[a])
                {
                    sBuild.Append(a);
                    sBuild.Append('\t');
                }
            }
        }
        return sBuild.ToString();
    }
    public string ConvertBihua()
    {
        sBuild.Length = 0;
        for (int i = 0; i < arrBihua.Count; i++)
        {
            sBuild.Append(string.Format("{0:0.0}", arrBihua[i].center.x));
            sBuild.Append(",");
            sBuild.Append(string.Format("{0:0.0}", arrBihua[i].center.y));
            sBuild.Append(",");
            sBuild.Append(Mathf.CeilToInt(arrBihua[i].orgList[0].x));
            sBuild.Append(",");
            sBuild.Append(Mathf.CeilToInt(arrBihua[i].orgList[0].y));
            sBuild.Append(",");
            sBuild.Append(Mathf.CeilToInt(arrBihua[i].orgList[arrBihua[i].orgList.Count - 1].x));
            sBuild.Append(",");
            sBuild.Append(Mathf.CeilToInt(arrBihua[i].orgList[arrBihua[i].orgList.Count - 1].y));
            sBuild.Append(",");
            sBuild.Append(arrBihua[i].bushou);
            sBuild.Append(",");
            sBuild.Append(arrBihua[i].bhKey);
            sBuild.Append("_");
            for (int j = 0; j < arrBihua[i].basePoint.Count; j++)
            {
                sBuild.Append(Mathf.CeilToInt(arrBihua[i].basePoint[j].x - arrBihua[i].center.x));
                sBuild.Append(",");
                sBuild.Append(Mathf.CeilToInt(arrBihua[i].basePoint[j].y - arrBihua[i].center.y));
                if (j != arrBihua[i].basePoint.Count - 1)
                    sBuild.Append(",");
            }
            sBuild.Append("|");
        }
        sBuild.Append(ConvertBushouPos());
        return sBuild.ToString();
    }
    List<int> arrBushouIdx = new List<int>();
    StringBuilder sBuild2 = new StringBuilder();
    public string ConvertBushouPos()
    {
        sBuild2.Length = 0;
        arrBushouIdx.Clear();
        for (int i = 0; i < arrBihua.Count; i++)
        {
            if (!arrBushouIdx.Contains(arrBihua[i].bushou))
            {
                arrBushouIdx.Add(arrBihua[i].bushou);
            }
        }
        for (int i = 0; i < arrBushouIdx.Count; i++)
        {
            float minX = float.MaxValue;
            float maxX = 0;
            float minY = float.MaxValue;
            float maxY = 0;
            for (int j = 0; j < arrBihua.Count; j++)
            {
                if (arrBihua[j].bushou == i)
                {
                    if (maxBushouV[j].x > maxX)
                        maxX = maxBushouV[j].x;
                    if (maxBushouV[j].y > maxY)
                        maxY = maxBushouV[j].y;
                    if (minBushouV[j].x < minX)
                        minX = minBushouV[j].x;
                    if (minBushouV[j].y < minY)
                        minY = minBushouV[j].y;
                }
            }
            sBuild2.Append(string.Format("{0:0.0}", (maxX + minX) * 0.5f));//x
            sBuild2.Append(",");
            sBuild2.Append(string.Format("{0:0.0}", -(maxY + minY) * 0.5f));//y
            if (i != arrBushouIdx.Count - 1)
                sBuild2.Append(",");
        }
        return sBuild2.ToString();
    }

    public class DrawInfo
    {
        public int quadColorIdx = 0;
        public int bihuaIdx = 0;
        public int drawStep = 0;
        public DrawInfo() { }
        public DrawInfo Clone()
        {
            DrawInfo dif = new DrawInfo();
            dif.quadColorIdx = this.quadColorIdx;
            dif.bihuaIdx = this.bihuaIdx;
            dif.drawStep = this.drawStep;
            return dif;
        }
    }
    public class Bihua
    {
        public List<LitNode> smoothBSPArr;
        public List<LitNode> upPoint;
        public List<LitNode> downPoint;
        public List<Vector3> dashList;
        public List<Vector3> orgList;
        public List<Vector3> quadList;
        public List<DrawInfo> quadDrawInfo;
        public List<Vector2> basePoint;
        public bool isShow = false;
        public Vector3 center = Vector3.zero;
        public int bushou = 0;
        public string name = "";
        public Vector3 maxRimPoint = Vector3.zero;
        public Vector3 minRimPoint = Vector3.zero;
        public string bhKey = "k";
        public Bihua()
        {
            smoothBSPArr = new List<LitNode>();
            upPoint = new List<LitNode>();
            downPoint = new List<LitNode>();
            dashList = new List<Vector3>();
            orgList = new List<Vector3>();
            quadList = new List<Vector3>();
            quadDrawInfo = new List<DrawInfo>();
            basePoint = new List<Vector2>();
            isShow = false;
            bushou = 0;
        }
        public Bihua Clone()
        {
            Bihua bh = new Bihua();
            for (int i = 0; i < this.smoothBSPArr.Count; i++)
                bh.smoothBSPArr.Add(this.smoothBSPArr[i].Clone());
            for (int i = 0; i < this.upPoint.Count; i++)
                bh.upPoint.Add(this.upPoint[i].Clone());
            for (int i = 0; i < this.upPoint.Count; i++)
                bh.downPoint.Add(this.downPoint[i].Clone());
            bh.dashList.AddRange(this.dashList);
            bh.orgList.AddRange(this.orgList);
            bh.quadList.AddRange(this.quadList);
            for (int i = 0; i < this.quadDrawInfo.Count; i++)
                bh.quadDrawInfo.Add(this.quadDrawInfo[i].Clone());
            bh.basePoint.AddRange(this.basePoint);
            bh.isShow = this.isShow;
            bh.bushou = this.bushou;
            bh.name = this.name;
            bh.center = this.center;
            bh.bhKey = this.bhKey;
            bh.maxRimPoint = this.maxRimPoint;
            bh.minRimPoint = this.minRimPoint;
            return bh;
        }
    }
    public class LitNode
    {
        public LitNode Clone()
        {
            LitNode ln = new LitNode();
            ln.x = this.x;
            ln.y = this.y;
            ln.pos = this.pos;
            ln.org = this.org;
            ln.vec = this.vec;
            return ln;
        }
        public LitNode()
        {
            x = 0;
            y = 0;
            pos = 0;
            org = 0;
            Vec = Vector3.zero;
        }
        public LitNode(XmlElement xml)
        {
            x = XmlConvert.ToInt32(xml.Attributes["x"].Value) * Scale_Value;
            y = XmlConvert.ToInt32(xml.Attributes["y"].Value) * Scale_Value;
            pos = XmlConvert.ToInt32(xml.Attributes["pos"].Value);
            org = 1;
            Vec = Vector3.zero;
        }
        static public float Distance(LitNode ln1, LitNode ln2)
        {
            return Vector2.Distance(new Vector2(ln1.x, ln1.y), new Vector2(ln2.x, ln2.y));
        }
        public float xmlX()
        {
            return x / Scale_Value;
        }
        public float xmlY()
        {
            return y / Scale_Value;
        }
        public float x;
        public float y;
        public int pos;
        public int org;
        Vector3 vec;
        public Vector3 Vec
        {
            get
            {
                vec.x = x;
                vec.y = y;
                vec.z = 0;
                return vec;
            }
            set { vec = value; }
        }
        public Vector3 TrsVec
        {
            get
            {
                vec.x = x;
                vec.y = - y;
                vec.z = 0;
                return vec;
            }
        }
    }
}
