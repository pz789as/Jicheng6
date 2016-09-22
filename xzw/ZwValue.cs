using UnityEngine;
using System.Collections;

public class ZwValue : MonoBehaviour {
    static private ZwValue ins = null;
    static public ZwValue Ins
    {
        get
        {
            if (ins == null)
            {
                ins = GameObject.FindObjectOfType<ZwValue>();
                if (ins == null)
                {
                    GameObject g = new GameObject("ZwValue");
                    ins = g.AddComponent<ZwValue>();
                }
            }
            return ins;
        }
    }
    #region otherValue
        [SerializeField]
        public float moveNextDis = 6.27f;
        [SerializeField]
        public float moveNextTime = 1f;
        [SerializeField]
        public int singleErrorMax = 2;
        [SerializeField]
        public int totalErrorMax = 3;
    #endregion

    #region bishunClear
        [SerializeField]
        public Vector2 clearTouchSize = new Vector2(2f, 1f);
    #endregion

    #region ZwScale
        [SerializeField]
        public Color pointPerfectColor = Color.blue;
        [SerializeField]
        public float pointScaleMax = 4f;
        [SerializeField]
        public int pointFadeFrame = 8;
    #endregion
}
