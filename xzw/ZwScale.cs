using UnityEngine;
using System.Collections;

public class ZwScale : MonoBehaviour {
    Color orgColor;
    Color tmpColor;
    Color oldColor;
    SpriteRenderer m_Sprite = null;
    int frameCount = 8;
    Vector3 tempVec = Vector3.zero;
    float maxS = 4f;
    [HideInInspector]
    public bool isScale = false;
    bool blnStartColor = false;
    float changeColorSpeed = 1;
    float lastScale = 0;
    void Start()
    {
        m_Sprite = this.gameObject.GetComponent<SpriteRenderer>();
        orgColor = m_Sprite.color;
        maxS = ZwValue.Ins.pointScaleMax;
        frameCount = ZwValue.Ins.pointFadeFrame;
    }
    public void ResetColor()
    {
        m_Sprite.color = orgColor;
    }
    public void SetEndColor(float t, float colf)
    {
        StartCoroutine(Coloring(t, colf));
    }
    IEnumerator Coloring(float t, float colf)
    {
        float i = 1;
        yield return new WaitForSeconds(t);
        while (i <= frameCount)
        {
            tmpColor = m_Sprite.color;
            tmpColor.a = Mathf.Lerp(colf, 0, i / frameCount);
            m_Sprite.color = tmpColor;
            i++;
            yield return null;
        }
        this.gameObject.SetActive(false);
    }
    public void SetStartColor(float t)
    {
        m_Sprite.color = ZwValue.Ins.pointPerfectColor;
        changeColorSpeed = t;
        blnStartColor = true;
    }
    void Update()
    {
        if (blnStartColor)
        {
            oldColor = m_Sprite.color;
            oldColor.r = Mathf.Lerp(oldColor.r, 0, changeColorSpeed * Time.deltaTime);
            oldColor.g = Mathf.Lerp(oldColor.g, 0, changeColorSpeed * Time.deltaTime);
            oldColor.b = Mathf.Lerp(oldColor.b, 0, changeColorSpeed * Time.deltaTime);
            m_Sprite.color = oldColor;
            if (m_Sprite.color.r * 100 < 1 && m_Sprite.color.g * 100 < 1 && m_Sprite.color.b * 100 < 1)
                blnStartColor = false;
        }
        if (isScale)
        {
            tempVec = transform.localScale;
            tempVec.x = Mathf.Lerp(tempVec.x, lastScale, Time.deltaTime);
            tempVec.y = tempVec.x;
            if (tempVec.x > maxS)
                tempVec.x = maxS;
            if (tempVec.y > maxS)
                tempVec.y = maxS;
            transform.localScale = tempVec;
            if (Mathf.Abs(tempVec.x - lastScale) < 0.1f)
            {
                isScale = false;
            }
        }
    }
    public void SetStartScale(float scale)
    {
        isScale = true;
        lastScale = scale * transform.localScale.x;
    }
    public void SetNowScale(float scale)
    {
        //isScale = true;
        //lastScale = scale;
        if (!isScale)
        {
            tempVec = transform.localScale;
            tempVec.x = Mathf.Lerp(tempVec.x, scale, Time.deltaTime);
            tempVec.y = tempVec.x;
            if (tempVec.x > maxS)
                tempVec.x = maxS;
            if (tempVec.y > maxS)
                tempVec.y = maxS;
            transform.localScale = tempVec;
        }
    }
    void OnEnable()
    {
        m_Sprite = this.gameObject.GetComponent<SpriteRenderer>();
    }
    void OnDisable()
    {
        StopAllCoroutines();
        isScale = false;
        blnStartColor = false;
    }
}
