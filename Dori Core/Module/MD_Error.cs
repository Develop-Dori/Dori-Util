using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MD_Error : IDModuleBase
{
    public const string WebError = "0x57";
    public const string ServerError = "0x53";
    public const string ClientError = "0x43";

    Dictionary<string, UnityAction> errorCallback = new Dictionary<string, UnityAction>();

    public void Initialize()
    {
        errorCallback.Clear();
    }

    public static string GetWebErrorCode(WebCode code)
    {
        return WebError + (int)code;
    }

    public static string GetServerErrorCode(ServerCode code)
    {
        return ServerError + (int)code;
    }

    public static string GetClientErrorCode(ClientCode code)
    {
        return ClientError + (int)code;
    }

    public void SetWebErrorCallback(WebCode code, UnityAction callback)
    {
        SetErrorCallback(GetWebErrorCode(code), callback);
    }

    public void SetServerErrorCallback(ServerCode code, UnityAction callback)
    {
        SetErrorCallback(GetServerErrorCode(code), callback);
    }

    public void SetClientErrorCallback(ClientCode code, UnityAction callback)
    {
        SetErrorCallback(GetClientErrorCode(code), callback);
    }

    void SetErrorCallback(string errorCode, UnityAction callback)
    {
        if (errorCallback.ContainsKey(errorCode))
        {
            errorCallback[errorCode] = callback;
        }
        else
        {
            errorCallback.Add(errorCode, callback);
        }
    }

    public void InvokeWebErrorCallback(WebCode code)
    {
        if (errorCallback.TryGetValue(GetWebErrorCode(code), out UnityAction callback))
            callback?.Invoke();
    }

    public void InvokeServerErrorCallback(ServerCode code)
    {
        if (errorCallback.TryGetValue(GetServerErrorCode(code), out UnityAction callback))
            callback?.Invoke();
    }

    public void InvokeClientErrorCallback(ClientCode code)
    {
        if (errorCallback.TryGetValue(GetClientErrorCode(code), out UnityAction callback))
            callback?.Invoke();
    }


    #region WebCode
    public enum WebCode
    {
        None1 = 1,
        None2 = 2,
        None3 = 3,
        None4 = 4,
        None5 = 5,
        None6 = 6,
        None7 = 7,
        None8 = 8,
        None9 = 9,
    }
    #endregion

    #region ServerCode
    public enum ServerCode
    {
        None1 = 100,

        None2 = 200,

        None3 = 300,

        None4 = 400,

        None5 = 500,

        None6 = 600,

        None7 = 700,

        None8 = 800,

        None9 = 900,
    }
    #endregion

    #region ClientCode
    public enum ClientCode
    {
        None1 = 100,

        None2 = 200,

        None3 = 300,

        None4 = 400,

        None5 = 500,

        None6 = 600,

        Equipment = 700,
        Equipment_RecognitionError = 701,

        None8 = 800,

        None9 = 900,
    }
    #endregion
}
