using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace CodeTrip.Utils.ConfigFileChanger.Tests
{
    [TestFixture]
    public class ConfigFileChangerTests
    {
        [Test]
        public void Full_run_through()
        {
            Console.WriteLine(Environment.CurrentDirectory);
            Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);

            var processStartInfo = new ProcessStartInfo(Path.Combine(Environment.CurrentDirectory, "CodeTrip.Utils.ConfigFileChanger.exe"),
                                                        @"/M:Deploy /Env:dev /A:+ /Inst:TestInstructions\Replacements /Config:TestInstructions\AllInSameFile /D-"){};
            RunProcess(processStartInfo);
            using (
                var sr =
                    new StreamReader(Path.Combine(Environment.CurrentDirectory,
                        @"TestInstructions\AllInSameFile\Instructions.inst")))
            {
                Console.WriteLine(sr.ReadToEnd());
            }

            processStartInfo = new ProcessStartInfo("CodeTrip.Utils.ConfigFileChanger.exe",
                                                        @"/M:Deploy /Env:dev /A:+ /Inst:TestInstructions\AllInSameFile /Config:TestConfig");


            RunProcess(processStartInfo);

            using (
                var sr =
                    new StreamReader(Path.Combine(Environment.CurrentDirectory,
                        @"TestConfig\test.config")))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }

        [Test]
        public void Multi_envs()
        {
            var processStartInfo = new ProcessStartInfo("CodeTrip.Utils.ConfigFileChanger.exe",
                                                        @"/M:Deploy /Env:dev /Env:b /A:+ /Inst:TestInstructions\MultiEnvs /Config:TestConfig /D-");


            RunProcess(processStartInfo);

            using (
                var sr =
                    new StreamReader(Path.Combine(Environment.CurrentDirectory,
                        @"TestConfig\test.config")))
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }

        private void RunProcess(ProcessStartInfo processStartInfo)
        {
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;

            var process = Process.Start(processStartInfo);
           
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                using (var sr = process.StandardError)
                {
                    Console.WriteLine(sr.ReadToEnd());
                }

                Assert.Fail("Exit code " + process.ExitCode);
            }

            using (var sr = process.StandardError)
            {
                Console.WriteLine(sr.ReadToEnd());
            }

            using (var sr = process.StandardOutput)
            {
                Console.WriteLine(sr.ReadToEnd());
            }
        }
    }
}
