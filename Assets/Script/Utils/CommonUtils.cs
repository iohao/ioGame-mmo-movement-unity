using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class CommonUtils
{
    /**
     * json字符串转bean
     */
    public static T JsonToBean<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static float positionToFloat(float position)
    {
        return (Mathf.Round(position * 100f) / 100f);
    }
    
    public static string positionToFloatStr(float position)
    {
        return positionToFloat(position).ToString();
    }
}
