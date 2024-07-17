using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class BinaryDataMgr
{
    //数据存储的位置
    private static string SAVE_PATH = Application.persistentDataPath + "/Data/";
    //用于存储所有Excel表数据的容器
    private Dictionary<string,object> tableDic = new Dictionary<string,object>();
    //二进制数据 存储路径
    public static string DATA_BINARY_PATH = Application.dataPath + "/BinaryDatas/";

    private static BinaryDataMgr instance = new BinaryDataMgr();
    public static BinaryDataMgr Instance => instance;
    private BinaryDataMgr() { }

    /// <summary>
    /// 加载Excel表的2进制数据到内存中
    /// </summary>
    /// <typeparam name="T">容器类</typeparam>
    /// <typeparam name="K">数据结构类</typeparam>
    /// <returns></returns>
    public void LoadTable<T, K>()
    {
        //读取 Excel表对应的2进制数据
        using (FileStream fs = File.Open(DATA_BINARY_PATH + typeof(K).Name + ".luoying",FileMode.Open,FileAccess.Read))
        {
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);

            int index = 0;  //用于记录读取了多少个字节

            //读取多少行数据
            int count = BitConverter.ToInt32(bytes,index);
            index += 4;
            //读取主键的名字
            int keyNameLength = BitConverter.ToInt32(bytes,index);
            index += 4;
            string keyName = Encoding.UTF8.GetString(bytes,index,keyNameLength);
            index += keyNameLength;

            //创建容器类对象
            Type contaninerType = typeof(T);
            object contaninerObj =  Activator.CreateInstance(contaninerType);

            //得到数据结构类的Type
            Type classType = typeof(K);
            //得到数据结构类 所有字段的信息
            FieldInfo[] infos = classType.GetFields();
            //读取每一行的信息
            for (int i = 0; i < count; i++)
            {
                //实例化一个数据结构类 对象
                object dataObj = Activator.CreateInstance(classType);
                foreach (FieldInfo info in infos)
                {
                    if(info.FieldType == typeof(int))
                    {
                        //把2进制数据转为int 然后赋值给对应字段
                        info.SetValue(dataObj,BitConverter.ToInt32(bytes, index));
                        index += 4;
                    }
                    else if(info.FieldType == typeof(string))
                    {
                        int length = BitConverter.ToInt32(bytes, index);
                        index += 4;
                        info.SetValue(dataObj, Encoding.UTF8.GetString(bytes,index,length));
                        index += length;
                    }
                    else if(info.FieldType == typeof(float))
                    {
                        info.SetValue(dataObj, BitConverter.ToSingle(bytes, index));
                        index += 4;
                    }
                    else if(info.FieldType == typeof(bool))
                    {
                        info.SetValue(dataObj, BitConverter.ToBoolean(bytes, index));
                        index += 1;
                    }
                }
                //读取完一行数据 将数据添加到容器中

                //得到容器对象中的字典对象
                object dicObj = contaninerType.GetField("dataDic").GetValue(contaninerObj);
                //通过字典对象得到其中的Add方法
                MethodInfo methodInfo = dicObj.GetType().GetMethod("Add");
                //得到数据结构类对象中 指定主键字段的值
                object keyValue = classType.GetField(keyName).GetValue(dataObj);
                methodInfo.Invoke(dicObj, new object[] { keyValue, dataObj });
            }

            //把读取完的表记录下来
            tableDic.Add(typeof(T).Name, contaninerObj);

            fs.Close();
        }
    }

    /// <summary>
    /// 得到一张表的信息
    /// </summary>
    /// <typeparam name="T">容器类名</typeparam>
    /// <returns></returns>
    public T GetTable<T>() where T : class
    {
        string tableName = typeof(T).Name;
        if(tableDic.ContainsKey(tableName))
        {
            return tableDic[tableName] as T;
        }
        return null;
    }

    /// <summary>
    /// 存储类对象为二进制数据
    /// </summary>
    /// <param name="obj">类对象</param>
    /// <param name="fileName">文件名</param>
    public void Save(object obj,string fileName)
    {
        if (!Directory.Exists(SAVE_PATH))
        {
            Directory.CreateDirectory(SAVE_PATH);
        }
        
        using(FileStream fs = File.Open(SAVE_PATH + fileName + ".luoying", FileMode.OpenOrCreate, FileAccess.Write))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, obj);

            fs.Close();
        }
    }
    /// <summary>
    /// 读取二进制数据为类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName">文件名</param>
    /// <returns></returns>
    public T Load<T>(string fileName) where T : class
    {
        if(!File.Exists(SAVE_PATH + fileName + ".luoying"))
        {
            return default(T);
        }

        T obj = null;

        using (FileStream fs  = File.Open(SAVE_PATH + fileName + ".luoying", FileMode.Open, FileAccess.Read))
        {
            BinaryFormatter bf = new BinaryFormatter();
            obj = bf.Deserialize(fs) as T;

            fs.Close();
        }

        return obj;
    }

}
