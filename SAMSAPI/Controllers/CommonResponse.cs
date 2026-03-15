using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SAMSAPI.Areas.Common;
namespace SAMSAPI.Controllers
{
    public class CommonResponse : ApiController
    {
        public Object getDataResponse(Object obj, int totalCount, ResponseCodes statusCode = ResponseCodes.SUCCESS, string title = "", string message = "")
        {
            DataResponse dataResponse = new DataResponse();
            dataResponse.data = obj;
            dataResponse.totalCount = totalCount;

            List<SuccessErrorResponse> ListsEResponse = new List<SuccessErrorResponse>();
            SuccessResponse successResponse = new SuccessResponse();
            SuccessErrorResponse sEResponse = new SuccessErrorResponse();
            sEResponse.code = statusCode;
            sEResponse.title = title;
            sEResponse.message = message;
            ListsEResponse.Add(sEResponse);
            dataResponse.success = ListsEResponse;

            //dataResponse.success = getSuccessResponse(statusCode, title, message);
            return dataResponse;
        }

        public Object getSuccessResponse(ResponseCodes statusCode, string title, string message)
        {
            List<SuccessErrorResponse> ListsEResponse = new List<SuccessErrorResponse>();
            SuccessResponse successResponse = new SuccessResponse();
            SuccessErrorResponse sEResponse = new SuccessErrorResponse();
            sEResponse.code = statusCode;
            sEResponse.title = title;
            sEResponse.message = message;
            ListsEResponse.Add(sEResponse);
            successResponse.success = ListsEResponse;
            return successResponse;
        }

        public Object getErrorResponse(ResponseCodes statusCode, string title, string message)
        {
            List<SuccessErrorResponse> ListsEResponse = new List<SuccessErrorResponse>();
            ErrorResponse errorResponse = new ErrorResponse();
            SuccessErrorResponse sEResponse = new SuccessErrorResponse();
            sEResponse.code = statusCode;
            sEResponse.title = title;
            sEResponse.message = message;
            ListsEResponse.Add(sEResponse);
            errorResponse.error = ListsEResponse;
            return errorResponse;
        }
    }

    public class SuccessResponse
    {
        public Object success = new Object();

    }

    public class ErrorResponse
    {
        public Object error = new Object();
    }

    public class DataResponse
    {
        public Object data { get; set; }
        public int totalCount { get; set; }
        public Object success { get; set; }

        public DataResponse()
        {
            data = new object();
            totalCount = 0;
            success = new object();
        }
    }

    public class DataResponseWithoutCount
    {
        public Object data = new Object();
    }

    public class SuccessErrorResponse
    {
        public ResponseCodes code
        {
            get;
            set;
        }
        public string title
        {
            get;
            set;
        }

        public string message
        {
            get;
            set;
        }
    }
}
