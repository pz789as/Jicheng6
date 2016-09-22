using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

public class ZwController : MonoBehaviour {
    static private ZwController ins = null;
    static public ZwController Ins
    {
        get
        {
            if (ins == null)
            {
                ins = GameObject.FindObjectOfType<ZwController>();
                if (ins == null)
                {
                    GameObject g = new GameObject("ZwController");
                    ins = g.AddComponent<ZwController>();
                    CV.allStatus = CV.Status_Cs;
                    ins.csIdx = 0;
                }
            }
            return ins;
        }
    }
    void Awake()
    {
        if (Ins == null)
        {
            Debug.Log("Awake error!");
            Application.Quit();
        }
    }

    int csIdx = 0;
    void Update()
    {
        if (CV.allStatus == CV.Status_Cs)
        {
            if (InitControl() == 3)
                ChangeStatus(CV.Status_Xz);
        }

        if (Time.frameCount % 50 == 0)
        {
            System.GC.Collect();
        }
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Home) || Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    string strTemp = "";
    string[] bezierDataStr = null;
    string[] strArr0, strArr1, strArr2;
    Vector3 tempVec = Vector3.zero;
    [HideInInspector]
    public Dictionary<int, List<Vector3>> listGestrue = new Dictionary<int, List<Vector3>>();
    List<Vector3> listPoint;
    Dictionary<string, GameObject> prefabList = null;
    List<WordInfo> arrDataInfo = new List<WordInfo>();
    [HideInInspector]
    public Transform uiParent = null;
    [HideInInspector]
    public ZwTouch zwTouch = null;
    Transform selHanziUI = null;
    Transform writeHanziUI = null;
    Transform showBujianUI = null;
    Transform spriteCenter;
    Vector3 scOrgPosition = new Vector3(-3, 3, 0);
    List<GameObject> allWordButton = new List<GameObject>();
    int selWord = 0;
    ZwWords showWord = null;
    float cValue = 0;
    public bool isOrder = true;
    int errorWriteCount = 0;
    int singleErrorCount = 0;
    static public int lastErrorCount = 0;
    static public int lastBihuaCount = 0;
    bool blnComplate;
    int selBujian = 0;
    string selBujianStr = "";
    List<WordInfo> lyi, lyin, lji;
    List<GameObject> listBjhz;
    int InitControl()
    {
        switch (csIdx)
        {
            case 0:
                {
                    prefabList = new Dictionary<string, GameObject>();
                    GetPrefab("Wkp");
                    GetPrefab("BasePoint");
                    GetPrefab("FingerPress");
                    GetPrefab("WordCards");
                    InitGesture();
                    csIdx++;
                    break;
                }
            case 1:
                {
                    InitWordInfo();
                    if (!uiParent)
                        uiParent = GameObject.FindGameObjectWithTag("UIRoot").transform;
                    selHanziUI = uiParent.FindChild("SelectHanzi");
                    writeHanziUI = uiParent.FindChild("WriteHanzi");
                    showBujianUI = uiParent.FindChild("ShowBujian");
                    if (!spriteCenter)
                        spriteCenter = GameObject.FindGameObjectWithTag("SpriteCenter").transform;
                    csIdx++;
                    break;
                }
            case 2:
                {
                    lyi = new List<WordInfo>();
                    lyin = new List<WordInfo>();
                    lji = new List<WordInfo>();
                    listBjhz = new List<GameObject>();
                    csIdx++;
                    break;
                }
        }
        return csIdx;
    }
    public GameObject GetPrefab(string key)
    {
        if (!prefabList.ContainsKey(key))
            prefabList.Add(key, Resources.Load<GameObject>("Prefabs/" + key));
        return prefabList[key];
    }
    void InitGesture()
    {
        strTemp = Resources.Load<TextAsset>("Gesture").text;
        try
        {
            if (string.IsNullOrEmpty(strTemp))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    Debug.LogError("gesture data is error!");
                return;
            }
            bezierDataStr = strTemp.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < bezierDataStr.Length; i++)
            {
                strTemp = bezierDataStr[i];
                strArr0 = strTemp.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                listPoint = new List<Vector3>();
                if (strArr0.Length > 0)
                {
                    strArr1 = strArr0[0].Split('_');
                    for (int p = 0; p < strArr1.Length; p++)
                    {
                        strArr2 = strArr1[p].Split(',');
                        tempVec.x = System.Convert.ToSingle(strArr2[0]);
                        tempVec.y = System.Convert.ToSingle(strArr2[1]);
                        tempVec.z = System.Convert.ToSingle(strArr2[2]);
                        listPoint.Add(tempVec);
                    }
                }
                listGestrue.Add(i, listPoint);
            }
        }
        catch (System.Exception e)
        {
            print(e.Message);
        }
    }
    void InitWordInfo()
    {
        strTemp = Resources.Load<TextAsset>("DataInfo").text;
        strArr0 = strTemp.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        strTemp = Resources.Load<TextAsset>("DrawInfo").text;
        strArr1 = strTemp.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        strTemp = Resources.Load<TextAsset>("OrderInfo").text;
        strArr2 = strTemp.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < strArr0.Length; i++)
        {
            WordInfo wi = new WordInfo(strArr0[i]);
            wi.LoadDrawInfo(strArr1[i]);
            wi.SetOrderInfo(strArr2[i]);
            arrDataInfo.Add(wi);
        }
    }
    public void ChangeStatus(int newStatus)
    {
        CV.allStatus = newStatus;
        if (newStatus == CV.Status_Xz)
        {
            InitXZ();
        }
        else if (newStatus == CV.Status_Xxz)
        {
            InitXXZ();
        }
        else if (newStatus == CV.Status_Xz_Over)
        {

        }
        else if (newStatus == CV.Status_Bjck)
        {
            InitBJCK();
        }
    }
    void InitXZ()
    {
        GameObject tp = GetPrefab("Wkp");
        selHanziUI.gameObject.SetActive(true);
        //float w = tp.GetComponent<RectTransform>().rect.width + 10;
        //float h = tp.GetComponent<RectTransform>().rect.height + 10;
        //float sx = -(w * 6 - 10) / 2 + (w - 10) / 2;
        //float sy = 300;
        //for (int i = 0; i < arrDataInfo.Count; i++)
        //{
        //    GameObject np = (GameObject)Instantiate(tp);
        //    np.name = "" + i;
        //    RectTransform rt = np.GetComponent<RectTransform>();
        //    rt.SetParent(selHanziUI);
        //    rt.localScale = Vector3.one;
        //    rt.localPosition = new Vector3(sx + (i % 6) * w, sy - (i / 6) * h, 0);
        //    Text text = np.transform.FindChild("Thz").gameObject.GetComponent<Text>();
        //    text.text = arrDataInfo[i].strHZ;
        //    ZwCallBack cb = np.AddComponent<ZwCallBack>();
        //    np.GetComponent<Button>().onClick.AddListener(cb.ZwOnClick);
        //    cb.onClick = SelectHzCallBack;
        //    allWordButton.Add(np);
        //}

        Camera.main.backgroundColor = new Color(222f / 255, 220f / 255, 198f / 255);
        spriteCenter.gameObject.SetActive(false);
        selHanziUI.FindChild("StartBtn").GetComponent<Button>().onClick.AddListener(this.GotoXXZ);
        //if (showWord == null)
        //{
        //    GameObject wordObj = new GameObject("word");
        //    showWord = wordObj.AddComponent<ZwWords>();
        //    showWord.tag = ZwWords.WordTagName;
        //}
        //showWord.transform.localPosition = Vector3.zero;
        //showWord.transform.localScale = Vector3.one;
        //showWord.transform.eulerAngles = Vector3.zero;
        //showWord.Init(arrDataInfo[selWord]);
        //showWord.SetAllShowImm(true);
    }
    public void GotoXXZ()
    {
        selWord = 0;
        ChangeStatus(CV.Status_Xxz);
        ClearWordMessage();
    }
    public void BackToMenu()
    {
        writeHanziUI.gameObject.SetActive(false);
        selHanziUI.gameObject.SetActive(true);
        CV.allStatus = CV.Status_Xz;
    }
    void InitXXZ()
    {
        if (spriteCenter.gameObject.activeSelf == false)
            spriteCenter.gameObject.SetActive(true);
        Camera.main.backgroundColor = new Color(49f / 255, 77f / 255, 121f / 255);
        showBujianUI.gameObject.SetActive(false);
        selHanziUI.gameObject.SetActive(false);
        writeHanziUI.gameObject.SetActive(true);
        spriteCenter.position = scOrgPosition;
        spriteCenter.localScale = Vector3.one;

        Button btn = writeHanziUI.FindChild("Restart").gameObject.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(new UnityAction(ReWriteHanzi));
        btn = writeHanziUI.FindChild("Next").gameObject.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(new UnityAction(NextWord));
        btn = writeHanziUI.FindChild("Last").gameObject.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(new UnityAction(LastWord));

        InitShowWord();
        //showWord.SetAllShowImm(true);
    }
    void InitShowWord()
    {
        if (showWord == null)
        {
            GameObject wordObj = new GameObject("word");
            showWord = wordObj.AddComponent<ZwWords>();
            showWord.tag = ZwWords.WordTagName;
        }
        showWord.transform.parent = spriteCenter;
        showWord.transform.localPosition = Vector3.zero;
        showWord.transform.localScale = Vector3.one;
        showWord.transform.eulerAngles = Vector3.zero;
        showWord.Init(arrDataInfo[selWord]);

        Transform uiLeft = writeHanziUI.FindChild("Left");
        uiLeft.FindChild("HSKLevel").gameObject.GetComponent<Text>().text = arrDataInfo[selWord].strHSK;
        uiLeft.FindChild("GJJYLevel").gameObject.GetComponent<Text>().text = arrDataInfo[selWord].strGJ;
        uiLeft.FindChild("CihuiTitle").FindChild("CihuiText").gameObject.GetComponent<Text>().text = arrDataInfo[selWord].strEnglish;

        Transform uiTop = writeHanziUI.FindChild("Top");
        uiTop.FindChild("Pinyin").GetComponent<Text>().text = arrDataInfo[selWord].strPinyin;
    }
    void ReWriteHanzi()
    {
        ResetWord();
        CV.allStatus = CV.Status_Xxz;
    }
    void LastWord()
    {
        ResetWord();
        selWord = (selWord + arrDataInfo.Count - 1) % arrDataInfo.Count;
        InitShowWord();
        ClearWordMessage();
        CV.allStatus = CV.Status_Xxz;
    }
    void NextWord()
    {
        ResetWord();
        selWord = (selWord + 1) % arrDataInfo.Count;
        InitShowWord();
        ClearWordMessage();
        CV.allStatus = CV.Status_Xxz;
    }
    void SelectHzCallBack(GameObject obj)
    {
        selWord = System.Convert.ToInt32(obj.name);
        ChangeStatus(CV.Status_Xxz);
        ClearWordMessage();
    }
    public void CompareBihua(List<Vector3> listOrgPoint)
    {
        if (!CmpBihua(listOrgPoint))
        {
            singleErrorCount++;
            if (singleErrorCount >= ZwValue.Ins.singleErrorMax)
            {
                showWord.SetWarningTips();
            }
        }
        else
        {
            if (singleErrorCount > 0)
            {
                errorWriteCount += singleErrorCount;
                singleErrorCount = 0;
            }
            if (CV.allStatus == CV.Status_Xz_Over)
            {
                SetWordMessage();
                //print("complate!!");
            }
        }
    }
    bool CmpBihua(List<Vector3> listOrgPoint)
    {
        bool blnRet = false;
        if (listOrgPoint.Count > 2)
        {
            int showNum = 0;
            for (int i = 0; i < arrDataInfo[selWord].arrBihua.Count; i++)
            {
                if (arrDataInfo[selWord].arrBihua[i].isShow)
                {
                    showNum++;
                    continue;
                }
                else
                {
                    cValue = CompareLine.CompareGesture(listOrgPoint, listGestrue[arrDataInfo[selWord].arrOrder[i]]);
                    //print("compare value: " + cValue);
                    if (cValue <= 4)
                    {
                        showNum++;
                        showWord.SetAnimation(i, listOrgPoint[0], listOrgPoint[listOrgPoint.Count - 1], arrDataInfo[selWord].arrBihua[i]);
                        if (showWord.IsWriteBack(i, listOrgPoint[0], listOrgPoint[listOrgPoint.Count - 1], arrDataInfo[selWord].arrBihua[i]))
                        {
                            print("is write back!!");
                            //if (backtips)
                            //{
                            //    backtips.transform.position = word.GetRightPos(i);
                            //    backtips.SetActive(true);
                            //}
                        }
                        arrDataInfo[selWord].arrBihua[i].isShow = true;
                        blnRet = true;
                        break;
                    }
                    else
                    {
                        if (isOrder)
                        {
                            blnRet = false;
                            break;
                        }
                    }
                }
            }
            if (showNum == arrDataInfo[selWord].arrBihua.Count)
            {
                lastErrorCount = errorWriteCount;
                ChangeStatus(CV.Status_Xz_Over);
            }
        }
        return blnRet;
    }
    public void ResetWord()
    {
        showWord.ReInit();
        arrDataInfo[selWord].ResetShow();
        lastErrorCount = errorWriteCount;
        errorWriteCount = 0;
        singleErrorCount = 0;
    }
    void ClearWordMessage()
    {
        Transform uiRight = writeHanziUI.FindChild("Right");
        Transform parent = uiRight.FindChild("YiParent");
        for (int i = 0; i < parent.childCount; i++)
            Destroy(parent.GetChild(i).gameObject);
        parent = uiRight.FindChild("YinParent");
        for (int i = 0; i < parent.childCount; i++)
            Destroy(parent.GetChild(i).gameObject);
        parent = uiRight.FindChild("JiParent");
        for (int i = 0; i < parent.childCount; i++)
            Destroy(parent.GetChild(i).gameObject);
    }
    void SetWordMessage()
    {
        WordInfo wi = arrDataInfo[selWord];
        Transform uiRight = writeHanziUI.FindChild("Right");
        if (wi.iBJSL > 0)
        {
            int yiCount = 0;
            int yinCount = 0;
            int jiCount = 0;
            for (int i = 0; i < wi.iBJSL; i++)
            {
                bool bln = false;
                if (wi.arrBJYI[i] != 0)
                {
                    CreateWordCards(yiCount, uiRight.FindChild("YiParent"), System.Convert.ToInt32(wi.arrBJUinc[i], 16), i);
                    yiCount++;
                    bln = true;
                }
                if (wi.arrBJYIN[i] != 0)
                {
                    CreateWordCards(yinCount, uiRight.FindChild("YinParent"), System.Convert.ToInt32(wi.arrBJUinc[i], 16), i);
                    yinCount++;
                    bln = true;
                }
                if (!bln)
                {
                    CreateWordCards(jiCount, uiRight.FindChild("JiParent"), System.Convert.ToInt32(wi.arrBJUinc[i], 16), i);
                    jiCount++;
                }
            }
        }
    }
    GameObject CreateWordCards(int count, Transform parent, int unic, int nameIdx)
    {
        GameObject tempObj = (GameObject)Instantiate(GetPrefab("WordCards"));
        tempObj.name = nameIdx.ToString();
        tempObj.transform.SetParent(parent);
        float w = tempObj.GetComponent<RectTransform>().sizeDelta.x;
        float h = tempObj.GetComponent<RectTransform>().sizeDelta.y;
        if (parent.name == "JiParent")
        {
            tempObj.transform.localPosition = new Vector3((count % 3) * w, -(count / 3) * h, 0);
        }
        else
        {
            tempObj.transform.localPosition = new Vector3(count * w, 0, 0);
        }
        tempObj.transform.localScale = Vector3.one;
        GameObject text = tempObj.transform.FindChild("Highlight0").gameObject;
        text.SetActive(true);
        text.GetComponent<Text>().text = string.Format("{0}", (char)unic);
        ZwCallBack cb = tempObj.AddComponent<ZwCallBack>();
        tempObj.GetComponent<Button>().onClick.RemoveAllListeners();
        tempObj.GetComponent<Button>().onClick.AddListener(new UnityAction(cb.ZwOnClick));
        cb.onClick = ClickBujian;
        return tempObj;
    }
    public void ClickWord(GameObject click, string strIdx, Vector3 mPos)
    {
        if (click != null)
        {
            print(click.name + "  " + click.tag);
            //ZwWords word = click.GetComponent<ZwWords>();
        }
    }
    void ClickBujian(GameObject obj)
    {
        selBujian = System.Convert.ToInt32(obj.name);
        selBujianStr = arrDataInfo[selWord].arrBJ[selBujian];
        print(obj.name + " " + selBujianStr);
        ChangeStatus(CV.Status_Bjck);
    }
    void InitBJCK()
    {
        writeHanziUI.gameObject.SetActive(false);
        showBujianUI.gameObject.SetActive(true);
        ResetWord();
        showWord.SetBujianShow(selBujian);
        spriteCenter.position = new Vector3(-5.5f, scOrgPosition.y / 2, 0);
        spriteCenter.localScale = Vector3.one * 0.5f;

        GetYiYinJiArr(selBujianStr);

        for (int i = 0; i < listBjhz.Count; i++)
        {
            for (int m = 0; m < listBjhz[i].transform.childCount; m++)
            {
                listBjhz[i].transform.GetChild(m).gameObject.SetActive(false);
            }
            listBjhz[i].SetActive(false);
        }

        float w = GetPrefab("WordCards").GetComponent<RectTransform>().rect.width;
        float h = GetPrefab("WordCards").GetComponent<RectTransform>().rect.height;
        float sx = 10 + w /2;
        float sy = -50 - h / 2;
        for (int i = 0; i < lyi.Count; i++)
        {
            CreateBjckCard(sx, sy, showBujianUI, lyi[i], i, 0);
        }
        sy -= (lyi.Count / 6 + 1) * (h + 5);
        showBujianUI.FindChild("YinTitle").localPosition = new Vector3(10, sy + h / 2 - 5, 0);
        sy -= showBujianUI.FindChild("YinTitle").GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < lyin.Count; i++)
        {
            CreateBjckCard(sx, sy, showBujianUI, lyin[i], i, 1);
        }
        sy -= (lyi.Count / 6 + 1) * (h + 5);
        showBujianUI.FindChild("JiTitle").localPosition = new Vector3(10, sy + h / 2 - 5, 0);
        sy -= showBujianUI.FindChild("JiTitle").GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < lji.Count; i++)
        {
            CreateBjckCard(sx, sy, showBujianUI, lji[i], i, 2);
        }
    }
    void GetYiYinJiArr(string bjStr)
    {
        lyi.Clear();
        lyin.Clear();
        lji.Clear();
        for (int i = 0; i < arrDataInfo.Count; i++)
        {
            for (int b = 0; b < arrDataInfo[i].iBJSL; b++)
            {
                if (arrDataInfo[i].arrBJ[b] == bjStr)
                {
                    bool bln = false;
                    if (arrDataInfo[i].arrBJYI[b] > 0)
                    {
                        if (!lyi.Contains(arrDataInfo[i]))
                            lyi.Add(arrDataInfo[i]);
                        bln = true;
                    }
                    if (arrDataInfo[i].arrBJYIN[b] > 0)
                    {
                        if (!lyin.Contains(arrDataInfo[i]))
                            lyin.Add(arrDataInfo[i]);
                        bln = true;
                    }
                    if (!bln)
                    {
                        if (!lji.Contains(arrDataInfo[i]))
                            lji.Add(arrDataInfo[i]);
                    }
                }
            }
        }
    }
    void CreateBjckCard(float sx, float sy, Transform parent, WordInfo info, int idx, int kind)
    {
        GameObject tempObj = null;
        for (int i = 0; i < listBjhz.Count; i++)
        {
            if (listBjhz[i].activeSelf == false)
            {
                tempObj = listBjhz[i];
                tempObj.SetActive(true);
                break;
            }
        }
        if (tempObj == null)
        {
            tempObj = (GameObject)Instantiate(GetPrefab("WordCards"));
            listBjhz.Add(tempObj);
        }
        
        float w = tempObj.GetComponent<RectTransform>().rect.width + 5;
        float h = tempObj.GetComponent<RectTransform>().rect.height + 5;

        tempObj.name = arrDataInfo.IndexOf(info).ToString();
        tempObj.transform.SetParent(parent);
        tempObj.transform.localPosition = new Vector3(sx + (idx % 6) * w, sy - (idx / 6) * h, 0);
        tempObj.transform.localScale = Vector3.one;

        GameObject text = tempObj.transform.FindChild("BaseText").gameObject;
        text.SetActive(true);
        text.GetComponent<Text>().text = info.strHZ;

        int count = 0;
        for (int i = 0; i < info.iBJSL; i++)
        {
            if (info.arrBJ[i] == selBujianStr)
            {
                if (kind == 0)
                {
                    if (info.arrBJYI[i] > 0)
                    {
                        text = tempObj.transform.FindChild("Highlight" + count).gameObject;
                        text.SetActive(true);
                        int unic = System.Convert.ToInt32(info.arrBJUinc[i], 16);
                        text.GetComponent<Text>().text = string.Format("{0}", (char)unic);
                        text.GetComponent<Text>().color = Color.yellow;
                        count++;
                        continue;
                    }
                }
                else if (kind == 1)
                {
                    if (info.arrBJYIN[i] > 0)
                    {
                        text = tempObj.transform.FindChild("Highlight" + count).gameObject;
                        text.SetActive(true);
                        int unic = System.Convert.ToInt32(info.arrBJUinc[i], 16);
                        text.GetComponent<Text>().text = string.Format("{0}", (char)unic);
                        text.GetComponent<Text>().color = Color.yellow;
                        count++;
                        continue;
                    }
                }
                else
                {
                    if (info.arrBJYIN[i] <=0 && info.arrBJYI[i] <=0)
                    {
                        text = tempObj.transform.FindChild("Highlight" + count).gameObject;
                        text.SetActive(true);
                        int unic = System.Convert.ToInt32(info.arrBJUinc[i], 16);
                        text.GetComponent<Text>().text = string.Format("{0}", (char)unic);
                        text.GetComponent<Text>().color = Color.yellow;
                        count++;
                        continue;
                    }
                }
            }
        }

        ZwCallBack cb = tempObj.AddComponent<ZwCallBack>();
        tempObj.GetComponent<Button>().onClick.RemoveAllListeners();
        tempObj.GetComponent<Button>().onClick.AddListener(new UnityAction(cb.ZwOnClick));
        cb.onClick = SelectHzCallBack;
    }
    public void BackToWzz()
    {
        ChangeStatus(CV.Status_Xxz);
    }
}

