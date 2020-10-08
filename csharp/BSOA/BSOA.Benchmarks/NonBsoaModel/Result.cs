﻿using BSOA.Benchmarks.Model;

using System;
using System.Collections.Generic;

namespace BSOA.Benchmarks.NonBsoaModel
{
    public class Result
    {
        public string RuleId { get; set; }
        public bool IsActive { get; set; }
        public string Message { get; set; }
        public int StartLine { get; set; }
        public DateTime WhenDetectedUtc { get; set; }
        public BaselineState BaselineState { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public IList<string> Tags { get; set; }
    }
}
