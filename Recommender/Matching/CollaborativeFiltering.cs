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

    public double Predict(int userId, int movieId)
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
            .OrderByDescending(x => Similarity.UserFeatureCosine(x, user!))
            .Take(30)
            .ToList();
        foreach (var item in sim)
        {
            var rating = _context.Ratings
                .FirstOrDefault(e => e.UserId == item.UserId && e.MovieId == movieId);
            if (rating!.UserRating < 1)
            {
                continue;
            }
            else
            {
                numerator += Similarity.UserFeatureCosine(item, user!) * rating.UserRating;
                denominator += Similarity.UserFeatureCosine(item, user!);
                //Console.WriteLine($"{Similarity.UserFeatureCosine(item, user!)} {rating.UserRating}");
            }
        }
        if (denominator == 0)
        {
            numerator = user!.AverageRating;
            denominator = 1;
        }
        return numerator / denominator;
    }
}
