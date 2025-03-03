using Data;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UniRx;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
public class DeviceManager
{
    public Vector2 targetResolution = new Vector2(1280, 720);
    public const float TableRatio = 1.7f;

    public Vector2 originResolution
    {
        get { return new Vector2(Screen.width, Screen.height); }
    }

    public Vector2 canvasSize = new Vector2(1280, 720);
    public bool TestSafeArea = false;
    private string signatureHash = "";


    public Rect GetSafeArea()
    {
        if (TestSafeArea)
        {
            var normal = new Rect(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f);
            return new Rect(Screen.width * normal.x, Screen.height * normal.y, Screen.width * normal.width, Screen.height * normal.height);
        }

#if UNITY_EDITOR_IPHONEX
        var normal = new Rect(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f);
        return new Rect(Screen.width * normal.x, Screen.height * normal.y, Screen.width * normal.width, Screen.height * normal.height);
#elif UNITY_ANDROID
        return new Rect(0, 0, Screen.width, Screen.height);
#else
        return Screen.safeArea;
#endif
    }

    public bool IsIPadDevice()
    {
#if UNITY_EDITOR_IPAD
        return true;
#elif UNITY_IOS
        return UnityEngine.iOS.Device.generation.ToString().Contains("iPad");
#else
        return false;
#endif
    }

    public bool IsIOSDeviceLowSpecification()
    {
#if UNITY_IOS
        //DeviceGeneration generation = UnityEngine.iOS.Device.generation;
        //switch (generation)
        //{
        //    case DeviceGeneration.iPhone:
        //    case DeviceGeneration.iPhone3G:
        //    case DeviceGeneration.iPhone3GS:
        //    case DeviceGeneration.iPodTouch1Gen:
        //    case DeviceGeneration.iPodTouch2Gen:
        //    case DeviceGeneration.iPodTouch3Gen:
        //    case DeviceGeneration.iPad1Gen:
        //    case DeviceGeneration.iPhone4:
        //    case DeviceGeneration.iPodTouch4Gen:
        //    case DeviceGeneration.iPad2Gen:
        //    case DeviceGeneration.iPhone4S:
        //    case DeviceGeneration.iPad3Gen:
        //    case DeviceGeneration.iPhone5:
        //    case DeviceGeneration.iPodTouch5Gen:
        //    case DeviceGeneration.iPadMini1Gen:
        //    case DeviceGeneration.iPad4Gen:
        //    case DeviceGeneration.iPhone5C:
        //    case DeviceGeneration.iPhone5S:
        //    case DeviceGeneration.iPadAir1:
        //    case DeviceGeneration.iPadMini2Gen:
        //    case DeviceGeneration.iPhone6:
        //    case DeviceGeneration.iPhone6Plus:
        //    case DeviceGeneration.iPadMini3Gen:
        //        return true;
        //}

#endif
        return false;
    }


    private Version version = null;
    private int prevWidth;
    private int prevHeight;
    public float screenSafeAreaRatio = 1;
    public float canvasMatchWidthOrHeight = 1;
    public Subject<Unit> OnChangeScreenSize = new Subject<Unit>();
    public bool IOS13Upper => this.version != null ? this.version >= new Version("13.0") : false;
    public bool IOS14Upper => this.version != null ? this.version >= new Version("14.0") : false;

