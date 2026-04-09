using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class MD_WebNetwork : IDModuleBase
{
    List<Texture2D> textureList = new List<Texture2D>();
    Dictionary<string, string> uriMap = new Dictionary<string, string>();

    DCore_Object coreObj;

    const string requestString = "token_key";
    const string requestValue = "";
    const int timeOut = 30;

    public class FormData
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        public enum ImageType
        {
            JPEG,
            PNG,
            TGA,
        }

        public void Add<T>(string key, T value)
        {
            string stringValue = value?.ToString() ?? string.Empty;
            formData.Add(new MultipartFormDataSection(key, stringValue));
        }

        public void Add(string key, Texture2D texture, string fileName, ImageType type)
        {
            byte[] fileData = null;
            string mimeType = null;

            switch (type)
            {
                case ImageType.JPEG:
                    fileData = texture.EncodeToJPG();
                    fileName += ".jpg";
                    mimeType = "image/jpeg";
                    break;
                case ImageType.PNG:
                    fileData = texture.EncodeToPNG();
                    fileName += ".png";
                    mimeType = "image/png";
                    break;
                case ImageType.TGA:
                    fileData = texture.EncodeToTGA();
                    fileName += ".tga";
                    mimeType = "image/x-tga";
                    break;
            }

            formData.Add(new MultipartFormFileSection(key, fileData, fileName, mimeType));
        }

        public void Add(string key, byte[] fileData, string fileName, string mimeType = "application/octet-stream")
        {
            formData.Add(new MultipartFormFileSection(key, fileData, fileName, mimeType));
        }

        public List<IMultipartFormSection> GetFormData()
        {
            return formData;
        }

        public void Clear()
        {
            formData.Clear();
        }
    }

    public void Initialize()
    {
        CreateNetworkObject();
    }

    void CreateNetworkObject()
    {
        GameObject networkObj = new GameObject("NetworkObj");
        GameObject.DontDestroyOnLoad(networkObj);

        networkObj.transform.position = Vector3.zero;
        networkObj.transform.localRotation = Quaternion.identity;
        networkObj.transform.localScale = Vector3.one;

        coreObj = networkObj.AddComponent<DCore_Object>();
    }

    public void AddUri(string requestName, string uri)
    {
        if (uriMap.ContainsKey(requestName))
        {
            uriMap[requestName] = uri;
        }
        else
        {
            uriMap.Add(requestName, uri);
        }
    }

    public string GetUri(string requestName, params string[] args)
    {
        if (!uriMap.ContainsKey(requestName))
        {
            Debug.LogError("Null Uri!!!!!");
            return null;
        }
        try
        {
            return string.Format(uriMap[requestName], args);
        }
        catch (System.FormatException)
        {
            Debug.LogError($"Uri Format Error");
            return null;
        }
    }

    void SetCustomRequest(UnityWebRequest request)
    {
        if (!string.IsNullOrWhiteSpace(requestString) && !string.IsNullOrWhiteSpace(requestValue))
            request.SetRequestHeader(requestString, requestValue);

        request.timeout = timeOut;
    }

    public void GetRequest(string uri, UnityAction<string> successCallback, UnityAction<long> failCallback = null)
    {
        coreObj.StartCoroutine(GetStringRequest(uri, successCallback, failCallback));
    }

    public void GetRequest(string uri, UnityAction<Texture2D> successCallback, UnityAction<long> failCallback = null)
    {
        coreObj.StartCoroutine(GetTextureRequest(uri, successCallback, failCallback));
    }

    public void PostRequest(string uri, List<IMultipartFormSection> formData, UnityAction<string> successCallback = null, UnityAction<UnityWebRequest> failCallback = null)
    {
        coreObj.StartCoroutine(PostStringRequest(uri, formData, successCallback, failCallback));
    }

    public void PostRequest(string uri, List<IMultipartFormSection> formData, UnityAction<Texture2D> successCallback, UnityAction<long> failCallback = null)
    {
        coreObj.StartCoroutine(PostTextureRequest(uri, formData, successCallback, failCallback));
    }

    public void PostRequest<T>(string uri, T classData, UnityAction<string> successCallback, UnityAction<UnityWebRequest> failCallback = null)
    {
        coreObj.StartCoroutine(PostJsonRequest(uri, JsonUtility.ToJson(classData), successCallback, failCallback));
    }

    IEnumerator GetStringRequest(string uri, UnityAction<string> successCallback, UnityAction<long> failCallback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            SetCustomRequest(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                successCallback?.Invoke(request.downloadHandler.text);
            else
                failCallback?.Invoke(request.responseCode);
        }
    }

    IEnumerator GetTextureRequest(string uri, UnityAction<Texture2D> successCallback, UnityAction<long> failCallback)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(uri))
        {
            SetCustomRequest(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                textureList.Add(texture);
                successCallback?.Invoke(texture);
            }
            else
            {
                failCallback?.Invoke(request.responseCode);
            }
        }
    }

    IEnumerator PostStringRequest(string uri, List<IMultipartFormSection> formData, UnityAction<string> successCallback, UnityAction<UnityWebRequest> failCallback)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(uri, formData))
        {
            SetCustomRequest(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                successCallback?.Invoke(request.downloadHandler.text);
            else
                failCallback?.Invoke(request);

            formData?.Clear();
        }
    }

    IEnumerator PostTextureRequest(string uri, List<IMultipartFormSection> formData, UnityAction<Texture2D> successCallback, UnityAction<long> failCallback)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(uri, formData))
        {
            SetCustomRequest(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                textureList.Add(texture);
                successCallback?.Invoke(texture);
            }
            else
            {
                failCallback?.Invoke(request.responseCode);
            }

            formData?.Clear();
        }
    }

    IEnumerator PostJsonRequest(string uri, string jsonData, UnityAction<string> successCallback, UnityAction<UnityWebRequest> failCallback = null)
    {
        using (UnityWebRequest request = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            SetCustomRequest(request);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                successCallback?.Invoke(request.downloadHandler.text);
            else
                failCallback?.Invoke(request);
        }
    }

    public void ClearTexture()
    {
        for (int i = 0; i < textureList.Count; i++)
        {
            if (textureList[i] != null)
                GameObject.Destroy(textureList[i]);
        }
        textureList.Clear();
    }

    public static string UrlEncode(string str)
    {
        string encode = System.Uri.EscapeDataString(str);
        encode = encode.Replace("+", "%20");

        return encode;
    }
}
