using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace MarketPlaceApi.Services
{
    public class CloudinaryOptions
{
    public string CloudName { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string ApiSecret { get; set; } = default!;
}

public class CloudinarySigner : ICloudinarySigner
{
    private readonly Cloudinary _cloudinary;

    public CloudinarySigner(IOptions<CloudinaryOptions> options)
    {
        var o = options.Value;
        var account = new Account(o.CloudName, o.ApiKey, o.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public object CreateUploadSignature(string folder, string? publicId = null)
    {
        // Timestamp is required for signed uploads
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var parameters = new SortedDictionary<string, object>
        {
            ["timestamp"] = timestamp,
            ["folder"] = folder,
        };

        if (!string.IsNullOrWhiteSpace(publicId))
            parameters["public_id"] = publicId;

        // Signature is computed using your API secret (server only)
        var signature = _cloudinary.Api.SignParameters(parameters);

        return new
        {
            timestamp,
            signature,
            apiKey = _cloudinary.Api.Account.ApiKey,
            cloudName = _cloudinary.Api.Account.Cloud,
            folder,
            publicId
        };
    }
}
}
