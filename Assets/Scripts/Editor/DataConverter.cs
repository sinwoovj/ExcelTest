using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExcelDataReader;
using System.Data;
using System.IO;
using System;

public class DataConverter 
{

    [MenuItem("Test/Reflection")]
    public static void TestReflection()
    {
        var type = typeof(ITEM);
        ITEM item = new ITEM();

    }

    [MenuItem("Test/ReadFromExcel")]
    public static void ReadFromExcel()
    {
        Debug.Log("Test");
        System.Data.DataSet dataset;

        string filePath = "Assets/Excel/Data.xlsx";
        using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                dataset = reader.AsDataSet();
            }
        }

        Debug.Log($"TableCount :{dataset.Tables.Count}");

        string assetPath = "Assets/Data/data.asset";

        var gameData = AssetDatabase.LoadAssetAtPath<GameData>(assetPath);

        if (gameData == null)
        {
            gameData = ScriptableObject.CreateInstance<GameData>();
        }
        else
        {
            EditorUtility.SetDirty(gameData);
        }

        var gtype = typeof(GameData);
        var dtype = typeof(DataConverter);
        var gfis = gtype.GetFields();
        var dmi = dtype.GetMethod("MakeListFromDataset", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
        foreach (var f in gfis)
        {
            var type = f.FieldType;
            if(type.IsGenericType == false || type.GetGenericTypeDefinition() != typeof(List<>))
            {
                continue;
            }
            var nmi = dmi.MakeGenericMethod(type.GenericTypeArguments[0]);
            Debug.Log(f.Name);
            nmi.Invoke(null,new object[] { dataset, f.Name });
            //gameData. = MakeListFromDataset<>(dataset, f.Name.ToString());
            //Debug.Log(f.Name.ToString() + type.ToString()+ type.GetGenericTypeDefinition().Name.ToString());
        }
       /* gameData.NPC = MakeListFromDataset<NPC>(dataset, "NPC");
        gameData.Item = MakeListFromDataset<ITEM>(dataset, "ITEM");
        gameData.Item2 = MakeListFromDataset<ITEM>(dataset, "ITEM");
*/


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static List<T> MakeListFromDataset<T>(DataSet dataset, string tableName) where T : new()
    {
        List<T> list = new List<T>();
        var table = FindTable(dataset, tableName);
        if (table == null)
        {
            throw new Exception($"{tableName}이 없습니다.");
        }
        var itemType = typeof(T);
        for (int row = 1; row < table.Rows.Count; row++)
        {
            var item = new T();
            list.Add(item);
            for (int col = 0; col < table.Columns.Count; col++)
            {
                var Fieldname = table.Rows[0][col] as string;
                var fi = itemType.GetField(Fieldname);
                if (fi == null)
                {
                    Debug.LogError($"{Fieldname}이 {itemType.Name}에 없습니다.");
                }
                var value = table.Rows[row][col];
                var fieldType = fi.FieldType;

                if(fieldType.GetInterface("IFillFromStr")!=null)
                {
                    var obj=System.Activator.CreateInstance(fieldType) as IFillFromStr;
                    obj.FillFromString(value as string);
                    fi.SetValue(item, obj);
                }
                else if (fieldType.IsEnum)
                {
                    fi.SetValue(item, Enum.Parse( fi.FieldType, value as string));
                }
                else
                {
                    fi.SetValue(item, Convert.ChangeType(value, fi.FieldType));

                }

            }
        }
        return list;
    }




    private static DataTable FindTable(DataSet dataset, string tablename)
    {
        DataTable table = null;
        foreach (DataTable t in dataset.Tables)
        {
            if (t.TableName == tablename)
            {
                table = t;
                break;
            }
        }
        return table;
    }
    /*
    private static void FillITEMList(DataSet dataset, List<ITEM> itemList)
    {
        DataTable table = FindTable(dataset, "ITEM");
        if (table == null)
        {
            throw new Exception("ITEM 테이블이 없다.");
        }
        itemList.Clear();
        for (int i = 1; i < table.Rows.Count; i++)
        {
            string idstr = table.Rows[i][0].ToString();
            string name = table.Rows[i][1].ToString();
            string description = table.Rows[i][2].ToString();
            int id;
            if (string.IsNullOrEmpty(idstr))
            {
                break;
            }
            if (int.TryParse(idstr, out id) == false)
            {
                throw new Exception($"{i}번째 id 파싱에 실패했습니다.");
            }

            ITEM item = new ITEM()
            {
                Id = id,
                Name = name,
                Description = description,
            };
            itemList.Add(item);
        }
    }
    
    private static void FillNPCList(DataSet dataset, List<NPC> npcList)
    {
        DataTable table=null;
        foreach (DataTable t in dataset.Tables)
        {
            if (t.TableName == "NPC")
            { 
                table = t;
                break;
            }
        }
        if (table == null)
        {
            throw new Exception("NPC 테이블이 없다.");
        }
        npcList.Clear(); 
        for (int i = 1; i < table.Rows.Count; i++)
        {
            string numStr = table.Rows[i][0].ToString();
            string type = table.Rows[i][1].ToString();
            string sex = table.Rows[i][2].ToString();
            string name = table.Rows[i][3].ToString();
            int id;
            if (string.IsNullOrEmpty(numStr))
            {
                break;
            }
            if(int.TryParse(numStr, out id) == false)
            {
                throw new Exception($"{i}번째 id 파싱에 실패했습니다.");
            }
            NPC npc = new NPC()
            {
                Id = id,
                Type = type,
                Sex = sex,
                Name = name
            };
            npcList.Add(npc);

        }

    }
    */
}
