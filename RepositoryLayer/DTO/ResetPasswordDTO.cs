﻿namespace RepositoryLayer.DTO
{
    public class ResetPasswordDTO
    {
        public string OldPasssword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
