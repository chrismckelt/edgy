﻿/*
This is the entry point for the WebApp module. Code comments are only for modifications beyond the 
template for a fresh project.  The template for this project is a web API template from the command:
$dotnet new webapi 
rather than
$dotnet new webapp
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
                    
            try
            {
                Log.Information("WebApp starting");
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            /*UseUrls("http://*:8080) is used here to set the port to use to
            access the HMI module through localhost.  This port is also set
            in the docker files. */
            
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:8089")
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
