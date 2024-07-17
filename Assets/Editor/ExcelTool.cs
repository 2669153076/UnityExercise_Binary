using Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExcelTool
{
    //Excel文件存储路径
    private static string EXCEL_PATH = Application.dataPath + "/Excels/";

    //数据结构类 脚本存储路径
    private static string DATA_CLASS_PATH = Application.dataPath + "/Scripts/ExcelData/DataClass/";
    //数据容器类 脚本存储路径
    private static string DATA_CONTAINER_PATH = Application.dataPath + "/Scripts/ExcelData/Container/";

    /// <summary>
    /// 读取Excel数据 真正内容开始的行号
    /// </summary>
    private static int BEGIN_INDEX = 4;

    [MenuItem("GameTool/GenerateExcel")]
    private static void GenerateExcelInfo()
    {
        DirectoryInfo directoryInfo = null;
        if (!Directory.Exists(EXCEL_PATH)){
            directoryInfo = Directory.CreateDirectory(EXCEL_PATH);
        }
        else
        {
            directoryInfo = new DirectoryInfo(EXCEL_PATH);
        }

        //得到指定路径中所有文件信息
        FileInfo[] fileInfos = directoryInfo.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
        
            if (fileInfos[i].Extension != ".xlsx" && fileInfos[i].Extension != ".xls" && fileInfos[i].Extension != ".csv")
            {
                continue;
            }
            //数据表容器
            DataTableCollection dataTableCollection;

            using (FileStream fs = fileInfos[i].Open(FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelDataReader  = ExcelReaderFactory.CreateOpenXmlReader(fs);
                dataTableCollection = excelDataReader.AsDataSet().Tables;

                fs.Close();
            }

            foreach (DataTable table in dataTableCollection)
            {
                Debug.Log(table.TableName);

                //生成数据结构类
                GenerateExcelDataClass(table);
                //生成容器类
                GenerateExcelContainer(table);
                //生成2进制容器类
                GenerateExcelBinary(table);
            }
        }
    }

    /// <summary>
    /// 生成Excel表对应的数据结构类
    /// </summary>
    /// <param name="table"></param>
    private static void GenerateExcelDataClass(DataTable table)
    {
        //字段名 行
        DataRow rowName = GetVariableNameRow(table);
        //字段类型 行
        DataRow rowType = GetVariableTypeRow(table);

        if(!Directory.Exists(DATA_CLASS_PATH))
        {
            Directory.CreateDirectory(DATA_CLASS_PATH);
        }

        //生成对应的数据结构类脚本
        string str = "public class " + table.TableName + "\n{\n";
        //变量进行字符串拼接
        for (int i = 0;i<table.Columns.Count;i++)
        {
            str += "\tpublic " + rowType[i].ToString() + " " + rowName[i].ToString() + ";\n";
        }
        str += "}";
        //将拼接好的字符串存储到指定文件中
        File.WriteAllText(DATA_CLASS_PATH+table.TableName+".cs", str);
        
        //刷新Project窗口
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 生成Excel表对应的容器类
    /// </summary>
    /// <param name="table"></param>
    private static void GenerateExcelContainer(DataTable table)
    {
        //得到主键索引
        int keyIndex = GetKeyIndex(table);
        //得到字段类型行
        DataRow rowType = GetVariableTypeRow(table);

        if(!Directory.Exists (DATA_CONTAINER_PATH))
        {
            Directory .CreateDirectory(DATA_CONTAINER_PATH);
        }

        string str = "using System.Collections.Generic;\n";
        str += "public class " + table.TableName + "Container" + "\n{\n";
        str += "\t";
        str += "public Dictionary<" + rowType[keyIndex].ToString() + ", " + table.TableName + "> ";
        str += "dataDic = new Dictionary<" + rowType[keyIndex].ToString() + ", " + table.TableName + ">();\n";
        str += '}';

        File.WriteAllText(DATA_CONTAINER_PATH+table.TableName+"Container.cs",str);

        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 生成Excel表对应的二进制数据类
    /// </summary>
    /// <param name="table"></param>
    private static void GenerateExcelBinary(DataTable table)
    {
        if(!Directory.Exists(BinaryDataMgr.DATA_BINARY_PATH))
        {
            Directory.CreateDirectory(BinaryDataMgr.DATA_BINARY_PATH);
        }

        //创建一个二进制文件进行写入
        using(FileStream fs  = new FileStream(BinaryDataMgr.DATA_BINARY_PATH + table.TableName + ".luoying", FileMode.OpenOrCreate, FileAccess.Write))
        {
            //存储具体的Excel对应的二进制信息
            //1.首先存储需要写多少行的数据
            //-4是因为 前四行是配置规则，不需要记录的内容
            fs.Write(BitConverter.GetBytes(table.Rows.Count- 4), 0, 4);
            //2.存储主键的变量名
            string keyName = GetVariableNameRow(table)[GetKeyIndex(table)].ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(keyName);
            //存储字符串字节数的长度
            fs.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
            //存储字符串字节数组
            fs.Write(bytes, 0, bytes.Length);
            //遍历所有内容的行 进行2进制写入
            DataRow row;
            DataRow rowType= GetVariableTypeRow(table); //得到类型行 根据类型决定如何写入数据
            for (int i = BEGIN_INDEX; i < table.Rows.Count; i++)
            {
                row = table.Rows[i];
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    switch (rowType[j])
                    {
                        case "int":
                            fs.Write(BitConverter.GetBytes(int.Parse(row[j].ToString())),0,4);
                            break;
                        case "float":
                            fs.Write(BitConverter.GetBytes(float.Parse(row[j].ToString())), 0, 4);
                            break;
                        case "string":
                            bytes = Encoding.UTF8.GetBytes(row[j].ToString());
                            //写入字符串字节数组的长度
                            fs.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                            //写入字符串数组
                            fs.Write(bytes,0, bytes.Length);
                            break;
                        case "bool":
                            fs.Write(BitConverter.GetBytes(bool.Parse(row[j].ToString())), 0, 1);
                            break;
                    }
                }
            }
            fs.Close();
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取变量名所在的行
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    private static DataRow GetVariableNameRow(DataTable table)
    {
        return table.Rows[0];
    }
    /// <summary>
    /// 获取变量类型所在的行
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    private static DataRow GetVariableTypeRow(DataTable table)
    {
        return table.Rows[1];
    }
    /// <summary>
    /// 获取主键索引
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public static int GetKeyIndex(DataTable table)
    {
        DataRow row = table.Rows[3];
        for (int i = 0; i < table.Columns.Count; i++)
        {
            if (row[i].ToString() == "key")
            {
                return i;
            }
        }
        return 0;
    }
}

