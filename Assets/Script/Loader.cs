//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.UI;

//public class BrushDrawerEditor : BrushDrawer
//{
//    [Header("UI")]
//    public TMP_InputField inputFileName;

//    public void LoadFromDisk()
//    {
//        string fileName = inputFileName.text;
//        if (string.IsNullOrEmpty(fileName)) fileName = "drawing.json";

//        string path = Path.Combine(Application.persistentDataPath, fileName);
//        if (!File.Exists(path))
//        {
//            Debug.LogError("Файл не найден: " + path);
//            return;
//        }

//        string json = File.ReadAllText(path);
//        StrokeListWrapper wrapper = JsonUtility.FromJson<StrokeListWrapper>(json);
//        strokes = wrapper.strokes;
//        RedrawAll();
//        Debug.Log("Загружено: " + path);
//    }

//    public void SaveToDisk()
//    {
//        string fileName = inputFileName.text;
//        if (string.IsNullOrEmpty(fileName)) fileName = "drawing.json";

//        StrokeListWrapper wrapper = new StrokeListWrapper { strokes = strokes };
//        string json = JsonUtility.ToJson(wrapper, true);
//        string path = Path.Combine(Application.persistentDataPath, fileName);
//        File.WriteAllText(path, json);
//        Debug.Log("Сохранено: " + path);
//    }
//}