﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserLog
{
    public class Logger : ILogger
    {
        public void Error(string message)
        {
            Console.WriteLine("Error: "+message);
        }

        public void Info(string message)
        {
            Console.WriteLine("Info: " + message);
        }

        public void Warn(string message)
        {
            Console.WriteLine("Warn: " + message);
        }
    }
}
