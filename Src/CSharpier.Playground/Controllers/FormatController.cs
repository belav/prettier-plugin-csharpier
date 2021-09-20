using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CSharpier;
using CSharpier.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CSharpier.Playground.Controllers
{
    public class FormatResult
    {
        public string Code { get; set; }
        public string Json { get; set; }
        public string Doc { get; set; }
        public List<FormatError> Errors { get; set; }
    }

    public class FormatError
    {
        public FileLinePositionSpan LineSpan { get; set; }
        public string Description { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class FormatController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ILogger logger;

        // ReSharper disable once SuggestBaseTypeForParameter
        public FormatController(
            IWebHostEnvironment webHostEnvironment,
            ILogger<FormatController> logger
        ) {
            this.webHostEnvironment = webHostEnvironment;
            this.logger = logger;
        }

        [HttpPost]
        public FormatResult Post([FromBody] string content)
        {
            var result = new CodeFormatter().Format(
                content,
                new PrinterOptions
                {
                    IncludeAST = true,
                    IncludeDocTree = true,
                    Width = PrinterOptions.WidthUsedByTests
                }
            );

            return new FormatResult
            {
                Code = result.Code,
                Json = result.AST,
                Doc = result.DocTree,
                Errors = result.Errors.Select(ConvertError).ToList(),
            };
        }

        private FormatError ConvertError(Diagnostic diagnostic)
        {
            var lineSpan = diagnostic.Location.SourceTree.GetLineSpan(
                diagnostic.Location.SourceSpan
            );
            return new FormatError { LineSpan = lineSpan, Description = diagnostic.ToString(), };
        }

        public string ExecuteApplication(string pathToExe, string workingDirectory, string args)
        {
            var processStartInfo = new ProcessStartInfo(pathToExe, args)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            var output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            this.logger.LogInformation(
                "Output from '" + pathToExe + " " + args + "' was: " + Environment.NewLine + output
            );

            return output;
        }
    }
}
