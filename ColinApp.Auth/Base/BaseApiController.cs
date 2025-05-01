using ColinApp.Common.BaseController;
using Microsoft.AspNetCore.Mvc;

namespace ColinApp.Auth.Base
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// 返回成功（同步）
        /// </summary>
        protected IActionResult OkResponse(object? data = null, string msg = "成功")
        {
            return Ok(new ApiResponse
            {
                Code = 0,
                Msg = msg,
                Data = data
            });
        }

        /// <summary>
        /// 返回失败（同步）
        /// </summary>
        protected IActionResult FailResponse(string msg = "操作失败", int code = -1)
        {
            return Ok(new ApiResponse
            {
                Code = code,
                Msg = msg,
                Data = null
            });
        }

        /// <summary>
        /// 返回成功（异步）
        /// </summary>
        protected Task<IActionResult> OkResponseAsync(object? data = null, string msg = "成功")
        {
            return Task.FromResult(OkResponse(data, msg));
        }

        /// <summary>
        /// 返回失败（异步）
        /// </summary>
        protected Task<IActionResult> FailResponseAsync(string msg = "操作失败", int code = -1)
        {
            return Task.FromResult(FailResponse(msg, code));
        }

        /// <summary>
        /// 条件结果返回（异步）
        /// </summary>
        protected Task<IActionResult> ResultAsync(bool success, object? data = null, string? successMsg = null, string? failMsg = null, int failCode = -1)
        {
            return success
                ? OkResponseAsync(data, successMsg ?? "成功")
                : FailResponseAsync(failMsg ?? "失败", failCode);
        }
    }
}
