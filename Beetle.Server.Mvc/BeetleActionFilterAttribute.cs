using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Beetle.Server.Mvc.Properties;

namespace Beetle.Server.Mvc {

    /// <summary>
    /// Imitate OData behavior for MVC Controllers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class BeetleActionFilterAttribute : ActionFilterAttribute {
        private BeetleConfig _beetleConfig;
        private bool? _checkRequestHash;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeetleActionFilterAttribute" /> class.
        /// </summary>
        /// <param name="configType">Type of the config.</param>
        /// <exception cref="System.ArgumentException">Cannot create config instance.</exception>
        public BeetleActionFilterAttribute(Type configType = null) {
            if (configType != null) {
                _beetleConfig = Activator.CreateInstance(configType) as BeetleConfig;
                if (_beetleConfig == null)
                    throw new ArgumentException(Resources.CannotCreateConfigInstance);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeetleActionFilterAttribute"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public BeetleActionFilterAttribute(BeetleConfig config) {
            _beetleConfig = config;
        }

        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <exception cref="BeetleException">BeetleActionFilterAttribute should only be applied to Queryable actions.</exception>
        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            var controller = filterContext.Controller;
            var action = filterContext.ActionDescriptor;
            var parameters = filterContext.ActionParameters;

            var service = controller as IBeetleService;
            if (_beetleConfig == null)
                _beetleConfig = service != null ? service.BeetleConfig : BeetleConfig.Instance;

            string queryString;
            NameValueCollection queryParams;
            // handle request message
            GetParameters(filterContext, out queryString, out queryParams);

            base.OnActionExecuting(filterContext);

            // execute the action method
            var actionMethod = controller.GetType().GetMethod(action.ActionName);
            var contentValue = actionMethod.Invoke(controller, parameters.Values.ToArray());
            var contentTaskValue = contentValue as Task;
            if (contentTaskValue != null) {
                contentTaskValue.Wait();
                var taskType = contentTaskValue.GetType();
                var resultProperty = taskType.GetProperty("Result");
                contentValue = resultProperty == null ? null : resultProperty.GetValue(contentTaskValue);
            }
            // get client hash
            // process the request and return the result
            var actionContext = new ActionContext(action.ActionName, contentValue, queryString, queryParams, MaxResultCount, CheckRequestHashNullable);
            var processResult = ProcessRequest(contentValue, actionContext, service);
            // handle response message
            filterContext.Result = HandleResponse(filterContext, processResult);
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <param name="queryString">The query string.</param>
        /// <param name="queryParams">The query parameters.</param>
        protected virtual void GetParameters(ActionExecutingContext filterContext, out string queryString, out NameValueCollection queryParams) {
            Helper.GetParameters(out queryString, out queryParams, _beetleConfig, filterContext.ActionDescriptor.GetParameters(), filterContext.ActionParameters);
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        /// <param name="contentValue">The content value.</param>
        /// <param name="actionContext">The action context.</param>
        /// <param name="service">The service.</param>
        /// <returns></returns>
        protected virtual ProcessResult ProcessRequest(object contentValue, ActionContext actionContext, IBeetleService service) {
            return service != null
                ? service.ProcessRequest(contentValue, actionContext)
                : Helper.ProcessRequest(contentValue, actionContext);
        }

        /// <summary>
        /// Handles the response.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        protected virtual ActionResult HandleResponse(ActionExecutingContext filterContext, ProcessResult result) {
            return Helper.HandleResponse(result, _beetleConfig);
        }

        /// <summary>
        /// Gets the beetle config.
        /// </summary>
        /// <value>
        /// The beetle config.
        /// </value>
        protected BeetleConfig BeetleConfig {
            get { return _beetleConfig; }
        }

        /// <summary>
        /// Gets or sets the maximum result count.
        /// </summary>
        /// <value>
        /// The maximum result count.
        /// </value>
        public int MaxResultCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [check request hash].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [check request hash]; otherwise, <c>false</c>.
        /// </value>
        public bool CheckRequestHash {
            get { return _checkRequestHash.GetValueOrDefault(); }
            set { _checkRequestHash = value; }
        }

        /// <summary>
        /// Gets the check request hash nullable.
        /// </summary>
        /// <value>
        /// The check request hash nullable.
        /// </value>
        internal bool? CheckRequestHashNullable {
            get { return _checkRequestHash; }
        }
    }
}
