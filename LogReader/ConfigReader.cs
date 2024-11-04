using System;
using System.Reflection;
using LogReader.Models;
using Microsoft.Extensions.Configuration;

public static class ConfigReader {
    public static AppParameters appParameters = new();

    public static void ReadAppSettings() {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        appParameters = configuration.GetSection("AppParameters").Get<AppParameters>()
                     ?? throw new Exception("Failed to load AppParameters from appsettings.json. Please check the configuration.");

    }

    public static void PrintOutAllSettings() {
        Console.WriteLine("AppSettings:");

        Type type = appParameters.GetType();
        PropertyInfo[] properties = type.GetProperties();

        foreach (PropertyInfo property in properties) {
            object? value = property.GetValue(appParameters);
            Console.WriteLine($" > {property.Name}: {value}");
        }
    }
}