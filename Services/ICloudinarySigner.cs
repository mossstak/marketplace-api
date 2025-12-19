using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceApi.Services
{
    public interface ICloudinarySigner
    {
        object CreateUploadSignature(string folder, string? publicId = null);
    }
}