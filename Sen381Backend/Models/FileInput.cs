using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Sen381Backend.Models
{
    public class FileInput
    {
        public IFormFile File { get; set; }
        public string? Name { get; set; }
    }
}

