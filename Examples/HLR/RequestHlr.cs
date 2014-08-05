﻿using System;

using MessageBird;
using MessageBird.Exceptions;
using MessageBird.Objects;

namespace Examples
{
    class RequestHlr
    {
        static void Main(string[] args)
        {
            Client client = Client.CreateDefault("YOUR_ACCESS_KEY");

            try
            {
                Hlr hlr = client.RequestHlr(31612345678, "Custom reference");
                Console.WriteLine("{0}", hlr);
                
            }
            catch (ErrorException e)
            {
                // Either the request fails with error descriptions from the endpoint.
                if (e.HasErrors)
                {
                    foreach (Error error in e.Errors)
                    {
                        Console.WriteLine("code: {0} description: '{1}' parameter: '{2}'", error.Code, error.Description, error.Parameter);
                    }
                }
                // or fails without error information from the endpoint, in which case the reason contains a 'best effort' description.
                if (e.HasReason)
                {
                    Console.WriteLine(e.Reason);
                }
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}