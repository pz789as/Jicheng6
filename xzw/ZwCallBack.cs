using UnityEngine;
using System.Collections;

public class ZwCallBack : MonoBehaviour {
    public delegate void OnClick(GameObject obj);
    public OnClick onClick = null;

    public void ZwOnClick()
    {
        if (onClick != null)
            onClick(this.gameObject);
    }
}
