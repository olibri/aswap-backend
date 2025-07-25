using Aswap_back.Controllers;
using Domain.Interfaces.Services.Account;
using Domain.Models.Api.Rating;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.AccountAuth;

namespace Tests_back;

public class RatingTests(TestFixture f) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task AddReview_Then_Summary()
  {
    PostgresDatabase.ResetState("rating_reviews");
    f.ResetDb("rating_reviews");

    var ctrl = f.GetService<RatingController>().WithUser("from_wallet");
    var dto = new AddReviewDto("to_wallet", 4.5m, "good deal", 123UL);

    var id = (await ctrl.Add(dto, default)).OkProp<long>("id");
    id.ShouldBeGreaterThan(0);

    var summary = await ctrl.GetSummary("to_wallet", default);
    summary.Reviews.ShouldBe(1);
    summary.Positive.ShouldBe(1);
    summary.AvgScore.ShouldBe(4.5m);
  }

  [Fact]
  public async Task GetReviews_Returns_List()
  {
    PostgresDatabase.ResetState("rating_reviews");

    var svc = f.GetService<IRatingService>();
    await svc.AddReviewAsync("a", new AddReviewDto("x", 5m, "nice", null), default);
    await svc.AddReviewAsync("b", new AddReviewDto("x", 3m, "meh", null), default);

    var ctrl = f.GetService<RatingController>();
    var list = await ctrl.GetReviews("x", 0, 10, default);

    list.Length.ShouldBe(2);
    list[0].Comment.ShouldNotBeNull();
  }
}