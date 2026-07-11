using CoparentHub.Application.Features.Documents;
using CoparentHub.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoparentHub.Api.Controllers
{
    [Route("api/families/{familyId:guid}/documents")]
    [Authorize]
    public class DocumentsController(ISender sender) : ApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(Guid familyId, CancellationToken ct)
            => ToResponse(await sender.Send(new GetDocumentsQuery(familyId, CurrentUserId), ct));

        [HttpPost]
        [RequestSizeLimit(11_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 11_000_000)]
        public async Task<IActionResult> Upload(Guid familyId, [FromForm] UploadDocumentRequest req, CancellationToken ct)
        {
            await using var stream = new MemoryStream();
            await req.File.CopyToAsync(stream, ct);

            return ToResponse(await sender.Send(
                new UploadDocumentCommand(
                    familyId,
                    CurrentUserId,
                    req.ChildId,
                    req.File.FileName,
                    req.File.ContentType,
                    stream.ToArray(),
                    req.Category,
                    req.Description), ct));
        }

        [HttpGet("{documentId:guid}/content")]
        public async Task<IActionResult> GetContent(Guid familyId, Guid documentId, CancellationToken ct)
        {
            var result = await sender.Send(new GetDocumentContentQuery(familyId, documentId, CurrentUserId), ct);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            var content = result.Value!;
            return File(content.Content, content.ContentType, content.FileName);
        }

        [HttpDelete("{documentId:guid}")]
        public async Task<IActionResult> Remove(Guid familyId, Guid documentId, CancellationToken ct)
            => ToResponse(await sender.Send(new RemoveDocumentCommand(familyId, documentId, CurrentUserId), ct));
    }

    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; } = default!;
        public Guid? ChildId { get; set; }
        public DocumentCategory Category { get; set; }
        public string? Description { get; set; }
    }
}
