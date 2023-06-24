using NetTopologySuite.Utilities;
using Recommender.Model;
using Recommender.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Matching;

public class CollaborativeFiltering
{
    private Context _context { get; set; }
    public CollaborativeFiltering(Context context)
    {
        _context = context;
    }

    public double PredictUserRating(int userId, int movieId)
    {
        double numerator = 0;
        double denominator = 0;
        var user = _context.UserFeatures.FirstOrDefault(x => x.UserId == userId);
        if (user == null)
        {
            return 0;
        }
        var sim = _context.UserFeatures
            .Where(x => _context.Ratings.Any(e => e.UserId == x.UserId && e.MovieId == movieId))
            .ToList()
            .OrderByDescending(x => Similarity.UserCosine(x, user))
            .Take(10)
            .ToList();
        foreach (var item in sim)
        {
            var rating = _context.Ratings
                .FirstOrDefault(e => e.UserId == item.UserId && e.MovieId == movieId);
            if (rating!.UserRating <= 0)
            {
                continue;
            }
            else
            {
                double similarity = Similarity.UserCosine(item, user);
                numerator += similarity * rating.UserRating;
                denominator += similarity;
            }
        }
        if (denominator == 0)
        {
            numerator = user!.AverageRating;
            denominator = 1;
        }
        return numerator / denominator;
    }

    public List<int>? RecommendMovies(int userId, int k)
    {
        var list = new List<int>();
        var user = _context.UserFeatures.FirstOrDefault(x => x.UserId == userId);
        if (user == null)
        {
            return null;
        }
        var sim = _context.UserFeatures
            .ToList()
            .OrderByDescending(x => Similarity.UserCosine(x, user))
            .Take(10);

        foreach ( var item in sim)
        {
            var movies = _context.Ratings
                .Where(e => e.UserId == item.UserId)
                .OrderByDescending(p => p.Timestamp)
                .ThenByDescending(q => q.UserRating)
                .Take(k)
                .Select(e => e.MovieId);

            foreach (var movie in movies)
            {
                list.Add(movie);
            }
        }

        return list.Distinct().ToList();
    }
}
