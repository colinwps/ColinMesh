using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColinApp.Common.BaseController
{
    public class ApiResponse
    {
        public int Code { get; set; }          // 0：成功，其它为错误码
        public string Msg { get; set; } = "";  // 提示信息
        public object? Data { get; set; }      // 返回数据（可以是任意类型）

        public static ApiResponse Success(object? data = null, string msg = "成功") =>
            new ApiResponse { Code = 0, Msg = msg, Data = data };

        public static ApiResponse Fail(string msg = "操作失败", int code = -1) =>
            new ApiResponse { Code = code, Msg = msg };
    }
}
