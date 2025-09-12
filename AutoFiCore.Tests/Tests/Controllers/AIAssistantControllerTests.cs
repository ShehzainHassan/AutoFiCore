using AutoFiCore.Controllers;
using AutoFiCore.Dto;
using AutoFiCore.Enums;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Controllers
{
    public class AIAssistantControllerTests
    {
        private readonly Mock<IAIAssistantService> _aiServiceMock;
        private readonly Mock<IUserContextService> _userContextServiceMock;
        private readonly Mock<ILogger<AIAssistantController>> _loggerMock;
        private readonly TestAIAssistantController _controller;

        public AIAssistantControllerTests()
        {
            _aiServiceMock = new Mock<IAIAssistantService>();
            _userContextServiceMock = new Mock<IUserContextService>();
            _loggerMock = new Mock<ILogger<AIAssistantController>>();
            _controller = new TestAIAssistantController(
                _aiServiceMock.Object,
                _userContextServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task QueryAI_ReturnsOk_WhenUserContextValid_AndUserIdMatches()
        {
            var payload = new EnrichedAIQuery
            {
                UserId = 1,
                Query = new AIQueryRequest { Question = "Q" }
            };
            var response = new AIResponseModel { Answer = "A" };
            _controller.SetUserContextValid(true, 1);
            _controller.SetAuthorizationHeader("Bearer token");
            _aiServiceMock.Setup(s => s.QueryFastApiAsync(payload, It.IsAny<string>(), "token"))
                .ReturnsAsync(response);

            var result = await _controller.QueryAI(payload);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task QueryAI_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            var payload = new EnrichedAIQuery { UserId = 1 };
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.QueryAI(payload);

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task QueryAI_ReturnsForbid_WhenUserIdMismatch()
        {
            var payload = new EnrichedAIQuery { UserId = 2 };
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.QueryAI(payload);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetUserContext_ReturnsOk_WhenUserContextValid()
        {
            var context = new UserContextDTO();
            _controller.SetUserContextValid(true, 1);
            _userContextServiceMock.Setup(s => s.GetUserContextAsync(1)).ReturnsAsync(context);

            var result = await _controller.GetUserContext();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(context, okResult.Value);
        }

        [Fact]
        public async Task GetUserContext_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.GetUserContext();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetContextualSuggestions_ReturnsOk_WhenUserContextValid_AndUserIdMatches()
        {
            var suggestions = new List<string> { "S1" };
            _controller.SetUserContextValid(true, 1);
            _controller.SetAuthorizationHeader("Bearer token");
            _aiServiceMock.Setup(s => s.GetSuggestionsAsync(1, It.IsAny<string>(), "token"))
                .ReturnsAsync(suggestions);

            var result = await _controller.GetContextualSuggestions(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(suggestions, okResult.Value);
        }

        [Fact]
        public async Task GetContextualSuggestions_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.GetContextualSuggestions(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetContextualSuggestions_ReturnsForbid_WhenUserIdMismatch()
        {
            _controller.SetUserContextValid(true, 2);

            var result = await _controller.GetContextualSuggestions(1);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task GetChatTitles_ReturnsOk_WhenUserContextValid()
        {
            var titles = new List<ChatTitleDto> { new ChatTitleDto() };
            _controller.SetUserContextValid(true, 1);
            _aiServiceMock.Setup(s => s.GetChatTitlesAsync(1)).ReturnsAsync(titles);

            var result = await _controller.GetChatTitles();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(titles, okResult.Value);
        }

        [Fact]
        public async Task GetChatTitles_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.GetChatTitles();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetChat_ReturnsOk_WhenChatFound()
        {
            var chat = new ChatSessionDto();
            _controller.SetUserContextValid(true, 1);
            _aiServiceMock.Setup(s => s.GetFullChatAsync(1, "sid")).ReturnsAsync(chat);

            var result = await _controller.GetChat("sid");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(chat, okResult.Value);
        }

        [Fact]
        public async Task GetChat_ReturnsNotFound_WhenChatNotFound()
        {
            _controller.SetUserContextValid(true, 1);
            _aiServiceMock.Setup(s => s.GetFullChatAsync(1, "sid")).ReturnsAsync((ChatSessionDto)null!);

            var result = await _controller.GetChat("sid");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetChat_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.GetChat("sid");

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteSession_ReturnsNoContent_WhenUserContextValid()
        {
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.DeleteSession("sid");

            Assert.IsType<NoContentResult>(result);
            _aiServiceMock.Verify(s => s.DeleteSessionAsync("sid", 1), Times.Once);
        }

        [Fact]
        public async Task DeleteSession_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.DeleteSession("sid");

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAllSessions_ReturnsNoContent_WhenUserContextValid()
        {
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.DeleteAllSessions();

            Assert.IsType<NoContentResult>(result);
            _aiServiceMock.Verify(s => s.DeleteAllSessionsAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteAllSessions_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.DeleteAllSessions();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateSessionTitle_ReturnsOk_WhenSuccess()
        {
            var req = new UpdateChatTitle { NewTitle = "T" };
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.UpdateSessionTitle("sid", req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Session updated successfully", okResult.Value);
        }

        [Fact]
        public async Task UpdateSessionTitle_ReturnsBadRequest_WhenTitleEmpty()
        {
            var req = new UpdateChatTitle { NewTitle = "" };
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.UpdateSessionTitle("sid", req);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateSessionTitle_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            var req = new UpdateChatTitle { NewTitle = "T" };
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.UpdateSessionTitle("sid", req);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateSessionTitle_ReturnsNotFound_WhenInvalidOperationException()
        {
            var req = new UpdateChatTitle { NewTitle = "T" };
            _controller.SetUserContextValid(true, 1);
            _aiServiceMock.Setup(s => s.UpdateSessionTitleAsync("sid", 1, "T"))
                .ThrowsAsync(new System.InvalidOperationException("not found"));

            var result = await _controller.UpdateSessionTitle("sid", req);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateSessionTitle_ReturnsServerError_WhenException()
        {
            var req = new UpdateChatTitle { NewTitle = "T" };
            _controller.SetUserContextValid(true, 1);
            _aiServiceMock.Setup(s => s.UpdateSessionTitleAsync("sid", 1, "T"))
                .ThrowsAsync(new System.Exception("fail"));

            var result = await _controller.UpdateSessionTitle("sid", req);

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
        }

        [Fact]
        public async Task SubmitFeedback_ReturnsOk_WhenSuccess()
        {
            var dto = new AIQueryFeedbackDto { MessageId = 1, Vote = QueryFeedback.Upvoted };
            var resp = new FeedbackResponseDto { MessageId = 1, Feedback = QueryFeedback.Upvoted };
            _controller.SetUserContextValid(true, 1);
            _controller.SetAuthorizationHeader("Bearer token");
            _aiServiceMock.Setup(s => s.SubmitFeedbackAsync(dto, It.IsAny<string>(), "token"))
                .ReturnsAsync(resp);

            var result = await _controller.SubmitFeedback(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resp, okResult.Value);
        }

        [Fact]
        public async Task SubmitFeedback_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            var dto = new AIQueryFeedbackDto();
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.SubmitFeedback(dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task SubmitFeedback_ReturnsServerError_WhenHttpRequestException()
        {
            var dto = new AIQueryFeedbackDto();
            _controller.SetUserContextValid(true, 1);
            _controller.SetAuthorizationHeader("Bearer token");
            _aiServiceMock.Setup(s => s.SubmitFeedbackAsync(dto, It.IsAny<string>(), "token"))
                .ThrowsAsync(new HttpRequestException("fail"));

            var result = await _controller.SubmitFeedback(dto);

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, status.StatusCode);
        }

        [Fact]
        public async Task GetPopularQueries_ReturnsOk_WhenValid()
        {
            var queries = new List<PopularQueryDto> { new PopularQueryDto() };
            _controller.SetUserContextValid(true, 1);
            _controller.SetAuthorizationHeader("Bearer token");
            _aiServiceMock.Setup(s => s.GetPopularQueriesAsync(10, It.IsAny<string>(), "token"))
                .ReturnsAsync(queries);

            var result = await _controller.GetPopularQueries();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(queries, okResult.Value);
        }

        [Fact]
        public async Task GetPopularQueries_ReturnsUnauthorized_WhenUserContextInvalid()
        {
            _controller.SetUserContextValid(false, 0);

            var result = await _controller.GetPopularQueries();

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPopularQueries_ReturnsBadRequest_WhenLimitInvalid()
        {
            _controller.SetUserContextValid(true, 1);

            var result = await _controller.GetPopularQueries(0);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPopularQueries_ReturnsServerError_WhenHttpRequestException()
        {
            _controller.SetUserContextValid(true, 1);
            _controller.SetAuthorizationHeader("Bearer token");
            _aiServiceMock.Setup(s => s.GetPopularQueriesAsync(10, It.IsAny<string>(), "token"))
                .ThrowsAsync(new HttpRequestException("fail"));

            var result = await _controller.GetPopularQueries();

            var status = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, status.StatusCode);
        }

        private class TestAIAssistantController : AIAssistantController
        {
            private bool _isUserContextValid;
            private int _userId;

            public TestAIAssistantController(
                IAIAssistantService aiService,
                IUserContextService userContextService,
                ILogger<AIAssistantController> logger)
                : base(aiService, userContextService, logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                };
            }

            public void SetUserContextValid(bool valid, int userId)
            {
                _isUserContextValid = valid;
                _userId = userId;
            }

            public void SetAuthorizationHeader(string value)
            {
                HttpContext.Request.Headers["Authorization"] = value;
            }

            protected override bool IsUserContextValid(out int userId)
            {
                userId = _userId;
                return _isUserContextValid;
            }
        }
    }
}