public class WordInfo
{
    public string strHZ = "";   //汉字字符
    public string strUnic = "";   //unicode的码
    public string strPinyin = ""; //拼音
    public string strHSK = "";  //hsk等级
    public string strGJ = ""; //国际教育等级
    public int iCC = 0; //层次类型
    public int iBJSL = 0;   //部件数量
    public string[] arrBJ = new string[5];  //部件字符
    public string[] arrBJUinc = new string[5];    //部件字体中所用unicode的码
    public int[] arrBJYI = new int[5];  //部件表义
    public int[] arrBJYIN = new int[5]; //部件表音
    public string strEnglish = "";
    string drawInfo = "";
    public List<int> arrOrder = new List<int>();
    public List<Bihua> arrBihua = new List<Bihua>();
    public List<Vector3> arrBSCenter = new List<Vector3>();

    public WordInfo()
    {

    }
    public WordInfo(string data)
    {
        Init(data);
    }
    public string GetBjUnic(string bjStr)
    {
        for (int i = 0; i < iBJSL; i++)
        {
            if (arrBJ[i] == bjStr)
                return arrBJUinc[i];
        }
        return "";
    }
    public void Init(string data)
    {
        string[] arr = data.Split('\t');
        strHZ = arr[0];
        strUnic = arr[1];
        strPinyin = arr[2];
        strHSK = arr[3];
        strGJ = arr[4];
        strEnglish = arr[5].Replace(';', '\n');
        iCC = System.Convert.ToInt32(arr[6]);
        for (int i = 7; i < arr.Length;)
        {
            if (!string.IsNullOrEmpty(arr[i]))
            {
                arrBJ[iBJSL] = arr[i];
                arrBJUinc[iBJSL] = arr[i + 1];
                arrBJYI[iBJSL] = System.Convert.ToInt32(arr[i + 2]);
                arrBJYIN[iBJSL] = System.Convert.ToInt32(arr[i + 3]);
                iBJSL++;
                i += 4;
            }
            else
                break;
        }
    }
    public void SetOrderInfo(string str)
    {
        string[] arr = str.Split(new char[] { '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < arr.Length; i++)
        {
            arrOrder.Add(System.Convert.ToInt32(arr[i]));
        }
    }
    public void LoadDrawInfo(string str)
    {
        drawInfo = str;
        arrBihua.Clear();
        arrBSCenter.Clear();
        string[] arr0 = drawInfo.Split(new char[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
        string[] arr1, arr2;
        Bihua tempBihua;
        Vector3 tempVec;
        for (int i = 0; i < arr0.Length; i++)
        {
            if (i < arr0.Length - 1)
            {
                arr1 = arr0[i].Split(new char[] { '_' }, System.StringSplitOptions.RemoveEmptyEntries);
                arr2 = arr1[0].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                tempBihua = new Bihua();
                tempBihua.center.x = System.Convert.ToSingle(arr2[0]);
                tempBihua.center.y = System.Convert.ToSingle(arr2[1]);
                tempBihua.center.z = 0;
                tempVec = Vector3.zero;
                tempVec.x = System.Convert.ToSingle(arr2[2]);
                tempVec.y = System.Convert.ToSingle(arr2[3]);
                tempVec.z = 0;
                tempBihua.orgList.Add(tempVec);
                tempVec = Vector3.zero;
                tempVec.x = System.Convert.ToSingle(arr2[4]);
                tempVec.y = System.Convert.ToSingle(arr2[5]);
                tempVec.z = 0;
                tempBihua.orgList.Add(tempVec);
                tempBihua.isShow = false;
                tempBihua.bushou = System.Convert.ToInt32(arr2[6]);
                if (arr2.Length > 7)
                    tempBihua.picName = arr2[7] + "/" + i.ToString();

                arr2 = arr1[1].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < arr2.Length / 2; j++)
                {
                    tempVec = Vector3.zero;
                    tempVec.x = System.Convert.ToSingle(arr2[j * 2 + 0]);
                    tempVec.y = System.Convert.ToSingle(arr2[j * 2 + 1]);
                    tempBihua.basePoint.Add(tempVec);
                }
                arrBihua.Add(tempBihua);
            }
            else
            {
                arr1 = arr0[i].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < arr1.Length / 2; j++)
                {
                    tempVec = Vector3.zero;
                    tempVec.x = System.Convert.ToSingle(arr1[j * 2 + 0]);
                    tempVec.y = System.Convert.ToSingle(arr1[j * 2 + 1]);
                    tempVec.z = 0;
                    arrBSCenter.Add(tempVec);
                }
            }
        }
    }
    public void ResetShow()
    {
        for (int i = 0; i < arrBihua.Count; i++)
        {
            arrBihua[i].isShow = false;
        }
    }
}

public class Bihua
{
    public List<Vector3> orgList;
    public List<Vector2> basePoint;
    public bool isShow = false;
    public Vector3 center = Vector3.zero;
    public int bushou = 0;
    public string picName = "";
    public Bihua()
    {
        orgList = new List<Vector3>();
        basePoint = new List<Vector2>();
        isShow = false;
        bushou = 0;
    }
    public Vector3 TrnsCenter()
    {
        return center / 100;
    }
    public Vector2[] TrnsBasePoint()
    {
        Vector2[] arr = basePoint.ToArray();
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] /= 100;
        }
        return arr;
    }
}
