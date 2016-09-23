using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Bishun2Control : MonoBehaviour {
    private static Bishun2Control ins = null;
    public static Bishun2Control Ins
    {
        get
        {
            if (!ins)
                ins = GameObject.FindObjectOfType(typeof(Bishun2Control)) as Bishun2Control;
            return ins;
        }
    }

    public Transform pointParent;
    public GameObject pointPrefab;
    public XmlDocument xmlChar;
    List<GameObject> listPoint;
    List<GameObject> listOrgPoint;
    List<GameObject> unUsedPoint;
    public GameObject pointCamera;
    List<List<Vector2>> resPos;
    string strXml = "";
    string compareStr = "";
    List<Vector2> mousePos;
    Vector2 lastMousePos;
    public float bestDis = 25;
    public float failPercent = 0.5f;
    public float bestPercent = .3f;
    public float[] bestScore = new float[5] { 0.9f, 0.7f, 0.5f, 0.3f, 0f };
    int lastDiameter;
    public GameObject finger;
    public GameObject arrowPerfab;
    public GameObject dashLinePrefab;
    public DrawWord drawWord;
    public GameObject colorPrefab;
    public Transform colorParent;
    public GameObject firstColor;
    public GameObject secondColor;
    List<string> wordName;
    int wordIdx = 0;
    float pressedTime;
    public GameObject fingerPressPar;
    public GameObject fingerMovePar;
    
    void Start()
    {
        if (!ins)
            ins = this;
        listPoint = new List<GameObject>();
        listOrgPoint = new List<GameObject>();
        unUsedPoint = new List<GameObject>();
        xmlChar = new XmlDocument();

        resPos = new List<List<Vector2>>();
        mousePos = new List<Vector2>();

        wordName = new List<string>();
        wordName.Add("19968");
        wordName.Add("26597");
        wordName.Add("38718");
        wordName.Add("39635");
        wordIdx = UnityEngine.Random.Range(0, wordName.Count);

        fingerPressPar.SetActive(false);
        fingerMovePar.SetActive(false);

        //pointCamera.GetComponent<Camera>().targetTexture.
        //Texture2D text;// = new Texture2D()
        //text.LoadImage()
        //text.ReadPixels()

        //gameObject.renderer.material.mainTexture = Resources.Load("xxx.png") as Texture2D;
        //gameObject.renderer.material.SetTexture("_MainTex", Resources.Load("xxx.png") as Texture2D);

        StartCoroutine(LoadXmlDatas());

        //TranslateBin();
        InitColorButton();
    }
    void InitColorButton()
    {
        float[] col = new float[]
        {
            0,0,0,
            255,255,255,
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
            216,0,125
        };
        int colorIdx = 0;
        Color tmpCol = Color.black;
        UIEventListener ue;
        UISprite us;
        UIButton ub;
        for (int i = 0; i < 14; i++)
        {
            float x = -(i % 2 * 82) - 80;
            float y = (i / 2 * 82) + 50 - 240;
            GameObject colorButton = (GameObject)Instantiate(colorPrefab);
            colorButton.transform.parent = colorParent;
            colorButton.transform.localScale = Vector3.one;
            colorButton.transform.localPosition = new Vector3(x, y, 0);
            colorButton.transform.localEulerAngles = Vector3.zero;
            colorButton.name = "Color" + i;
            ue = colorButton.GetComponent<UIEventListener>();
            if (ue)
                ue.onClick += SelectColor;
            us = colorButton.GetComponent<UISprite>();
            ub = colorButton.GetComponent<UIButton>();
            if (us)
            {
                colorIdx = i % drawWord.fillColors.Length;
                tmpCol = new Color(col[i * 3 + 0] / 255, col[i * 3 + 1] / 255, col[i * 3 + 2] / 255);
                us.color = tmpCol;
                ub.defaultColor = tmpCol;
                ub.hover = tmpCol;
                ub.pressed = tmpCol;
                ub.disabledColor = new Color(tmpCol.r, tmpCol.g, tmpCol.b, tmpCol.a / 4);
                ub.UpdateColor(true, true);
            }
        }
        ue = firstColor.GetComponent<UIEventListener>();
        if (ue)
            ue.onClick += SelectColor;
        us = firstColor.GetComponent<UISprite>();
        ub = firstColor.GetComponent<UIButton>();
        colorIdx = UnityEngine.Random.Range(0, col.Length / 3 - 2) + 2;
        tmpCol = new Color(col[colorIdx * 3 + 0] / 255, col[colorIdx * 3 + 1] / 255, col[colorIdx * 3 + 2] / 255);
        us.color = tmpCol;
        ub.defaultColor = tmpCol;
        ub.hover = tmpCol;
        ub.pressed = tmpCol;
        ub.disabledColor = new Color(tmpCol.r, tmpCol.g, tmpCol.b, tmpCol.a / 4); ;
        ub.UpdateColor(true, true);

        ue = secondColor.GetComponent<UIEventListener>();
        if (ue)
            ue.onClick += SelectColor;
        us = secondColor.GetComponent<UISprite>();
        ub = secondColor.GetComponent<UIButton>();
        colorIdx = UnityEngine.Random.Range(0, col.Length / 3 - 2) + 2;
        tmpCol = new Color(col[colorIdx * 3 + 0] / 255, col[colorIdx * 3 + 1] / 255, col[colorIdx * 3 + 2] / 255);
        us.color = tmpCol;
        ub.defaultColor = tmpCol;
        ub.hover = tmpCol;
        ub.pressed = tmpCol;
        ub.disabledColor = new Color(tmpCol.r, tmpCol.g, tmpCol.b, tmpCol.a / 4);
        ub.UpdateColor(true, true);
        firstColor.transform.parent.localPosition = new Vector3(-120, -272, 0);
    }
    void SelectColor(GameObject button)
    {
        UIButton fub = firstColor.GetComponent<UIButton>();
        UIButton sub = secondColor.GetComponent<UIButton>();
        if (button == firstColor || button == secondColor)
        {
            Color tmpCol = (fub.defaultColor + sub.defaultColor) * 0.5f;
            fub.defaultColor = tmpCol;
            fub.hover = tmpCol;
            fub.pressed = tmpCol;
            fub.disabledColor = new Color(tmpCol.r, tmpCol.g, tmpCol.b, tmpCol.a / 4); ;
            fub.UpdateColor(true, true);
            sub.defaultColor = tmpCol;
            sub.hover = tmpCol;
            sub.pressed = tmpCol;
            sub.disabledColor = new Color(tmpCol.r, tmpCol.g, tmpCol.b, tmpCol.a / 4); ;
            sub.UpdateColor(true, true);
        }
        else
        {
            UIButton nub = button.GetComponent<UIButton>();
            Color tmpCol = nub.defaultColor;
            sub.defaultColor = fub.defaultColor;
            sub.hover = fub.defaultColor;
            sub.pressed = fub.defaultColor;
            sub.disabledColor = new Color(fub.disabledColor.r, fub.disabledColor.g, fub.disabledColor.b, fub.disabledColor.a / 4);
            sub.UpdateColor(true, true);
            fub.defaultColor = tmpCol;
            fub.hover = tmpCol;
            fub.pressed = tmpCol;
            fub.disabledColor = new Color(tmpCol.r, tmpCol.g, tmpCol.b, tmpCol.a / 4); ;
            fub.UpdateColor(true, true);
        }
    }
    void Update()
    {
        if (ins == this)
        {
            Control();
        }
        if (Application.platform == RuntimePlatform.Android && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Home)))
        {
            Application.Quit();
        }
    }
    void Control()
    {
        FingerMoving();
    }
	void OnEnable()
	{
		MyTouch.On_OneBegan += OnOneBegan;
		MyTouch.On_OneMoved += OnOneMoved;
		MyTouch.On_OneEnded += OnOneEnded;
	}
	void OnDisable()
	{
		UnsubscribeEvent();
	}
	void OnDestroy()
	{
		UnsubscribeEvent();
	}
	void UnsubscribeEvent()
	{
		MyTouch.On_OneBegan -= OnOneBegan;
		MyTouch.On_OneMoved -= OnOneMoved;
		MyTouch.On_OneEnded -= OnOneEnded;
	}
	void OnOneBegan(MyGesture mg)
	{
        if (writeState == 1)
        {
            if (blnNext)
            {
                mousePos = new List<Vector2>();
                blnNext = false;
            }
            Vector2 pos = mg.position;
            mousePos.Add(new Vector2(pos.x, Screen.height - pos.y));
            lastMousePos = pos;
            lastDiameter = 20;
            //CalculateThrough(pos, THROUGH_TYPE.THROUGH_BEGAN);
            pressedTime = Time.time;

            fingerPressPar.transform.localPosition = new Vector3(pos.x, pos.y - Screen.height);
            if (!fingerPressPar.activeSelf)
                fingerPressPar.SetActive(true);
            fingerPressPar.GetComponent<ParticleSystem>().Play();
        }
	}
	void OnOneMoved(MyGesture mg)
	{
        ParticleSystem fmp_ps = fingerMovePar.GetComponent<ParticleSystem>();
        if (writeState == 1)
        {
            Vector2 pos = mg.position;
            float dis = Vector2.Distance(pos, lastMousePos);
            fingerPressPar.transform.localPosition = new Vector3(pos.x, pos.y - Screen.height);
            fingerMovePar.transform.localPosition = new Vector3(pos.x, pos.y - Screen.height);
            if (!fingerMovePar.activeSelf)
            {
                fingerMovePar.SetActive(true);
            }
            if (dis >= 1 && (Time.time - pressedTime) * 10 >= 1)
            {
                int count = Mathf.Max(1, (int)(dis / 4));
                mousePos.Add(new Vector2(pos.x, Screen.height - pos.y));
                float speed = 25 - dis * 6 * 25 / Screen.height;
                int diameter = Mathf.Max((int)speed, 10);
                int nowDiameter = lastDiameter;
                for (int i = 0; i < count; i++)
                {
                    nowDiameter = (int)Mathf.Lerp(lastDiameter, diameter, (float)(i + 1) / count);
                    Vector2 tmp = Vector2.Lerp(lastMousePos, pos, (float)(i + 1) / count);
                    CalculateThrough(tmp, THROUGH_TYPE.THROUGH_MOVED);
                }
                lastDiameter = nowDiameter;
                lastMousePos = pos;

                if (fmp_ps.isStopped)
                    fmp_ps.Play();
            }
            else
            {
                if (fmp_ps.isPlaying)
                    fmp_ps.Stop();
            } 
        }
	}
	void OnOneEnded(MyGesture mg)
	{
        if (writeState == 1)
        {
            if (isThrough.Count > 0)
            {
                int overIdx = isThrough.Count * 90 / 100 - 1;
                if (overIdx < 1) overIdx = 1;
                if (isThrough[overIdx])
                {
                    CalculateThrough(mg.position, THROUGH_TYPE.THROUGH_ENDED);
                    resPos.Add(mousePos);
                    drawWord.DrawingPecent(1000, firstColor.GetComponent<UIButton>().defaultColor);
                    NextDashLine();
                    drawWord.SetDrawNext();
                }
            }
        }
        fingerPressPar.GetComponent<ParticleSystem>().Stop();
        fingerMovePar.GetComponent<ParticleSystem>().Stop();
	}
    void OnGUI()
    {
        //if (GUI.Button(new Rect(200, Screen.height - 55, 180, 50), "Compare"))
        //{
        //    if (drawWord.isInited)
        //    {
        //        if (resPos.Count > 0)
        //        {
        //            float value = -1;
        //            int goodScore = 0;
        //            int badScore = int.MaxValue;
        //            compareStr = "";
        //            if (resPos.Count < drawWord.arrBihua.Count)
        //            {
        //                compareStr = "respos.count < arrBihua.count\n";
        //            }
        //            else
        //            {
        //                for (int i = 0, j = 0; i < drawWord.arrBihua.Count; )
        //                {
        //                    if (j >= resPos.Count)
        //                        break;
        //                    else
        //                    {
        //                        if (resPos[j].Count < 2)
        //                            j++;
        //                        else
        //                        {
        //                            if (value == -1)
        //                                value = 0;
        //                            CompareLine.NormalizedPointCount = drawWord.arrBihua[i].orgList.Count;
        //                            float tmpV = CompareLine.CompareNormal(drawWord.arrBihua[i].orgList, resPos[j]);
        //                            float percent = tmpV / (bestDis * drawWord.arrBihua[i].orgList.Count);
        //                            value += percent;
        //                            if (percent * 100 <= failPercent * 100)//success
        //                            {
        //                                for (int p = 0; p < bestScore.Length; p++)
        //                                {
        //                                    if ((1 - percent) * 100 >= bestScore[p] * 100)
        //                                    {
        //                                        goodScore += bestScore.Length - p;
        //                                        break;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                for (int p = 0; p < bestScore.Length; p++)
        //                                {
        //                                    if ((1 - percent) * 100 >= bestScore[p] * 100)
        //                                    {
        //                                        if (bestScore.Length - p < badScore)
        //                                        {
        //                                            badScore = bestScore.Length - p;
        //                                        }
        //                                        break;
        //                                    }
        //                                }
        //                            }
        //                            i++;
        //                            j++;
        //                        }
        //                    }
        //                }
        //            }
        //            int score = goodScore / drawWord.arrBihua.Count;
        //            if (badScore != int.MaxValue)
        //                score = badScore;
        //            compareStr += "Similarity:" + value.ToString() + " Score:" + score + "\n" +
        //                "GoodScore:" + goodScore + " BadScore:" + badScore;
        //        }
        //    }
        //}
        if (GUI.Button(new Rect(400, Screen.height - 55, 180, 50), "Clear"))
        {
            while (listPoint.Count > 0)
            {
                listPoint[0].SetActive(false);
                unUsedPoint.Add(listPoint[0]);
                listPoint.RemoveAt(0);
            }
            resPos.Clear();

            StartWaitWrite();
        }
        if (GUI.Button(new Rect(600, Screen.height - 55, 180, 50), "Restart"))
        {
            Application.LoadLevel(0);
        }
        if (GUI.Button(new Rect(0, Screen.height - 55, 180, 50), "Next"))
        {
            ClearDashLine();
            StopMoveFinger();
            while (listPoint.Count > 0)
            {
                listPoint[0].SetActive(false);
                unUsedPoint.Add(listPoint[0]);
                listPoint.RemoveAt(0);
            }
            resPos.Clear();
            wordIdx = (wordIdx + 1) % wordName.Count;
            StopAllCoroutines();
            StartCoroutine(LoadXmlDatas());
        }
        GUI.color = Color.red;
        GUI.Label(new Rect(300, Screen.height - 100, Screen.width, 100), compareStr);
    }
    IEnumerator LoadXmlDatas()
    {
        strXml = Resources.Load(wordName[wordIdx]).ToString();
        xmlChar.LoadXml(strXml);
        drawWord.InitWord(xmlChar);
        while (!drawWord.isInited)
            yield return null;
        Debug.Log("all init!");
		drawWord.DrawBihua(true);
        StartWaitWrite();
    }
    void TranslateBin()
    {
        strXml = Resources.Load("38718").ToString();
        XmlNodeList xnlPoints = xmlChar.SelectNodes("character/points");
        List<short> outData = new List<short>();
        outData.Add((short)xnlPoints.Count);
        for (int k = 0; k < xnlPoints.Count; k++)
        {
            XmlNodeList xnl = xnlPoints[k].SelectNodes("point");
            outData.Add((short)xnl.Count);
            for (int i = 0; i < xnl.Count; i++)
            {
                XmlElement xe = (XmlElement)xnl[i];
                int x = XmlConvert.ToInt32(xe.Attributes["x"].Value);
                int y = XmlConvert.ToInt32(xe.Attributes["y"].Value);
                int pos = XmlConvert.ToInt32(xe.Attributes["pos"].Value);
                outData.Add((short)x);
                outData.Add((short)y);
                outData.Add((short)pos);
            }
        }
        if (outData.Count > 0)
            Helper.SaveFileData("38718.bytes", outData.ToArray(), true);
    }

    int writeState = 0;
    int nowBihuaIdx = 0;
    int fingerIdx = 0;
    bool blnFingerMove = false;
    int moveFingerIdx = 0;
    bool blnNext = false;
    float fingerMoveTime = 0;
    GameObject arrowObj;
    List<GameObject> dashPoint = new List<GameObject>();
    List<GameObject> unUseDashPoint = new List<GameObject>();
    List<Vector2> fingerPath = new List<Vector2>();
    List<bool> isThrough = new List<bool>();
    void StartWaitWrite()
    {
        nowBihuaIdx = -1;
        writeState = 1;
        NextDashLine();
        drawWord.SetBeginDrawMsg();
    }
    void NextDashLine()
    {
        ClearDashLine();
        fingerPath.Clear();
        isThrough.Clear();
        nowBihuaIdx++;
        blnNext = true;
        if (nowBihuaIdx < drawWord.arrBihua.Count)
        {
            DrawWord.Bihua bh = drawWord.arrBihua[nowBihuaIdx];
            fingerIdx = 0;
            if (bh.dashList != null)
            {
                float x = (bh.upPoint[bh.upPoint.Count - 1].x + bh.downPoint[bh.downPoint.Count - 1].x) / 2 + drawWord.Offx;
                float y = (bh.upPoint[bh.upPoint.Count - 1].y + bh.downPoint[bh.downPoint.Count - 1].y) / 2;
                for (int i = bh.dashList.Count < 3 ? 0 : 1; i < bh.dashList.Count - 1; i++)
                {
                    AddDashLine(new Vector2(bh.dashList[i].x, -bh.dashList[i].y), new Vector2(bh.dashList[i + 1].x, -bh.dashList[i + 1].y));
                    if (i == bh.dashList.Count - 2)
                        AddArrow(new Vector2(bh.dashList[i + 1].x, -bh.dashList[i + 1].y), new Vector2(x, -y));
                }
            }
            StartMoveFinger();
            //colorParent.gameObject.SetActive(true);
            UIButton[] arrButton = colorParent.GetComponentsInChildren<UIButton>();
            for (int b = 0; b < arrButton.Length; b++)
            {
                arrButton[b].isEnabled = true;
            }
        }
        else
        {
            writeState = 0;
        }
    }
    public enum THROUGH_TYPE
    {
        THROUGH_BEGAN,
        THROUGH_MOVED,
        THROUGH_ENDED
    }
    void CalculateThrough(Vector2 pos, THROUGH_TYPE type)
    {
        //Mathf.Max(0, fingerIdx - 1)
        Vector2 tmpP = Vector2.zero;
        for (int i = 0; i < fingerPath.Count; i++)
        {
            if (i < fingerIdx + 1)
            {
                tmpP.x = pos.x;
                tmpP.y = pos.y - Screen.height;
                float dis = Vector2.Distance(fingerPath[i], tmpP);
                float standard = 20 * DrawWord.Scale_Value;
                if (type == THROUGH_TYPE.THROUGH_ENDED)
                    standard = 10 * DrawWord.Scale_Value;
                if (dis <= standard)
                {
                    //AddPoint(pos.x, pos.y - Screen.height, lastDiameter, firstColor.GetComponent<UIButton>().defaultColor, false);
                    if (blnFingerMove)
                    {
                        StopMoveFinger();
                        //colorParent.gameObject.SetActive(false);
                        UIButton[] arrButton = colorParent.GetComponentsInChildren<UIButton>();
                        for (int b = 0; b < arrButton.Length; b++)
                        {
                            arrButton[b].isEnabled = false;
                        }
                    }
                    if (i == 0 || (i > 0 && isThrough[i - 1]))
                    {
                        if (!isThrough[i])
                        {
                            int tmpI = i * drawWord.arrBihua[nowBihuaIdx].orgList.Count / drawWord.arrBihua[nowBihuaIdx].dashList.Count;
                            drawWord.DrawingPecent(Mathf.Max(tmpI - 1, 0), firstColor.GetComponent<UIButton>().defaultColor);
                        }
                        isThrough[i] = true;
                        if (fingerIdx <= i)
                        {
                            fingerIdx = i + 1;
                        }
                    }
                }
            }
        }
        if (fingerIdx >= fingerPath.Count)
            drawWord.DrawingPecent(1000, firstColor.GetComponent<UIButton>().defaultColor);
    }
    void StartMoveFinger()
    {
        blnFingerMove = true;
        finger.SetActive(true);
        moveFingerIdx = 0;
        finger.transform.localPosition = fingerPath[moveFingerIdx];
        moveFingerIdx++;
        fingerMoveTime = Time.time;
        Eyelid el = finger.GetComponent<Eyelid>();
        if (el && arrowObj)
        {
            el.lookTarget = arrowObj.transform;
        }
    }
    void StopMoveFinger()
    {
        blnFingerMove = false;
        finger.SetActive(false);
    }
    void FingerMoving()
    {
        if (blnFingerMove && fingerMoveTime <= Time.time)
        {
            //finger.transform.localPosition = Vector3.Lerp(finger.transform.localPosition, fingerPath[moveFingerIdx], );
            finger.transform.localPosition = Vector3.MoveTowards(finger.transform.localPosition, fingerPath[moveFingerIdx], 2);
            UISprite us = finger.GetComponent<UISprite>();
            Color c = us.color;
            if (us)
            {
                if (fingerPath.Count >= 4)
                {
                    if (moveFingerIdx > fingerPath.Count / 2)
                        c.a = 1 - (float)(moveFingerIdx - fingerPath.Count / 2) / (fingerPath.Count / 2);
                    else
                        c.a = 1;
                }
                else
                    c.a = 1 - (float)moveFingerIdx / fingerPath.Count;
                us.color = c;
            }
            if (Helper.IsEqual(finger.transform.localPosition, fingerPath[moveFingerIdx]))
            {
                moveFingerIdx++;
                if (moveFingerIdx >= fingerPath.Count)
                {
                    c.a = 0;
                    us.color = c;
                    finger.transform.localPosition = fingerPath[0];
                    moveFingerIdx = 1;
                    fingerMoveTime = Time.time + 1.5f;
                }
            }
        }
    }
    void MoveFinger(Vector2 from, Vector2 to)
    {
        finger.transform.localPosition = to;
    }
    void AddArrow(Vector2 from, Vector2 to)
    {
        if (arrowObj==null)
            arrowObj = (GameObject)Instantiate(arrowPerfab);
        arrowObj.SetActive(true);
        arrowObj.transform.parent = finger.transform.parent;
        arrowObj.transform.localPosition = to;
        arrowObj.transform.localScale = Vector3.one; 
        //arrowObj.transform.localEulerAngles = new Vector3(0, 0, Helper.GetNGUIAngle(from, to));
        fingerPath.Add(from);
        isThrough.Add(false);
    }
    void ClearDashLine()
    {
        if (arrowObj)
            arrowObj.SetActive(false);
        while (dashPoint.Count > 0)
        {
            dashPoint[0].SetActive(false);
            unUseDashPoint.Add(dashPoint[0]);
            dashPoint.RemoveAt(0);
        }
    }
    void AddDashLine(Vector2 from, Vector2 to)
    {
        GameObject point = null;
        if (unUseDashPoint.Count > 0)
        {
            point = unUseDashPoint[0];
            unUseDashPoint.RemoveAt(0);
            point.SetActive(true);
        }
        else
            point = (GameObject)Instantiate(dashLinePrefab);
        point.transform.parent = pointParent;
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = from;
        //point.transform.localEulerAngles = new Vector3(0, 0, Helper.GetNGUIAngle(from, to));
        dashPoint.Add(point);
        fingerPath.Add(from);
        isThrough.Add(false);
    }
    void AddPoint(float x, float y, int diameter, Color col,bool org)
    {
        GameObject point = null;
        if (unUsedPoint.Count > 0)
        {
            point = unUsedPoint[0];
            unUsedPoint.RemoveAt(0);
            point.SetActive(true);
        }
        else
            point = (GameObject)Instantiate(pointPrefab);
        point.transform.parent = pointParent;
        point.transform.localScale = Vector3.one;
        point.transform.localPosition = new Vector3(x, y, 0);
        UISprite us = point.GetComponent<UISprite>();
        us.width = diameter;
        us.height = diameter;
        us.color = col;
        if (org)
            listOrgPoint.Add(point);
        else
            listPoint.Add(point);
    }
    /// <summary>
    ///   cp[0] start point
    ///   cp[1] control point1
    ///   cp[2] control point2
    ///   cp[3] end point
    ///   t time for 0~1
    /// </summary>
    Vector3 GetBezierPos(List<Vector3> cp, float t)
    {
        float ax, bx, cx;
        float ay, by, cy;
        float tSquared, tCubed;
        Vector3 result = Vector3.zero;
        cx = 3.0f * (cp[1].x - cp[0].x);
        bx = 3.0f * (cp[2].x - cp[1].x) - cx;
        ax = cp[3].x - cp[0].x - cx - bx;
        cy = 3.0f * (cp[1].y - cp[0].y);
        by = 3.0f * (cp[2].y - cp[1].y) - cy;
        ay = cp[3].y - cp[0].y - cy - by;
        tSquared = t * t;
        tCubed = tSquared * t;
        result.x = (ax * tCubed) + (bx * tSquared) + (cx * t) + cp[0].x;
        result.y = (ay * tCubed) + (by * tSquared) + (cy * t) + cp[0].y;
        return result;
    }
    void ComputeBezier(List<Vector3> cp, int numberOfPoints, List<Vector3> bezier)
    {
        float dt = 1.0f / (numberOfPoints - 1);
        for (int i = 0; i < numberOfPoints; i++)
        {
            bezier.Add(GetBezierPos(cp, dt * i));
        }
    }

    Mesh CreateMesh(List<Vector3> verts, Mesh mesh)
    {
		mesh.Clear();
        int numTiangle = (verts.Count - 2) * 3;
        //Vector3[] normals;
        Vector2[] newUV;
        //normals = new Vector3[verts.Count];
        newUV = new Vector2[verts.Count];
        int[] newTiangles = new int[numTiangle];
        mesh.vertices = verts.ToArray();
        ChangeToTriangle(newTiangles, verts);
        mesh.triangles = newTiangles;
        //for (int i = 0; i < verts.Count; i++)
        //{
        //    normals[i] = Vector3.up;
        //}
        //mesh.normals = normals;
        for (int i = 0; i < verts.Count; i++)
        {
            newUV[i] = new Vector2(verts[i].x * 50 / 256, verts[i].y * 50 / 256);
        }
        mesh.uv = newUV;
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        return mesh;
    }
    void ChangeToTriangle(int[] Triangles, List<Vector3> verts)
    {
        Node head = new Node();
        head.nodeCoordinate = verts[0];
        head.index = 0;
        Node p = head;
        int i = 0;
        for (i = 1; i < verts.Count; i++)
        {
            Node q = new Node();
            q.nodeCoordinate = verts[i];
            q.nodeVector = q.nodeCoordinate - p.nodeCoordinate;
            q.index = i;
            q.flag = 0;
            p.next = q;
            p = q;
        }
        p.next = head;
        head.nodeVector = head.nodeCoordinate - p.nodeCoordinate;
        head.flag = 0;

        Node work = head;
        Node s;
        int j = 0;
        do{
            s = work.next;
            if ((s.nodeVector.x * s.next.nodeVector.y - s.nodeVector.y * s.next.nodeVector.x) >= 0 || Isintersectant(work))
            {
                work = work.next;
                s.flag = 1;
            }
            else if ((s.nodeVector.x * s.next.nodeVector.y - s.nodeVector.y * s.next.nodeVector.x) < 0 && !Isintersectant(work))
            {
                Triangles[j] = work.index;
                Triangles[j + 1] = s.index;
                Triangles[j + 2] = s.next.index;
                work.next = s.next;
                work.next.nodeVector = work.next.nodeCoordinate - work.nodeCoordinate;
                i--;
                j += 3;
            }
        }
        while(i > 3 && work.next.flag != 1);
        Node k = work;
        while (k.next != work)
        {
            Triangles[j] = k.index;
            k = k.next;
            j++;
        }
        Triangles[j] = k.index;
    }
    bool Isintersectant(Node tp)
    {
        //检测是否为凸点
        Node s = tp.next.next.next;
        while (s.next != tp)
        {
            if (IsInTriggel(tp.nodeCoordinate, tp.next.nodeCoordinate, tp.next.next.nodeCoordinate, s.nodeCoordinate))
            {
                return true;
            }
            s = s.next;
        }
        return false;
    }
    bool IsInTriggel(Vector3 a, Vector3 b, Vector3 c, Vector3 m)
    {
        //检测m点是否在三角形内
        return IsSameSide(a, b, c, m) && IsSameSide(b, c, a, m) && IsSameSide(c, a, b, m);
    }
    bool IsSameSide(Vector3 a, Vector3 b, Vector3 c, Vector3 m)
    {
        //差乘检测m点是否在边内侧
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 am = m - a;
        Vector3 v1 = Vector3.Cross(ab, ac);
        Vector3 v2 = Vector3.Cross(ab, am);
        return Vector3.Dot(v1, v2) >= 0;
    }
    public class Node
    {
        public Vector3 nodeVector;
        public Vector3 nodeCoordinate;
        public int index;
        public int flag;
        public Node next;
    }

    #region Sample MessageBox
    public enum MB_INDEX
    {
        MB_NORMOL = 0,
        MB_SUREDELETE = 1
    }
    public enum MB_YESORNO
    {
        MB_YES = 1,
        MB_NO = 2
    }
    public void SetMessageBox(string showStr, int mb_yesorno, MB_INDEX idx = MB_INDEX.MB_NORMOL)
    {
        //Camera.current;
    }
    void SaveTransform(string key, Transform t)
    {
        string tmp = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}",
            t.position.x, t.position.y, t.position.z,
            t.eulerAngles.x, t.eulerAngles.y, t.eulerAngles.z,
            t.localScale.x, t.localScale.y, t.localScale.z);
        PlayerPrefs.SetString(key, tmp);
    }
    void ReadTransform(string key,Transform t)
    {
        if (PlayerPrefs.HasKey(key))
        {
            string[] value = PlayerPrefs.GetString(key).Split('_');
            for (int i = 0; i < 3; i++)
            {
                float x = 0, y = 0, z = 0;
                float.TryParse(value[i*3], out x);
                float.TryParse(value[i * 3 + 1], out y);
                float.TryParse(value[i * 3 + 2], out z);
                if (i == 0)
                    t.position = new Vector3(x, y, z);
                else if (i == 2)
                    t.eulerAngles = new Vector3(x, y, z);
                else
                    t.localScale = new Vector3(x, y, z);
            }
        }
        else
        {
            //默认的设置或者不处理
        }
    }
    #endregion
}
