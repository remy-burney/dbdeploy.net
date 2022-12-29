using System;
using Dbdeploy.Core;
using Dbdeploy.Core.Exceptions;

namespace Dbdeploy.Console
{
    public class CommandLine
    {
        public static void Main(string[] args)
        {
            var exitCode = 0;

            try
            {
                // Read arguments from command line
                var deploymentsConfig = OptionsManager.ParseOptions(args);
                var deployer = new DbDeployer();
                foreach (var config in deploymentsConfig.Deployments)
                {
                    deployer.Execute(config, System.Console.Out);
                }
            }
            catch (UsageException ex)
            {
                System.Console.Error.WriteLine("ERROR: " + ex.Message);
                
                OptionsManager.PrintUsage();
            }
            catch (DbDeployException ex)
            {
                System.Console.Error.WriteLine(ex.Message);
                exitCode = 1;
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine("Failed to apply changes: " + ex.Message);
                System.Console.Error.WriteLine(ex.StackTrace);
                exitCode = 2;
            }

            Environment.Exit(exitCode);
        }
    }
}