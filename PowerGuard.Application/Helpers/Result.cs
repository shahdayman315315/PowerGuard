using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerGuard.Application.Helpers
{
    public  class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public static Result<T> Success(T Data, string Message = "Success")
            => new Result<T> { IsSuccess = true, Data = Data, Message = Message };

        public static Result<T> Failure(string Message, int StatusCode = 400)
            => new Result<T> { IsSuccess = false, Message = Message, StatusCode = StatusCode };
    }
}
