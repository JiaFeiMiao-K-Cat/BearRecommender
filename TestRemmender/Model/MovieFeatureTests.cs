using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recommender.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Model.Tests;

[TestClass()]
public class MovieFeatureTests
{
    [TestMethod()]
    public void SquareSumTest()
    {
        MovieFeature movieFeature = new MovieFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 0,
            RatingCount3 = 0,
            RatingCount4 = 0,
            RatingCount5 = 0,
            TotalRating = 0,
            Genres = 0,
        };
        Assert.IsTrue(Math.Abs(movieFeature.SquareSum() - 0) < 1e-3); 

        movieFeature = new MovieFeature()
        {
            RatingCount1 = 1,
            RatingCount2 = 0,
            RatingCount3 = 4,
            RatingCount4 = 0,
            RatingCount5 = 6,
            TotalRating = 36.5,
            Genres = 0b0001_0000_1101_0101,
        };
        Assert.IsTrue(Math.Abs(movieFeature.SquareSum() - 17.44835) < 1e-3);
    }
}