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
        multiply += MultiEncoding.Multiply(itemA.Genres, itemB.Genres);
        return multiply / (Math.Sqrt(squareA) * Math.Sqrt(squareB) + 1e-9);
        // 防止除零, 分母加1e-9
    }
    public static double UserFeatureCosine(UserFeature itemA, UserFeature itemB)
    {
        double squareA = itemA.SquareSum();
        double squareB = itemB.SquareSum();
        double multiply = 0;
        multiply += itemA.AverageRating * itemB.AverageRating;
        multiply += itemA.RatingRate1 * itemB.RatingRate1;
        multiply += itemA.RatingRate2 * itemB.RatingRate2;
        multiply += itemA.RatingRate3 * itemB.RatingRate3;
        multiply += itemA.RatingRate4 * itemB.RatingRate4;
        multiply += MultiEncoding.Multiply(itemA.Perfer, itemB.Perfer);
        return multiply / (Math.Sqrt(squareA) * Math.Sqrt(squareB) + 1e-9);
        // 防止除零, 分母加1e-9
    }
}