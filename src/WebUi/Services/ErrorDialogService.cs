using AntDesign;
using Microsoft.AspNetCore.Components;

namespace WebUi.Services;

/// <summary>
/// Global error dialog service — shows a modal popup when unexpected errors occur,
/// keeping the Blazor circuit alive instead of crashing the app.
/// </summary>
public sealed class ErrorDialogService
{
    private readonly ModalService _modalService;
    private readonly ILogger<ErrorDialogService> _logger;

    public ErrorDialogService(ModalService modalService, ILogger<ErrorDialogService> logger)
    {
        _modalService = modalService;
        _logger = logger;
    }

    public async Task ShowAsync(string title, Exception ex)
    {
        _logger.LogError(ex, "Unhandled error: {Title}", title);

        var message = ex switch
        {
            Microsoft.Data.SqlClient.SqlException sqlEx => "ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาตรวจสอบการเชื่อมต่อและลองใหม่อีกครั้ง",
            TimeoutException => "การดำเนินการใช้เวลานานเกินไป กรุณาลองใหม่อีกครั้ง",
            _ => ex.Message
        };

        var detail = ex.InnerException?.Message ?? ex.Message;

        await _modalService.ErrorAsync(new ConfirmOptions
        {
            Title = title,
            Width = 480,
            Content = BuildContent(message, detail),
            OkText = "รับทราบ"
        });
    }

    public async Task ShowAsync(string title, string message)
    {
        await _modalService.ErrorAsync(new ConfirmOptions
        {
            Title = title,
            Width = 480,
            Content = BuildContent(message, null),
            OkText = "รับทราบ"
        });
    }

    private static RenderFragment BuildContent(string message, string? detail) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "style", "padding: 8px 0;");

        builder.OpenElement(2, "p");
        builder.AddAttribute(3, "style", "font-size: 15px; margin-bottom: 8px;");
        builder.AddContent(4, message);
        builder.CloseElement();

        if (!string.IsNullOrWhiteSpace(detail))
        {
            builder.OpenElement(5, "details");
            builder.OpenElement(6, "summary");
            builder.AddAttribute(7, "style", "cursor: pointer; color: #888; font-size: 13px;");
            builder.AddContent(8, "รายละเอียดข้อผิดพลาด");
            builder.CloseElement(); // summary
            builder.OpenElement(9, "pre");
            builder.AddAttribute(10, "style", "white-space: pre-wrap; font-size: 12px; color: #c41e3a; margin-top: 8px;");
            builder.AddContent(11, detail);
            builder.CloseElement(); // pre
            builder.CloseElement(); // details
        }

        builder.CloseElement(); // div
    };
}
