using System;
using System.Collections.Generic;
using System.Text;

using AutoFixture;
namespace NModules
{
    public interface ILogger
    {
        void WriteLine(string value);
    }

    public class ConsoleLogger : ILogger
    {
        public static ConsoleLogger Instance => new ConsoleLogger();
        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public void WriteHeader(string value)
        {
            Console.WriteLine("");
            Console.WriteLine("############################################");
            Console.WriteLine(value);
            Console.WriteLine("############################################");
        }
    }

    public class MyFixture : Fixture
    {
        public MyFixture()
        {
            this.Register<string>(() => new Fixture().Create<string>().Substring(0, 10));
        }
    }
}