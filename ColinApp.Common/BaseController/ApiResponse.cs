using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColinApp.Common.BaseController
{
    public class ApiResponse<T>
    {
        public ApiResponse(int code, string msg, T data)
        {
            Code = code;
            Msg = msg;
            Data = data;
        }

        public int Code { get; set; }
        public string Msg { get; set; }
        public T Data { get; set; }

        // 成功响应
        public static ApiResponse<T> Success(T data) => new ApiResponse<T>(200, "OK", data);

        // 失败响应
        public static ApiResponse<T> Fail(string msg = "fail") => new ApiResponse<T>(500, msg, default);

    }
}
