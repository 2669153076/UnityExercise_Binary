using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Lesson1 : MonoBehaviour
{
    #region 为编辑器菜单栏添加新的特性入口
    //可以通过Unity提供我们的MenuItem特性在菜单栏添加选项按钮
    //特性名:MenuItem
    //命名空间:UnityEditor

    //规则一:—定是静态方法
    //规则二:我们这个菜单栏按钮 必须有至少一个斜杠 不然会报错 它不支持只有一个菜单栏入口
    //规则三:这个特性可以用在任意的类当中
    [MenuItem("GameTool/Test/创建测试文件夹")]
    private static void Test()
    {
        Debug.Log("测试");

        Directory.CreateDirectory(Application.dataPath+"/测试文件夹");
        AssetDatabase.Refresh();
    }
    #endregion

    #region 刷新Project窗口内容
    //类名:AssetDatabase
    //命名空间:unityEditor
    //方法:Refresh

    //AssetDatabase.Refresh();
    #endregion

    #region Editor文件夹
    //Editor文件夹可以放在项目的任何文件夹下,可以有多个
    //放在其中的内容,项目打包时不会被打包到项目中
    //一般编辑器相关代码都可以放在该文件夹中

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
