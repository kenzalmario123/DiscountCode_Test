// See https://aka.ms/new-console-template for more information

using DiscountApp.Service;

Console.WriteLine("Hello, World!");

var app = new DiscountApplication();

await app.RunAsync(args);
