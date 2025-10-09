﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_CIRRO.Shared.Common;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public ApiResponse(int statusCode, string message, T? data = default, object? errors = null)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
        Errors = errors;
    }
}
