namespace PencilCase.Shared.DTOs.Requests.Identity;

public record ResetPasswordRequestDto(string Email, string ResetCode, string NewPassword);