    public int marketVer = 0;
    public string[] marketVerList;
    public int guildWidth = 0;
    public int rankHeight = 0;
    public void Init()
    {
#if !UNITY_EDITOR && UNITY_IOS
        this.version = new Version(Device.systemVersion);
        Debug.Log("IOS Device Version : " + Device.systemVersion);
        Debug.Log("IOS Version : " + this.version.ToString());
        Debug.Log("Version 13 Upper ?  : " + IOS13Upper.ToString());
#endif

        InitializeDeviceInfo();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_EDITOR
        Application.runInBackground = true;
#endif

        MainThreadDispatcher.UpdateAsObservable().Subscribe(_1 =>
        {
            if (prevWidth != Screen.width || prevHeight != Screen.height)
            {
                prevWidth = Screen.width;
                prevHeight = Screen.height;

                float screenRatio = Managers.Device.originResolution.x / Managers.Device.originResolution.y;
                float targetRatio = Managers.Device.targetResolution.x / Managers.Device.targetResolution.y;

                var graterWidth = screenRatio >= targetRatio || float.Epsilon > Mathf.Abs(screenRatio - targetRatio);

                {
                    canvasMatchWidthOrHeight = graterWidth ? 1f : 0f;
                    if (canvasMatchWidthOrHeight == 1)
                    {
                        if (Screen.height <= Screen.safeArea.height)
                            screenSafeAreaRatio = 1;
                        else
                            screenSafeAreaRatio = Screen.height * 1f / Screen.safeArea.height;
                    }
                    else
                    {
                        if (Screen.width <= Screen.safeArea.width)
                            screenSafeAreaRatio = 1;
                        else
                            screenSafeAreaRatio = Screen.width * 1f / Screen.safeArea.width;
                    }
                }
                {
                    if (graterWidth) //°¡·Î°¡ ±è
                    {
                        var ratio = Screen.height * 1f / 720;
                        canvasSize = new Vector2(Screen.width / ratio * Managers.Device.screenSafeAreaRatio,
                            720 * Managers.Device.screenSafeAreaRatio);
                    }
                    else //¼¼·Î°¡ ±è
                    {
                        var ratio = Screen.width * 1f / 1280;
                        canvasSize = new Vector2(1280 * Managers.Device.screenSafeAreaRatio,
                            Screen.height / ratio * Managers.Device.screenSafeAreaRatio);
                    }
                }
                OnChangeScreenSize.OnNext(Unit.Default);
            }
        });

    }

//    public void GetLevel2()
//    {
//#if UNITY_ANDROID && UNITY_EDITOR == false
//        marketVerList = DetectNormal();
//        if(marketVerList.IsNullOrEmpty() == false)
//        {
//            foreach (var item in marketVerList)
//            {
//                Debug.Log($"GetLevel2 : {item}");
//            }
//        }
//#endif
//    }

//    public void GetLevel()
//    {
//#if UNITY_ANDROID && UNITY_EDITOR == false
//        var string1 = Managers.ServiceInfo.GetServerString(304);
//        if (string1.IsNullOrEmpty() == false)
//        {
//            var string2 = string1.Replace(" ", string.Empty);
//            string[] split = string2.Split(',');

//            foreach (string mit in split)
//            {
//                Debug.Log($"Getvipenum : {mit}");
//                marketVer = Getvipenum(mit);
//                guildWidth = Getvipenum(mit);
//                rankHeight = Getvipenum(mit);
//                if (marketVer > 0)
//                    break;
//            }
//        }
//#endif
//    }
  

    public void Quit()
    {
#if UNITY_ANDROID
        AndroidJavaClass ajc = new AndroidJavaClass("com.lancekun.quit_helper.AN_QuitHelper");
        AndroidJavaObject UnityInstance = ajc.CallStatic<AndroidJavaObject>("Instance");
        UnityInstance.Call("AN_Exit");
#else
        ApplicationQuit();
#endif
    }

