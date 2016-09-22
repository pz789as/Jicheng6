using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZwWords : MonoBehaviour {
    [HideInInspector]
    public WordInfo myInfo = null;
    GameObject[] arrSprBh;
    GameObject[] tempSprBh;
    GameObject[] arrBushou;
    bool[] isMove;
    float[] waitMoveTime;
    Vector3[] arrTempPos;
    Vector3[] arrTempScale;
    float[] arrTempAngle;
    int bihuaCount = 0;
    Color orgColor = Color.black;
    static public string WordLayerName = "WordLayerName";
    static public string WordTagName = "WordTagName";
    static public string WordTagBushou = "WordTagBushou";
    static public string WordTagBihua = "WordTagBihua";
    Vector3 tempVec;
    Color warningColor = new Color(0, 107f / 255, 1);
    int warningIdx = 0;
    bool blnWarning = false;
    float warningTime = 0;

	void Start () {
	    
	}

    public void Init(WordInfo info)
    {
        myInfo = info;
        bihuaCount = info.arrBihua.Count;
        if (arrSprBh != null)
        {
            if (arrSprBh.Length < bihuaCount)
            {
                GameObject[] tList = arrSprBh;
                arrSprBh = new GameObject[bihuaCount];
                tList.CopyTo(arrSprBh, 0);
                tList = tempSprBh;
                tempSprBh = new GameObject[bihuaCount];
                tList.CopyTo(tempSprBh, 0);
                bool[] arrM = isMove;
                isMove = new bool[bihuaCount];
                arrM.CopyTo(isMove, 0);
                float[] arrW = waitMoveTime;
                waitMoveTime = new float[bihuaCount];
                arrW.CopyTo(waitMoveTime, 0);
                Vector3[] arrP = arrTempPos;
                arrTempPos = new Vector3[bihuaCount];
                arrP.CopyTo(arrTempPos, 0);
                arrP = arrTempScale;
                arrTempScale = new Vector3[bihuaCount];
                arrP.CopyTo(arrTempScale, 0);
                arrW = arrTempAngle;
                arrTempAngle = new float[bihuaCount];
                arrW.CopyTo(arrTempAngle, 0);
            }
            else if (arrSprBh.Length > bihuaCount)
            {
                for (int i = bihuaCount; i < arrSprBh.Length; i++)
                {
                    arrSprBh[i].transform.parent = this.transform;
                    arrSprBh[i].SetActive(false);
                    if (tempSprBh[i])
                        tempSprBh[i].SetActive(false);
                }
            }
        }
        else
        {
            arrSprBh = new GameObject[bihuaCount];
            tempSprBh = new GameObject[bihuaCount];
            isMove = new bool[bihuaCount];
            waitMoveTime = new float[bihuaCount];
            arrTempPos = new Vector3[bihuaCount];
            arrTempScale = new Vector3[bihuaCount];
            arrTempAngle = new float[bihuaCount];
        }
        SpriteRenderer sp = null;
        for (int i = 0; i < bihuaCount; i++)
        {
            if (arrSprBh[i] == null)
            {
                arrSprBh[i] = new GameObject(i.ToString());
                arrSprBh[i].AddComponent<SpriteRenderer>();
            }
            arrSprBh[i].transform.parent = this.transform;
            arrSprBh[i].transform.localPosition = myInfo.arrBihua[i].TrnsCenter();
            arrSprBh[i].transform.localEulerAngles = Vector3.zero;
            arrSprBh[i].transform.localScale = Vector3.one;
            sp = arrSprBh[i].GetComponent<SpriteRenderer>();
            string path = CV.pngPath + myInfo.arrBihua[i].picName;
            sp.sprite = Resources.Load<Sprite>(path);
            if (sp.sprite == null)
                print(path);
            sp.color = orgColor;
            sp.sortingLayerName = "BihuaLayer";
            sp.sortingOrder = 0;
            PolygonCollider2D pc = arrSprBh[i].GetComponent<PolygonCollider2D>();
            if (pc == null)
                pc = arrSprBh[i].AddComponent<PolygonCollider2D>();
            pc.SetPath(0, myInfo.arrBihua[i].TrnsBasePoint());
            arrSprBh[i].layer = LayerMask.NameToLayer(WordLayerName);
            arrSprBh[i].tag = WordTagBihua;
            arrSprBh[i].SetActive(false);
            isMove[i] = false;
            waitMoveTime[i] = 0;
            arrTempPos[i] = Vector3.zero;
            arrTempScale[i] = Vector3.one;
            arrTempAngle[i] = 0;
        }

        if (arrBushou == null)
        {
            arrBushou = new GameObject[myInfo.iBJSL];
        }
        else
        {
            if (arrBushou.Length < myInfo.iBJSL)
            {
                GameObject[] tList = arrBushou;
                arrBushou = new GameObject[myInfo.iBJSL];
                tList.CopyTo(arrBushou, 0);
            }
            else if (arrBushou.Length > myInfo.iBJSL)
            {
                for (int i = myInfo.iBJSL; i < arrBushou.Length; i++)
                {
                    arrBushou[i].SetActive(false);
                }
            }
        }
        //print(myInfo.iBJSL);
        for (int i = 0; i < myInfo.iBJSL; i++)
        {
            if (arrBushou[i] == null)
            {
                arrBushou[i] = new GameObject(i.ToString());
            }
            arrBushou[i].transform.parent = this.transform;
            arrBushou[i].transform.localPosition = myInfo.arrBSCenter[i] / 100;
            arrBushou[i].transform.localEulerAngles = Vector3.zero;
            arrBushou[i].transform.localScale = Vector3.one;
            arrBushou[i].tag = WordTagBushou;
            arrBushou[i].SetActive(true);
            for (int k = 0; k < bihuaCount; k++)
            {
                if (myInfo.arrBihua[k].bushou == i)
                {
                    arrSprBh[k].transform.parent = arrBushou[i].transform;
                }
            }
        }
    }
    public void ReInit()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        for (int i = 0; i < bihuaCount; i++)
        {
            if (arrSprBh[i])
            {
                arrSprBh[i].GetComponent<SpriteRenderer>().color = orgColor;
                arrSprBh[i].SetActive(false);
            }
            if (tempSprBh[i])
            {
                tempSprBh[i].SetActive(false);
            }
            isMove[i] = false;
            waitMoveTime[i] = 0;
            arrTempPos[i] = Vector3.zero;
            arrTempScale[i] = Vector3.one;
            arrTempAngle[i] = 0;
        }
        blnWarning = false;
        warningIdx = 0;
    }
    public void SetAllShowImm(bool blnVisb)
    {
        for (int i = 0; i < bihuaCount; i++)
        {
            if (arrSprBh[i])
            {
                arrSprBh[i].GetComponent<SpriteRenderer>().color = orgColor;
                arrSprBh[i].SetActive(blnVisb);
            }
        }
    }
    public void SetAnimation(int idx, Vector3 start, Vector3 end, Bihua bh)
    {
        start = Camera.main.ScreenToWorldPoint(start);
        start.z = 0;
        end = Camera.main.ScreenToWorldPoint(end);
        end.z = 0;
        if (tempSprBh[idx] == null)
        {
            tempSprBh[idx] = (GameObject)Instantiate(arrSprBh[idx]);
            tempSprBh[idx].transform.parent = this.transform.parent;
            tempSprBh[idx].layer = LayerMask.NameToLayer("Default");
            Destroy(tempSprBh[idx].GetComponent<PolygonCollider2D>());
        }
        tempSprBh[idx].SetActive(true);
        SpriteRenderer sr = tempSprBh[idx].GetComponent<SpriteRenderer>();
        sr.sprite = arrSprBh[idx].GetComponent<SpriteRenderer>().sprite;
        sr.color = orgColor;
        tempVec = Vector3.one;
        tempVec.x = Mathf.Max(Mathf.Abs(start.x - end.x), 0.3f, sr.sprite.bounds.size.x);
        tempVec.x = tempVec.x / sr.sprite.bounds.size.x;
        tempVec.y = Mathf.Max(Mathf.Abs(start.y - end.y), 0.3f, sr.sprite.bounds.size.y);
        tempVec.y = tempVec.y / sr.sprite.bounds.size.y;
        tempVec.z = 1;
        tempSprBh[idx].transform.localScale = tempVec;
        tempSprBh[idx].transform.position = (start + end) * 0.5f;
        arrTempPos[idx] = tempSprBh[idx].transform.position;
        arrTempScale[idx] = tempSprBh[idx].transform.localScale;
        tempVec = Vector3.zero;
        tempVec.z = (Mathf.Atan2((end - start).y, (end - start).x) - Mathf.Atan2((bh.orgList[bh.orgList.Count - 1] - bh.orgList[0]).y, (bh.orgList[bh.orgList.Count - 1] - bh.orgList[0]).x)) * Mathf.Rad2Deg;
        if (Mathf.Abs(tempVec.z) > 60)
            tempVec.z = 0;
        tempSprBh[idx].transform.localEulerAngles = tempVec;
        arrTempAngle[idx] = tempVec.z;
        isMove[idx] = true;
        waitMoveTime[idx] = Time.time + 0.2f;
    }
	void Update () {
        if (blnWarning)
        {
            Color c = arrSprBh[warningIdx].GetComponent<SpriteRenderer>().color;
            float time = Time.time - warningTime;
            c.a = Mathf.Lerp(0.2f, 0, (time % 0.5f) * 2);
            arrSprBh[warningIdx].GetComponent<SpriteRenderer>().color = c;
            if (Time.time - warningTime >= 1.5f)
            {
                arrSprBh[warningIdx].SetActive(false);
                blnWarning = false;
            }
        }
        if (tempSprBh != null)
        {
            for (int i = 0; i < bihuaCount; i++)
            {
                if (tempSprBh[i] != null && isMove[i] && Time.time >= waitMoveTime[i])
                {
                    float dt = Time.time - waitMoveTime[i];
                    tempSprBh[i].transform.position = Vector3.Lerp(arrTempPos[i], arrSprBh[i].transform.position, dt * 4f);
                    float z = Mathf.LerpAngle(arrTempAngle[i], arrSprBh[i].transform.localEulerAngles.z, dt * 4f);
                    tempSprBh[i].transform.localEulerAngles = Vector3.forward * z;
                    tempSprBh[i].transform.localScale = Vector3.Lerp(arrTempScale[i], arrSprBh[i].transform.localScale, dt * 4f);
                    if (dt >= 0.25f)
                    {
                        tempSprBh[i].SetActive(false);
                        isMove[i] = false;
                        if (blnWarning && i == warningIdx)
                            blnWarning = false;
                        arrSprBh[i].GetComponent<SpriteRenderer>().color = orgColor;
                        arrSprBh[i].SetActive(true);
                    }
                }
            }
        }
	}
    public void SetWarningTips()
    {
        if (!blnWarning)
        {
            for (int i = 0; i < bihuaCount; i++)
            {
                if (!arrSprBh[i].activeSelf && (!tempSprBh[i] || !tempSprBh[i].activeSelf))
                {
                    warningIdx = i;
                    Color c = warningColor;
                    c.a = 0.2f;
                    arrSprBh[i].GetComponent<SpriteRenderer>().color = c;
                    blnWarning = true;
                    arrSprBh[i].SetActive(true);
                    warningTime = Time.time;
                    break;
                }
            }
        }
    }
    public bool IsWriteBack(int idx, Vector3 start, Vector3 end, Bihua bh)
    {
        start = Camera.main.ScreenToWorldPoint(start);
        start.z = 0;
        end = Camera.main.ScreenToWorldPoint(end);
        end.z = 0;
        float v = (Mathf.Atan2((end - start).y, (end - start).x) - Mathf.Atan2((bh.orgList[bh.orgList.Count - 1] - bh.orgList[0]).y, (bh.orgList[bh.orgList.Count - 1] - bh.orgList[0]).x)) * Mathf.Rad2Deg;
        if (v > 60)
            return true;
        return false;
    }
    public Vector3 GetRightPos(int idx)
    {
        Vector3 v = Vector3.zero;
        SpriteRenderer sr = arrSprBh[idx].GetComponent<SpriteRenderer>();
        v = sr.sprite.bounds.center + arrSprBh[idx].transform.position;
        v.x += sr.sprite.bounds.size.x / 2 + 0.1f;
        return v;
    }
    public void SetBujianShow(int bujianIdx)
    {
        for (int k = 0; k < bihuaCount; k++)
        {
            if (myInfo.arrBihua[k].bushou == bujianIdx)
            {
                arrSprBh[k].SetActive(true);
            }
        }
    }
}
