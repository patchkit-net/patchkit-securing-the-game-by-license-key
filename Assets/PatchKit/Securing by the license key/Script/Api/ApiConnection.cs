using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using PatchKit.Api.Models.Main;
using PatchKit.SecuringByTheLicenseKey.Core;
using PatchKit.SecuringByTheLicenseKey.UI;
using UnityEngine;

namespace PatchKit.SecuringByTheLicenseKey.Api
{
public class ApiConnection
{
    private string _hostKeys = "keys2.patchkit.net";
    private string _hostApi = "api2.patchkit.net";
    private string _path = "/v2/keys/{key}";
    private AppSecret _appSecret;
    private InfoMessage _infoMessage;

    public ApiConnection(AppSecret appSecret, InfoMessage infoMessage)
    {
        _appSecret = appSecret;
        _infoMessage = infoMessage;
    }

    public bool CheckNetwork()
    {
        var checkNetwork = GetResponse("", "", _hostApi);
        return checkNetwork == null;
    }

    /// <summary>
    /// Gets detailes app info
    /// </summary>
    /// <param name="appSecret">Secret of an application.</param>
    public App GetAppInfo()
    {
        Debug.Log("Getting app info.");
        string path = "/1/apps/{app_secret}";
        path = path.Replace("{app_secret}", _appSecret.Value);
        string query = string.Empty;
        UnityHttpResponse response = GetResponse(path, query, _hostApi);

        return ParseResponse<App>(response);
    }

    public T ParseResponse<T>(UnityHttpResponse httpResponse)
    {
        try
        {
            var responseStream = httpResponse.ContentStream;
            var responseEncoding = Encoding.GetEncoding(httpResponse.CharacterSet);

            string body;
            using (var reader = new StreamReader(responseStream, responseEncoding))
            {
                body = reader.ReadToEnd();
            }

            Debug.Log("Body: '" + body);
            T response = JsonUtility.FromJson<T>(body);
            return response;
        }
        catch (Exception e)
        {
            Debug.Log("Couldn't deserialize: " + e);
        }

        return default(T);
    }

    public bool CheckStatusCode(UnityHttpResponse keyInfo)
    {
        if (keyInfo != null && keyInfo.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        if (keyInfo != null && keyInfo.StatusCode == HttpStatusCode.Gone)
        {
            _infoMessage.LabelMessage = "License key is blocked";
            _infoMessage.ContentMessage = "License key is blocked. Please contact with your developer!";
        }
        else if (keyInfo != null && keyInfo.StatusCode == HttpStatusCode.NotFound)
        {
            _infoMessage.LabelMessage = "License key is not found.";
            _infoMessage.ContentMessage = "License key is not found. Please enter the license key:";
        }
        else if (keyInfo != null && keyInfo.StatusCode == HttpStatusCode.Forbidden)
        {
            _infoMessage.LabelMessage = "License key validation service is not available";
            _infoMessage.ContentMessage = "License key validation service is not available. Try again.";
        }
        else
        {
            _infoMessage.LabelMessage = "Unrecognized server or API error";
            _infoMessage.ContentMessage =
                "Unrecognized server or API error. Please check your internet connection!";
        }

        return false;
    }

    public UnityHttpResponse GetKeyInfo(string key)
    {
        List<string> queryList = new List<string>();
        string path = _path;
        path = path.Replace("{key}", key);
        queryList.Add("app_secret=" + _appSecret.Value);

        string query = string.Join("&", queryList.ToArray());
        UnityHttpResponse response = GetResponse(path, query, _hostKeys);
        return response;
    }

    static private UnityHttpResponse GetResponse(string path, string query, string host)
    {
        try
        {
            Debug.Log("Getting response for path: '" + host + path + "' and query: '" + query + "'...");

            var uri = new UriBuilder
            {
                Scheme = "https",
                Host = host,
                Path = path,
                Query = query
            }.Uri;
            HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(uri);
            httpWebRequest.Timeout = 5000; //5s
            return Get(httpWebRequest);
        }
        catch (WebException e)
        {
            Debug.Log("Failed to get response. " + e);
            return null;
        }
    }

    static public UnityHttpResponse Get(HttpWebRequest getRequest)
    {
        var result = new WWWResult();

        using (var www = new WWW(getRequest.Address.ToString()))
        {
            while (!www.isDone)
            {
                // Wait
            }

            result.IsDone = www.isDone;
            result.ResponseHeaders = www.responseHeaders;
            result.Text = www.text;
        }

        lock (result)
        {
            if (!result.IsDone)
            {
                throw new WebException(
                    "Timeout after " + getRequest.Timeout,
                    WebExceptionStatus.Timeout);
            }

            var statusCode = ReadStatusCode(result);

            return new UnityHttpResponse(
                result.Text,
                statusCode);
        }
    }

    static private HttpStatusCode ReadStatusCode(WWWResult result)
    {
        if (result.ResponseHeaders == null ||
            !result.ResponseHeaders.ContainsKey("STATUS"))
        {
            // Based on tests, if response doesn't contain status it has probably timed out.
            throw new WebException("Timeout.", WebExceptionStatus.Timeout);
        }

        var status = result.ResponseHeaders["STATUS"];

        var s = status.Split(' ');

        int statusCode;

        if (s.Length < 3 || !int.TryParse(s[1], out statusCode))
        {
            throw new WebException("Timeout.", WebExceptionStatus.Timeout);
        }

        return (HttpStatusCode) statusCode;
    }
}
}