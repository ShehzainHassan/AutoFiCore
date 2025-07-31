using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFiCore.Data;
using System.IdentityModel.Tokens.Jwt;

public class CheckoutAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public CheckoutAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IUnitOfWork unitOfWork)
    {
        var path = context.Request.Path.Value ?? "";

        if (path.StartsWith("/auction/") && path.EndsWith("/checkout", StringComparison.OrdinalIgnoreCase))
        {
            var segments = path.Split('/');
            if (segments.Length >= 3 && int.TryParse(segments[2], out int auctionId))
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) ?? 
                                context.User.FindFirst(JwtRegisteredClaimNames.Sub);

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized: Missing or invalid user ID.");
                    return;
                }

                var auction = await unitOfWork.Auctions.GetAuctionByIdAsync(auctionId);
                if (auction == null || auction.Status != AutoFiCore.Enums.AuctionStatus.Ended)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: Auction not ended or not found.");
                    return;
                }

                var winnerId = await unitOfWork.Bids.GetHighestBidderIdAsync(auctionId);
                if (winnerId != userId)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Unauthorized: No access");
                    return;
                }
            }
        }

        await _next(context);
    }
}
