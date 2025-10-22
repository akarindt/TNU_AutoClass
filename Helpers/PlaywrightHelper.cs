using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Playwright;

namespace TNU_AutoClass.Helpers;
public sealed class PlaywrightBrowserSession : IAsyncDisposable
{
    public PlaywrightBrowserSession(IPlaywright playwright, IBrowser browser)
    {
        Playwright = playwright;
        Browser = browser;
    }

    public IPlaywright Playwright { get; }
    public IBrowser Browser { get; }

    public async ValueTask DisposeAsync()
    {
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}

public sealed class PlaywrightRequestSession : IAsyncDisposable
{
    public PlaywrightRequestSession(IPlaywright playwright, IAPIRequestContext request)
    {
        Playwright = playwright;
        Request = request;
    }

    public IPlaywright Playwright { get; }
    public IAPIRequestContext Request { get; }

    public async ValueTask DisposeAsync()
    {
        await Request.DisposeAsync();
        Playwright.Dispose();
    }
}

public class PlaywrightHelper
{
    public async Task<PlaywrightBrowserSession> InitBrowserAsync(BrowserTypeLaunchOptions? launchOptions = null)
    {
        IPlaywright playwright = await Playwright.CreateAsync();
        IBrowser browser =
            await playwright.Chromium.LaunchAsync(launchOptions ?? new BrowserTypeLaunchOptions { Headless = false });
        return new PlaywrightBrowserSession(playwright, browser);
    }

    public async Task<PlaywrightRequestSession> InitRequestAsync(APIRequestNewContextOptions? apiRequestOptions = null)
    {
        IPlaywright playwright = await Playwright.CreateAsync();
        IAPIRequestContext request =
            await playwright.APIRequest.NewContextAsync(apiRequestOptions ?? new APIRequestNewContextOptions());
        return new PlaywrightRequestSession(playwright, request);
    }
}
