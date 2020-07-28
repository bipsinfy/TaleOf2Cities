using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using TaleOf2Cities.Interface;

namespace TaleOf2Cities
{
    public class FileParserConsole
    {
        private readonly IFileParseReportGenerator _fileParseReportGenerator;
        private readonly TaleConfiguration _configuration;
        public FileParserConsole(IFileParseReportGenerator fileParseReportGenerator, TaleConfiguration configuration)
        {
            _fileParseReportGenerator = fileParseReportGenerator;
            _configuration = configuration;
        }

        public void Run()
        {
            _fileParseReportGenerator.GenerateFileSummary(_configuration.KthTopOrderWordPosition);
        }
    }
}
