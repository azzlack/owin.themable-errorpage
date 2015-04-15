namespace Owin.ThemableErrorPage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Owin;
    using Microsoft.Owin.Diagnostics.Views;

    using RazorEngine;
    using RazorEngine.Templating;

    /// <summary>
    /// Middleware for showing an error page when exceptions are encountered.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <typeparam name="TViewModel">The viewmodel type.</typeparam>
    public class ThemableErrorPageMiddleware<TOptions, TViewModel> : OwinMiddleware
        where TOptions : ThemableErrorPageOptions<TViewModel>
        where TViewModel : ErrorPageModel
    {
        /// <summary>
        /// The error page options
        /// </summary>
        private readonly TOptions options;

        /// <summary>
        /// Instantiates the middleware with an optional pointer to the next component.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="options">The options.</param>
        public ThemableErrorPageMiddleware(OwinMiddleware next, TOptions options)
            : base(next)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            this.options = options;
        }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await this.Next.Invoke(context);
            }
            catch (Exception ex1)
            {
                var devMode = context.Get<string>("host.AppMode");

                context.Response.StatusCode = 500;
                context.Response.ReasonPhrase = "Internal Server Error";
                context.Response.ContentType = "text/html";

                try
                {
                    // Display Error Page
                    this.DisplayException(context, ex1);
                }
                catch (Exception ex2)
                {
                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        writer.Write(
                            string.Equals("development", devMode, StringComparison.Ordinal)
                                ? ex2.Message
                                : "Sorry, an error occurred while processing your request.");
                    }
                }

                throw;
            }
        }

        /// <summary>
        /// Displays the error page.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        /// <param name="ex">The exception.</param>
        private void DisplayException(IOwinContext context, Exception ex)
        {
            var model = Activator.CreateInstance<TViewModel>();
            model.Options = this.options;

            if (this.options.ConfigureViewModel != null)
            {
                model = this.options.ConfigureViewModel(model);
            }

            if (this.options.ShowExceptionDetails)
            {
                model.ErrorDetails = this.GetErrorDetails(ex, this.options.ShowSourceCode).Reverse();
            }

            if (this.options.ShowQuery)
            {
                model.Query = context.Request.Query;
            }

            if (this.options.ShowCookies)
            {
                model.Cookies = context.Request.Cookies;
            }

            if (this.options.ShowHeaders)
            {
                model.Headers = context.Request.Headers;
            }

            if (this.options.ShowEnvironment)
            {
                model.Environment = context.Request.Environment;
            }

            // Set response
            using (var writer = new StreamWriter(context.Response.Body))
            {
                writer.Write(RazorViewEngine.RenderErrorPage(model, this.options.ErrorPagePath));
            }
        }

        private IEnumerable<ErrorDetails> GetErrorDetails(Exception ex, bool showSource)
        {
            for (var scan = ex; scan != null; scan = scan.InnerException)
            {
                yield return new ErrorDetails()
                {
                    Error = scan,
                    StackFrames = this.StackFrames(scan, showSource)
                };
            }
        }

        private IEnumerable<StackFrame> StackFrames(Exception ex, bool showSource)
        {
            var stackTrace = ex.StackTrace;

            if (!string.IsNullOrEmpty(stackTrace))
            {
                var heap = new Chunk()
                {
                    Text = stackTrace + Environment.NewLine,
                    End = stackTrace.Length + Environment.NewLine.Length
                };

                for (var line = heap.Advance(Environment.NewLine);
                     line.HasValue;
                     line = heap.Advance(Environment.NewLine))
                {
                    yield return this.StackFrame(line, showSource);
                }
            }
        }

        private StackFrame StackFrame(Chunk line, bool showSource)
        {
            line.Advance("  at ");

            var function = line.Advance(" in ").ToString();
            var file = line.Advance(":line ").ToString();
            var lineNumber = line.ToInt32();

            if (!string.IsNullOrEmpty(file))
            {
                return this.LoadFrame(function, file, lineNumber, showSource);
            }

            return this.LoadFrame(string.IsNullOrEmpty(function) ? line.ToString() : function, string.Empty, 0, showSource);
        }

        private StackFrame LoadFrame(string function, string file, int lineNumber, bool showSource)
        {
            var stackFrame = new StackFrame()
            {
                Function = function,
                File = file,
                Line = lineNumber
            };

            if (showSource && File.Exists(file))
            {
                var strArray = File.ReadAllLines(file);

                stackFrame.PreContextLine = Math.Max(lineNumber - this.options.SourceCodeLineCount, 1);
                stackFrame.PreContextCode = strArray.Skip(stackFrame.PreContextLine - 1).Take(lineNumber - stackFrame.PreContextLine).ToArray();
                stackFrame.ContextCode = strArray.Skip(lineNumber - 1).FirstOrDefault();
                stackFrame.PostContextCode = strArray.Skip(lineNumber).Take(this.options.SourceCodeLineCount).ToArray();
            }

            return stackFrame;
        }

        internal class Chunk
        {
            public string Text { get; set; }

            public int Start { get; set; }

            public int End { get; set; }

            public bool HasValue
            {
                get
                {
                    return this.Text != null;
                }
            }

            public Chunk Advance(string delimiter)
            {
                var num = this.HasValue ? this.Text.IndexOf(delimiter, this.Start, this.End - this.Start, StringComparison.Ordinal) : -1;
                if (num < 0)
                {
                    return new Chunk();
                }

                var chunk = new Chunk()
                {
                    Text = this.Text,
                    Start = this.Start,
                    End = num
                };

                this.Start = num + delimiter.Length;

                return chunk;
            }

            public override string ToString()
            {
                if (!this.HasValue)
                {
                    return string.Empty;
                }

                return this.Text.Substring(this.Start, this.End - this.Start);
            }

            public int ToInt32()
            {
                int result;
                if (!this.HasValue
                    || !int.TryParse(
                        this.Text.Substring(this.Start, this.End - this.Start),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out result))
                {
                    return 0;
                }

                return result;
            }
        }

        internal class RazorViewEngine
        {
            public static string RenderErrorPage(TViewModel model, string path)
            {
                var fullPath = AppDomain.CurrentDomain.BaseDirectory + path;

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException("Could not find an error page view at '" + fullPath + "'");
                }

                return Engine.Razor.RunCompile(File.ReadAllText(fullPath), "ErrorPage", model.GetType(), model);
            }
        }
    }
}