using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using Helpers;

namespace graphconsoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
    

            var config = LoadAppSettings();
            if (config == null)
            {
                Console.WriteLine("Invalid appsettings.json file.");
                return;
            }
 
            var userName = ReadUsername();
            var userPassword = ReadPassword();

            var client = GetAuthenticatedGraphClient(config, userName, userPassword);

            // request 1 - get user's files
            /* var request = client.Me.Drive.Root.Children.Request();

            var results = request.GetAsync().Result;
            foreach (var file in results)
            {
                Console.WriteLine(file.Id + ": " + file.Name);
            }
            */
            // request 2 - get specific file
            var fileId = "01GS35PQD24WVUPNR6QFA2KYQA3MM3Y6XK";
            var request = client.Me.Drive.Items[fileId].Request();

            var results = request.GetAsync().Result;
            Console.WriteLine(results.Id + ": " + results.Name);
            
        }

        private static IConfigurationRoot LoadAppSettings()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", false, true)
                                .Build();

                if (string.IsNullOrEmpty(config["applicationId"]) ||
                    string.IsNullOrEmpty(config["tenantId"]))
                {
                    return null;
                }
                return config;

            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }   
        }

        private static IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config, string userName, SecureString userPassword)
        {
            var clientId = config["applicationId"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            List<string> scopes = new List<string>();
            scopes.Add("User.Read");
            scopes.Add("Files.Read");

            var cca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .Build();
            return MsalAuthenticationProvider.GetInstance(cca, scopes.ToArray(), userName, userPassword);
        }

        private static GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config, string userName, SecureString userPassword)
        {
            var authenticationProvider = CreateAuthorizationProvider(config, userName, userPassword);
            var graphClient = new GraphServiceClient(authenticationProvider);
            return graphClient;
        }

        private static SecureString ReadPassword()
        {
            Console.WriteLine("Enter your password");
            SecureString password = new SecureString();
            while (true)
            {
                ConsoleKeyInfo c = Console.ReadKey(true);
                if (c.Key == ConsoleKey.Enter)
                {
                    break;
                }
                password.AppendChar(c.KeyChar);
                Console.Write("*");
            }
            Console.WriteLine();
            return password;
        }

        private static string ReadUsername()
        {
            string username;
            Console.WriteLine("Enter your username");
            username = Console.ReadLine();
            return username;
        }

    }

}
