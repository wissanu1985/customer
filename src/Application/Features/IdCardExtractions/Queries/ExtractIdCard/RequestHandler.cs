using Application.Commons.Exceptions;
using Application.Commons.Services;
using Application.Commons.Wrappers;
using Mediator;
using System.Net;

namespace Application.Features.IdCardExtractions.Queries.ExtractIdCard;

public sealed class RequestHandler : IRequestHandler<Request, Result<Response>>
{
    private readonly ITyphoonOcrService _ocrService;
    private readonly ITyphoonChatService _chatService;

    public RequestHandler(ITyphoonOcrService ocrService, ITyphoonChatService chatService)
    {
        _ocrService = ocrService;
        _chatService = chatService;
    }

    public async ValueTask<Result<Response>> Handle(Request request, CancellationToken cancellationToken)
    {
        try
        {
            var markdown = await _ocrService.OcrAsync(request.ImageBytes, request.FileName, cancellationToken);
            var data = await _chatService.ExtractIdCardAsync(markdown, cancellationToken);
            return Result<Response>.Success(new Response { Data = data }, statusCode: HttpStatusCode.OK);
        }
        catch (TyphoonOcrException ex) when (ex.StatusCode == 429)
        {
            return Result<Response>.Failure("ระบบยุ่ง กรุณารอสักครู่แล้วลองใหม่", HttpStatusCode.TooManyRequests);
        }
        catch (TyphoonOcrException)
        {
            return Result<Response>.Failure("ไม่สามารถประมวลผล OCR ได้ กรุณาลองใหม่", HttpStatusCode.BadGateway);
        }
        catch (Exception)
        {
            return Result<Response>.Failure("ไม่สามารถประมวลผลข้อมูลบัตรได้ กรุณาลองใหม่", HttpStatusCode.InternalServerError);
        }
    }
}
