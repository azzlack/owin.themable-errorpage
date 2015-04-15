namespace Owin.ThemableErrorPage
{
    using System;

    using Microsoft.Owin.Diagnostics;
    using Microsoft.Owin.Diagnostics.Views;

    /// <summary>
    /// The error page manager configuration class.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public class ThemableErrorPageOptions<TViewModel> : ErrorPageOptions where TViewModel : ErrorPageModel
    {
        public ThemableErrorPageOptions()
        {
            // Set default options
            this.ErrorPagePath = "Views/Shared/Error.cshtml";
        }

        /// <summary>
        /// The error page path, relative to the root of your application.
        /// </summary>
        public string ErrorPagePath { get; set; }

        /// <summary>
        /// Callback for configuring the viewmodel. 
        /// Use this to make any last changes to the viewmodel before displaying the page.
        /// </summary>
        public Func<TViewModel, TViewModel> ConfigureViewModel { get; set; }
    }
}