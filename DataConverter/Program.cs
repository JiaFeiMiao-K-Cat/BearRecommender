﻿using CsvHelper;
using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using Recommender.Model;
using System.Formats.Asn1;
using System.Globalization;

namespace DataConverter;

public class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine($"Start: {DateTime.Now}");

        var context = new Context($"Data Source={args[0]}");
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
        };
        #region Convert movies.csv to sqlite file
        using var moviesReader = new StreamReader(args[1]);
        using var moviesCsv = new CsvReader(moviesReader, config);
        var moviesRecords = moviesCsv.GetRecords<Movie>();
        await context.BulkInsertAsync<Movie>(moviesRecords.ToList()); //不使用ToList()会导致无法写入数据库
        await context.BulkSaveChangesAsync();
        #endregion

        Console.WriteLine($"Movies converted: {DateTime.Now}");

        #region Convert ratings.csv to sqlite file
        using var ratingsReader = new StreamReader(args[2]);
        using var ratingsCsv = new CsvReader(ratingsReader, config);
        var ratingsRecords = ratingsCsv.GetRecords<Rating>();
        await context.BulkInsertAsync<Rating>(ratingsRecords.ToList()); //不使用ToList()会导致无法写入数据库
        await context.BulkSaveChangesAsync();
        #endregion

        Console.WriteLine($"End: {DateTime.Now}");
    }
}