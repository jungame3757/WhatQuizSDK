using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Proyecto26;
using UnityEngine;
using FullSerializer;

namespace FirebaseWebGL.Scripts.FirebaseBridge
{
    public static class FirebaseFunctions
    {
        private static readonly fsSerializer _serializer = new fsSerializer();
        
        /// <summary>
        /// Calls an HTTP Firebase cloud function
        /// Returns the response of the request 
        /// </summary>
        /// <param name="functionName"> Name of the function to call </param>
        /// <param name="parameters"> Possible function parameters </param>
        /// <param name="callback"> Method to call when the operation was successful </param>
        /// <param name="fallback"> Method to call when the operation was unsuccessful </param>
        public static void CallCloudFunction(string functionName, Dictionary<string, string> parameters,
            Action<ResponseHelper> callback,
            Action<Exception> fallback)
        {
            var projectId = GetCurrentProjectId();
            
            // Dictionary를 fsData로 변환
            Dictionary<string, fsData> fsDataDict = new Dictionary<string, fsData>();
            foreach (var kvp in parameters)
            {
                fsDataDict[kvp.Key] = new fsData(kvp.Value);
            }
            
            // fsData를 JSON 문자열로 변환
            string jsonBody = fsJsonPrinter.CompressedJson(new fsData(fsDataDict));
            Debug.Log("요청 데이터: " + jsonBody);
            
            RestClient.Request(new RequestHelper
            {
                Method = "POST",
                Uri = $"https://auth-zuwrenkajq-du.a.run.app/{functionName}",
                Headers = new Dictionary<string, string>
                {
                    {"Access-Control-Allow-Origin", "*"},
                    {"Content-Type", "application/json"}
                },
                BodyString = jsonBody
            }).Then(callback).Catch(fallback);
        }

        [DllImport("__Internal")]
        private static extern string GetCurrentProjectId();
    }
}