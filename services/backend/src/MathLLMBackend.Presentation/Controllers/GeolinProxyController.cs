using MathLLMBackend.Core.Services.GeolinService;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Presentation.Dtos.Geolin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/geolin-proxy")]
    [Authorize]
    public class GeolinProxyController : ControllerBase
    {
        private readonly IGeolinService _geolinService;

        public GeolinProxyController(IGeolinService geolinService)
        {
            _geolinService = geolinService ?? throw new ArgumentNullException(nameof(geolinService));
        }

        [HttpPost("check-answer-direct")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> CheckAnswerDirect([FromBody] CheckAnswerRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Hash))
            {
                return BadRequest(new CheckAnswerResponse
                {
                    Error = "Hash is required.",
                    Hash = request.Hash,
                    AnswerAttempt = request.AnswerAttempt,
                    Seed = request.Seed
                });
            }

            if (string.IsNullOrWhiteSpace(request.AnswerAttempt))
            {
                return BadRequest(new CheckAnswerResponse
                {
                    Error = "Answer attempt is required.",
                    Hash = request.Hash,
                    AnswerAttempt = request.AnswerAttempt,
                    Seed = request.Seed
                });
            }

            var result = await _geolinService.CheckAnswer(
                request.Hash,
                request.AnswerAttempt,
                request.Seed,
                request.ProblemParams,
                ct);

            var response = new CheckAnswerResponse
            {
                IsCorrect = result.IsCorrect,
                Message = result.IsCorrect
                    ? $"Ответ правильный (verdict: {result.Verdict})"
                    : $"Ответ неправильный (verdict: {result.Verdict})",
                Hash = request.Hash,
                AnswerAttempt = request.AnswerAttempt,
                Seed = request.Seed
            };

            return Ok(response);
        }
    }
}