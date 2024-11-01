﻿using Dotmim.Sync;
using Dotmim.Sync.SqlServer;
using System;
using System.Threading.Tasks;

namespace OutDated
{
    internal class Program
    {

        private static string serverConnectionString = $"Data Source=(localdb)\\mssqllocaldb; Initial Catalog=AdventureWorks;Integrated Security=true;";
        private static string clientConnectionString = $"Data Source=(localdb)\\mssqllocaldb; Initial Catalog=Client;Integrated Security=true;";

        private static async Task Main(string[] args)
        {
            await OutDatedAsync().ConfigureAwait(false);
        }

        private static async Task OutDatedAsync()
        {
            // Database script used for this sample : https://github.com/Mimetis/Dotmim.Sync/blob/master/CreateAdventureWorks.sql

            // Create 2 Sql Sync providers
            var serverProvider = new SqlSyncProvider(serverConnectionString);

            // Second provider is using plain old Sql Server provider, relying on triggers and tracking tables to create the sync environment
            var clientProvider = new SqlSyncProvider(clientConnectionString);

            // Tables involved in the sync process:
            var tables = new string[]
            {
                "ProductCategory", "ProductModel", "Product",
                        "Address", "Customer", "CustomerAddress", "SalesOrderHeader", "SalesOrderDetail",
            };

            // Creating an agent that will handle all the process
            var agent = new SyncAgent(clientProvider, serverProvider);

            Console.WriteLine("- Initialize the databases with initial data");

            // Make a first sync to have everything in place
            Console.WriteLine(await agent.SynchronizeAsync(tables).ConfigureAwait(false));

            // Call a server delete metadata to update the last valid timestamp value in scope_info_server table
            var dmc = await agent.RemoteOrchestrator.DeleteMetadatasAsync().ConfigureAwait(false);

            Console.WriteLine("- Insert data in client database and then generate an out dated scenario");

            // Insert a value on client
            await Helper.InsertOneCustomerAsync(clientProvider.CreateConnection(), "John", "Doe").ConfigureAwait(false);

            // Simulate an outdated situation in the local database
            await Helper.SimulateOutDateScenarioAsync(clientProvider.CreateConnection(), dmc.TimestampLimit - 1).ConfigureAwait(false);

            // Action when outdate occurs
            agent.LocalOrchestrator.OnOutdated(oa =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("local database is too old to synchronize with the server.");
                Console.ResetColor();
                Console.WriteLine("Do you want to synchronize anyway, and potentially lost data ? ");
                Console.Write("Enter a value ('r' for reinitialize or 'ru' for reinitialize with upload): ");
                var answer = Console.ReadLine();

                if (answer.Equals("r", StringComparison.InvariantCultureIgnoreCase))
                    oa.Action = OutdatedAction.Reinitialize;
                else if (answer.Equals("ru", StringComparison.InvariantCultureIgnoreCase))
                    oa.Action = OutdatedAction.ReinitializeWithUpload;
            });

            do
            {
                try
                {
                    Console.WriteLine("- Launch synchronization");
                    var res = await agent.SynchronizeAsync().ConfigureAwait(false);
                    Console.WriteLine(res);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            while (Console.ReadKey().Key != ConsoleKey.Escape);

            Console.WriteLine("End");
        }
    }
}