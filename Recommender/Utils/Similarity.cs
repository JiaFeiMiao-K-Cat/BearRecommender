using Recommender.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Utils;

public class Similarity
{
    public static double MovieFeatureCosine(MovieFeature itemA, MovieFeature itemB)
    {
        double squareA = itemA.SquareSum();
        double squareB = itemB.SquareSum();
        double multiply = 0;
        multiply += itemA.AverageRating * itemB.AverageRating;
        multiply += itemA.RatingRate1 * itemB.RatingRate1;
        multiply += itemA.RatingRate2 * itemB.RatingRate2;
        multiply += itemA.RatingRate3 * itemB.RatingRate3;
        multiply += itemA.RatingRate4 * itemB.RatingRate4;
        multiply += itemA.RatingRate5 * itemB.RatingRate5;
        multiply += MultiEncoding.Multiply(itemA.Genres, itemB.Genres);
        return multiply / (Math.Sqrt(squareA) * Math.Sqrt(squareB) + 1e-18);
        // 防止除零, 分母加1e-18
    }
    public static double UserFeatureCosine(UserFeature userA, UserFeature userB)
    {
        double squareA = userA.SquareSum();
        double squareB = userB.SquareSum();
        double multiply = 0;
        multiply += userA.AverageRating * userB.AverageRating;
        multiply += userA.RatingRate1 * userB.RatingRate1;
        multiply += userA.RatingRate2 * userB.RatingRate2;
        multiply += userA.RatingRate3 * userB.RatingRate3;
        multiply += userA.RatingRate4 * userB.RatingRate4;
        multiply += userA.RatingRate5 * userB.RatingRate5;
        multiply += MultiEncoding.Multiply(userA.Perfer, userB.Perfer);
        return multiply / (Math.Sqrt(squareA) * Math.Sqrt(squareB) + 1e-18);
        // 防止除零, 分母加1e-18
    }

    private static double Covariance(UserFeature userA, UserFeature userB)
    {
        double avgA = userA.Average();
        double avgB = userB.Average();

        double cov = 0;

        cov += (userA.AverageRating / 5 - avgA) * (userB.AverageRating / 5 - avgB);
        cov += (userA.RatingRate1 - avgA) * (userB.RatingRate1 - avgB);
        cov += (userA.RatingRate2 - avgA) * (userB.RatingRate2 - avgB);
        cov += (userA.RatingRate3 - avgA) * (userB.RatingRate3 - avgB);
        cov += (userA.RatingRate4 - avgA) * (userB.RatingRate4 - avgB);
        cov += (userA.RatingRate5 - avgA) * (userB.RatingRate5 - avgB);

        cov /= 6;

        return cov;
    }
    public static double UserFeaturePearson(UserFeature userA, UserFeature userB)
    {
        double cov = Covariance(userA, userB);

        double varA = userA.Variance();
        double varB = userB.Variance();

        return cov / (Math.Sqrt(varA) * Math.Sqrt(varB) + 1e-18);
    }
    public static double EmbeddingUserCosine(UserFeature userA, UserFeature userB)
    {
        double numerator = 0;
        double denominatorA = 0;
        double denominatorB = 0;
        for (int i = 0; i < userA.FeaturesEncoded.Count(); i++)
        {
            // 不转double可能会产生Inf
            numerator += (double)userA.FeaturesEncoded[i] * userB.FeaturesEncoded[i];
            denominatorA += Math.Pow(userA.FeaturesEncoded[i], 2);
            denominatorB += Math.Pow(userB.FeaturesEncoded[i], 2);
        }
        return numerator / (Math.Sqrt(denominatorA) * Math.Sqrt(denominatorB) + 1e-18);
    }

    public static double UserCosine(UserFeature userA, UserFeature userB)
    {
        return 0.1 * UserFeaturePearson(userA, userB) + 0.5 * UserFeatureCosine(userA, userB) + 0.4 * EmbeddingUserCosine(userA, userB);
    }
}