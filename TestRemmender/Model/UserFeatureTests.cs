using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recommender.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Model.Tests
{
    [TestClass()]
    public class UserFeatureTests
    {
        [TestMethod()]
        public void SquareSumTest()
        {
            UserFeature userFeature = new UserFeature()
            {
                RatingCount1 = 0,
                RatingCount2 = 0,
                RatingCount3 = 0,
                RatingCount4 = 0,
                RatingCount5 = 0,
                TotalRating = 0,
                Perfer = 0,
            };
            Assert.IsTrue(Math.Abs(userFeature.SquareSum() - 0) < 1e-3);

            userFeature = new UserFeature()
            {
                RatingCount1 = 1,
                RatingCount2 = 0,
                RatingCount3 = 4,
                RatingCount4 = 0,
                RatingCount5 = 6,
                TotalRating = 36.5
            };
            Assert.IsTrue(Math.Abs(userFeature.SquareSum() - 11.44835) < 1e-3);
        }

        [TestMethod()]
        public void AverageTest()
        {
            UserFeature userFeature = new UserFeature()
            {
                RatingCount1 = 0,
                RatingCount2 = 0,
                RatingCount3 = 0,
                RatingCount4 = 0,
                RatingCount5 = 0,
                TotalRating = 0,
            };
            Assert.IsTrue(Math.Abs(userFeature.Average() - 0) < 1e-3);

            userFeature = new UserFeature()
            {
                RatingCount1 = 1,
                RatingCount2 = 0,
                RatingCount3 = 4,
                RatingCount4 = 0,
                RatingCount5 = 6,
                TotalRating = 36.5
            };
            Assert.IsTrue(Math.Abs(userFeature.Average() - 0.27727) < 1e-3);
        }

        [TestMethod()]
        public void VarianceTest()
        {
            UserFeature userFeature = new UserFeature()
            {
                RatingCount1 = 0,
                RatingCount2 = 0,
                RatingCount3 = 0,
                RatingCount4 = 0,
                RatingCount5 = 0,
                TotalRating = 0
            };
            Assert.IsTrue(Math.Abs(userFeature.Variance() - 0) < 1e-3);

            userFeature = new UserFeature()
            {
                RatingCount1 = 1,
                RatingCount2 = 0,
                RatingCount3 = 4,
                RatingCount4 = 0,
                RatingCount5 = 6,
                TotalRating = 36.5
            };
            Console.WriteLine(Math.Abs(userFeature.Variance()));
            Assert.IsTrue(Math.Abs(userFeature.Variance() - 0.41715) < 1e-3);
        }
    }
}