using Contract_Monthly_Claim_System.Models;

namespace Contract_Monthly_Claim_System.Services
{
    public interface IEmailService
    {
        Task SendClaimSubmittedEmailAsync(Claim claim, string recipientEmail);
        Task SendClaimApprovedEmailAsync(Claim claim, string recipientEmail);
        Task SendClaimRejectedEmailAsync(Claim claim, string recipientEmail, string reason);
        Task SendClaimReturnedEmailAsync(Claim claim, string recipientEmail, string comments);
        Task SendNotificationEmailAsync(string recipientEmail, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendClaimSubmittedEmailAsync(Claim claim, string recipientEmail)
        {
            var subject = $"Claim Submitted - {claim.ClaimNumber}";
            var message = $@"
                <h3>Claim Submission Confirmation</h3>
                <p>Your claim <strong>{claim.ClaimNumber}</strong> has been successfully submitted.</p>
                <p><strong>Details:</strong></p>
                <ul>
                    <li>Amount: R{claim.TotalAmount}</li>
                    <li>Period: {claim.PeriodStart:dd MMM yyyy} to {claim.PeriodEnd:dd MMM yyyy}</li>
                    <li>Hours: {claim.HoursWorked}</li>
                </ul>
                <p>You will be notified once your claim is reviewed.</p>";

            await SendNotificationEmailAsync(recipientEmail, subject, message);
        }

        public async Task SendClaimApprovedEmailAsync(Claim claim, string recipientEmail)
        {
            var subject = $"Claim Approved - {claim.ClaimNumber}";
            var message = $@"
                <h3>Claim Approved</h3>
                <p>Your claim <strong>{claim.ClaimNumber}</strong> has been approved.</p>
                <p><strong>Approved Amount:</strong> R{claim.TotalAmount}</p>
                <p>The payment will be processed according to the payment schedule.</p>";

            await SendNotificationEmailAsync(recipientEmail, subject, message);
        }

        public async Task SendClaimRejectedEmailAsync(Claim claim, string recipientEmail, string reason)
        {
            var subject = $"Claim Requires Attention - {claim.ClaimNumber}";
            var message = $@"
                <h3>Claim Review Required</h3>
                <p>Your claim <strong>{claim.ClaimNumber}</strong> requires your attention.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p>Please review and resubmit your claim with the necessary corrections.</p>";

            await SendNotificationEmailAsync(recipientEmail, subject, message);
        }

        public async Task SendClaimReturnedEmailAsync(Claim claim, string recipientEmail, string comments)
        {
            var subject = $"Claim Returned for Correction - {claim.ClaimNumber}";
            var message = $@"
                <h3>Claim Returned for Correction</h3>
                <p>Your claim <strong>{claim.ClaimNumber}</strong> has been returned for correction.</p>
                <p><strong>Comments:</strong> {comments}</p>
                <p>Please make the necessary corrections and resubmit your claim.</p>";

            await SendNotificationEmailAsync(recipientEmail, subject, message);
        }

        public async Task SendNotificationEmailAsync(string recipientEmail, string subject, string message)
        {
            try
            {
                // In a real application, integrate with SMTP server or email service
                _logger.LogInformation("Email sent to {RecipientEmail}: {Subject}", recipientEmail, subject);

                // Simulate email sending
                await Task.Delay(100);

                _logger.LogInformation("Email successfully sent to {RecipientEmail}", recipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {RecipientEmail}", recipientEmail);
                // Don't throw - email failure shouldn't break the application
            }
        }
    }
}