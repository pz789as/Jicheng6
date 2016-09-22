using UnityEngine;
using System.Collections;

public class ZwClear : MonoBehaviour {
    RaycastHit2D hit;
    Vector3 tempVec = Vector3.zero;
    public static bool collideClear = false;
    BoxCollider2D boxCol;
    void Start()
    {
        boxCol = this.gameObject.GetComponent<BoxCollider2D>();
        boxCol.size = ZwValue.Ins.clearTouchSize;
        tempVec = Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width / 2);
        tempVec.y += boxCol.size.y / 2;
        transform.position = tempVec;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tempVec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            hit = Physics2D.Raycast(tempVec, Vector2.zero);
            if (hit.collider && hit.collider.tag == "ClearDistrict")
            {
                collideClear = true;
            }
            else
            {
                collideClear = false;
            }
        }
    }
    static public bool CheckClear(Vector3 start, Vector3 end)
    {
        if (collideClear)
        {
            collideClear = false;
            float angle = Mathf.Atan2((end - start).y, (end - start).x) * Mathf.Rad2Deg;
            float dis = Vector3.Distance(start, end);
            if (angle >= 80 && angle <= 100 && dis >= 3)
            {
                return true;
            }
        }
        return false;
    }
}
