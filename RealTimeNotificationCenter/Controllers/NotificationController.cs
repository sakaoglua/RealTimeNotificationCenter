using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeNotificationCenter.Hubs;
using RealTimeNotificationCenter.Models;
using RealTimeNotificationCenter.Services;

namespace RealTimeNotificationCenter.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly OnlineUserTrackerService _onlineUserTrackerService;

        public NotificationController(IHubContext<NotificationHub> hubContext, OnlineUserTrackerService onlineUserTrackerService)
        {
            _hubContext = hubContext;
            _onlineUserTrackerService = onlineUserTrackerService;
        }

        [HttpPost("SendNotification")]
        public async Task<IActionResult> SendNotification(NotificationMessage notification)
        {
            var senderUserName = User?.Identity?.Name;
            notification.CreatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(senderUserName))
            {
                return Unauthorized();
            }

            notification.Title = notification.Title.Trim();
            notification.SenderUserName = senderUserName;
            notification.Message = notification.Message.Trim();

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification); // Bütün clientlara aynı anda bildirim gider.
            return Ok(new
            {
                message = "Bildirim tüm kullanıcılara gönderildi.",
                notification
            });
        }

        [HttpPost("SendPrivateNotification")]
        public async Task<IActionResult> SendPrivateNotification(PrivateNotificationRequest request)
        {
            var senderUserName = User.Identity?.Name?.Trim();

            if (string.IsNullOrWhiteSpace(senderUserName))
            {
                return Unauthorized();
            }

            var recipientUserName = request.UserName?.Trim();

            if (string.IsNullOrWhiteSpace(recipientUserName))
            {
                return BadRequest(new
                {
                    message = "Alıcı kullanıcı adı zorunludur."
                });
            }

            if (string.Equals(recipientUserName, senderUserName, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Kendinize özel bildirim gönderemezsiniz."
                });
            }

            var connectionIds = _onlineUserTrackerService.GetConnectionIds(recipientUserName);

            if (connectionIds.Count == 0)
            {
                return NotFound(new
                {
                    message = $"{recipientUserName} şu anda online değil."
                });
            }

            var notification = new NotificationMessage
            {
                Title = request.Title,
                Message = request.Message,
                CreatedAt = DateTime.UtcNow,
                SenderUserName = senderUserName,
            };

            await _hubContext.Clients.Clients(connectionIds).SendAsync("ReceivePrivateNotification", notification); // Sadece belirli clientlara bildirim gider.

            return Ok(new
            {
                message =
                    $"Bildirim yalnızca {recipientUserName} kullanıcısına gönderildi.",
                connectionCount = connectionIds.Count,
                notification
            });
        }

        [HttpPost("SendGroupNotification")]
        public async Task<IActionResult> SendGroupNotification(GroupNotificationRequest request)
        {
            var senderUserName = User.Identity?.Name?.Trim();

            if (string.IsNullOrWhiteSpace(senderUserName))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.GroupName))
            {
                return BadRequest(new
                {
                    message = "Grup adı zorunludur."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new
                {
                    message = "Bildirim başlığı zorunludur."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    message = "Bildirim mesajı zorunludur."
                });
            }

            var notification = new NotificationMessage
            {
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                CreatedAt = DateTime.UtcNow,
                SenderUserName = senderUserName,
            };

            var groupName = request.GroupName.Trim();

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveGroupNotification", groupName, notification); // Sadece belirli grup üyelerine bildirim gider.

            return Ok(new
            {
                message = $"Bildirim {groupName} grubuna gönderildi.",
                groupName,
                notification
            });
        }
    }
}
