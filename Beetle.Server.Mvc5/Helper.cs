﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Beetle.Server.Mvc {

    public static class Helper {
        private static readonly Lazy<JavaScriptSerializer> _javaScriptSerializer = new Lazy<JavaScriptSerializer>();

        public static void GetParameters(out string queryString, out NameValueCollection queryParams,
                                         IBeetleConfig config = null,
                                         ParameterDescriptor[] parameterDescriptors = null, IDictionary<string, object> parameters = null,
                                         HttpRequest request = null) {
            if (config == null)
                config = BeetleConfig.Instance;
            if (request == null)
                request = HttpContext.Current.Request;

            if (request.HttpMethod == "POST") {
                object postData;
                // read post data
                request.InputStream.Position = 0;
                queryString = new StreamReader(request.InputStream).ReadToEnd();
                if (request.ContentType.Contains("application/json")) {
                    dynamic d = postData = config.Serializer.DeserializeToDynamic(queryString);
                    postData = d;
                    // now there is no query string parameters, we must populate them manually for beetle queries
                    // otherwise beetle cannot use query parameters when using post method
                    queryParams = new NameValueCollection();
                    if (d != null) {
                        foreach (var p in TypeDescriptor.GetProperties(d)) {
                            var v = d[p.Name];
                            queryParams.Add(p.Name, v == null ? string.Empty : v.ToString());
                        }
                    }
                }
                else {
                    var prms = request.Params;
                    queryParams = prms;
                    var d = prms.AllKeys.ToDictionary(k => k, k => prms[k]);
                    var jsonStr = _javaScriptSerializer.Value.Serialize(d);
                    postData = config.Serializer.DeserializeToDynamic(jsonStr);
                }

                // modify the action parameters to allow model binding to object, dynamic and json.net parameters
                if (parameterDescriptors != null && parameters != null) {
                    foreach (var pd in parameterDescriptors) {
                        var t = pd.ParameterType;
                        if (t == typeof(object) || pd.GetCustomAttributes(typeof(DynamicAttribute), false).Any()) {
                            parameters[pd.ParameterName] = postData;
                        }
                    }
                }
            }
            else {
                queryString = request.Url.Query;
                if (queryString.StartsWith("?"))
                    queryString = queryString.Substring(1);
                queryString = queryString.Replace(":", "%3A");

                queryParams = request.QueryString;
            }
        }

        public static ProcessResult ProcessRequest(object contentValue, ActionContext actionContext, IBeetleConfig actionConfig, IBeetleService service = null) {
            // beetle should be used for all content types except mvc actions results. so we check only if content is not an mvc action result
            if (!string.IsNullOrEmpty(actionContext.QueryString)) {
                bool checkHash;
                if (!actionContext.CheckRequestHash.HasValue)
                    checkHash = service != null && service.CheckRequestHash;
                else
                    checkHash = actionContext.CheckRequestHash.Value;

                if (checkHash)
                    CheckRequestHash(actionContext.QueryString);
            }

            // get beetle query parameters (supported parameters by default)
            var beetlePrms = Server.Helper.GetBeetleParameters(actionContext.QueryParameters);

            var contextHandler = service == null ? null : service.ContextHandler;
            // allow context handler to process the value
            if (contextHandler != null)
                return contextHandler.ProcessRequest(contentValue, beetlePrms, actionContext, actionConfig, service);

            return Server.Helper.DefaultRequestProcessor(contentValue, beetlePrms, actionContext, service, null, actionConfig);
        }

        public static ActionResult HandleResponse(ProcessResult processResult, IBeetleConfig config = null, HttpResponse response = null) {
            var result = processResult.Result;

            if (config == null)
                config = BeetleConfig.Instance;
            if (response == null)
                response = HttpContext.Current.Response;

            // set InlineCount header info if exists
            object inlineCount = processResult.InlineCount;
            if (inlineCount != null && response.Headers["X-InlineCount"] == null)
                response.Headers.Add("X-InlineCount", inlineCount.ToString());
            if (processResult.UserData != null && response.Headers["X-UserData"] == null) {
                var userDataStr = config.Serializer.Serialize(processResult.UserData);
                response.Headers.Add("X-UserData", userDataStr);
            }

            var actionResult = result as ActionResult;
            if (actionResult != null) return actionResult;

            // write the result to response content
            return new BeetleJsonResult(config, processResult) {
                Data = result,
                ContentEncoding = response.HeaderEncoding,
                ContentType = "application/json",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public static void CheckRequestHash(string queryString) {
            var request = HttpContext.Current.Request;

            var clientHash = request.Headers["x-beetle-request"];
            if (!string.IsNullOrEmpty(clientHash)) {
                var hashLenStr = request.Headers["x-beetle-request-len"];
                if (!string.IsNullOrEmpty(hashLenStr)) {
                    var queryLen = Convert.ToInt32(hashLenStr);
                    queryString = queryString.Substring(0, queryLen);
                    var serverHash = Server.Helper.CreateQueryHash(queryString).ToString(CultureInfo.InvariantCulture);

                    if (serverHash == clientHash)
                        return;
                }
            }

            throw new BeetleException(Server.Properties.Resources.AlteredRequestException);
        }
    }
}
