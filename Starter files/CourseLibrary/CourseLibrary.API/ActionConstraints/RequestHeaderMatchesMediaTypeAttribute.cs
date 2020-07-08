using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionConstraints
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly MediaTypeCollection _mediaTypes = new MediaTypeCollection();
        private readonly string _requestHeaderToMatch;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch, string mediaType,
            params string[] otherMediaTypes)
        {
            _requestHeaderToMatch =
                requestHeaderToMatch ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));

            // check if the inputted media types are valid media types
            // and add them to the _mediaTypes collection

            if (MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType))
                _mediaTypes.Add(parsedMediaType);
            else
                throw new ArgumentException(nameof(mediaType));

            foreach (var otherMediaType in otherMediaTypes)
                if (MediaTypeHeaderValue.TryParse(otherMediaType, out var parsedOtherMediaType))
                    _mediaTypes.Add(parsedOtherMediaType);
                else
                    throw new ArgumentException(nameof(mediaType));
        }

        public bool Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
            if (!requestHeaders.ContainsKey(_requestHeaderToMatch)) return false;

            var parseRequestMediaType = new MediaType(requestHeaders[_requestHeaderToMatch]);

            // if one of the media types matches, return true
            foreach (var mediaType in _mediaTypes)
            {
                var parseMediaType = new MediaType(mediaType);
                if (parseRequestMediaType.Equals(parseMediaType)) return true;
            }

            return false;
        }

        public int Order => 0;
    }
}