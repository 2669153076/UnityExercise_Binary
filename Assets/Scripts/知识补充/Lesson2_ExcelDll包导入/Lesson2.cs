using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lesson2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        #region 了解Excel表的本质
        //Excel表本质上也是—堆数据
        //只不过它有自己的存储读取规则
        //如果我们想要通过代码读取它
        //那么必须知道它的存储规则

        //官网是专门提供了对应的DLL文件用来解析Excel文件的

        //Dll文件:库文件,你可以理解为它是许多代码的集合
        //将相关代码集合在库文件中可以方便迁移和使用
        //有了某个DLL文件,我们就可以使用其中已经写好的代码

        //而Excel的DLL包就是官方已经把解析Excel表的相关类和方法写好了
        //方便用户直接使用

        #endregion

        #region 导入官方提供的Excel相关DLL文件

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
