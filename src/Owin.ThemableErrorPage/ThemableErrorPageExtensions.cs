namespace Owin.ThemableErrorPage
{
    using System;

    using Microsoft.Owin.Diagnostics.Views;

    using Owin;

    /// <summary>
    /// IAppBuilder extension methods for the ThemableErrorPageMiddleware.
    /// </summary>
    public static class ThemableErrorPageExtensions
    {
        /// <summary>
        /// Captures synchronous and asynchronous exceptions from the pipeline and generates HTML error responses.
        ///             Full error details are only displayed by default if 'host.AppMode' is set to 'development' in the IAppBuilder.Properties.
        /// </summary>
        /// <param name="builder"/>
        /// <returns/>
        public static IAppBuilder UseThemableErrorPage(this IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var appMode = builder.Properties["host.AppMode"];
            var devMode = appMode != null && string.Equals("development", appMode.ToString(), StringComparison.Ordinal);

            return builder.UseThemableErrorPage(new ThemableErrorPageOptions<ErrorPageModel>()
                                                    {
                                                        ShowCookies = devMode,
                                                        ShowEnvironment = devMode,
                                                        ShowExceptionDetails = devMode,
                                                        ShowHeaders = devMode,
                                                        ShowQuery = devMode,
                                                        ShowSourceCode = devMode
                                                    });
        }

        /// <summary>
        /// Captures synchronous and asynchronous exceptions from the pipeline and generates HTML error responses.
        ///             Full error details are only displayed by default if 'host.AppMode' is set to 'development' in the IAppBuilder.Properties.
        /// </summary>
        /// <param name="builder"/>
        /// <param name="options"/>
        /// <returns/>
        public static IAppBuilder UseThemableErrorPage<TViewModel>(this IAppBuilder builder, ThemableErrorPageOptions<TViewModel> options) where TViewModel : ErrorPageModel
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use<ThemableErrorPageMiddleware<ThemableErrorPageOptions<TViewModel>, TViewModel>>(options);
        }

        /// <summary>
        /// Captures synchronous and asynchronous exceptions from the pipeline and generates HTML error responses.
        ///             Full error details are only displayed by default if 'host.AppMode' is set to 'development' in the IAppBuilder.Properties.
        /// </summary>
        /// <param name="builder"/>
        /// <param name="options"/>
        /// <returns/>
        public static IAppBuilder UseThemableErrorPage<TOptions, TViewModel>(this IAppBuilder builder, TOptions options)
            where TOptions : ThemableErrorPageOptions<TViewModel>
            where TViewModel : ErrorPageModel
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use<ThemableErrorPageMiddleware<TOptions, TViewModel>>(options);
        }
    }
}