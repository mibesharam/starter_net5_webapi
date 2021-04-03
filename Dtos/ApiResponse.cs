using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web_Api.Dtos
{
    public class ApiResponse
    {
        public ApiResponse()
        {
            Success = false;
        }
        public bool Success { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }
}
