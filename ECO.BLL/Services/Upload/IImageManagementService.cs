using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.BLL.Services.Upload
{
    public interface IImageManagementService
    {
        public Task<List<string>> AddImageAsync(IFormFileCollection files, string src);
        Task DeleteImageAsync(string src);
    }
}
