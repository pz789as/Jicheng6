using UnityEngine;
using System.Collections;

public class CV : MonoBehaviour {
    public const int Status_Cs = 0;//初始
    public const int Status_Xz = 1;//选字
    public const int Status_Xxz = 2;//学习字
    public const int Status_Xz_Over = 3;//写字完毕
    public const int Status_Bjck = 4;//部件查看

    static public int allStatus = Status_Cs;

    public static string pngPath = "";
}
