using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.Text;

namespace LandlordServer.Data
{
    public class OSSManager
    {
        static OssClient oss;
        /// <summary>
        /// 个人存储库，请勿胡乱使用
        /// </summary>
        static string accessKeyId = "LTAIps8wkImlvvAo";
        static string accessKeySecret = "SMMoNOlZ16k943tbFHhKmQLm0X6Upx";
        static string securityToken;
        static string folder = "security/";
        static string DllPath;
        static string UIPath;
        public static string GetDllUri()
        {
            if (DllPath != null)
                return DllPath;
            if(oss==null)
            {
                string Endpoint = "oss-cn-shenzhen.aliyuncs.com";
                oss = new OssClient(Endpoint, accessKeyId, accessKeySecret);
            }
            var uri = oss.GeneratePresignedUri("huqiang1990", folder+ "HotfixDll");
            DllPath = uri.AbsoluteUri;
            return DllPath;
        }
        public static string GetUIUri()
        {
            if (UIPath != null)
                return UIPath;
            if (oss == null)
            {
                string Endpoint = "oss-cn-shenzhen.aliyuncs.com";
                oss = new OssClient(Endpoint, accessKeyId, accessKeySecret);
            }
            var uri = oss.GeneratePresignedUri("huqiang1990", folder + "HotfixUI");
            UIPath= uri.AbsoluteUri;
            return UIPath;
        }
    }
}
