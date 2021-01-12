using Domain.Infrastructure.Messaging.HTTP.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Skeleton.Http.Helper
{
     public static class HttpMetadataHelper
    {
        public static Uri RebuildUriWithUploadInfo(Uri uri, object info)
        {
            UriBuilder builder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (PropertyInfo propertyInfo in info.GetType().GetProperties())
            {
                HttpQuery attribute = propertyInfo.GetCustomAttribute<HttpQuery>();
                if (attribute != null)
                {
                    query[attribute.Name] = propertyInfo.GetValue(info).ToString();
                }
            }
            builder.Query = query.ToString();
            return builder.Uri;
        }

        public static void AddHeaderValuesWithUploadInfo(HttpRequestHeaders headers, object info)
        {
            foreach (PropertyInfo propertyInfo in info.GetType().GetProperties())
            {
                HttpHeader attribute = propertyInfo.GetCustomAttribute<HttpHeader>();
                if (attribute != null)
                {
                    headers.TryAddWithoutValidation(attribute.Name, propertyInfo.GetValue(info).ToString());
                }
            }
        }

        public static Uri RebuildUriWithParams(Uri uri, Dictionary<string, string> parameters)
        {
            UriBuilder uriBuilder = new UriBuilder(uri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                query[parameter.Key] = parameter.Value;
            }
            uriBuilder.Query = query.ToString();
            return uriBuilder.Uri;
        }


        public static void SetObjectValuesFromHeaders(object obj, HttpHeaders headers)
        {
            foreach (PropertyInfo propInfo in obj.GetType().GetProperties())
            {
                HttpHeader attr = propInfo.GetCustomAttribute<HttpHeader>();
                if (null != attr)
                {
                    IEnumerable<string> crtHeaderValues;
                    headers.TryGetValues(attr.Name, out crtHeaderValues);
                    if (crtHeaderValues != null && crtHeaderValues.Any())
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(propInfo.PropertyType) && propInfo.PropertyType.IsGenericType)
                            propInfo.SetValue(obj, Convert.ChangeType(crtHeaderValues, propInfo.PropertyType), null);
                        else
                        {
                            propInfo.SetValue(obj, Convert.ChangeType(crtHeaderValues.FirstOrDefault(), propInfo.PropertyType), null);
                        }
                    }
                }
            }
        }
    }
}
