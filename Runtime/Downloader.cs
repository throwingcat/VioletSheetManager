using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Violet.SheetManager
{
    public static class Downloader
    {
        public static IEnumerator Get(string url, System.Action<bool,string> result)
        {
            using UnityWebRequest www = UnityWebRequest.Get(url);
            
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                result?.Invoke(false,$"Error {www.error}");
            else
                result?.Invoke(true,www.downloadHandler.text);
        }
    }
}