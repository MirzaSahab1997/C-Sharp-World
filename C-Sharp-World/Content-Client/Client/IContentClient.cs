using IdentityVerificationService.Domain.DTOs;
using Refit;

namespace IdentityVerificationService.Content_Client.Client
{
    public interface IContentClient
    {
        [Multipart]
        [Post(path: "/upload?directory={directory}")]
        Task<Response<List<string>>> UploadDocuments([AliasAs("file")] IEnumerable<StreamPart> streams, string directory);

        [Get("")]
        Task<Response<List<string>>> GetDocuments(string prefix);
    }
}