    public void ApplicationQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        Debug.Log("QUIT SERVER APPLICATION QUIT");
    }

    private const float ASPECT_16_9 = 16.0f / 9.0f;
    private const float ASPECT_16_10 = 16.0f / 10.0f;
    private const float ASEPCT_4_3 = 4.0f / 3.0f;
    private const int BEST_TARGET_HEIGHT_3 = 1080;
    private const int BEST_TARGET_HEIGHT_2 = 900;
    private const int BEST_TARGET_HEIGHT_1 = 720;
    private const int BEST_TARGET_HEIGHT_0 = 540;

    public void SetScreenResolution(long value = -1)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;

        long optionResolution = value;

        var height = BEST_TARGET_HEIGHT_3;
        switch (optionResolution)
        {
            case 0:
                height = BEST_TARGET_HEIGHT_0;
                break;
            case 1:
                height = BEST_TARGET_HEIGHT_1;
                break;
            case 2:
                height = BEST_TARGET_HEIGHT_2;
                break;
            case 3:
                height = BEST_TARGET_HEIGHT_3;
                break;
            default:
                break;
        }


        int resolutionWidth = Mathf.RoundToInt(screenAspect * height);
        int resolutionHeight = height;
        Screen.SetResolution(resolutionWidth, resolutionHeight, true);
        Debug.Log($"Screen Resolution ({resolutionWidth},{resolutionHeight})");
    }


    public string GetCertificateSHA1Fingerprint()
    {
#if UNITY_ANDROID && UNITY_EDITOR == false
       AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager");
        string packageName = Application.identifier;
        int GET_SIGNATURES = packageManager.GetStatic<int>("GET_SIGNATURES");
        AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, GET_SIGNATURES);
        AndroidJavaObject[] signatures = packageInfo.Get<AndroidJavaObject[]>("signatures");
        string signaturesHash = signatures[0].Call<int>("hashCode").ToString("X");
        return signaturesHash;
#else
        return "";
#endif
    }

    public string ToMD5(string strToEncrypt)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(strToEncrypt));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public void InitializeDeviceInfo()
    {
        string sha1 = GetCertificateSHA1Fingerprint();
        if (sha1.IsNullOrEmpty() == false)
        {
            signatureHash = ToMD5(sha1);
        }
    }

    public string JoFP()
    {
        return signatureHash;
    }
#if UNITY_IOS
    private class PInvoke
    {
        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern string GetMobileCountryCode();
    }
#endif
    //     public string GetTelephonyCountry()
    //     {
    //         Debug.Log("GetTelephonyCountry");
    //         try
    //         {
    // #if UNITY_ANDROID
    //             AndroidJavaObject AOSUnityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
    //             if (AOSUnityActivity == null) return "";
    //             Debug.Log("GetTelephonyCountry1");
    //
    //             AndroidJavaObject pServiceObj = AOSUnityActivity.Call<AndroidJavaObject>("getSystemService", "phone");
    //             if (pServiceObj == null) return "";
    //             Debug.Log("GetTelephonyCountry2");
    //             AndroidJavaObject pDest = new AndroidJavaObject("android.telephony.TelephonyManager");
    //             if (pDest == null) return "";
    //             Debug.Log("GetTelephonyCountry3");
    //             var str = pDest.Call<string>("getNetworkCountryIso");
    //             Debug.Log("GetTelephonyCountry4");
    //             return str.ToUpper(CultureInfo.InvariantCulture);
    // #elif UNITY_IOS
    //             string code = PInvoke.GetMobileCountryCode();
    //             if (code.IsNullOrEmpty())
    //             {
    //                 return "";
    //             }
    //             return code.ToUpper(CultureInfo.InvariantCulture);
    // #endif
    //         }
    //         catch (Exception e)
    //         {
    //             Debug.Log(e.Message);
    //         }
    //
    //         return "";
    //     }


    public string GetIPCountryCode(int timeout = 1000)
    {
        string url = "https://ip2c.org/self";
        string ret = string.Empty;

        //Managers.SendGetRequest(url, timeout, (x) =>
        //{
        //    Debug.Log("SendPostRequest : " + x.downloadHandler.text);
        //    Debug.Log("[TEST LOG] get ip2c response " + x.downloadHandler.text);
        //    if (x.downloadHandler.text.IsNullOrEmpty() == false)
        //        ret = x.downloadHandler.text.Split(';').ElementAtOrDefault(1);
        //});

        return ret;
    }
}