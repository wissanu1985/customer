using Application.Features.Customers.Queries.SearchCustomers;
using Mediator;
using Microsoft.AspNetCore.Components;
using WebUi.Services;

namespace WebUi.Components.Pages;

public partial class Home
{
    [Inject] private ScopedMediator Mediator { get; set; } = default!;

    private int totalCustomers;
    private List<CustomerSearchItem> recentCustomers = new();
    private bool loading = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        try
        {
            var result = await Mediator.Send(new Request(
                null, null, null, null, null, null, null, 1, 1));

            if (result.IsSuccess && result.Data is not null)
            {
                totalCustomers = result.Data.Total;
            }

            var recent = await Mediator.Send(new Request(
                null, null, null, null, null, null, null, 1, 5));

            if (recent.IsSuccess && recent.Data is not null)
            {
                recentCustomers = recent.Data.Data.ToList();
            }
        }
        finally
        {
            loading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
