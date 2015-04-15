# owin.themable-errorpage
Themable error page for OWIN, based on Microsoft.Owin.Diagnostics.ErrorPage

### Usage
#### Basic initialization
The themable error page is initialized like a normal OWIN middleware in `Startup.cs`.  
By default it will use the Razor file located at `~/Views/Shared/Error.cshtml`.

```csharp
public void Configuration(IAppBuilder app)
{
    ...
    app.UseThemableErrorPage();
    ...
}
```

#### Set available tabs
```csharp
public void Configuration(IAppBuilder app)
{
    ...
	app.UseThemableErrorPage(new ThemableErrorPageOptions<ErrorPageViewModel>()
								{
									ShowCookies = true,
									ShowHeaders = true,
									ShowQuery = true,
									ShowEnvironment = false,
									ShowExceptionDetails = false,
									ShowSourceCode = false
								});
	...
}
```

#### Custom viewmodel and error page path
```csharp
public void Configuration(IAppBuilder app)
{
    ...
	app.UseThemableErrorPage(new ThemableErrorPageOptions<FriendlyErrorPageViewModel>()
								{
									ShowCookies = true,
									ShowHeaders = true,
									ShowQuery = true,
									ShowEnvironment = false,
									ShowExceptionDetails = false,
									ShowSourceCode = false,
									ErrorPagePath = "Views/Shared/Error.cshtml",
									ConfigureViewModel = (x) =>
										{
											x.Debug = true;
											x.Deployment = "STAGING";
											x.FileVersion = "1.0.0.12-build56";
											x.Version = "1.0.0";

											return x;
										}
								});
	...
}
```